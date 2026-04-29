using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Helpers.Math;

public class CameraController : MonoBehaviour
{
	[field: SerializeField] public ShakeBehaviour Shake { get; private set; }
	[SerializeField] ParticleSystem speedLines;
	ICanControlCamera con = null;
	public static ICanControlCamera Controller => Instance.con;

	[SerializeField] AnimationCurve speedLinesSpeedCurve = AnimationCurve.Constant(0, 1, 1);
	public float minDistance = 0.3f;
	public float defaultDistance = 1f;
	public float maxDistance = 3f;
	public float scrollRate = 0.1f;
	public float distanceLambda = 7f;
	public float lookHeightOffset = 0.2f;

	public float defaultPitch = 20;
	public float maxPitch = 60;

	[SerializeField, ReadOnly] float pitch = 0;
	[SerializeField, ReadOnly] float yaw = 0;

	float targetDistance;
	float distance;
	Vector3 cachedPosition;

	public static CameraController Instance { get; private set; }
	private void Awake()
	{
		Instance = this;
		targetDistance = distance = defaultDistance;
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnValidate()
	{
		pitch = defaultPitch;
		transform.localEulerAngles = new Vector3(pitch, yaw, 0);
	}

	private void OnDestroy()
	{
		con = null;
		Instance = null;
		Cursor.lockState = CursorLockMode.None;
	}

	public static void AssignControl(ICanControlCamera controller)
	{
		Instance.con = controller;
		if (controller == null)
		{
			Instance.speedLines.Stop();
			HUD.Instance.SpectatingObj.SetActive(false);
		}
		else if (controller is NetworkBehaviour nb)
		{
			HUD.Instance.SpectatingObj.SetActive(!nb.Object.HasInputAuthority);
		}
	}

	public static bool HasControl(ICanControlCamera controller)
	{
		return Controller?.Equals(controller) == true;
	}

	private void LateUpdate()
	{
		if (!(GameManager.Instance?.Runner?.IsRunning == true) || PlayerRegistry.CountPlayers == 0) return;

		if (con == null && PlayerRegistry.CountWhere(p => p.Controller) > 0 && (GameManager.State.Current == GameState.EGameState.Intro || GameManager.State.Current == GameState.EGameState.Game))
		{
			AssignControl(PlayerRegistry.First(p => p.Controller != null).Controller);
		}

		if (con == null) return;

		if (con is NetworkBehaviour nb)
		{
			if (nb.Object.HasInputAuthority == false)
			{
				if (Input.GetMouseButtonDown(0))
				{
					AssignControl(PlayerRegistry.NextWhere(PlayerRegistry.First(p => p.Controller == nb), p => p.Controller).Controller);
					if (con == null) return;
				}
			}
		}

		distance = MathUtil.Damp(distance, targetDistance, distanceLambda, Time.deltaTime);

		float scroll = Input.mouseScrollDelta.y;
		if (scroll != 0)
		{
			targetDistance -= scroll * scrollRate * targetDistance.Remap(minDistance, maxDistance, 0.25f, 4f);
			targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
		}

		con.ControlCamera(ref pitch, ref yaw);
		yaw = Mathf.Repeat(yaw + 180, 360) - 180;
		pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

		Quaternion orientation = Quaternion.Euler(pitch, yaw, 0);
		Vector3 fwd = orientation * Vector3.forward;
		Vector3 up = orientation * Vector3.up;
		transform.position = con.Position - fwd * distance;
		transform.LookAt(con.Position + Vector3.up * lookHeightOffset * distance.Remap(minDistance, maxDistance, 0.25f, 2));

		ParticleSystem.MainModule main = speedLines.main;
		main.startSpeed = speedLinesSpeedCurve.Evaluate(Vector3.Distance(con.Position, cachedPosition) / Time.deltaTime);
		
		if (main.startSpeed.constant > 0)
			speedLines.transform.rotation = Quaternion.LookRotation(con.Position - cachedPosition);

		cachedPosition = con.Position;
	}

	public static void Recenter()
	{
		Instance.pitch = Instance.defaultPitch;
		Instance.yaw = 0;
	}
}
