using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CreativeSpore.SuperTilemapEditor;
using UnityEngine.SceneManagement;

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

    [SerializeField, Tooltip("Max speed player can achieve when falling downwards. Must be negative")]
    float maxFallSpeed;

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

    [SerializeField, Tooltip("Time until player detaches from wall when holding the opposite input from the wall")]
    float detachWaitTime;

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

    [SerializeField, Tooltip("Hidden object that is used to color player in final level")]
    SpriteRenderer colorBody;

    [SerializeField, Tooltip("Child object used for sticking objects")]
    SpriteRenderer stickySprRend;

    [SerializeField, Tooltip("Debug text")]
    Text debugText;

    BoxCollider2D col;
    Vector2 velocity, bufferedVelocity;
    bool grounded, landed;
    bool isClinged = false, canMove = true;
    float currentGravity;
    bool isBlinking = false, lockBlink = false;
    bool isTeleporting = false;
    bool playOnce, isDead = false;
    float clingBufferTimeCurrent = 0f, coyoteTimeCurrent = 0f;
    float jumpBufferCurrent = 0f;
    bool isJumping = false, isWallJumping = false, allowJumping = false, jumpActive = false;
    float speedX;
    float jumpActiveTime = 0, jumpButtonDownTime = 0f;
    bool movingToTargetPos = false, forcedInput = false;
    RaycastHit2D leftCheck, rightCheck;
    
    float moveInput, forcedMoveInput;
    float targetPosX;

    bool overlap, touchingGround;
    float currentDetachWaitTime;
    bool detachActive = false;
    STETilemap tilemap;

    PauseParent pauseParent;
    
    SpriteRenderer sprRend;

    Vector2 eyesTargetPos;
    float eyesDialoguePosX;

    bool allowInput = true, lockVelocityY = false;

    #endregion
    
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        sprRend = GetComponent<SpriteRenderer>();
        currentGravity = gravity;
        clingBufferTimeCurrent = clingBufferTime;
        coyoteTimeCurrent = coyoteTime;
        currentDetachWaitTime = detachWaitTime;
        bufferedVelocity = Vector2.zero;
        if (SceneManager.GetActiveScene().buildIndex == 1) pauseParent = UIController.UIControl.GetPauseParent().GetComponent<PauseParent>();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0) StartCoroutine(SetUpBlink());
        StartCoroutine(EnableJump());
        // Change this pls
        tilemap = FindObjectOfType<STETilemap>();
    }

    private void Update()
    {
        #region Input & Jump Logic

        if (bufferedVelocity != Vector2.zero)
        {
            velocity = bufferedVelocity;
            bufferedVelocity = Vector2.zero;
        }

        bool inputJump = Input.GetButtonDown("Jump");

        // Ensure PauseParent is within scene before checking for bool
        if (pauseParent != null)
        {
            if (!allowInput || pauseParent.GetIsPaused())
                inputJump = false;
        }
        else if (!allowInput)
                inputJump = false;


        if (inputJump && jumpActiveTime <= 0)
            jumpActiveTime = .033f;

        if (jumpActiveTime > 0)
        {
            jumpActive = true;
            jumpActiveTime -= Time.deltaTime;
        }
        else jumpActive = false;

        // Use GetAxisRaw to ensure input is either 0, 1 or -1. LeftInput = -1, RightInput = 1, NoInput = 0
        if (allowInput) 
            moveInput = Input.GetAxisRaw("Horizontal");
        else
            moveInput = 0;

        // If the player is grounded, but not clinged to a walll
        if (grounded && !isClinged)
        {
            coyoteTimeCurrent = coyoteTime;
            velocity.y = -.5f;

            // If the player is able to jump and has tried to jump
            if (jumpActive && allowJumping)
            {
                Debug.Log("Grounded jump");
                float pitch = 1 + Random.Range(-.25f, .25f);
                AudioManager.am.Play("Jump", pitch);

                // Velocity needed to achieve jump height
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(currentGravity));
                isJumping = true;
                landed = false;
                grounded = false;
                clingBufferTimeCurrent = clingBufferTime;

                if (moveInput == 0)
                    velocity.x = 0f;
            }
        }
        // If the player is in mid-air and hasn't jumped
        else if (!grounded && !isClinged && !isJumping)
        {
            // Coyote time!
            coyoteTimeCurrent -= Time.deltaTime;

            // If the player has attempted to jump and coyote time allows the jump to happen
            if (jumpActive && coyoteTimeCurrent > 0 && allowJumping)
            {
                Debug.Log("Coyote jump");
                float pitch = 1 + Random.Range(-.25f, .25f);
                AudioManager.am.Play("Jump", pitch);
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(currentGravity));
                isJumping = true;
                landed = false;
                grounded = false;
                coyoteTimeCurrent = coyoteTime;
                clingBufferTimeCurrent = clingBufferTime;

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
                Debug.Log("Buffered jump");
                float pitch = 1 + Random.Range(-.25f, .25f);
                AudioManager.am.Play("Jump", pitch);

                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(currentGravity));
                isJumping = true;
                landed = false;
                grounded = false;
                coyoteTimeCurrent = coyoteTime;
                clingBufferTimeCurrent = clingBufferTime;

                if (moveInput == 0)
                    velocity.x = 0f;
            }
        }

        if (jumpActive && allowJumping)
            jumpBufferCurrent = 0f;

        // Apply gravity
        if (isJumping && velocity.y > 3f && !Input.GetButton("Jump") && !isWallJumping && !lockVelocityY)
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

            if (canMove || movingToTargetPos || forcedInput)
            {
                if (movingToTargetPos || forcedInput)
                    moveInput = forcedMoveInput;

                if (moveInput != 0)
                    // Accelerate when moving
                    velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, acceleration * Time.deltaTime);
                else
                    // Decelerate when not moving
                    velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
            }

            if (movingToTargetPos)
            {
                Debug.Log("Moving target");
                float targetDistance = Mathf.Abs(targetPosX - transform.position.x);

                if (targetDistance <= .2f)
                {
                    Debug.Log(movingToTargetPos);
                    velocity.x = 0f;
                    movingToTargetPos = false;
                }
            }

            velocity.y += currentGravity * Time.deltaTime; // Vertical movement
            if (velocity.y < maxFallSpeed) velocity.y = maxFallSpeed; // Terminal velocity
            transform.Translate(velocity * Time.deltaTime); // Horizontal movement
            grounded = false; // Resets grounded state
            #endregion

            #region Collision
            // Get overlapping colliders after velocity is applied
            Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, col.size, 0);

            // Wall cling and ceiling checking 
            leftCheck = Physics2D.Raycast(transform.position, Vector2.left, .5f, whatIsCling); // Left side
            rightCheck = Physics2D.Raycast(transform.position, Vector2.right, .5f, whatIsCling); // Right side
            //Debug.DrawRay(transform.position, new Vector2(-.5f, 0f), Color.green);
            //Debug.DrawRay(transform.position, new Vector2(.5f, 0f), Color.red);

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

            // Reset teleportating bool
            isTeleporting = false;

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

                    // Teleport collision
                    if (hit.gameObject.CompareTag("Teleport") && !isTeleporting)
                    {
                        TeleportTile teleport = hit.GetComponentInParent<TeleportTile>().GetOtherTeleporter();
                        Vector3 newPos = teleport.GetTeleportPos();
                        transform.position = newPos;
                        bufferedVelocity = velocity;

                        landed = false;
                        grounded = false;
                        isTeleporting = true;
                        isClinged = false;
                    }

                    overlap = true;
                    transform.Translate(colliderDistance.pointA - colliderDistance.pointB);

                    // If the object beneath us intersects, grounded is true 
                    if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0 && !isTeleporting)
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
                            eyes.localScale = new Vector2(eyes.localScale.x, 0f);
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
                                
                                // Somehow fixed by having tile colliders having IsTrigger set to false
                                // What if we stopped making games like all of them :)
                                /*
                                RaycastHit2D castToGround = Physics2D.Raycast(transform.position, Vector2.down, .5f, whatIsCling); // Right side

                                // Adjust player position to ensure player does not end up inside tile
                                if (castToGround.collider != null && castToGround.collider.gameObject.CompareTag("Bounce"))
                                {
                                    Vector2 gridPos = TilemapUtils.GetGridPosition(tilemap, castToGround.point);
                                    Vector2 tilePos = TilemapUtils.GetGridWorldPos(tilemap, (int)gridPos.x, (int)gridPos.y);
                                    
                                    transform.position = new Vector2(transform.position.x, tilePos.y + .25f);

                                }
                                */

                                velocity.y = Mathf.Sqrt(bounceForceY * jumpHeight * Mathf.Abs(currentGravity));
                                isClinged = false;
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

                    // If dialogueActive is true
                        // Call WriteText to disable text
                }
            }
            
            RaycastHit2D groundCheck = Physics2D.Raycast(transform.position, Vector2.down, .5f, whatIsCling);
            if (isClinged && groundCheck)
            {
                velocity.x = 0f;
                isClinged = false;
            }

            if (((leftCheck && moveInput == 1) || (rightCheck && moveInput == -1)) && isClinged)
            {
                if (currentDetachWaitTime > 0)
                {
                    detachActive = false;
                    currentDetachWaitTime -= Time.deltaTime;
                }
                else detachActive = true;
            }
            else
            {
                detachActive = false;
                currentDetachWaitTime = detachWaitTime;
            }

            // If player is clinged, stop and lock movement
            if (!detachActive && isClinged)
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
                    eyes.localScale = new Vector2(eyes.localScale.x, 0f);
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
                        pressureTile.ChangeMoveState(true);
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
                            detachActive = false;
                            currentDetachWaitTime = detachWaitTime;
                            StartCoroutine(SetCanMove(.66f));
                        }
                    }

                    if (tileCol.CompareTag("Teleport") && !isTeleporting)
                    {
                        TeleportTile teleport = tileCol.GetComponentInParent<TeleportTile>().GetOtherTeleporter();
                        Vector3 newPos = teleport.GetTeleportPos();
                        transform.position = newPos;
                        bufferedVelocity = velocity;

                        landed = false;
                        grounded = false;
                        isTeleporting = true;
                        isClinged = false;
                    }

                    // Spawn particles when colliding with wall
                    if (leftCheck)
                        Instantiate(jumpParticle, new Vector2(transform.position.x - .5f, transform.position.y), Quaternion.Euler(0f, 0f, -90f));
                    else
                        Instantiate(jumpParticle, new Vector2(transform.position.x + .5f, transform.position.y), Quaternion.Euler(0f, 0f, 90f));

                    landed = true;
                }

                // Wall jump
                if (jumpActive && allowJumping)
                {
                    Collider2D tileCol = leftCheck ? leftCheck.collider : rightCheck.collider;
                    if (tileCol.CompareTag("Pressure"))
                    {
                        PressureTile pressureTile = tileCol.GetComponentInParent<PressureTile>();
                        pressureTile.ChangeMoveState(false);
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
                    detachActive = false;
                    currentDetachWaitTime = detachWaitTime;
                    StartCoroutine(SetCanMove(true));
                }
            }
            else if (detachActive && isClinged)
            {
                Collider2D tileCol = leftCheck ? leftCheck.collider : rightCheck.collider;
                if (tileCol.CompareTag("Pressure"))
                {
                    PressureTile pressureTile = tileCol.GetComponentInParent<PressureTile>();
                    pressureTile.ChangeMoveState(false);
                }

                currentGravity = gravity;
                speedX = leftCheck ? speed : -speed; // Are we clinged on the left or right? 
                velocity = new Vector2(speedX / 4, 0f);
                clingBufferTimeCurrent = clingBufferTime;

                isClinged = false;
                isJumping = true;
                isWallJumping = true;
                landed = false;
                grounded = false;
                detachActive = false;
                currentDetachWaitTime = detachWaitTime;
                canMove = true;
            }
            else
            {
                currentGravity = gravity;
            }

            // Stops upward velocity if player hits ceiling
            if (ceilingCheck)
            {
                bool hittingCeiling = true;
                Collider2D[] ceilingHits = Physics2D.OverlapBoxAll(transform.position, col.size, 0);

                // If colliding with a Bounce tile, the player is not hitting the ceiling
                foreach (Collider2D hit in ceilingHits)
                {
                    if (hit.transform.tag == "Bounce")
                        hittingCeiling = false;
                }

                if (hittingCeiling)
                    velocity.y = 0f;
            }

            #endregion

            #region Animations

            // Changes position of eyes based on velocity of player
            if (allowInput || movingToTargetPos || forcedInput)
            {
                eyesTargetPos = new Vector2(Mathf.Lerp(eyes.localPosition.x, velocity.x / 80, .25f), Mathf.Lerp(eyes.localPosition.y, velocity.y / 100, .25f));
            }
            else
            {
                eyesTargetPos = new Vector2(Mathf.Lerp(eyes.localPosition.x, eyesDialoguePosX, .25f), 0f);
            }

            eyes.localPosition = eyesTargetPos;

            // Eyes blinking
            if (!lockBlink)
            {
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

                // Hotfix for alleviating lerp issues relative to position
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

    private IEnumerator SetCanMove(float time)
    {
        yield return new WaitForSeconds(time);
        if (!isClinged)
            canMove = true;
    }

    /// <summary>
    /// Sets bool that determines if player's eyes are blinking
    /// </summary>
    /// <returns></returns>
    public IEnumerator SetUpBlink()
    {
        while (true)
        {
            int i = Random.Range(3, 6);
            yield return new WaitForSeconds(i);
            isBlinking = true;
        }
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
        stickySprRend.transform.localPosition = new Vector2(Random.Range(-.66f, .66f), Random.Range(0, .75f));
        stickySprRend.transform.localEulerAngles = new Vector3(0f, 0f, Random.Range(-45, 45));
    }

    void DelayTransition()
    {
        StartCoroutine(LevelController.levelController.LoadLevel(false));
    }

    public void SetMove(bool move)
    {
        allowInput = move;
    }

    public void SetEyesTargetPos(Transform charPos)
    {
        float dif = charPos.position.x - transform.position.x;

        // If value is positive, eyes look to right
        if (dif >= 0)
            eyesDialoguePosX = .08f;
        // If value is negative, eyes look to left
        else
            eyesDialoguePosX = -.08f;
    }

    public bool GetGrounded()
    {
        return grounded;
    }

    public void ForceSetMoveInput(float posX)
    {
        movingToTargetPos = true;
        targetPosX = posX;
        float dif = targetPosX - transform.position.x;

        forcedMoveInput = dif >= 0 ? 1 : -1;
    }
    
    public void ForceSetMoveInput(bool left, bool right, bool up, bool down)
    {
        forcedInput = true;

        if (left)
        {
            forcedMoveInput = -1;
        }
        else if (right)
        {
            forcedMoveInput = 1;
        }
        else if (up)
        {
            lockVelocityY = true;
            forcedMoveInput = 0;
            velocity.x = 0f;
            velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(currentGravity));
        }
        else if (down)
        {
            forcedMoveInput = 0;
            velocity.x = 0f;
        }
    }

    public void SetBlinking(bool blink, bool lockB)
    {
        isBlinking = blink;
        lockBlink = lockB;
    }

    public bool MovingToTargetPos()
    {
        return movingToTargetPos;
    }

    public Transform GetEyes()
    {
        return eyes;
    }

    public STETilemap GetTilemap()
    {
        return tilemap;
    }

    public SpriteRenderer GetColorBody()
    {
        return colorBody;
    }
}