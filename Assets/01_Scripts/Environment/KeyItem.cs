using UnityEngine;

public class KeyItem : MonoBehaviour, IInteractable
{
    [SerializeField] private float spinSpeed = 90f;

    public string Prompt => "Tomar llave";

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    public void Interact()
    {
        GameManager.Instance.ObtainKey();
        gameObject.SetActive(false);
        GameUI.Instance?.ShowMessage("Llave obtenida. Ahora busca la salida.");
    }
}
