using UnityEngine;

public class Ledge : MonoBehaviour
{
    public void Select()
    {
        GetComponent<MeshRenderer>().material.color = Color.red;
    }

    public void UnSelect()
    {
        GetComponent<MeshRenderer>().material.color = Color.white;
    }
}
