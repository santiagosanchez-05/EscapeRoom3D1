using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Camera playerCamera;

    private void Start()
    {
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        GameUI ui = GameUI.Instance;
        if (ui != null && ui.IsBlockingInput)
            return;

        IInteractable target = FindTarget();

        if (ui != null)
            ui.SetPrompt(target != null ? $"[E] {target.Prompt}" : null);

        if (target != null && Input.GetKeyDown(KeyCode.E))
            target.Interact();
    }

    private IInteractable FindTarget()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player camera is not assigned.");
            enabled = false;
            return null;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            // El propio jugador no bloquea el rayo.
            if (hit.transform.IsChildOf(transform))
                continue;

            // Solo el primer objeto que golpea el rayo cuenta: no se interactúa a través de paredes.
            return hit.collider.GetComponentInParent<IInteractable>();
        }

        return null;
    }
}
