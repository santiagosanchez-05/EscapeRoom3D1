using UnityEngine;

/// <summary>Controla el progreso del puzzle que desbloquea el cofre.</summary>
public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private int totalPieces = 3;
    [SerializeField] private string correctCode = "1234";

    private int solvedPieces;
    public bool IsSolved { get; private set; }

    public void SolvePiece()
    {
        if (IsSolved) return;
        solvedPieces++;
        Debug.Log($"Pieza resuelta {solvedPieces}/{totalPieces}");
    }

    public bool CheckCode(string code)
    {
        if (solvedPieces < totalPieces)
        {
            Debug.Log("Faltan piezas por resolver");
            return false;
        }
        IsSolved = code == correctCode;
        Debug.Log(IsSolved ? "Código correcto: puzzle resuelto exitosamente y cofre listo para abrir." : "Código incorrecto");
        return IsSolved;
    }

    // Prueba en Play Mode: P = resolver pieza, C = enviar código
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) SolvePiece();
        if (Input.GetKeyDown(KeyCode.C)) CheckCode(correctCode);
    }
}