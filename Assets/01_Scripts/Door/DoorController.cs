using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField] private float openAngle = 90f;
    private bool isOpen;

    [ContextMenu("Open")]
    public void Open()
    {
        // Bug introducido: la puerta se traba y arroja error
        Debug.LogError("Error crítico: Mecanismo de la puerta trabado, no abre.");
        return;
    }
}