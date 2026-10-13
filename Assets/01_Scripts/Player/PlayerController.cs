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
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? speed * 2f : speed;

        Vector3 movement = new Vector3(h, 0f, v);
        transform.Translate(movement * currentSpeed * Time.deltaTime);
    }
}