/// <summary>Contrato común para todo objeto con el que el jugador puede interactuar (tecla E).</summary>
public interface IInteractable
{
    /// <summary>Texto corto que se muestra al apuntar al objeto (ej. "Abrir cofre").</summary>
    string Prompt { get; }

    void Interact();
}
