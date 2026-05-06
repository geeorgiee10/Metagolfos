using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanControlCamera
{
	Vector3 Position { get; }
	Vector3 Up { get; }
	void ControlCamera(ref float pitch, ref float yaw);
}
