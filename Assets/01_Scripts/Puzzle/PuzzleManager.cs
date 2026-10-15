using UnityEngine;

/// <summary>Teclado numérico del cofre: valida el código y desbloquea el cofre.</summary>
public class PuzzleManager : MonoBehaviour, IInteractable
{
    [SerializeField] private string correctCode = "1234";
    [SerializeField] private ChestController chest;

    public bool IsSolved { get; private set; }

    public string Prompt => IsSolved ? "Teclado (resuelto)" : "Usar teclado numérico";

    public void Interact()
    {
        if (IsSolved)
        {
            GameUI.Instance?.ShowMessage("El teclado ya está resuelto. El cofre está desbloqueado.");
            return;
        }

        GameUI.Instance?.OpenCodePanel(this);
    }

    /// <summary>Valida el código ingresado para desbloquear el cofre.</summary>
    /// <param name="code">Código introducido por el jugador.</param>
    /// <returns>true si el código es correcto.</returns>
    public bool CheckCode(string code)
    {
        if (IsSolved) return true;

        IsSolved = code == correctCode;

        if (IsSolved)
        {
            GameUI.Instance?.ShowMessage("Código correcto. Se escuchó un clic en el cofre.");
            if (chest != null)
                chest.Unlock();
            else
                Debug.LogWarning("PuzzleManager no tiene un cofre asignado.");
        }
        else
        {
            GameUI.Instance?.ShowMessage("Código incorrecto.");
        }

        return IsSolved;
    }
}
