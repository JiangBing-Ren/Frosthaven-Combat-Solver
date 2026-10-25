using UnityEngine;

namespace Chronos;

public class WindZoneTimeline : ComponentTimeline<WindZone>
{
	private float _windMain;

	private float _windTurbulence;

	private float _windPulseFrequency;

	private float _windPulseMagnitude;

	public float windMain
	{
		get
		{
			return _windMain;
		}
		set
		{
			_windMain = value;
			AdjustProperties();
		}
	}

	public float windTurbulence
	{
		get
		{
			return _windTurbulence;
		}
		set
		{
			_windTurbulence = value;
			AdjustProperties();
		}
	}

	public float windPulseMagnitude
	{
		get
		{
			return _windPulseMagnitude;
		}
		set
		{
			_windPulseMagnitude = value;
			AdjustProperties();
		}
	}

	public float windPulseFrequency
	{
		get
		{
			return _windPulseFrequency;
		}
		set
		{
			_windPulseFrequency = value;
			AdjustProperties();
		}
	}

	public WindZoneTimeline(Timeline timeline, WindZone component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(WindZone source)
	{
		_windMain = source.windMain;
		_windTurbulence = source.windTurbulence;
		_windPulseFrequency = source.windPulseFrequency;
		_windPulseMagnitude = source.windPulseMagnitude;
	}

	public override void AdjustProperties(float timeScale)
	{
		component.windTurbulence = windTurbulence * timeScale * Mathf.Abs(timeScale);
		component.windPulseFrequency = windPulseFrequency * timeScale;
		component.windPulseMagnitude = windPulseMagnitude * Mathf.Sign(timeScale);
	}
}
