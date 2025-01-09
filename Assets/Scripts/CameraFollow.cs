using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform camPos;
    void Update()
    {
        transform.position = camPos.position;
    }
}
