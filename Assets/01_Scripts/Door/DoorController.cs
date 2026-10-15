using UnityEngine;

public class DoorController : MonoBehaviour, IInteractable
{
    [Tooltip("Negativo abre hacia afuera de la habitación")]
    [SerializeField] private float openAngle = -90f;
    private bool isOpen;

    public string Prompt => isOpen ? "Salir" : "Abrir puerta";

    public void Interact()
    {
        if (isOpen)
        {
            GameManager.Instance.CompleteEscape();
            return;
        }

        if (!GameManager.Instance.HasKey)
        {
            GameUI.Instance?.ShowMessage("La puerta está cerrada con llave.");
            return;
        }

        Open();
        GameManager.Instance.CompleteEscape();
    }

    [ContextMenu("Open")]
    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        // La bisagra es el borde izquierdo de la puerta.
        Vector3 hinge = transform.position - transform.right * (transform.lossyScale.x * 0.5f);
        transform.RotateAround(hinge, Vector3.up, openAngle);
        Debug.Log("Door opened");
    }
}
