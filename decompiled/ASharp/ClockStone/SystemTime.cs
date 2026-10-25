using System;

namespace ClockStone;

public static class SystemTime
{
	private static double _timeAtLaunch = time;

	public static double time => (double)DateTime.Now.Ticks * 1E-07;

	public static double timeSinceLaunch => time - _timeAtLaunch;
}
