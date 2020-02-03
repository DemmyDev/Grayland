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
    float bounceForceY = 4f;

    [SerializeField, Tooltip("Multiplier of speed when player collides with Bounce tile on the X-axis")]
    float bounceForceX = 2f;

    [SerializeField, Tooltip("Time it takes after wall-jumping until player is able to cling to a wall again")]
    float clingBufferTime = .25f;

    [SerializeField, Tooltip("Time that player has to input a jump after falling off ledge without jumping")]
    float coyoteTime = .1f;

    [SerializeField, Tooltip("Amount of time a jump can be buffered for before landing")]
    float jumpBufferTime = .1f;

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
    float clingBufferTimeCurrent = 0f, coyoteTimeCurrent = 0f;
    float jumpBufferCurrent = 0f;
    bool isJumping = false, isWallJumping = false, allowJumping = false, jumpActive = false;
    float speedX;
    float jumpActiveTime = 0, jumpButtonDownTime = 0f, detachActiveTime = 0f;
    
    float moveInput;

    bool detachActive;

    bool overlap, touchingGround;
    
    STETilemap tilemap;
    SpriteRenderer sprRend;

    #endregion
    
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        sprRend = GetComponent<SpriteRenderer>();
        currentGravity = gravity;
        clingBufferTimeCurrent = clingBufferTime;
        coyoteTimeCurrent = coyoteTime;
    }

    private void Start()
    {
        tilemap = FindObjectOfType<STETilemap>();
        StartCoroutine(SetUpBlink());
        StartCoroutine(EnableJump());
    }

    private void Update()
    {
        #region Input & Jump Logic

        if (Input.GetButtonDown("Detach") && detachActiveTime <= 0)
            detachActiveTime = .033f;

        if (detachActiveTime > 0)
        {
            detachActive = true;
            detachActiveTime -= Time.deltaTime;
        }
        else detachActive = false;

        if (Input.GetButtonDown("Jump") && jumpActiveTime <= 0)
            jumpActiveTime = .033f;

        if (jumpActiveTime > 0)
        {
            jumpActive = true;
            jumpActiveTime -= Time.deltaTime;
        }
        else jumpActive = false;

        // Use GetAxisRaw to ensure input is either 0, 1 or -1. LeftInput = -1, RightInput = 1, NoInput = 0
        moveInput = Input.GetAxisRaw("Horizontal");

        if (grounded && !isClinged)
        {
            coyoteTimeCurrent = coyoteTime;
            velocity.y = -.5f;

            if (jumpActive && allowJumping)
            {
                float pitch = 1 + Random.Range(-.25f, .25f);
                AudioManager.am.Play("Jump", pitch);

                // Velocity needed to achieve jump height
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(currentGravity));
                isJumping = true;
                landed = false;
                grounded = false;

                if (moveInput == 0)
                    velocity.x = 0f;
            }
        }
        // If the player is in mid-air and hasn't jumped
        else if (!grounded && !isClinged && !isJumping)
        {
            // Coyote time!
            coyoteTimeCurrent -= Time.deltaTime;

            if (jumpActive && coyoteTimeCurrent > 0 && allowJumping)
            {
                float pitch = 1 + Random.Range(-.25f, .25f);
                AudioManager.am.Play("Jump", pitch);
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(currentGravity));
                isJumping = true;
                landed = false;
                grounded = false;
                coyoteTimeCurrent = coyoteTime;

                if (moveInput == 0)
                    velocity.x = 0f;
            }
        }

        // Jump buffer!
        if (jumpBufferCurrent < jumpBufferTime)
        {
            jumpBufferCurrent += Time.deltaTime;

            if (grounded && allowJumping)
            {
                float pitch = 1 + Random.Range(-.25f, .25f);
                AudioManager.am.Play("Jump", pitch);

                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(currentGravity));
                isJumping = true;
                landed = false;
                grounded = false;
                coyoteTimeCurrent = coyoteTime;

                if (moveInput == 0)
                    velocity.x = 0f;
            }
        }

        if (jumpActive && allowJumping)
            jumpBufferCurrent = 0f;

        if (isJumping && velocity.y > 3f && !Input.GetButton("Jump") && !isWallJumping)
        {
            velocity.y += 2 * currentGravity * Time.deltaTime;
        }

        #endregion
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            #region Movement

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
            RaycastHit2D leftCheck = Physics2D.Raycast(transform.position, Vector2.left, .5f, whatIsCling); // Left side
            RaycastHit2D rightCheck = Physics2D.Raycast(transform.position, Vector2.right, .5f, whatIsCling); // Right side
            Debug.DrawRay(transform.position, new Vector2(-.5f, 0f), Color.green);
            Debug.DrawRay(transform.position, new Vector2(.5f, 0f), Color.red);

            RaycastHit2D ceilingCheck = Physics2D.BoxCast(transform.position, new Vector2(.75f, .1f), 0f, Vector2.up, .35f, whatIsCeiling); // Ceiling check
            
            // If the cling buffer time doesn't apply anymore
            if (clingBufferTimeCurrent <= 0)
            {
                // Determines if the player is clinged to a wall. If both are true, the player is inside the ground and isn't clinging.
                isClinged = leftCheck ^ rightCheck;
            }
            // Otherwise, continue counting down the cling buffer time
            else
            {
                clingBufferTimeCurrent -= Time.deltaTime;
            }

            // Fix for ensuring landed is false when player is in the air
            if (hits.Length == 1 && !isClinged)
                landed = false;

            // Ground checking
            foreach (Collider2D hit in hits)
            {
                // Ignore own collider
                if (hit == col)
                    continue;

                ColliderDistance2D colliderDistance = hit.Distance(col);

                // Are we still overlapping this collider?
                if (colliderDistance.isOverlapped)
                {
                    // Deathbox collision
                    if (hit.gameObject.CompareTag("Deathbox"))
                    {
                        AudioManager.am.Play("Death");
                        Invoke("DelayTransition", .5f);
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
                        isJumping = false;
                        touchingGround = true;
                        isWallJumping = false;
                        grounded = true;
                        canMove = true;
                        velocity.y = 0f;

                        // Ensures particle only spawns once
                        if (!landed)
                        {
                            AudioManager.am.Play("Land");
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
                                AudioManager.am.Play("Bounce");

                                velocity.y = Mathf.Sqrt(bounceForceY * jumpHeight * Mathf.Abs(currentGravity));
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
            if (isClinged && !detachActive)
            {
                currentGravity = 0f;
                velocity = Vector2.zero;
                moveInput = 0f;
                canMove = false;
                isWallJumping = false;

                // Ensures particle only spawns once
                if (!landed)
                {
                    AudioManager.am.Play("Land");
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
                        AudioManager.am.Play("Bounce");

                        BounceTile bounceTile = tileCol.GetComponentInParent<BounceTile>();
                        // Assuming the bounce tile will boost the X-axis
                        if (bounceTile.GetAxis())
                        {
                            isWallJumping = true;
                            currentGravity = gravity;
                            speedX = leftCheck ? speed : -speed; // Are we clinged on the left or right? 
                            velocity = new Vector2(bounceForceX * speedX, Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(currentGravity)));
                            
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

                if (jumpActive && allowJumping)
                {
                    Collider2D tileCol = leftCheck ? leftCheck.collider : rightCheck.collider;
                    if (tileCol.CompareTag("Pressure"))
                    {
                        PressureTile pressureTile = tileCol.GetComponentInParent<PressureTile>();
                        pressureTile.ChangeMoveState(true);
                    }

                    float pitch = 1 + Random.Range(-.25f, .25f);
                    AudioManager.am.Play("Jump", pitch);
                    currentGravity = gravity;
                    speedX = leftCheck ? speed : -speed; // Are we clinged on the left or right? 
                    velocity = new Vector2(speedX, Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(currentGravity)));
                    clingBufferTimeCurrent = clingBufferTime;
                    
                    isJumping = true;
                    isWallJumping = true;
                    isClinged = false;
                    landed = false;
                    grounded = false;
                    StartCoroutine(SetCanMove(true));
                }
            }
            else if (isClinged && detachActive)
            {
                Collider2D tileCol = leftCheck ? leftCheck.collider : rightCheck.collider;
                if (tileCol.CompareTag("Pressure"))
                {
                    PressureTile pressureTile = tileCol.GetComponentInParent<PressureTile>();
                    pressureTile.ChangeMoveState(true);
                }

                currentGravity = gravity;
                speedX = leftCheck ? speed : -speed; // Are we clinged on the left or right? 
                velocity = new Vector2(speedX / 2, 0f);
                clingBufferTimeCurrent = clingBufferTime;

                isClinged = false;
                isJumping = true;
                isWallJumping = true;
                landed = false;
                grounded = false;
                StartCoroutine(SetCanMove(true));
            }
            else
            {
                currentGravity = gravity;
            }

            // Stops upward velocity if player hits ceiling
            if (ceilingCheck)
            {
                bool hittinCeiling = true;
                Collider2D[] ceilingHits = Physics2D.OverlapBoxAll(transform.position, col.size, 0);

                // If colliding with a Bounce tile, the player is not hitting the ceiling
                foreach (Collider2D hit in ceilingHits)
                {
                    if (hit.transform.tag == "Bounce")
                        hittinCeiling = false;
                }

                if (hittinCeiling)
                    velocity.y = 0f;
            }

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

            #region Debug

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

    private IEnumerator EnableJump()
    {
        yield return new WaitForSeconds(.5f);
        allowJumping = true;
    }

    /// <summary>
    /// Sets a sprite that sticks to the player
    /// </summary>
    /// <param name="spr">Sprite sticking to player</param>
    public void SetStickyChild(Sprite spr)
    {
        stickySprRend.sprite = spr;
        stickySprRend.transform.localPosition = new Vector2(Random.Range(-.75f, .75f), Random.Range(0, .75f));
        stickySprRend.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(0, 360));
    }

    void DelayTransition()
    {
        StartCoroutine(LevelController.levelController.LoadLevel(false));
    }
}