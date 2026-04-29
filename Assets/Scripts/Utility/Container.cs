using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container<T>
{
	public T value;

	public Container() { }

	public Container(T value)
	{
		this.value = value;
	}
}
