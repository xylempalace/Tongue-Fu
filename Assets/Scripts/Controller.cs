using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private BoxCollider2D playerCollider;
    [SerializeField] private Transform firePoint;
    [Space]

    [Header("Layer Masks")]
    [SerializeField] private LayerMask ground;
    [Space]

    [Header("X Movement")]
    [SerializeField] private float moveSpeed;
    [Range(0.01f, 1f), SerializeField] private float xDamping;
    [Space]

    [Header("Jumps")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpBufferTime;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float apexModifier;
    [Space]

    [Header("Falling")]
    [SerializeField] private float gravity;
    [SerializeField] private float fallingGravity;
    [SerializeField] private float fallSpeedClamp;
    [Space]

    [Header("Tongue")]
    [SerializeField] private float tongueDistance;
    [SerializeField] private float swingJump;
    

    private bool variable_jump = true;
    private float jump_buffer;
    private float coyote_timer;
    private bool jump = true;
    private bool facing_right = true;
    private Vector3 facing = Vector3.right;
    private DistanceJoint2D dist_joint;

    private void Update()
    {
        // X movement controls and apex-modifiers
        float sx = rigidBody.velocity.x;
        if (dist_joint == null) {
            sx += Input.GetAxisRaw("Horizontal") * moveSpeed + (!isGrounded() && Mathf.Abs(rigidBody.velocity.y) < 1 ? Input.GetAxisRaw("Horizontal") * apexModifier : 0f);
            sx *= Mathf.Pow(1f - xDamping, Time.deltaTime * 10f);
            rigidBody.velocity = new Vector2(sx, rigidBody.velocity.y);
        }
        else
        {
            
        }

        if ((facing_right && sx < 0) || (!facing_right && sx > 0))
        {
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            // Starting jump buffer timer
            jump_buffer = Time.time;
        } 

        getDirection();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (dist_joint == null)
            {
                RaycastHit2D tongueHit = Physics2D.Raycast(firePoint.position, facing, tongueDistance);
                Debug.DrawLine(firePoint.position, firePoint.position + (facing * tongueDistance), Color.green, 0.1f);
                if (tongueHit.collider != null && tongueHit.collider.name == "Hook")
                {
                    gameObject.AddComponent<DistanceJoint2D>();
                    dist_joint = gameObject.GetComponent<DistanceJoint2D>();
                    dist_joint.connectedBody = tongueHit.rigidbody;
                }
            }
            else
            {
                Destroy(dist_joint);
                dist_joint = null;
                rigidBody.velocity = new Vector2(rigidBody.velocity.x, swingJump);
            }
        }

        if (isGrounded())
        {
            // Staring coyote timer
            coyote_timer = Time.time;
            jump = true;
        }
        else if (!variable_jump && rigidBody.velocity.y > 0 && !Input.GetKey(KeyCode.M))
        {
            // Modifies gravity depending if the player is falling or not
            if (rigidBody.velocity.y < 0)
            {
                rigidBody.gravityScale = fallingGravity;
            }
            else
            {
                rigidBody.gravityScale = gravity;
            }

            // Checks if a variable jump is yet to be executed, if the player is moving up, and if the jump key was released
            // Then halves the y velocity and confirms that the variable jump has been executed
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, rigidBody.velocity.y / 2f);
            variable_jump = true;
        }

        if (jump && Time.time - jump_buffer < jumpBufferTime && Time.time - coyote_timer < coyoteTime)
        {
            // Checks if a jump buffer is valid while on the ground or in coyote time and then executes a jump while stating a variable jump has not been executed
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpHeight);
            variable_jump = false;
            jump = false;
        }

        clampFallSpeed();
    }

    private void getDirection()
    {
        if (facing_right)
        {
            facing = Vector3.right;
        }
        else
        {
            facing = Vector3.left;
        }

        float vertical = Input.GetAxisRaw("Vertical");
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            facing += Vector3.up * (isGrounded() && vertical == -1 ? 0 : vertical);
        }
        else if (vertical != 0)
        {
            if (!(isGrounded() && vertical == -1))
            {
                facing = Vector3.up * vertical;
            }
        }
        facing = facing.normalized;
    }

    private void Flip()
    {
        facing_right = !facing_right;

        transform.Rotate(0f, 180f, 0f);
    }

    private void clampFallSpeed()
    {
        if (rigidBody.velocity.y < -fallSpeedClamp)
        {
            // Clamps fall speed
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, -fallSpeedClamp);
        }
    }

    private bool isGrounded()
    {
        // Checks if the player is touching the ground
        RaycastHit2D raycastHit = Physics2D.BoxCast(playerCollider.bounds.center + new Vector3(0f, -playerCollider.bounds.size.y / 2f, 0f), new Vector3(playerCollider.bounds.size.x, 0.1f, playerCollider.bounds.size.z), 0, Vector2.down, 0.1f, ground);
        return raycastHit.collider != null;
    }
}
