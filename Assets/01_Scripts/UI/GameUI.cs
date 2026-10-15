using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Interfaz mínima del juego dibujada con IMGUI: mira, indicaciones de interacción,
/// mensajes, panel del código numérico y pantalla de victoria.
/// </summary>
public class GameUI : MonoBehaviour
{
    private static GameUI instance;

    public static GameUI Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<GameUI>();
            return instance;
        }
    }

    private const int CodeLength = 4;
    private const string CodeFieldName = "CodeField";

    private string prompt;
    private string message;
    private float messageUntil;

    private PuzzleManager activePuzzle;
    private string codeInput = "";
    private bool focusCodeField;
    private bool victory;

    public bool IsBlockingInput => activePuzzle != null || victory;

    private void Awake()
    {
        instance = this;
    }

    public void SetPrompt(string text)
    {
        prompt = text;
    }

    public void ShowMessage(string text, float duration = 3f)
    {
        message = text;
        messageUntil = Time.time + duration;
        Debug.Log(text);
    }

    public void OpenCodePanel(PuzzleManager puzzle)
    {
        activePuzzle = puzzle;
        codeInput = "";
        focusCodeField = true;
        prompt = null;
        PlayerController.LockCursor(false);
    }

    public void ShowVictory()
    {
        victory = true;
        prompt = null;
        PlayerController.LockCursor(false);
    }

    private void CloseCodePanel()
    {
        activePuzzle = null;
        PlayerController.LockCursor(true);
    }

    private void SubmitCode()
    {
        PuzzleManager puzzle = activePuzzle;
        if (puzzle.CheckCode(codeInput))
            CloseCodePanel();
        else
            codeInput = "";
        focusCodeField = true;
    }

    private void OnGUI()
    {
        float scale = Screen.height / 720f;
        GUIStyle label = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(20 * scale),
            wordWrap = true
        };
        GUIStyle box = new GUIStyle(GUI.skin.box) { fontSize = label.fontSize };

        if (victory)
        {
            DrawVictory(label, scale);
            return;
        }

        if (activePuzzle != null)
        {
            DrawCodePanel(label, scale);
        }
        else
        {
            // Mira en el centro de la pantalla.
            GUI.Label(new Rect(Screen.width / 2f - 10, Screen.height / 2f - 15, 20, 30), "+", label);

            if (!string.IsNullOrEmpty(prompt))
                GUI.Label(new Rect(0, Screen.height / 2f + 30 * scale, Screen.width, 40 * scale), prompt, label);
        }

        if (!string.IsNullOrEmpty(message) && Time.time < messageUntil)
        {
            float width = Mathf.Min(Screen.width - 40, 800 * scale);
            Rect rect = new Rect((Screen.width - width) / 2f, 30 * scale, width, 70 * scale);
            GUI.Box(rect, GUIContent.none, box);
            GUI.Label(rect, message, label);
        }
    }

    private void DrawCodePanel(GUIStyle label, float scale)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                SubmitCode();
                e.Use();
            }
            else if (e.keyCode == KeyCode.Escape)
            {
                CloseCodePanel();
                e.Use();
                return;
            }
        }

        float w = 420 * scale, h = 230 * scale;
        Rect panel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
        GUI.Box(panel, GUIContent.none);

        GUI.Label(new Rect(panel.x, panel.y + 15 * scale, w, 40 * scale), "Introduce el código de 4 dígitos", label);

        GUIStyle field = new GUIStyle(GUI.skin.textField)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.RoundToInt(32 * scale)
        };
        GUI.SetNextControlName(CodeFieldName);
        string typed = GUI.TextField(new Rect(panel.x + w * 0.25f, panel.y + 65 * scale, w * 0.5f, 55 * scale), codeInput, CodeLength, field);
        codeInput = FilterDigits(typed);

        if (focusCodeField)
        {
            GUI.FocusControl(CodeFieldName);
            focusCodeField = false;
        }

        GUIStyle button = new GUIStyle(GUI.skin.button) { fontSize = label.fontSize };
        float by = panel.y + 145 * scale, bw = 150 * scale, bh = 45 * scale;
        if (GUI.Button(new Rect(panel.x + w / 2f - bw - 10 * scale, by, bw, bh), "Probar", button))
            SubmitCode();
        if (GUI.Button(new Rect(panel.x + w / 2f + 10 * scale, by, bw, bh), "Cerrar", button))
            CloseCodePanel();
    }

    private void DrawVictory(GUIStyle label, float scale)
    {
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), GUIContent.none);

        GUIStyle title = new GUIStyle(label) { fontSize = Mathf.RoundToInt(48 * scale), fontStyle = FontStyle.Bold };
        GUI.Label(new Rect(0, Screen.height / 2f - 120 * scale, Screen.width, 80 * scale), "¡Escape completado!", title);
        GUI.Label(new Rect(0, Screen.height / 2f - 40 * scale, Screen.width, 40 * scale), "Abriste la puerta y saliste de la habitación.", label);

        GUIStyle button = new GUIStyle(GUI.skin.button) { fontSize = label.fontSize };
        float bw = 220 * scale, bh = 50 * scale;
        if (GUI.Button(new Rect((Screen.width - bw) / 2f, Screen.height / 2f + 30 * scale, bw, bh), "Jugar de nuevo", button))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static string FilterDigits(string text)
    {
        var sb = new System.Text.StringBuilder(CodeLength);
        foreach (char c in text)
        {
            if (char.IsDigit(c) && sb.Length < CodeLength)
                sb.Append(c);
        }
        return sb.ToString();
    }
}
