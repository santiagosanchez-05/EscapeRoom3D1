using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;
    private bool isOpen;

    [ContextMenu("Open")]
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        transform.Rotate(0f, openAngle, 0f);
        Debug.Log("Door opened");
    }
}