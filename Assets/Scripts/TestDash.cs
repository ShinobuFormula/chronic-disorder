using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDash : MonoBehaviour
{

    public float redDashForce = 8;
    public float blueDashForce = 15;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDash()
    {

        //Vector3 dash = transform.right * redDashForce * x + transform.forward * blueDashForce * z;
        //dash.y = 0;
        //Debug.Log(dash);
        rb.AddForce(Vector3.forward * 15, ForceMode.Impulse);
    }
}
