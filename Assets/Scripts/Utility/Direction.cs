using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum Direction
{
	Right	= 1 << 0,
	Up		= 1 << 1,
	Left	= 1 << 2,
	Down	= 1 << 3
}
