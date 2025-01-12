using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{

    public Rigidbody rb;
    public PlayerMovement playerMovement;
    public Camera mainCamera;
    
    //detect nearest grapable
    public float maxDetectionDistance;
    public LayerMask detectionLayer;
    private GameObject nearestGrapable;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ShootSphereCast();
    }


    private void ShootSphereCast()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);

        RaycastHit[] hits = Physics.SphereCastAll(ray, 10, maxDetectionDistance, detectionLayer);

        if (hits.Length > 0)
        {
            // Trouver l'objet le plus proche
            GameObject closestObject = null;
            float closestDistance = Mathf.Infinity;

            foreach (RaycastHit hit in hits)
            {
                float distance = Vector3.Distance(mainCamera.transform.position, hit.point);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = hit.collider.gameObject;
                }
            }

            if (closestObject != null)
            {
                if(nearestGrapable != closestObject)
                {
                    if(nearestGrapable != null) nearestGrapable.GetComponent<Ledge>().UnSelect();
                    Debug.Log("Objet le plus proche : " + closestObject.name);
                    closestObject.GetComponent<Ledge>().Select();
                    nearestGrapable = closestObject;
                }
            }
        } else
        {
            if (nearestGrapable != null) nearestGrapable.GetComponent<Ledge>().UnSelect();
            nearestGrapable = null;
        }
    }

    void OnGrapple()
    {
        if(nearestGrapable != null)
        {
            playerMovement.grabbing = true;

            Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

            float grapplePointRelativeYPos = nearestGrapable.transform.position.y - lowestPoint.y;
            float highestPointOnArc = grapplePointRelativeYPos + 2;

            if (grapplePointRelativeYPos < 0) highestPointOnArc = 2;
            rb.velocity = CalculateJumpVelocity(transform.position, nearestGrapable.transform.position, highestPointOnArc);

            Invoke(nameof(ResetGrapple), 0.35f);
        }

    }

    void ResetGrapple()
    {
        playerMovement.tookOff = true;
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
