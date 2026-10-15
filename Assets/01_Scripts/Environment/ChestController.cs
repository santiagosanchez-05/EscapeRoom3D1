using UnityEngine;

public class ChestController : MonoBehaviour, IInteractable
{
    [SerializeField] private bool unlocked;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform lid;
    [SerializeField] private float lidOpenAngle = 110f;
    [SerializeField] private GameObject keyItem;

    private bool opened;

    public string Prompt => opened ? "Cofre (abierto)" : "Abrir cofre";

    public void Unlock()
    {
        unlocked = true;
        Debug.Log("Cofre desbloqueado.");
    }

    public void Interact()
    {
        if (!unlocked)
        {
            GameUI.Instance?.ShowMessage("El cofre está cerrado. Tiene un teclado numérico cerca...");
            return;
        }

        if (opened)
        {
            GameUI.Instance?.ShowMessage("El cofre ya está abierto.");
            return;
        }

        opened = true;

        if (animator != null && animator.runtimeAnimatorController != null)
            animator.SetTrigger("Open");
        else if (lid != null)
            OpenLid();

        if (keyItem != null)
            keyItem.SetActive(true);

        GameUI.Instance?.ShowMessage("Cofre abierto. ¡Hay una llave dentro!");
    }

    private void OpenLid()
    {
        // La bisagra es el borde trasero de la tapa.
        Vector3 hinge = lid.position + transform.forward * (lid.lossyScale.z * 0.5f);
        lid.RotateAround(hinge, transform.right, lidOpenAngle);
    }
}
