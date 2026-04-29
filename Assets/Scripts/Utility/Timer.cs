using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Timer
{
	public static Timer None = new Timer(0);

	public double StartTime { get; private set; }
	public double Duration { get; }
	private System.Func<double> timeFunction;

	public Timer(double duration)
	{
		StartTime = -1;
		Duration = duration;
		timeFunction = () => Time.timeAsDouble;
	}

	public Timer(double duration, System.Func<double> timeFunction)
	{
		StartTime = -1;
		Duration = duration;
		this.timeFunction = timeFunction;
	}

	public Timer Start()
	{
		StartTime = timeFunction.Invoke();
		return this;
	}

	public Timer Start(double when)
	{
		StartTime = when;
		return this;
	}

	public double TimeLeft()
	{
		return Duration - (timeFunction.Invoke() - StartTime);
	}

	public double TimeLeft(double when)
	{
		return Duration - (when - StartTime);
	}

	public bool IsUp()
	{
		if (StartTime == -1) return false;
		return timeFunction.Invoke() - StartTime >= Duration;
	}

	public bool IsUp(double when)
	{
		if (StartTime == -1) return false;
		return when - StartTime >= Duration;
	}

	public bool IsTicking()
	{
		if (StartTime == -1) return false;
		return timeFunction.Invoke() - StartTime < Duration;
	}
}
