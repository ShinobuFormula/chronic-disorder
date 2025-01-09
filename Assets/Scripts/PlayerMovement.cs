using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody rb;
    public float rbDrag = 2;
    public Transform orientation;

    public float speed = 12f;
    public float jumpHeight = 8f;
    public float airMultiplier = 0.4f;


    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;


    public bool running = false;
    bool isGrounded;

    private float x;
    private float z;

    public float dashForce = 50;
    public float upwardDashForce = 0;
    public float dashSpeed = 24f;

    private void Start()
    {
        rb.freezeRotation = true;
    }
    private void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if(isGrounded)
        {
            rb.drag = rbDrag;
        } else
        {
            rb.drag = 0;
        }

        SpeedControl();
    }
    private void FixedUpdate()
    {
        if(!isGrounded)
        {
            //descend faster that you jumped
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y - 0.1f, rb.velocity.z);
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
        Vector3 move = orientation.right * x + orientation.forward * z;
        
        if(isGrounded) rb.AddForce(move.normalized * speed * 10f, ForceMode.Force);
        else rb.AddForce(move.normalized * speed * 10f * airMultiplier, ForceMode.Force);
    }

    void OnJump() {
        if (isGrounded)
        {
            //the equation for jumping
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
        }
    }

    void OnSprint()
    {
        if(isGrounded)
        {
            if(!running)
            {
                speed *= 1.5f;
                running = true;
            } else if(running) {
                speed /= 1.5f;
                running = false;
            }
        }
    }

    void OnDash()
    {
        Vector3 dash = orientation.forward * dashForce + orientation.up * upwardDashForce;
        dash.y = 0;
        rb.AddForce(dash, ForceMode.Impulse);
        //speed = dashSpeed;
        //controller.Move(dash);
    }


    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if(flatVelocity.magnitude > speed)
        {
            Vector3 limitedVel = flatVelocity.normalized * speed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

}