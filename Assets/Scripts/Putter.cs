using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Random = UnityEngine.Random;

public class Putter : NetworkBehaviour, ICanControlCamera
{
	[Header("References Spawn")]
    private Vector3 initialPosition;
    private Vector3 lastPuttPosition;
    private bool hasLastPuttPosition = false;

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
	
    protected PortalTraveller traveller;

    private bool superstar = false;
	// [Header("Gravedad Sincronizada")]
    [Networked] public Vector3 LocalGravityDir { get; set; } = Vector3.down;
    public float gravityForce = 8.8f;

	private void LateUpdate()
	{
		if (CameraController.HasControl(this))
        {
            // La flecha debe apuntar hacia adelante según el YAW, 
            // pero su "UP" debe ser opuesto a la gravedad
            Vector3 up = -LocalGravityDir;
            Quaternion rotBase = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.forward, up), up);
            guideArrow.rotation = rotBase * Quaternion.AngleAxis((float)yaw, Vector3.up);
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

	private void OnCollisionEnter(Collision other)
	{
		if (superstar)
		{
			if (other.gameObject.tag == "Player")
			{
				if(other.gameObject.GetComponent<Putter>()!= null && initialPosition!=null)
				other.gameObject.GetComponent<Putter>().TeleportBall(initialPosition);
			}
		}
	}

	public override void Spawned()
	{
        // Spawn position
        initialPosition = transform.position;
        lastPuttPosition = initialPosition;

        PlayerObj = PlayerRegistry.GetPlayer(Object.InputAuthority);
		PlayerObj.Controller = this;

		ren.material.color = PlayerObj.Color;

		if (Object.HasInputAuthority)
			CameraController.AssignControl(this);
		else
			Instantiate(ResourcesManager.Instance.worldNicknamePrefab, InterfaceManager.Instance.worldCanvas.transform).SetTarget(this);
			
		if (Object.HasStateAuthority)
            LocalGravityDir = Vector3.down;


		rb.useGravity = false;
		rb.sleepThreshold = 0.01f;
        
        traveller = GetComponent<PortalTraveller>();
        if (traveller == null) traveller = gameObject.AddComponent<PortalTraveller>();
        traveller.graphicsObject = ren.gameObject;
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
					if (PlayerObj.Strokes >= GameManager.MaxStrokes)
					{
						GameManager.PlayerDNF(PlayerObj);
						return;
					}

                    lastPuttPosition = rb.position;
                    hasLastPuttPosition = true;

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

		if (IsGrounded())
		{
			Vector3 gravityDir = LocalGravityDir.normalized;

			// separar velocidad en vertical + horizontal
			Vector3 verticalVel = Vector3.Project(rb.velocity, gravityDir);
			Vector3 horizontalVel = rb.velocity - verticalVel;

			// frenar SOLO en plano del suelo
			horizontalVel = Vector3.MoveTowards(horizontalVel, Vector3.zero, Time.fixedDeltaTime * speedLoss);

			rb.velocity = horizontalVel + verticalVel;

			// dormir si está prácticamente parado
			if (horizontalVel.sqrMagnitude < 0.0001f && verticalVel.sqrMagnitude < 0.0001f)
			{
				rb.velocity = Vector3.zero;
			}
		}
        /*if (Object.HasInputAuthority)
        {
            // Tecla F: Volver al inicio del nivel
            if (Input.GetKeyDown(KeyCode.F))
            {
                TeleportBall(initialPosition);
            }
            // Tecla R: Volver al tiro anterior (si existe)
            else if (Input.GetKeyDown(KeyCode.R) && hasLastPuttPosition)
            {
                TeleportBall(lastPuttPosition);
            }
        }*/

		// --- GRAVEDAD POR JUGADOR (ESTABLE) ---
		if (Object.HasStateAuthority)
		{
			Vector3 gravityDir = LocalGravityDir.normalized;

			if (!IsGrounded())
			{
				rb.AddForce(gravityDir * gravityForce, ForceMode.Acceleration);
			}
			else
			{
				// mantener pegado al suelo sin rebotes
				rb.AddForce(gravityDir * gravityForce * 0.2f, ForceMode.Acceleration);
			}
		}
    }

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_SetGravity(Vector3 newGravity)
	{
		LocalGravityDir = newGravity.normalized;

		rb.angularVelocity = Vector3.zero;

		Rpc_OnGravityChanged(LocalGravityDir);
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	void Rpc_OnGravityChanged(Vector3 newGravity)
	{
		if (!Object.HasInputAuthority) return;

		Vector3 up = -newGravity;

		Transform cam = CameraController.Instance.transform;

		Vector3 forwardProjected = Vector3.ProjectOnPlane(cam.forward, up);

		if (forwardProjected.sqrMagnitude < 0.001f)
			forwardProjected = Vector3.ProjectOnPlane(cam.up, up);

		Quaternion targetRot = Quaternion.LookRotation(forwardProjected, up);

		cam.rotation = targetRot;
	}

    void Update()
    {
		if (!Object.HasInputAuthority) return;

		if (Input.GetKeyDown(KeyCode.F))
		{
			Rpc_BackBall(true);
		}
		else if (Input.GetKeyDown(KeyCode.R) && hasLastPuttPosition)
		{
			Rpc_BackBall(false);
		}

		// if (Input.GetKeyDown(KeyCode.Alpha1))
		// 	Rpc_SetGravity(Vector3.down);
			
		// if (Input.GetKeyDown(KeyCode.Alpha2))
		// 	Rpc_SetGravity(Vector3.up);
			
		// if (Input.GetKeyDown(KeyCode.Alpha3))
		// 	Rpc_SetGravity(Vector3.left);
			
		// if (Input.GetKeyDown(KeyCode.Alpha4))
		// 	Rpc_SetGravity(Vector3.right);
			
		// if (Input.GetKeyDown(KeyCode.Alpha5))
		// 	Rpc_SetGravity(Vector3.forward);
			
		// if (Input.GetKeyDown(KeyCode.Alpha6))
		// 	Rpc_SetGravity(Vector3.back);
			
    }

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_BackBall(bool toInitial)
	{
		if (toInitial)
		{
			TeleportBall(initialPosition);
		}
		else if (hasLastPuttPosition)
		{
			TeleportBall(lastPuttPosition);
		}
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]

	
	public void Rpc_Respawn(bool effect)
	{
		if (effect) Instantiate(ResourcesManager.Instance.splashEffect, transform.position, ResourcesManager.Instance.splashEffect.transform.rotation);
		if (Object.HasInputAuthority) CameraController.Recenter();

		rb.velocity = rb.angularVelocity = Vector3.zero;
		TeleportBall(lastPuttPosition);
		//rb.MovePosition(Level.Current.GetSpawnPosition(PlayerObj.Index));
	}

    private void TeleportBall(Vector3 targetPosition)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.MovePosition(targetPosition);
    }
    public void startSuperstar()
    {
	    StartCoroutine(SuperStar());
    }
    private IEnumerator SuperStar()
    {
	    MeshRenderer mr = gameObject.GetComponentInChildren<MeshRenderer>();
	    if (mr != null)
	    {
		    Color originalColor = mr.material.color;
		    int buffTime = 0; 
        
		    while (buffTime <= 8)
		    {
			   
			    mr.material.color = new Color(Random.value, Random.value, Random.value);
			    yield return new WaitForSeconds(1f);
			    buffTime += 1;
		    }

		    mr.material.color = originalColor;
	    }
	    yield return null;
    }

    public void startIntangible()
    {
	    StartCoroutine(Intangible());
    }
	
    private IEnumerator Intangible()
    {
	    
	    int originalLayer = gameObject.layer;
	    gameObject.layer = LayerMask.NameToLayer("PlayerIntangible");
    
	    yield return new WaitForSeconds(8f);
    
	    gameObject.layer = originalLayer;
    }
    bool IsGrounded()
	{
		Vector3 dir = LocalGravityDir.normalized;

		return Physics.Raycast(
			transform.position,
			dir,
			collider.radius * 1.1f
		);
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

	public Vector3 Up => -LocalGravityDir;
}
