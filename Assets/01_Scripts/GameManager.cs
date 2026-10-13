using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int initCount;

    private void Start()
    {
        // Inicialización del juego
    }

    public void Initialize()
    {
        initCount++;
        Debug.Log($"GameManager.Initialize() ejecutado: {initCount}");
        if (initCount > 1)
            Debug.LogError("ERROR: GameManager se inicializó más de una vez");
    }
}