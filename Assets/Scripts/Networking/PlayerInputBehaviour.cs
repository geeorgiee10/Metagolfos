using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

public class PlayerInputBehaviour : Fusion.Behaviour, INetworkRunnerCallbacks
{
	float accumulatedDelta = 0;

	private void Update()
	{
		accumulatedDelta += Input.GetAxis("Mouse Y");
	}

	public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (PlayerObject.Local == null || PlayerObject.Local.Controller == null) return;
        // ... (tus validaciones de UI y State)

        PlayerInput fwInput = new PlayerInput();
        if (fwInput.isDragging = Input.GetMouseButton(0))
        {
            fwInput.dragDelta = accumulatedDelta;
        }

		// --- CÁLCULO DE YAW RELATIVO A LA GRAVEDAD ---
		Transform cam = CameraController.Instance.transform;
		Vector3 playerUp = -PlayerObject.Local.Controller.LocalGravityDir;

		// Referencia base del plano (igual que en el Putter)
		Vector3 referenceForward = Vector3.ProjectOnPlane(Vector3.forward, playerUp).normalized;
		if (referenceForward.sqrMagnitude < 0.01f) 
			referenceForward = Vector3.ProjectOnPlane(Vector3.up, playerUp).normalized;

		// Dirección hacia donde mira la cámara proyectada en el plano
		Vector3 flattenedForward = Vector3.ProjectOnPlane(cam.forward, playerUp).normalized;

		// Calculamos el ángulo entre nuestra referencia frontal y la mirada de la cámara
		// El "SignedAngle" es vital para saber si giramos a izquierda o derecha
		fwInput.yaw = Vector3.SignedAngle(referenceForward, flattenedForward, playerUp);
        
        input.Set(fwInput);
        accumulatedDelta = 0;
    }

	#region INetworkRunnerCallbacks
	public void OnConnectedToServer(NetworkRunner runner) { }
	public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
	public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
	public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
	public void OnDisconnectedFromServer(NetworkRunner runner) { }
	public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
	public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
	public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
	public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
	public void OnSceneLoadDone(NetworkRunner runner) { }
	public void OnSceneLoadStart(NetworkRunner runner) { }
	public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
	public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
	public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

	public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

	public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

	public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
	public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

	public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    #endregion
}

public struct PlayerInput : INetworkInput
{
	public bool isDragging;
	public float dragDelta;
	public float yaw;
}