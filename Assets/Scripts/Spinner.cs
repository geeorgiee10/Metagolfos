using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Spinner : NetworkBehaviour
{
	public Rigidbody rb;
	public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
	public Vector3 axis = Vector3.forward;
	public float speedFactor = 1;
	[Range(0,1)] public float phaseOffset = 0;


	public override void FixedUpdateNetwork()
	{
		rb.MoveRotation(Quaternion.AngleAxis(
			curve.Evaluate(GameManager.Time * speedFactor + phaseOffset / curve.keys[curve.keys.Length - 1].time) * 360,
			transform.TransformDirection(axis)));
	}

	private void OnValidate()
	{
		rb.transform.rotation = Quaternion.AngleAxis(
			curve.Evaluate(phaseOffset / curve.keys[curve.keys.Length - 1].time) * 360,
			transform.TransformDirection(axis));
	}
}
