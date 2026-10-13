using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    [SerializeField] private string correctCode = "1234";
    public bool IsSolved { get; private set; }

    public bool CheckCode(string code)
    {
        IsSolved = code == correctCode;
        Debug.Log(IsSolved ? "Código correcto" : "Código incorrecto");
        return IsSolved;
    }
}