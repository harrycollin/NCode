using UnityEngine;
using System.Collections;
using System;
using NCode;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    protected NetworkBehaviour nb;

    public float walkSpeed = 6.0f;
    public float runSpeed = 11.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    
    // If true, diagonal speed (when strafing + moving forward or back) can't exceed normal move speed; otherwise it's about 1.4 times faster
    public bool limitDiagonalSpeed = true;

    // If checked, the run key toggles between running and walking. Otherwise player runs if the key is held down and walks otherwise
    // There must be a button set up in the Input Manager called "Run"
    public bool toggleRun = false;

    
   
    // Units that player can fall before a falling damage function is run. To disable, type "infinity" in the inspector
    public float fallingDamageThreshold = 10.0f;

    // If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down
    public bool slideWhenOverSlopeLimit = false;

    // If checked and the player is on an object tagged "Slide", he will slide down it regardless of the slope limit
    public bool slideOnTaggedObjects = false;

    public float slideSpeed = 12.0f;

    // If checked, then the player can change direction while in the air
    public bool airControl = false;

    // Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast
    public float antiBumpFactor = .75f;

    // Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping
    public int antiBunnyHopFactor = 1;

    public float ItemPickupDistance = 8f;

    public Camera camera;

    private Vector3 moveDirection = Vector3.zero;
    protected bool grounded = false;
    protected bool vehicleCollision;
    protected bool running = false;
    protected bool lastBool = false;
    private CharacterController controller;
    private Transform myTransform;
    private float speed;
    private RaycastHit hit;
    private float fallStartLevel;
    private bool falling;
    private float slideLimit;
    private float rayDistance;
    private Vector3 contactPoint;
    private bool playerControl = false;
    private int jumpTimer;
    public float speedH = 2.0f;
    public float speedV = 2.0f;
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    protected Vector2 mInput;
    float inputModifyFactor;
    protected bool jump = false;

    
    protected virtual void Awake()
    {
        nb = GetComponent<NetworkBehaviour>();
        controller = GetComponent<CharacterController>();
        myTransform = transform;
        speed = walkSpeed;
        rayDistance = controller.height * .5f + controller.radius;
        slideLimit = controller.slopeLimit - .1f;
        jumpTimer = antiBunnyHopFactor;

    }

    protected virtual void Update()
    {
          
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(0f, yaw, 0.0f);
        camera.transform.eulerAngles = new Vector3(Mathf.Clamp(pitch, -80, 80), camera.transform.eulerAngles.y, 0.0f);

        mInput.x = Input.GetAxis("Horizontal");
        mInput.y = Input.GetAxis("Vertical");

        jump = Input.GetButton("Jump");
        running = Input.GetButton("Run");     
    }

    protected virtual void FixedUpdate()
    {
        inputModifyFactor = (mInput.x != 0.0f && mInput.y != 0.0f && limitDiagonalSpeed) ? .7071f : 1.0f;

        if (grounded)
        {
            bool sliding = false;
            // See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
            // because that interferes with step climbing amongst other annoyances
            if (Physics.Raycast(myTransform.position, -Vector3.up, out hit, rayDistance))
            {
                if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                    sliding = true;
            }
            // However, just raycasting straight down from the center can fail when on steep slopes
            // So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
            else {
                Physics.Raycast(contactPoint + Vector3.up, -Vector3.up, out hit);
                if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                    sliding = true;
            }

            // If running isn't on a toggle, then use the appropriate speed depending on whether the run button is down
            if (!toggleRun)
            {
                speed = running ? runSpeed : walkSpeed;               
            }            

            // If sliding (and it's allowed), or if we're on an object tagged "Slide", get a vector pointing down the slope we're on
            if ((sliding && slideWhenOverSlopeLimit) || (slideOnTaggedObjects && hit.collider.tag == "Slide"))
            {
                Vector3 hitNormal = hit.normal;
                moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                Vector3.OrthoNormalize(ref hitNormal, ref moveDirection);
                moveDirection *= slideSpeed;
                playerControl = false;
            }
            // Otherwise recalculate moveDirection directly from axes, adding a bit of -y to avoid bumping down inclines
            else {
                moveDirection = new Vector3(mInput.x * inputModifyFactor, -antiBumpFactor, mInput.y * inputModifyFactor);
                moveDirection = myTransform.TransformDirection(moveDirection) * speed;
                playerControl = true;
            }

            // Jump! But only if the jump button has been released and player has been grounded for a given number of frames
            if (!jump)
                jumpTimer++;
            else if (jumpTimer >= antiBunnyHopFactor)
            {
                moveDirection.y = jumpSpeed;
                jumpTimer = 0;
                jump = false;
             

            }
        }
        else {
            // If we stepped over a cliff or something, set the height at which we started falling
            if (!falling)
            {
                falling = true;
                fallStartLevel = myTransform.position.y;
            }

            // If air control is allowed, check movement but don't touch the y component
            if (airControl && playerControl)
            {
                moveDirection.x = mInput.x * speed * inputModifyFactor;
                moveDirection.z = mInput.y * speed * inputModifyFactor;
                moveDirection = myTransform.TransformDirection(moveDirection);
            }
        }

        SetAnim();

        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller, and set grounded true or false depending on whether we're standing on something

        

        grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
    }

    /// <summary>
    /// A method that works out the required animations. Kept out of main functionality. 
    /// </summary>
    protected virtual void SetAnim()
    {
        Animator anim = GetComponentInChildren<Animator>();
        if (jump)
        {
           anim.SetTrigger("isJumping");
        }
        else
        {
            anim.SetTrigger("isIdle");
        }
        if(mInput.y > 0 && !running)
        {
            anim.SetBool("isWalking", true);
            anim.SetBool("isRunning", false);
        }
        else if(mInput.y > 0 && running)
        {
            anim.SetBool("isRunning", true);
            anim.SetBool("isWalking", false);
        }
       
        if (mInput.y == 0 )
        {
            anim.SetBool("isRunning", false);
            anim.SetBool("isWalking", false);
        }

        
    }

    
    public RaycastHit RayCast()
    {
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            print("I'm looking at " + hit.transform.root.name);
        }
        else {
            print("I'm looking at nothing!");
        }
        return hit; 
    }
}