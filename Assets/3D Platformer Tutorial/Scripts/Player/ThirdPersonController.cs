using UnityEngine;
using System.Collections;

[System.Serializable]
// The speed when walking
// after trotAfterSeconds of walking we trot with trotSpeed
// when pressing "Fire3" button (cmd) we start running
// How high do we jump when pressing jump and letting go immediately
// We add extraJumpHeight meters on top when holding the button down longer while jumping
// The gravity for the character
// The gravity in controlled descent mode
// The camera doesnt start following the target immediately but waits for a split second to avoid too much waving around.
// The current move direction in x-z
// The current vertical speed
// The current x-z move speed
// The last collision flags returned from controller.Move
// Are we jumping? (Initiated with jump button and not grounded yet)
// Are we moving backwards (This locks the camera to not do a 180 degree spin)
// Is the user pressing any keys?
// When did the user start walking (Used for going into trot after a while)
// Last time the jump button was clicked down
// Last time we performed a jump
// Average normal of the last touched geometry
// the height we jumped from (Used to determine for how long to apply extra jump power after jumping.)
// When did we touch the wall the first time during this jump (Used for wall jumping)
// This next function responds to the "HidePlayer" message by hiding the player. 
// The message is also 'replied to' by identically-named functions in the collision-handling scripts.
// - Used by the LevelStatus script when the level completed animation is triggered.
 // stop rendering the player.
 // disable player controls.
// This is a complementary function to the above. We don't use it in the tutorial, but it's included for
// the sake of completeness. (I like orthogonal APIs; so sue me!)
 // start rendering the player again.
 // allow player to control the character again.
// Forward vector relative to the camera along the x-z plane	
// Right vector relative to the camera
// Always orthogonal to the forward vector
// Are we moving backwards or looking backwards
// Target direction relative to the camera
// Grounded controls
 // Lock camera for short period when transitioning moving & standing still
// We store speed and direction seperately,
// so that when the character stands still we still have a valid forward direction
// moveDirection is always normalized, and we only update it if there is user input.
 // If we are really slow, just snap to the target direction
// Otherwise smoothly turn towards it
// Smooth the speed based on the current target direction
// Choose target speed
//* We want to support analog input but make sure you cant walk faster diagonally than just forward or sideways
// Pick speed modifier
// Reset walk time start when we slow down
// In air controls
 // Lock camera while in air
 // We must actually jump against a wall for this to work
// Store when we first touched a wall during this jump
// The user can trigger a wall jump by hitting the button shortly before or shortly after hitting the wall the first time.
// Prevent jumping too fast after each other
// Wall jump gives us at least trotspeed
 // Prevent jumping too fast after each other
// Jump
// - Only when pressing the button down
// - With a timeout so you can press the button slightly before landing		
 // don't move player at all if not controllable.
 // Apply gravity
// * When falling down we use controlledDescentGravity (only when holding down jump)
// When we reach the apex of the jump we send out a message
// * When jumping up we don't apply gravity for some time when the user is holding the jump button
//   This gives more control over jump height by pressing the button longer
 // From the jump height and gravity we deduce the upwards speed 
 // for the character to reach at the apex.
 // kill all inputs if not controllable.
// Apply gravity
// - extra power jump modifies gravity
// - controlledDescent mode modifies gravity
// Perform a wall jump logic
// - Make sure we are jumping against wall etc.
// - Then apply jump in the right direction)
// Apply jumping logic
// Calculate actual motion
// Move the controller
// Set rotation to the move direction
 // we got knocked over by an enemy. We need to reset some stuff
// We are in jump mode but just became grounded
//	Debug.DrawRay(hit.point, hit.normal);
 // * When falling down we use controlledDescentGravity (only when holding down jump)
// Require a character controller to be attached to the same game object
[UnityEngine.RequireComponent(typeof(CharacterController))]
[UnityEngine.AddComponentMenu("Third Person Player/Third Person Controller")]
public partial class ThirdPersonController : MonoBehaviour
{
    public float walkSpeed;
    public float trotSpeed;
    public float runSpeed;
    public float inAirControlAcceleration;
    public float jumpHeight;
    public float extraJumpHeight;
    public float gravity;
    public float controlledDescentGravity;
    public float speedSmoothing;
    public float rotateSpeed;
    public float trotAfterSeconds;
    public bool canJump;
    public bool canControlDescent;
    public bool canWallJump;
    private float jumpRepeatTime;
    private float wallJumpTimeout;
    private float jumpTimeout;
    private float groundedTimeout;
    private float lockCameraTimer;
    private Vector3 moveDirection;
    private float verticalSpeed;
    private float moveSpeed;
    private CollisionFlags collisionFlags;
    private bool jumping;
    private bool jumpingReachedApex;
    private bool movingBack;
    private bool isMoving;
    private float walkTimeStart;
    private float lastJumpButtonTime;
    private float lastJumpTime;
    private Vector3 wallJumpContactNormal;
    private float wallJumpContactNormalHeight;
    private float lastJumpStartHeight;
    private float touchWallJumpTime;
    private Vector3 inAirVelocity;
    private float lastGroundedTime;
    private float lean;
    private bool slammed;
    private bool isControllable;
    public virtual void Awake()
    {
        this.moveDirection = this.transform.TransformDirection(Vector3.forward);
    }

    public virtual void HidePlayer()
    {
        ((SkinnedMeshRenderer) GameObject.Find("rootJoint").GetComponent(typeof(SkinnedMeshRenderer))).enabled = false;
        this.isControllable = false;
    }

    public virtual void ShowPlayer()
    {
        ((SkinnedMeshRenderer) GameObject.Find("rootJoint").GetComponent(typeof(SkinnedMeshRenderer))).enabled = true;
        this.isControllable = true;
    }

    public virtual void UpdateSmoothedMovementDirection()
    {
        Transform cameraTransform = Camera.main.transform;
        bool grounded = this.IsGrounded();
        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        float v = Input.GetAxisRaw("Vertical");
        float h = Input.GetAxisRaw("Horizontal");
        if (v < -0.2f)
        {
            this.movingBack = true;
        }
        else
        {
            this.movingBack = false;
        }
        bool wasMoving = this.isMoving;
        this.isMoving = (Mathf.Abs(h) > 0.1f) || (Mathf.Abs(v) > 0.1f);
        Vector3 targetDirection = (h * right) + (v * forward);
        if (grounded)
        {
            this.lockCameraTimer = this.lockCameraTimer + Time.deltaTime;
            if (this.isMoving != wasMoving)
            {
                this.lockCameraTimer = 0f;
            }
            if (targetDirection != Vector3.zero)
            {
                if ((this.moveSpeed < (this.walkSpeed * 0.9f)) && grounded)
                {
                    this.moveDirection = targetDirection.normalized;
                }
                else
                {
                    this.moveDirection = Vector3.RotateTowards(this.moveDirection, targetDirection, (this.rotateSpeed * Mathf.Deg2Rad) * Time.deltaTime, 1000);
                    this.moveDirection = this.moveDirection.normalized;
                }
            }
            float curSmooth = this.speedSmoothing * Time.deltaTime;
            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1f);
            if (Input.GetButton("Fire3"))
            {
                targetSpeed = targetSpeed * this.runSpeed;
            }
            else
            {
                if ((Time.time - this.trotAfterSeconds) > this.walkTimeStart)
                {
                    targetSpeed = targetSpeed * this.trotSpeed;
                }
                else
                {
                    targetSpeed = targetSpeed * this.walkSpeed;
                }
            }
            this.moveSpeed = Mathf.Lerp(this.moveSpeed, targetSpeed, curSmooth);
            if (this.moveSpeed < (this.walkSpeed * 0.3f))
            {
                this.walkTimeStart = Time.time;
            }
        }
        else
        {
            if (this.jumping)
            {
                this.lockCameraTimer = 0f;
            }
            if (this.isMoving)
            {
                this.inAirVelocity = this.inAirVelocity + ((targetDirection.normalized * Time.deltaTime) * this.inAirControlAcceleration);
            }
        }
    }

    public virtual void ApplyWallJump()
    {
        if (!this.jumping)
        {
            return;
        }
        if (this.collisionFlags == CollisionFlags.CollidedSides)
        {
            this.touchWallJumpTime = Time.time;
        }
        bool mayJump = (this.lastJumpButtonTime > (this.touchWallJumpTime - this.wallJumpTimeout)) && (this.lastJumpButtonTime < (this.touchWallJumpTime + this.wallJumpTimeout));
        if (!mayJump)
        {
            return;
        }
        if ((this.lastJumpTime + this.jumpRepeatTime) > Time.time)
        {
            return;
        }
        if (Mathf.Abs(this.wallJumpContactNormal.y) < 0.2f)
        {
            this.wallJumpContactNormal.y = 0;
            this.moveDirection = this.wallJumpContactNormal.normalized;
            this.moveSpeed = Mathf.Clamp(this.moveSpeed * 1.5f, this.trotSpeed, this.runSpeed);
        }
        else
        {
            this.moveSpeed = 0;
        }
        this.verticalSpeed = this.CalculateJumpVerticalSpeed(this.jumpHeight);
        this.DidJump();
        this.SendMessage("DidWallJump", null, SendMessageOptions.DontRequireReceiver);
    }

    public virtual void ApplyJumping()
    {
        if ((this.lastJumpTime + this.jumpRepeatTime) > Time.time)
        {
            return;
        }
        if (this.IsGrounded())
        {
            if (this.canJump && (Time.time < (this.lastJumpButtonTime + this.jumpTimeout)))
            {
                this.verticalSpeed = this.CalculateJumpVerticalSpeed(this.jumpHeight);
                this.SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public virtual void ApplyGravity()
    {
        if (this.isControllable)
        {
            bool jumpButton = Input.GetButton("Jump");
            bool controlledDescent = ((this.canControlDescent && (this.verticalSpeed <= 0f)) && jumpButton) && this.jumping;
            if ((this.jumping && !this.jumpingReachedApex) && (this.verticalSpeed <= 0f))
            {
                this.jumpingReachedApex = true;
                this.SendMessage("DidJumpReachApex", SendMessageOptions.DontRequireReceiver);
            }
            bool extraPowerJump = ((this.IsJumping() && (this.verticalSpeed > 0f)) && jumpButton) && (this.transform.position.y < (this.lastJumpStartHeight + this.extraJumpHeight));
            if (controlledDescent)
            {
                this.verticalSpeed = this.verticalSpeed - (this.controlledDescentGravity * Time.deltaTime);
            }
            else
            {
                if (extraPowerJump)
                {
                    return;
                }
                else
                {
                    if (this.IsGrounded())
                    {
                        this.verticalSpeed = 0f;
                    }
                    else
                    {
                        this.verticalSpeed = this.verticalSpeed - (this.gravity * Time.deltaTime);
                    }
                }
            }
        }
    }

    public virtual float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        return Mathf.Sqrt((2 * targetJumpHeight) * this.gravity);
    }

    public virtual void DidJump()
    {
        this.jumping = true;
        this.jumpingReachedApex = false;
        this.lastJumpTime = Time.time;
        this.lastJumpStartHeight = this.transform.position.y;
        this.touchWallJumpTime = -1;
        this.lastJumpButtonTime = -10;
    }

    public virtual void Update()
    {
        if (!this.isControllable)
        {
            Input.ResetInputAxes();
        }
        if (Input.GetButtonDown("Jump"))
        {
            this.lastJumpButtonTime = Time.time;
        }
        this.UpdateSmoothedMovementDirection();
        this.ApplyGravity();
        if (this.canWallJump)
        {
            this.ApplyWallJump();
        }
        this.ApplyJumping();
        Vector3 movement = ((this.moveDirection * this.moveSpeed) + new Vector3(0, this.verticalSpeed, 0)) + this.inAirVelocity;
        movement = movement * Time.deltaTime;
        CharacterController controller = (CharacterController) this.GetComponent(typeof(CharacterController));
        this.wallJumpContactNormal = Vector3.zero;
        this.collisionFlags = controller.Move(movement);
        if (this.IsGrounded())
        {
            if (this.slammed)
            {
                this.slammed = false;
                controller.height = 2;

                {
                    float _16 = this.transform.position.y + 0.75f;
                    Vector3 _17 = this.transform.position;
                    _17.y = _16;
                    this.transform.position = _17;
                }
            }
            this.transform.rotation = Quaternion.LookRotation(this.moveDirection);
        }
        else
        {
            if (!this.slammed)
            {
                Vector3 xzMove = movement;
                xzMove.y = 0;
                if (xzMove.sqrMagnitude > 0.001f)
                {
                    this.transform.rotation = Quaternion.LookRotation(xzMove);
                }
            }
        }
        if (this.IsGrounded())
        {
            this.lastGroundedTime = Time.time;
            this.inAirVelocity = Vector3.zero;
            if (this.jumping)
            {
                this.jumping = false;
                this.SendMessage("DidLand", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public virtual void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y > 0.01f)
        {
            return;
        }
        this.wallJumpContactNormal = hit.normal;
    }

    public virtual float GetSpeed()
    {
        return this.moveSpeed;
    }

    public virtual bool IsJumping()
    {
        return this.jumping && !this.slammed;
    }

    public virtual bool IsGrounded()
    {
        return (this.collisionFlags & CollisionFlags.CollidedBelow) != (CollisionFlags) 0;
    }

    public virtual void SuperJump(float height)
    {
        this.verticalSpeed = this.CalculateJumpVerticalSpeed(height);
        this.collisionFlags = CollisionFlags.None;
        this.SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
    }

    public virtual void SuperJump(float height, Vector3 jumpVelocity)
    {
        this.verticalSpeed = this.CalculateJumpVerticalSpeed(height);
        this.inAirVelocity = jumpVelocity;
        this.collisionFlags = CollisionFlags.None;
        this.SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
    }

    public virtual void Slam(Vector3 direction)
    {
        this.verticalSpeed = this.CalculateJumpVerticalSpeed(1);
        this.inAirVelocity = direction * 6;
        direction.y = 0.6f;
        Quaternion.LookRotation(-direction);
        CharacterController controller = (CharacterController) this.GetComponent(typeof(CharacterController));
        controller.height = 0.5f;
        this.slammed = true;
        this.collisionFlags = CollisionFlags.None;
        this.SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
    }

    public virtual Vector3 GetDirection()
    {
        return this.moveDirection;
    }

    public virtual bool IsMovingBackwards()
    {
        return this.movingBack;
    }

    public virtual float GetLockCameraTimer()
    {
        return this.lockCameraTimer;
    }

    public virtual bool IsMoving()
    {
        return (Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal"))) > 0.5f;
    }

    public virtual bool HasJumpReachedApex()
    {
        return this.jumpingReachedApex;
    }

    public virtual bool IsGroundedWithTimeout()
    {
        return (this.lastGroundedTime + this.groundedTimeout) > Time.time;
    }

    public virtual bool IsControlledDescent()
    {
        bool jumpButton = Input.GetButton("Jump");
        return ((this.canControlDescent && (this.verticalSpeed <= 0f)) && jumpButton) && this.jumping;
    }

    public virtual void Reset()
    {
        this.gameObject.tag = "Player";
    }

    public ThirdPersonController()
    {
        this.walkSpeed = 3f;
        this.trotSpeed = 4f;
        this.runSpeed = 6f;
        this.inAirControlAcceleration = 3f;
        this.jumpHeight = 0.5f;
        this.extraJumpHeight = 2.5f;
        this.gravity = 20f;
        this.controlledDescentGravity = 2f;
        this.speedSmoothing = 10f;
        this.rotateSpeed = 500f;
        this.trotAfterSeconds = 3f;
        this.canJump = true;
        this.canControlDescent = true;
        this.jumpRepeatTime = 0.05f;
        this.wallJumpTimeout = 0.15f;
        this.jumpTimeout = 0.15f;
        this.groundedTimeout = 0.25f;
        this.moveDirection = Vector3.zero;
        this.lastJumpButtonTime = -10f;
        this.lastJumpTime = -1f;
        this.touchWallJumpTime = -1f;
        this.inAirVelocity = Vector3.zero;
        this.isControllable = true;
    }

}