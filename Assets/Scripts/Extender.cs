using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Extender : NetworkBehaviour
{
	public AnimationCurve curve;
	public Rigidbody extenderRb;
	public float timeOffset = 0;

	public override void FixedUpdateNetwork()
	{
		extenderRb.MovePosition(transform.TransformPoint(0, curve.Evaluate(GameManager.Time + timeOffset), 0));
	}
}
