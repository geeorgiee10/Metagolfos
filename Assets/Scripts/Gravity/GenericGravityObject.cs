using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GenericGravityObject : GravitableObject
{

    [Header("Restricciones de Movimiento")]
    public bool allowX = true;
    public bool allowY = true;
    public bool allowZ = true;
    public bool allowXrot = true;
    public bool allowYrot = true;
    public bool allowZrot = true;

    void Start()
    {
        RigidbodyConstraints constraints = RigidbodyConstraints.None;

        if (!allowX) constraints |= RigidbodyConstraints.FreezePositionX;
        if (!allowY) constraints |= RigidbodyConstraints.FreezePositionY;
        if (!allowZ) constraints |= RigidbodyConstraints.FreezePositionZ;
        
        if (!allowXrot) constraints |= RigidbodyConstraints.FreezeRotationX;
        if (!allowYrot) constraints |= RigidbodyConstraints.FreezeRotationY;
        if (!allowZrot) constraints |= RigidbodyConstraints.FreezeRotationZ;
    

        rb.constraints = constraints;
    }

    protected override void FixedUpdate()
    {
        ApplyFilteredGravity();
        LateFixedUpdateCleanup();
    }

    void LateFixedUpdateCleanup()
    {
        Vector3 mask = new Vector3(allowX ? 1f : 0f, allowY ? 1f : 0f, allowZ ? 1f : 0f);

        // Limpiar velocidad en ejes bloqueados
        rb.velocity = Vector3.Scale(rb.velocity, mask);

        // Opcional: eliminar rotaciones no deseadas
        rb.angularVelocity = Vector3.zero;
    }

    private void ApplyFilteredGravity()
    {
        Vector3 rawGravity = GetCurrentGravityDir() * gravityForce;
        
        // Creamos la máscara: 1 si está marcado, 0 si no
        Vector3 mask = new Vector3(allowX ? 1f : 0f, allowY ? 1f : 0f, allowZ ? 1f : 0f);
        
        // Multiplicamos componente a componente
        Vector3 filteredGravity = Vector3.Scale(rawGravity, mask);

        rb.AddForce(filteredGravity, ForceMode.Acceleration);
    }

    public void OnOutOfBounds() => 
        Destroy(gameObject);
}