using UnityEngine;

public class WallJump : MonoBehaviour
{
    public Transform orientation;
    public Rigidbody rb;
    public PlayerMovement playerMovement;
    public LayerMask wallMask;
    public float sideForce;
    public float upForce;

    public float wallCheckDistance;

    public bool wallRight;
    public bool wallLeft;
    public bool wallFront;


    RaycastHit rightWallHit;
    RaycastHit leftWallHit;
    RaycastHit frontWallHit;


    // Update is called once per frame
    void Update()
    {
        WallCheck();
    }

    void OnJump()
    {
        if ((wallRight || wallLeft || wallFront) && !playerMovement.isGrounded) {

            Vector3 wallNormal = Vector3.zero;

            if (wallFront)  wallNormal = frontWallHit.normal;
            else if(wallLeft) wallNormal = leftWallHit.normal;
            else if (wallRight) wallNormal = rightWallHit.normal;

            Vector3 forceToApply = transform.up * upForce + wallNormal * sideForce;
            
            rb.AddForce(forceToApply, ForceMode.Impulse);
        }
    }

    void OnHug()
    {

    }

    public void WallCheck()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, wallMask);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, wallMask);
        wallFront = Physics.Raycast(transform.position, orientation.forward, out frontWallHit, wallCheckDistance, wallMask);

    }
}

