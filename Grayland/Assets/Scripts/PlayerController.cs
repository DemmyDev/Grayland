using UnityEngine;
using System.Collections;

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

    [SerializeField, Tooltip("Gravity applied to player (must be negative value)")]
    float gravity = -9.8f;

    [SerializeField, Tooltip("Layers for objects that can be clinged to by the player")]
    LayerMask whatIsCling;

    [SerializeField, Tooltip("After wall jumping, how long it takes for the player to regain ")]
    float afterWallJumpBuffer = .1f;

    [SerializeField, Tooltip("Child object that holds eyes sprite")]
    Transform eyes;

    [SerializeField, Tooltip("Particle object for jumping, landing, and clinging to walls")]
    GameObject particleObj;

    BoxCollider2D col;
    Vector2 velocity;
    bool grounded, landed;
    bool isClinged = false, canMove = true;
    float currentGravity;

    #endregion

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();
        currentGravity = gravity;
    }

    private void Update()
    {
        // Use GetAxisRaw to ensure input is either 0, 1 or -1.
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (grounded && !isClinged)
        {
            velocity.y = 0;

            if (Input.GetButtonDown("Jump"))
            {
                // Velocity needed to achieve jump height
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(currentGravity));

                landed = false;
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

        // Get overlapping colliders after velocity is applied
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, col.size, 0);

        // Wall cling checking 
        RaycastHit2D leftCheck = Physics2D.Raycast(transform.position, Vector2.left, .51f, whatIsCling); // Left side
        RaycastHit2D rightCheck = Physics2D.Raycast(transform.position, Vector2.right, .51f, whatIsCling); // Right side
        Debug.DrawRay(transform.position, new Vector2(-.51f, 0f), Color.green);
        Debug.DrawRay(transform.position, new Vector2(.51f, 0f), Color.red);
        isClinged = leftCheck || rightCheck; // Determine if the player is clinged to a wall

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
                transform.Translate(colliderDistance.pointA - colliderDistance.pointB);

                // If the object beneath us intersects, grounded is true 
                if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0)
                {
                    grounded = true;

                    // Ensures particle only spawns once
                    if (!landed)
                    {
                        Instantiate(particleObj, new Vector2(transform.position.x, transform.position.y - .5f), Quaternion.identity);
                        landed = true;
                    }
                }
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
                if (leftCheck)
                    Instantiate(particleObj, new Vector2(transform.position.x -.5f, transform.position.y), Quaternion.Euler(0f, 0f, -90f));
                else
                    Instantiate(particleObj, new Vector2(transform.position.x +.5f, transform.position.y), Quaternion.Euler(0f, 0f, 90f));
                landed = true;
            }

            if (Input.GetButtonDown("Jump"))
            {
                currentGravity = gravity;
                float speedX = leftCheck ? speed : -speed;
                velocity = new Vector2(speedX, Mathf.Sqrt(2f * jumpHeight * Mathf.Abs(currentGravity)));
                
                isClinged = false;
                landed = false;
                StartCoroutine(SetCanMove(true));
            }
        }

        // Changes position of eyes based on velocity of player
        eyes.localPosition = new Vector2(Mathf.Lerp(eyes.localPosition.x, velocity.x / 80, .25f), Mathf.Lerp(eyes.localPosition.y, velocity.y / 100, .25f));
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
}
