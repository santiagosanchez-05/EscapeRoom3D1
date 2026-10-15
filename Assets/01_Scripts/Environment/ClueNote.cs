using UnityEngine;

/// <summary>Pista que el jugador puede leer para descubrir el código del teclado.</summary>
public class ClueNote : MonoBehaviour, IInteractable
{
    [TextArea]
    [SerializeField] private string clueText = "La nota dice: \"Uno, dos, tres... ¿y después?\"";

    public string Prompt => "Leer nota";

    public void Interact()
    {
        GameUI.Instance?.ShowMessage(clueText, 6f);
    }
}
