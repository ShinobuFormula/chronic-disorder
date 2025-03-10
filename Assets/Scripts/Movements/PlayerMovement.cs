using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public float rbDrag = 2;
    public Transform orientation;

    //camera
    public MouseMovement cam;
    public float dashFov = 95f;

    //walk, run & jump params
    public float walkSpeed = 12f;
    public float runSpeed = 18f;
    public float crouchSpeed = 8f;
    public float jumpHeight = 8f;
    public float airMultiplier = 0.4f;

    //state management
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    private float speedChangeFactor;
    private bool keepMomentum;
    public float speed = 12f;
    private MovementState lastState;


    //ground check
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    //movement flags
    public bool running = false;
    public bool crouching = false;
    public bool dashing = false;
    public bool grabbing = false;
    public bool isGrounded;

    private float x;
    private float z;

    //dash params
    public float dashForce = 50;
    public float upwardDashForce = 0;
    public float dashSpeed = 24f;
    public float dashDuration = 0.35f;
    public float dashSpeedChangeFactor = 50f;

    //grab params
    public float grabForce = 25;
    public bool tookOff = false;

    public MovementState state;
    public enum MovementState
    {
        walking,
        grabbing,
        running,
        crouching,
        dashing,
        air
    }

    private void Start()
    {
        rb.freezeRotation = true;
    }
    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        StateHandler();

        if (state == MovementState.walking || state == MovementState.running || state == MovementState.crouching)
        {
            rb.drag = rbDrag;
        }
        else
        {
            rb.drag = 0;
        }
        if (state == MovementState.grabbing && isGrounded && tookOff)
        {
            grabbing = false;
            tookOff = false;
        }

        SpeedControl();
    }
    private void FixedUpdate()
    {
        if (!isGrounded && state != MovementState.grabbing)
        {
            //descend faster that you jumped
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - 0.2f, rb.velocity.z);
        }
        Move();
    }

    void OnMove(InputValue moveVector)
    {
        x = moveVector.Get<Vector2>().x;
        z = moveVector.Get<Vector2>().y;
    }

    void Move()
    {
        if (state == MovementState.grabbing) return;

        Vector3 move = orientation.right * x + orientation.forward * z;

        if (isGrounded) rb.AddForce(move.normalized * speed * 10f, ForceMode.Force);
        else rb.AddForce(move.normalized * speed * 10f * airMultiplier, ForceMode.Force);
    }

    void OnJump()
    {
        if (isGrounded)
        {
            running = false;
            //reset rb velocity before jump
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
        }
    }

    void OnSprint()
    {
        if (isGrounded)
        {
            if (!running)
            {
                running = true;
            }
            else if (running)
            {
                running = false;
            }
        }
    }

    void OnDash()
    {
        dashing = true;

        cam.DoFov(dashFov);

        Vector3 direction = GetDirection();

        Vector3 dash = direction * dashForce + orientation.up * upwardDashForce;
        //see if useful
        rb.useGravity = false;
        rb.AddForce(dash, ForceMode.Impulse);

        Invoke(nameof(ResetDash), dashDuration);
    }

    private void ResetDash()
    {
        dashing = false;
        //see if useful
        cam.DoFov(70f);
        rb.useGravity = true;
    }


    private Vector3 GetDirection()
    {
        Vector3 direction = new Vector3();
        if (z == 0 && x == 0)
        {
            direction = orientation.forward;
        }
        else
        {
            direction = orientation.forward * z + orientation.right * x;
        }

        return direction.normalized;
    }

    private void SpeedControl()
    {
        if(!grabbing && !dashing)
        {
            Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVelocity.magnitude > speed)
            {
                Vector3 limitedVel = flatVelocity.normalized * speed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void StateHandler()
    {
        if (dashing)
        {
            state = MovementState.dashing;
            desiredMoveSpeed = dashSpeed;
            speedChangeFactor = dashSpeedChangeFactor;
        }

        else if (grabbing)
        {
            state = MovementState.grabbing;
        }
        // Mode - Crouching
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Running
        else if (running)
        {
            state = MovementState.running;
            desiredMoveSpeed = runSpeed;
        }

        // Mode - Walking
        else if (isGrounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;

            if (desiredMoveSpeed < runSpeed)
                desiredMoveSpeed = walkSpeed;
            else
                desiredMoveSpeed = runSpeed;
        }


        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;
        if (lastState == MovementState.dashing) keepMomentum = true;

        if (desiredMoveSpeedHasChanged)
        {
            if (keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                StopAllCoroutines();
                speed = desiredMoveSpeed;
            }
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
        lastState = state;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - speed);
        float startValue = speed;

        float boostFactor = speedChangeFactor;

        while (time < difference)
        {
            speed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            time += Time.deltaTime * boostFactor;

            yield return null;
        }

        speed = desiredMoveSpeed;
        speedChangeFactor = 1f;
        keepMomentum = false;
    }
}