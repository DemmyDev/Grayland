using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CreativeSpore.SuperTilemapEditor;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    #region Variables

    [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
    float speed = 9;

    [SerializeField, Tooltip("Acceleration while grounded.")]
    float walkAcceleration = 75;

    [SerializeField, Tooltip("Acceleration while in the air.")]
    float airAcceleration = 30;

    [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
    float groundDeceleration = 70;

    [SerializeField, Tooltip("Max height the character will jump, regardless of gravity")]
    float jumpHeight = 4;

    [SerializeField, Tooltip("Force applied on player when colliding with Bounce tile on the Y-axis")]
    float bounceForceY;

    [SerializeField, Tooltip("Multiplier of speed when player collides with Bounce tile on the X-axis")]
    float bounceForceX;

    [SerializeField, Tooltip("Gravity applied to player (must be negative value)")]
    float gravity = -9.8f;

    [SerializeField, Tooltip("Layers for objects that can be clinged to by the player")]
    LayerMask whatIsCling;

    [SerializeField, Tooltip("Layers for objects that are considered ceilings")]
    LayerMask whatIsCeiling;

    [SerializeField, Tooltip("After wall jumping, how long it takes for the player to regain input abilities")]
    float afterWallJumpBuffer = .1f;

    [SerializeField, Tooltip("Child object that holds eyes sprite")]
    Transform eyes;

    [SerializeField, Tooltip("Child object that holds body sprite")]
    Transform body;

    [SerializeField, Tooltip("Particle object for jumping, landing, and clinging to walls")]
    GameObject jumpParticle;

    [SerializeField, Tooltip("Particle object for ground movement")]
    ParticleSystem groundParticle;

    [SerializeField, Tooltip("Particle object that spawns on player death")]
    GameObject deathParticle;

    [SerializeField, Tooltip("Child object used for sticking objects")]
    SpriteRenderer stickySprRend;

    [SerializeField, Tooltip("Debug text")]
    Text debugText;

    BoxCollider2D col;
    Vector2 velocity;
    bool grounded, landed;
    bool isClinged = false, canMove = true;
    float currentGravity;
    bool isBlinking = false;
    bool playOnce, isDead = false;

    bool overlap, touchingGround;
    
    STETilemap tilemap;
    SpriteRenderer sprRend;

    #endregion
    
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        sprRend = GetComponent<SpriteRenderer>();
        currentGravity = gravity;
    }

    private void Start()
    {
        tilemap = FindObjectOfType<STETilemap>();
        StartCoroutine(SetUpBlink());
    }

    private void Update()
    {
        if (!isDead)
        {
            #region Movement
            bool hasJumped = false;
            // Use GetAxisRaw to ensure input is either 0, 1 or -1.
            float moveInput = Input.GetAxisRaw("Horizontal");

            if (grounded && !isClinged)
            {
                velocity.y = -.5f;

                if (Input.GetButtonDown("Jump") && !hasJumped)
                {
                    // Velocity needed to achieve jump height
                    velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(currentGravity));
                    hasJumped = true;
                    landed = false;
                    grounded = false;
                    if (moveInput == 0)
                        velocity.x = 0f;
                }
            }

            // Determine accel and decel values based on if the player is grounded or not
            float acceleration = grounded ? walkAcceleration : airAcceleration;
            float deceleration = grounded ? groundDeceleration : 0;

            if (canMove)
            {
                if (moveInput != 0)
                    // Accelerate when moving
                    velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, acceleration * Time.deltaTime);
                else
                    // Decelerate when not moving
                    velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
            }

            velocity.y += currentGravity * Time.deltaTime; // Vertical movement
            transform.Translate(velocity * Time.deltaTime); // Horizontal movement
            grounded = false; // Resets grounded state
            #endregion

            #region Collision
            // Get overlapping colliders after velocity is applied
            Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, col.size, 0);

            // Wall cling and ceiling checking 
            RaycastHit2D leftCheck = Physics2D.Raycast(transform.position, Vector2.left, .51f, whatIsCling); // Left side
            RaycastHit2D rightCheck = Physics2D.Raycast(transform.position, Vector2.right, .51f, whatIsCling); // Right side
            RaycastHit2D ceilingCheck = Physics2D.Raycast(transform.position, Vector2.up, .51f, whatIsCeiling); // Ceiling
            Debug.DrawRay(transform.position, new Vector2(-.51f, 0f), Color.green);
            Debug.DrawRay(transform.position, new Vector2(.51f, 0f), Color.red);

            // Determines if the player is clinged to a wall. If both are true, the player is inside the ground and isn't clinging.
            isClinged = leftCheck ^ rightCheck;

            // Fix for ensuring landed is false when player is in the air
            if (hits.Length == 1 && !isClinged)
                landed = false;

            // Ground checking
            foreach (Collider2D hit in hits)
            {
                // Ignore own collider
                if (hit == col)
                    continue;

                // Do death collision here


                ColliderDistance2D colliderDistance = hit.Distance(col);

                // Are we still overlapping this collider?
                if (colliderDistance.isOverlapped)
                {
                    // Deathbox collision
                    if (hit.gameObject.CompareTag("Deathbox"))
                    {
                        StartCoroutine(LevelController.levelController.LoadLevel(false));
                        Instantiate(deathParticle, transform.position, Quaternion.identity);
                        groundParticle.gameObject.SetActive(false);
                        eyes.gameObject.SetActive(false);
                        body.gameObject.SetActive(false);
                        isDead = true;
                        return;
                    }

                    // Endpoint collision
                    if (hit.gameObject.CompareTag("Endpoint"))
                    {
                        hit.gameObject.GetComponent<LevelEndpoint>().EndLevel(this);
                        LevelController.levelController.DisableSaturation();
                        return;
                    }

                    overlap = true;
                    transform.Translate(colliderDistance.pointA - colliderDistance.pointB);

                    // If the object beneath us intersects, grounded is true 
                    if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0)
                    {
                        touchingGround = true;
                        grounded = true;
                        canMove = true;
                        velocity.y = 0f;

                        // Ensures particle only spawns once
                        if (!landed)
                        {
                            Instantiate(jumpParticle, new Vector2(transform.position.x, transform.position.y - .5f), Quaternion.identity);
                            landed = true;
                            grounded = true;
                        }

                        // Bounce
                        if (hit.gameObject.CompareTag("Bounce"))
                        {
                            BounceTile bounceTile = hit.GetComponentInParent<BounceTile>();
                            // Assuming the bounce tile will boost the Y-axis
                            if (!bounceTile.GetAxis())
                            {
                                velocity.y = Mathf.Sqrt(bounceForceY * jumpHeight * Mathf.Abs(currentGravity));
                                hasJumped = true;
                                landed = false;
                                grounded = false;
                                if (moveInput == 0)
                                    velocity.x = 0f;
                            }
                        }
                    }
                    else
                        touchingGround = false;
                }
                else
                {
                    touchingGround = false;
                    overlap = false;
                }
            }

            // If player is clinged, stop and lock movement
            if (isClinged)
            {
                currentGravity = 0f;
                velocity = Vector2.zero;
                moveInput = 0f;
                canMove = false;

                // Ensures particle only spawns once
                if (!landed)
                {
                    Collider2D tileCol = leftCheck ? leftCheck.collider : rightCheck.collider;

                    // TILE INTERACTIONS

                    // Switch
                    if (tileCol.CompareTag("Switch"))
                    {
                        SwitchTile switchTile = tileCol.GetComponentInParent<SwitchTile>();
                        switchTile.ChangeLockState();
                    }

                    // Pressure
                    if (tileCol.CompareTag("Pressure"))
                    {
                        PressureTile pressureTile = tileCol.GetComponentInParent<PressureTile>();
                        pressureTile.ChangeMoveState(false);
                    }

                    // Bounce 
                    if (tileCol.CompareTag("Bounce"))
                    {
                        BounceTile bounceTile = tileCol.GetComponentInParent<BounceTile>();
                        // Assuming the bounce tile will boost the X-axis
                        if (bounceTile.GetAxis())
                        {
                            currentGravity = gravity;
                            float speedX = leftCheck ? speed : -speed; // Are we clinged on the left or right? 
                            velocity = new Vector2(bounceForceX * speedX, Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(currentGravity)));

                            hasJumped = true;
                            isClinged = false;
                            landed = false;
                            grounded = false;
                            StartCoroutine(SetCanMove(true));
                        }
                    }

                    if (leftCheck)
                        Instantiate(jumpParticle, new Vector2(transform.position.x - .5f, transform.position.y), Quaternion.Euler(0f, 0f, -90f));
                    else
                        Instantiate(jumpParticle, new Vector2(transform.position.x + .5f, transform.position.y), Quaternion.Euler(0f, 0f, 90f));
                    landed = true;
                }

                if (Input.GetButtonDown("Jump") && !hasJumped)
                {
                    Collider2D tileCol = leftCheck ? leftCheck.collider : rightCheck.collider;
                    if (tileCol.CompareTag("Pressure"))
                    {
                        PressureTile pressureTile = tileCol.GetComponentInParent<PressureTile>();
                        pressureTile.ChangeMoveState(true);
                    }

                    currentGravity = gravity;
                    float speedX = leftCheck ? speed : -speed; // Are we clinged on the left or right? 
                    velocity = new Vector2(speedX, Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(currentGravity)));

                    hasJumped = true;
                    isClinged = false;
                    landed = false;
                    grounded = false;
                    StartCoroutine(SetCanMove(true));
                }
            }
            else
            {
                currentGravity = gravity;
            }

            // Stops upward velocity if player hits ceiling
            if (ceilingCheck)
                velocity.y = 0f;
            #endregion

            #region Animations

            // Changes position of eyes based on velocity of player
            eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, velocity.x / 80, .25f), Mathf.Lerp(eyes.localPosition.y, velocity.y / 100, .25f));

            // Eyes blinking
            if (isBlinking)
            {
                eyes.localScale = new Vector2(eyes.localScale.x, Mathf.Lerp(eyes.localScale.y, 0f, .33f));

                if (eyes.localScale.y <= .025f)
                {
                    isBlinking = false;
                }
            }
            else
                eyes.localScale = new Vector2(eyes.localScale.x, Mathf.Lerp(eyes.localScale.y, 1f, .33f));

            // Hotfix for alleviating lerp issues relative to scale
            if (eyes.localScale.y >= .995f)
                eyes.localScale = new Vector2(eyes.localScale.x, 1f);

            // Hotifx for alleviating lerp issues relative to position
            if (eyes.localPosition.y < .01f && eyes.localPosition.y > -.01f && velocity.y > -1 && velocity.y < 0)
                eyes.localPosition = new Vector2(eyes.localPosition.x, 0f);

            // Plays particles if moving, stops particles if not moving
            if ((velocity.y > -1.5f && velocity.y < .25f && velocity.x > -.25f && velocity.x < .25f) || isClinged)
            {
                playOnce = true;
                groundParticle.Stop();
            }
            else if (playOnce)
            {
                playOnce = false;
                groundParticle.Play();
            }

            #endregion

            #region Debug and Hotfixes
            /*if (!grounded && landed && !isClinged)
            {
                Debug.Log("Called hotfix #2");
                canMove = true;
                grounded = true;
            }*/

            // Debug text
            if (debugText != null)
                debugText.text = "Velocity X: " + velocity.x + "\n" +
                "Velocity Y: " + velocity.y + "\n" +
                "Grounded: " + grounded + "\n" +
                "Landed: " + landed + "\n" +
                "Can Move: " + canMove + "\n" +
                "Clinging: " + isClinged + "\n" +
                "Colliding with Object: " + overlap + "\n" +
                "Colliding with Ground?: " + touchingGround;
            #endregion
        }
    }

    /// <summary>
    /// Sets bool that determines if player can use movement input
    /// </summary>
    /// <param name="move">Can the player use movement input?</param>
    /// <returns></returns>
    private IEnumerator SetCanMove(bool move)
    {
        yield return new WaitForSeconds(afterWallJumpBuffer);
        if (!isClinged)
            canMove = true;
    }

    /// <summary>
    /// Sets bool that determines if player's eyes are blinking
    /// </summary>
    /// <returns></returns>
    public IEnumerator SetUpBlink()
    {
        int i = Random.Range(3, 6);
        yield return new WaitForSeconds(i);
        isBlinking = true;
        StartCoroutine(SetUpBlink()); // Loops blink
    }

    public void SetStickyChild(Sprite spr)
    {
        stickySprRend.sprite = spr;
        stickySprRend.transform.localPosition = new Vector2(Random.Range(-.75f, .75f), Random.Range(0, .75f));
        stickySprRend.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
    }
}