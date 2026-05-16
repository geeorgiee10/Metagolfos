using Fusion;
using UnityEngine;

public class MagneticFieldSync : NetworkBehaviour
{
    [Header("Referencias de Componentes")]
    public MeshRenderer sphereVisual;
    public SphereCollider spherePhysics;

    [Networked]
    [OnChangedRender(nameof(OnStatusChanged))]
    public NetworkBool isBuffActive { get; set; }

    // Este método lo detectará Fusion 2 correctamente ahora
    public void OnStatusChanged()
    {
        if (sphereVisual != null) sphereVisual.enabled = isBuffActive;
        if (spherePhysics != null) spherePhysics.enabled = isBuffActive;
    }

    // El dueño de la bola llama a esto para activar/desactivar
    public void SetBuffState(bool state)
    {
        if (Object.HasStateAuthority)
        {
            isBuffActive = state;
        }
    }
}