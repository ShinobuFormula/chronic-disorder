using UnityEngine;


public class Grappling : MonoBehaviour
{

    public Rigidbody rb;
    public PlayerMovement playerMovement;
    public Camera mainCamera;

    public float sphereRadius;
    public float maxDetectionDistance;
    public LayerMask detectionLayer;
    public LayerMask obstacleLayer;
    private GameObject savedGrapable;

    void Update()
    {
        ShootSphereCast();
    }


    private void ShootSphereCast()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        RaycastHit hit;

        Physics.SphereCast(ray, sphereRadius, out hit, maxDetectionDistance, detectionLayer);

        if (hit.collider != null)
        {
            RaycastHit obstacleHit;
            GameObject closestObject = hit.collider.gameObject;

            Vector3 directionToTarget =  closestObject.transform.position - Camera.main.transform.position;

            Physics.Raycast(Camera.main.transform.position, directionToTarget, out obstacleHit, directionToTarget.magnitude, obstacleLayer);

            if (obstacleHit.collider == null) {
                closestObject.GetComponent<MeshRenderer>().material.color = Color.red;
                if (savedGrapable != closestObject)
                {
                    if(savedGrapable != null) savedGrapable.GetComponent<Ledge>().UnSelect();
                    closestObject.GetComponent<Ledge>().Select();
                    savedGrapable = closestObject;
                }
            } else
            {
                clearSavedGrapable();
            }
        } else
        {
            clearSavedGrapable();
        }
    }

    void clearSavedGrapable()
    {
        if (savedGrapable != null) savedGrapable.GetComponent<Ledge>().UnSelect();
        savedGrapable = null;
    }

    void OnGrapple()
    {
        if(savedGrapable != null)
        {
            playerMovement.grabbing = true;

            Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

            float grapplePointRelativeYPos = savedGrapable.transform.position.y - lowestPoint.y;
            float highestPointOnArc = grapplePointRelativeYPos + 0.5f;

            if (grapplePointRelativeYPos < 0) highestPointOnArc = 2;
            rb.velocity = CalculateJumpVelocity(transform.position, savedGrapable.transform.position, highestPointOnArc);

            Invoke(nameof(ResetGrapple), 0.35f);
        }

    }

    void ResetGrapple()
    {
        playerMovement.tookOff = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        playerMovement.grabbing = false;
        playerMovement.tookOff = false;
    }

    private Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }
}
