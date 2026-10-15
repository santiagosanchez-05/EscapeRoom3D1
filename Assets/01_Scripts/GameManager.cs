using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<GameManager>();
            return instance;
        }
    }

    public bool HasKey { get; private set; }
    public bool EscapeCompleted { get; private set; }

    private int initCount;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        Debug.Log("Iniciando juego");
        Initialize();
    }

    public void Initialize()
    {
        initCount++;
        Debug.Log($"GameManager.Initialize() ejecutado: {initCount}");
        if (initCount > 1)
        {
            Debug.LogError("ERROR: GameManager se inicializó más de una vez");
            return;
        }

        HasKey = false;
        EscapeCompleted = false;
        GameUI.Instance?.ShowMessage("Estás encerrado. Busca una pista para salir de la habitación.", 5f);
    }

    public void ObtainKey()
    {
        HasKey = true;
    }

    public void CompleteEscape()
    {
        if (EscapeCompleted) return;
        EscapeCompleted = true;
        Debug.Log("¡Escape completado!");
        GameUI.Instance?.ShowVictory();
    }
}
