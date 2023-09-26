using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Ability Switcher
    [SerializeField] private bool doubleJump = false;
    [SerializeField] private bool sprint = false;       // RightCtrl to use
    [SerializeField] private bool dash = false;         // LeftAlt to use
    [SerializeField] private bool jetPack = false;      // LeftShift to use
    [SerializeField] private bool antigravity = false;
    [SerializeField] private bool ghost = false;        // LeftCtrl to use
    [SerializeField] private bool wallJump = false;

    // Defaults from Inspector
    [SerializeField] private LayerMask ground;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask worldObjectLayer;
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private TextMeshProUGUI controllerGuides;

    // Defaults
    

    // Constants
    private readonly float defaultMoveSpeed = 10f;      // Must match moveSpeed

    // Player Body Control
    private Rigidbody2D rb;
    private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private float dirX;
    private Vector3 defaultScale;

    // Double Jump Variables
    private int extraJumps = 1;

    // Dash Variables
    int dash_direction = 1;
    float rayLength = 5f;

    // Wall Slide and Wall Jump Variables
    [SerializeField] private float xWallForce = 15f;
    [SerializeField] private float yWallForce = 15f;
    [SerializeField] private float wallJumpTime = 0.05f;
    [SerializeField] private float wallSlidingSpeed = 2f;
    [SerializeField] private float checkRadius = 0.5f;
    [SerializeField] private Transform frontCheck;
    private bool isTouchingFront;
    private bool wallSliding;
    private bool wallJumping;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();

        defaultScale = transform.localScale;

        // Rigid Body Defaults
        rb.mass = 2f;
        rb.gravityScale = 5f;
    }

    void FixedUpdate()
    {
        if (rb.bodyType.ToString() == "Dynamic")
        {
            EnableConditionalSprinting();
            EnableConditionalAntiGravity();
            EnableConditionalJetPack();
            EnableConditionalGhost();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.bodyType.ToString() == "Dynamic")
        {
            EnableCondtionalWallSlideAndWallJump();
            EnableConditionalDash();
            FaceDirectionOfMovement();
        }

        // Update UI Texts
        controllerGuides.text = "";

        if (doubleJump)
            controllerGuides.text += "Double Jump (Space 2x): ON <space=5em>";
        else
            controllerGuides.text += "Double Jump (Space 2x): OFF <space=5em>";

        if (sprint)
            controllerGuides.text += "Sprint (RightCtrl): ON <space=5em>";
        else
            controllerGuides.text += "Sprint (RightCtrl): OFF <space=5em>";

        if (dash)
            controllerGuides.text += "Dash (LeftAlt): ON <space=5em>";
        else
            controllerGuides.text += "Dash (LeftAlt): OFF <space=5em>";

        if (jetPack)
            controllerGuides.text += "JetPack (LeftShift): ON <space=5em>";
        else
            controllerGuides.text += "JetPack (LeftShift): OFF <space=5em>";

        if (antigravity)
        {
            controllerGuides.text += "Antigravity: ON <space=5em>";
            wallJump = false;        // For now, no wall jumping while antigravity is on
        }
        else
            controllerGuides.text += "Antigravity: OFF <space=5em>";

        if (ghost)
            controllerGuides.text += "Ghost (LeftCtrl): ON <space=5em>";
        else
            controllerGuides.text += "Ghost (LeftCtrl): OFF <space=5em>";

        if (wallJump)
            controllerGuides.text += "WallJump (Space): ON <space=5em>";
        else
            controllerGuides.text += "WallJump (Space): OFF <space=5em>";
    }

    // Face direction of movement
    private void FaceDirectionOfMovement()
    {
        if (dirX != 0)
            transform.localScale = defaultScale * dirX / Math.Abs(dirX);
    }

    public void Walk()
    {
        dirX = Input.GetAxisRaw("Horizontal");
        if (Input.GetKey(KeyCode.Z) || Input.GetButton("X"))
            rb.velocity = new Vector2(0f, rb.velocity.y);
        else
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
    }
    
    public void Jump(InputAction.CallbackContext context)
    {
        if (!Input.GetKey(KeyCode.Z) && !Input.GetButton("X") && context.performed)
        {
            if (IsGrounded())
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            else if (doubleJump && extraJumps > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                extraJumps--;
            }
        }
    }

    private void EnableConditionalSprinting()
    {
        if (sprint && (Input.GetKey(KeyCode.RightControl) || Input.GetButton("Left Bumper")))
        {
            moveSpeed = defaultMoveSpeed * 1.5f;
        }
        else
        {
            moveSpeed = defaultMoveSpeed;
        }
    }

    private void EnableConditionalAntiGravity()
    {
        if (antigravity)
        {
            rb.gravityScale = -5f;
        }
        else
        {
            rb.gravityScale = 5f;
            
        }
        jumpForce = rb.gravityScale * 3;

        // rotate player 180 degrees
        if (antigravity && Math.Abs((rb.rotation + 180f) % 360) > 2)
        {
            rb.rotation -= 15f;
        }

        else if (!antigravity && Math.Abs(rb.rotation % 360) > 2)
        {
            rb.rotation += 15f;
        }
    }

    private void EnableConditionalDash()
    {
        if (dash)
        {
            float dashDistance = ObstacleDistance();
            
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetButtonDown("Right Bumper"))
            {
                if (dash_direction == -1)
                {
                    transform.position = new Vector3(transform.position.x - dashDistance, transform.position.y);
                }
                else
                {
                    transform.position = new Vector3(transform.position.x + dashDistance, transform.position.y);
                }
            }
        }
    }

    private void EnableConditionalJetPack()
    {
        float thrust = 150f;

        if (jetPack)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetButton("B"))
            {
                rb.AddForce(transform.up * thrust);
            }
        }
    }

    private void EnableConditionalGhost()
    {
        if (ghost && (Input.GetKey(KeyCode.LeftControl) || Input.GetButton("Y")))
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 0.5f);
            Physics2D.IgnoreLayerCollision(0, LayerMask.NameToLayer("WorldObject"));
        }
        else
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, 1f);
            Physics2D.IgnoreLayerCollision(0, LayerMask.NameToLayer("WorldObject"), false);
        }
    }

    private void EnableCondtionalWallSlideAndWallJump()
    {
        if (wallJump)
        {
            // for wall sliding
            isTouchingFront = Physics2D.OverlapCircle(frontCheck.position, checkRadius, wallLayer);
            if (isTouchingFront && !IsGrounded() && dirX != 0)
                wallSliding = true;
            else
                wallSliding = false;

            if (wallSliding)
            {
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
            }

            // for wall jumping
            if (Input.GetButtonDown("Jump") && wallSliding)
            {
                wallJumping = true;
                Invoke("SetWallJumpingToFalse", wallJumpTime);
            }

            if (wallJumping)
            {
                rb.velocity = new Vector2(xWallForce * -dirX, yWallForce);
            }
        }
    }

    void SetWallJumpingToFalse()
    {
        wallJumping = false;
    }

    private float ObstacleDistance()
    {
        // Update dash_direction if dirX is not 0
        dash_direction = dirX != 0f? (int) (dirX / Math.Abs(dirX)) : dash_direction;

        // Detect nearest object to left ray. ~ flips the layermask i.e. selects all except
        RaycastHit2D leftRay = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.left, rayLength, ~(1<<LayerMask.NameToLayer("Default")));
        RaycastHit2D rightRay = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.right, rayLength, ~(1<<LayerMask.NameToLayer("Default")));
        
        // dash_direction is either -1 or 1, left or right
        RaycastHit2D raycastHit = dash_direction == -1 ? leftRay : rightRay;

        // Change color whether or not raycast is touching a wall
        Color rayColor = raycastHit.collider != null ? Color.green : Color.red;

        if (dash_direction == -1)
        {
            Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, 0), Vector2.left * (coll.bounds.extents.x + rayLength), rayColor);
        }
        else
        {
            Debug.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x, 0), Vector2.right * (coll.bounds.extents.x + rayLength), rayColor);
        }

        if (raycastHit.collider != null)
        {
            float edge = raycastHit.point.x;
            float playerFront = dash_direction == -1 ? coll.bounds.center.x - coll.bounds.extents.x : coll.bounds.center.x + coll.bounds.extents.x;
            float distanceToObject = Math.Abs(playerFront - edge);

            rayLength = distanceToObject;

            return distanceToObject;
        }
        else
        {
            rayLength = 5f;
        }

        return rayLength;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            extraJumps = 1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            extraJumps = 1;
    }

    // Checks to see if the Player's Collider is touching the Ground Layer
    private bool IsGrounded()
    {
        //LayerMask grounds = (1 << ground) | (1 << ceiling);         // Fancy code for selecting 2 layers 
        bool grounded = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, ground);
        
        if (antigravity)
        {
            return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.up, .1f, ground);
        }
        
        return grounded;
    }
}
