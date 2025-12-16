using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EndingPoint : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Game End");
    }
}
