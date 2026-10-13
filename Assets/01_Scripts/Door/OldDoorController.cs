using UnityEngine;

public class OldDoorController : MonoBehaviour
{
    public bool isOpen = false;

    public void OpenDoor()
    {
        isOpen = true;
        Debug.Log("Old door opened");
    }
}