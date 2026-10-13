using UnityEngine;

public class OldDoorController : MonoBehaviour
{
    public bool isOpen = false;

    public void OpenDoor()
    {
        isOpen = true;
        transform.Rotate(0f, 90f, 0f);
        Debug.Log("Old door opened (fixed)");
    }
}