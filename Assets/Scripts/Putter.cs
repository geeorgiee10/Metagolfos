using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Putter : NetworkBehaviour, ICanControlCamera
{
	public Transform interpolationTarget;
	public Transform guideArrow;
	public MeshRenderer guideArrowRen;
	public MeshRenderer ren;
	public Rigidbody rb;
	new public SphereCollider collider;
	public float maxPuttStrength = 10;
	public float puttGainFactor = 0.1f;
	public float speedLoss = 0.1f;

	[Space]
	public float shakeImpulseThreshold = 0.5f;
	public float shakeCollisionAmount = 0.75f;
	public float shakeCollisionLambda = 10f;
	
	public PlayerObject PlayerObj { get; private set; }

	[Networked]
	public TickTimer PuttTimer { get; set; }
	public bool CanPutt => PuttTimer.ExpiredOrNotRunning(Runner);
	public bool couldPutt;

	[Networked]
	float PuttStrength { get; set; }
	float PuttStrengthNormalized => PuttStrength / maxPuttStrength;


	[Networked]
	PlayerInput CurrInput { get; set; }
	PlayerInput prevInput = default;

	Vector3 prevVelocity = Vector3.zero;
	Angle yaw = default;

	bool isFirstUpdate = true;

	private void LateUpdate()
	{
		if (CameraController.HasControl(this))
		{
			guideArrow.rotation = Quaternion.AngleAxis((float)yaw, Vector3.up);
		}
	}

	//private void OnCollisionEnter(Collision collision)
	//{
	//	if (Runner.IsServer == false) Debug.Log("OnCollisionEnter client");

	//	if (CameraController.HasControl(this))
	//	{
	//		float dot = Vector3.Dot(rb.velocity.normalized, collision.impulse.normalized);
	//		if (dot > 0 && collision.impulse.magnitude > shakeImpulseThreshold)
	//		{
	//			CameraController.Instance.Shake.TriggerShake(collision.impulse.magnitude * dot * shakeCollisionAmount, shakeCollisionLambda);
	//		}
	//	}
	//}

	public override void Spawned()
	{
		PlayerObj = PlayerRegistry.GetPlayer(Object.InputAuthority);
		PlayerObj.Controller = this;

		ren.material.color = PlayerObj.Color;

		if (Object.HasInputAuthority)
		{
			CameraController.AssignControl(this);
		}
		else
		{
			Instantiate(ResourcesManager.Instance.worldNicknamePrefab, InterfaceManager.Instance.worldCanvas.transform).SetTarget(this);
		}
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		if (CameraController.HasControl(this))
		{
			CameraController.AssignControl(null);
		}

		if (!runner.IsShutdown)
		{
			if (PlayerObj.TimeTaken != PlayerObject.TIME_UNSET)
			{
				AudioManager.Play("ballInHoleSFX", AudioManager.MixerTarget.SFX, interpolationTarget.position);
			}
		}
	}

	public override void FixedUpdateNetwork()
	{
		if (GetInput(out PlayerInput input))
		{
			CurrInput = input;
		}

		if (Runner.IsForward)
		{
			// began dragging
			if (CurrInput.isDragging && prevInput.isDragging == false)
			{
				if (CameraController.HasControl(this)) HUD.ShowPuttCharge();
			}

			if (CurrInput.isDragging)
			{
				PuttStrength = Mathf.Clamp(PuttStrength - (CurrInput.dragDelta * puttGainFactor), 0, maxPuttStrength);
				if (CameraController.HasControl(this))
				{
					HUD.SetPuttCharge(PuttStrengthNormalized, CanPutt);

					guideArrow.localScale = new Vector3(1, 1, PuttStrengthNormalized);
					guideArrowRen.material.SetColor("_EmissionColor", HUD.Instance.PuttChargeColor.Evaluate(PuttStrengthNormalized) * Color.gray);
				}
			}

			// stopped dragging
			if (CurrInput.isDragging == false && prevInput.isDragging)
			{
				if (CanPutt && PuttStrength > 0)
				{
					Vector3 fwd = Quaternion.AngleAxis((float)CurrInput.yaw, Vector3.up) * Vector3.forward;

					if (IsGrounded())
					{
						rb.AddForce(fwd * PuttStrength, ForceMode.VelocityChange);
					}
					else
					{
						rb.velocity = fwd * PuttStrength;
					}

					PuttTimer = TickTimer.CreateFromSeconds(Runner, 3);
					PlayerObj.Strokes++;

					if (CameraController.HasControl(this))
					{
						HUD.SetStrokeCount(PlayerObj.Strokes);
					}
				}

				PuttStrength = 0;
				if (CameraController.HasControl(this))
				{
					HUD.HidePuttCharge();
					guideArrow.localScale = new Vector3(1, 1, 0);
				}
			}

			if (CameraController.HasControl(this) && !isFirstUpdate)
			{
				if (!CanPutt && couldPutt)
				{
					HUD.ShowPuttCooldown();
				}

				if (CanPutt && !couldPutt)
				{
					HUD.HidePuttCooldown();
				}

				if (PuttTimer.RemainingTime(Runner).HasValue)
				{
					HUD.SetPuttCooldown(PuttTimer.RemainingTime(Runner).Value / 3f);
				}

				//Vector3 impulse = rb.velocity - prevVelocity;

				//float dot = Vector3.Dot(rb.velocity.normalized, prevVelocity.normalized);
				//Vector3 delta = (rb.velocity - prevVelocity);
				//if (dot > 0 && delta.magnitude > shakeImpulseThreshold)
				//{
				//	CameraController.Instance.Shake.TriggerShake(delta.magnitude * dot * shakeCollisionAmount, shakeCollisionLambda);
				//}
			}


			couldPutt = CanPutt;
			prevInput = CurrInput;

			prevVelocity = rb.velocity;
			yaw = CurrInput.yaw;

			isFirstUpdate = false;
		}

		if (IsGrounded() && rb.velocity.sqrMagnitude > 0.00001f)
		{
			rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.zero, Time.fixedDeltaTime * speedLoss);
			if (rb.velocity.sqrMagnitude <= 0.00001f)
			{

				if (PlayerObj.Strokes >= GameManager.MaxStrokes)
				{
					Debug.Log("Out of strokes");
					GameManager.PlayerDNF(PlayerObj);
				}
			}
		}
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	public void Rpc_Respawn(bool effect)
	{
		if (effect) Instantiate(ResourcesManager.Instance.splashEffect, transform.position, ResourcesManager.Instance.splashEffect.transform.rotation);
		if (Object.HasInputAuthority) CameraController.Recenter();

		rb.velocity = rb.angularVelocity = Vector3.zero;
		rb.MovePosition(Level.Current.GetSpawnPosition(PlayerObj.Index));
	}

	bool IsGrounded()
	{
		return Physics.OverlapSphere(transform.position, collider.radius * 1.05f,
			LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore).Length > 0;
	}
	
	public Vector3 Position => interpolationTarget.position;
	public void ControlCamera(ref float pitch, ref float yaw)
	{
		if (!Object.HasInputAuthority || prevInput.isDragging == false)
		{
			pitch -= Input.GetAxis("Mouse Y");
		}
		yaw += Input.GetAxis("Mouse X");
	}
}
