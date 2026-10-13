using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private int totalPieces = 3;
    private int solvedPieces;

    public void SolvePiece()
    {
        solvedPieces++;
        Debug.Log($"Pieza resuelta {solvedPieces}/{totalPieces}");
        if (solvedPieces >= totalPieces)
            Debug.Log("Puzzle completado: el cofre se puede abrir");
    }
}