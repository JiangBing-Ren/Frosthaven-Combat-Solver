using System;
using Chronos;

public static class TimeManager
{
	[Flags]
	private enum TimeStates
	{
		None = 0,
		Regular = 2,
		SlowMo = 4,
		Frozen = 8,
		Paused = 0x10
	}

	private static TimeStates s_CurrentState;

	private static float s_DefaultTimeScale;

	public static float TimeScale => Timekeeper.instance.m_GlobalClock.timeScale;

	public static float DefaultTimeScale
	{
		get
		{
			return s_DefaultTimeScale;
		}
		set
		{
			s_DefaultTimeScale = value;
			if (IsRegular)
			{
				Timekeeper.instance.m_GlobalClock.localTimeScale = s_DefaultTimeScale;
				SaveData.Instance.Global.InvokeGameSpeedChanged();
			}
		}
	}

	public static float SlowMoTimeScale { get; private set; }

	public static bool IsRegular => s_CurrentState.HasFlag(TimeStates.Regular);

	public static bool IsSlowMo => s_CurrentState.HasFlag(TimeStates.SlowMo);

	public static bool IsFrozen => s_CurrentState.HasFlag(TimeStates.Frozen);

	public static bool IsPaused => s_CurrentState.HasFlag(TimeStates.Paused);

	static TimeManager()
	{
		s_CurrentState = TimeStates.Regular;
		s_DefaultTimeScale = Timekeeper.instance.m_GlobalClock.timeScale;
		SlowMoTimeScale = 0.25f;
	}

	public static void SlowTime(float? slowMoSpeed = null, bool refreshAudio = true)
	{
		SlowMoTimeScale = (slowMoSpeed.HasValue ? slowMoSpeed.Value : SlowMoTimeScale);
		if (s_CurrentState.HasFlag(TimeStates.SlowMo))
		{
			Log.Warning("[TimeManager]: TimeScale is already Slowed by other entity, please do check for this before slow down TimeScale");
		}
		else
		{
			s_CurrentState |= TimeStates.SlowMo;
		}
		if ((s_CurrentState & ~TimeStates.SlowMo) < TimeStates.SlowMo)
		{
			Timekeeper.instance.m_GlobalClock.localTimeScale = SlowMoTimeScale;
			SaveData.Instance.Global.InvokeGameSpeedChanged();
			if (refreshAudio)
			{
				AudioController.RefreshTimescale();
			}
		}
	}

	public static void UnslowTime()
	{
		s_CurrentState &= ~TimeStates.SlowMo;
		Log.Info("[TimeManager]: Unslow time");
		if (s_CurrentState <= TimeStates.SlowMo)
		{
			Log.Info("[TimeManager]: Fallback to  default time scale");
			Timekeeper.instance.m_GlobalClock.localTimeScale = DefaultTimeScale;
			SaveData.Instance.Global.InvokeGameSpeedChanged();
			AudioController.RefreshTimescale();
		}
	}

	public static void FreezeTime(bool refreshAudio = true)
	{
		if (s_CurrentState.HasFlag(TimeStates.Frozen))
		{
			Log.Warning("[TimeManager]: TimeScale is already Frozen by other entity, please do check for this before freeze TimeScale");
		}
		else
		{
			s_CurrentState |= TimeStates.Frozen;
			Log.Info("[TimeManager]: TimeScale Frozen");
		}
		if ((s_CurrentState & ~TimeStates.Frozen) < TimeStates.Frozen)
		{
			Timekeeper.instance.m_GlobalClock.localTimeScale = 0f;
			Log.Info("[TimeManager]: Set localTimeScale to 0");
			SaveData.Instance.Global.InvokeGameSpeedChanged();
			if (refreshAudio)
			{
				AudioController.RefreshTimescale();
			}
		}
	}

	public static void UnfreezeTime()
	{
		s_CurrentState &= ~TimeStates.Frozen;
		Log.Info($"[TimeManager]: TimeScale Unfreeze ({s_CurrentState})");
		if (s_CurrentState <= TimeStates.Frozen)
		{
			if (s_CurrentState.HasFlag(TimeStates.SlowMo))
			{
				Log.Info("[TimeManager]: Fallback to  SlowMo");
				Timekeeper.instance.m_GlobalClock.localTimeScale = SlowMoTimeScale;
				SaveData.Instance.Global.InvokeGameSpeedChanged();
				AudioController.RefreshTimescale();
			}
			else
			{
				Log.Info("[TimeManager]: Fallback to  default time scale");
				Timekeeper.instance.m_GlobalClock.localTimeScale = DefaultTimeScale;
				SaveData.Instance.Global.InvokeGameSpeedChanged();
				AudioController.RefreshTimescale();
			}
		}
	}

	public static void PauseTime(bool refreshAudio = true)
	{
		if (s_CurrentState.HasFlag(TimeStates.Paused))
		{
			Log.Warning("[TimeManager]: TimeScale is already Paused by other entity, please do check for this before pause TimeScale");
		}
		else
		{
			Log.Info("[TimeManager]: Pause time");
			s_CurrentState |= TimeStates.Paused;
		}
		Log.Info("[TimeManager]: Set localTimeScale to 0");
		Timekeeper.instance.m_GlobalClock.paused = true;
		Timekeeper.instance.m_GlobalClock.localTimeScale = 0f;
		SaveData.Instance.Global.InvokeGameSpeedChanged();
		if (refreshAudio)
		{
			AudioController.RefreshTimescale();
		}
	}

	public static void UnpauseTime()
	{
		s_CurrentState &= ~TimeStates.Paused;
		Timekeeper.instance.m_GlobalClock.paused = false;
		Log.Info($"[TimeManager]: Unpause time ({s_CurrentState})");
		if (!s_CurrentState.HasFlag(TimeStates.Frozen))
		{
			if (s_CurrentState.HasFlag(TimeStates.SlowMo))
			{
				Log.Info("[TimeManager]: Fallback to  SlowMo");
				Timekeeper.instance.m_GlobalClock.localTimeScale = SlowMoTimeScale;
				SaveData.Instance.Global.InvokeGameSpeedChanged();
				AudioController.RefreshTimescale();
			}
			else
			{
				Log.Info("[TimeManager]: Fallback to  default time scale");
				Timekeeper.instance.m_GlobalClock.localTimeScale = DefaultTimeScale;
				SaveData.Instance.Global.InvokeGameSpeedChanged();
				AudioController.RefreshTimescale();
			}
		}
	}
}
