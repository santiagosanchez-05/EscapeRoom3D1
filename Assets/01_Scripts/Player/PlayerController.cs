using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Tooltip("Velocidad de movimiento del jugador")]
    public float speed = 5f;

    private void Update()
    {
        Move();
        Debug.Log("Movement system active");
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Interaction key pressed");
        }
    }

    private void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(h, 0f, v);
        if (movement.magnitude > 1f) movement.Normalize();
        transform.Translate(movement * speed * Time.deltaTime);
    }
}