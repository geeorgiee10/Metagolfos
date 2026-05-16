using UnityEngine;

public class MagneticRepulsion : MonoBehaviour
{
    [Header("Configuración del Campo")]
    public float baseRepulsionForce = 15f; // Fuerza mínima de empuje
    public float incomingForceMultiplier = 0.5f; // Cuánto influye la velocidad de entrada
    public float fieldDamping = 2f; // Cuánto frena a la bola (resistencia)

    private void OnTriggerStay(Collider other)
    {
        // 1. Verificamos que lo que entró sea una bola con Rigidbody
        Rigidbody rbOther = other.GetComponent<Rigidbody>();

        if (rbOther != null && other.CompareTag("Player"))
        {
            // 2. Calcular Dirección (Desde el centro del campo hacia la otra bola)
            Vector3 direction = (other.transform.position - transform.position).normalized;

            // 3. Calcular Fuerza de Repulsión
            // Tomamos la velocidad actual de la bola para que, cuanto más rápido entre, más fuerte rebote
            float velocityMagnitude = rbOther.velocity.magnitude;
            float totalForce = baseRepulsionForce + (velocityMagnitude * incomingForceMultiplier);

            // Aplicamos la fuerza hacia afuera
            rbOther.AddForce(direction * totalForce, ForceMode.Acceleration);

            // 4. Efecto de Frenado (Damping)
            // Esto simula la "resistencia" del campo magnético para que no salgan disparadas al infinito
            rbOther.velocity = Vector3.Lerp(rbOther.velocity, Vector3.zero, fieldDamping * Time.deltaTime);
        }
    }
}