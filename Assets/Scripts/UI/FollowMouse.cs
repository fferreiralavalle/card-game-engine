using UnityEngine;
using UnityEngine.InputSystem; // Requerido para el nuevo sistema

public class FollowMouse : MonoBehaviour
{
    [Header("Configuración")]
    public float moveSpeed = 200f;
    public bool smoothFollow = false;

    private void Update()
    {
        // 1. Obtener posición del mouse en pantalla (Vector2)
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        float distanceFromCamera = transform.position.z - Camera.main.transform.position.z;

        // 2. Convertir a Vector3 y asignar la distancia a la cámara (Z)
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, distanceFromCamera));

        // Ajustar Z si es 2D (usualmente 0) o mantener la profundidad deseada
        mouseWorldPos.z = transform.position.z;

        // 3. Aplicar movimiento
        if (smoothFollow)
        {
            // Movimiento suave
            transform.position = Vector3.MoveTowards(transform.position, mouseWorldPos, moveSpeed * Time.deltaTime);
        }
        else
        {
            // Teletransporte instantáneo
            transform.position = mouseWorldPos;
        }
    }
}