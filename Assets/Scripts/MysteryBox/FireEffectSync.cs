using Fusion;
using UnityEngine;

public class FireEffectSync : NetworkBehaviour
{
    [Header("Referencias")]
    // Aquí arrastras el objeto "VFX_Fire" que vimos en tu imagen
    [SerializeField] private GameObject fireParticles;

    [Networked]
    [OnChangedRender(nameof(OnFireStatusChanged))]
    public NetworkBool isFireActive { get; set; }

    // Este método se ejecuta en TODOS los clientes cuando isFireActive cambia en la red
    public void OnFireStatusChanged()
    {
        if (fireParticles != null)
        {
            // Al activar el GameObject, si tiene "Play on Awake", empezará a soltar llamas
            fireParticles.SetActive(isFireActive);
        }
    }

    // Método para que el dueño de la bola cambie el estado
    public void SetFireState(bool state)
    {
        if (Object.HasStateAuthority)
        {
            isFireActive = state;
        }
    }
}