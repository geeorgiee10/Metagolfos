using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravitableObject : MonoBehaviour
{
    protected Rigidbody rb;

    [Header("Gravedad")]
    public bool useLocalGravity = false;
    public Vector3 localGravityDir = Vector3.down;
    public float gravityForce = 9.81f;
    public bool isHeld = false;

    protected PortalTraveller traveller;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        traveller = GetComponent<PortalTraveller>();
        if (traveller == null)
            traveller = gameObject.AddComponent<PortalTraveller>();

        traveller.graphicsObject = transform.GetChild(0).gameObject;
    }

    protected virtual void FixedUpdate()
    {
        if (isHeld) return;

        Vector3 gravityDir = useLocalGravity 
            ? localGravityDir 
            : GravityManager.worldGravityDir;

        rb.AddForce(gravityDir * gravityForce, ForceMode.Acceleration);
    }

    public Vector3 GetCurrentGravityDir() => useLocalGravity 
        ? localGravityDir 
        : GravityManager.worldGravityDir;

    public void ChangeGravity(Vector3 newDir)
    {
        useLocalGravity = true;
        localGravityDir = newDir.normalized;

        // rb.velocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;

        // Quita o reduce este impulso si es muy fuerte
        // rb.AddForce(localGravityDir * 5f, ForceMode.Impulse); 
    }

    public void ResetToWorldGravity()
    {
        useLocalGravity = false;

        Vector3 worldDir = GravityManager.worldGravityDir;

        rb.velocity = Vector3.ProjectOnPlane(rb.velocity, worldDir);
    }
}