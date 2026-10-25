using UnityEngine;

namespace FH.LevelComponents.EnvironmentFX;

public class ReflectionProbeSpawner : MonoBehaviour
{
	[SerializeField]
	private ReflectionProbe _probeTemplate;

	private ReflectionProbe _reflectionProbe;

	public ReflectionProbe Probe
	{
		get
		{
			if (_reflectionProbe == null)
			{
				return SpawnAndRenderProbe();
			}
			return _reflectionProbe;
		}
	}

	private void Awake()
	{
		if (_reflectionProbe == null)
		{
			SpawnAndRenderProbe();
		}
	}

	private ReflectionProbe SpawnAndRenderProbe()
	{
		if (_probeTemplate == null)
		{
			Log.Warning("Cannot spawn reflection probe - reflection probe template missing!");
			return null;
		}
		_reflectionProbe = Object.Instantiate(_probeTemplate, transform);
		_reflectionProbe.name = "Runtime-spawned reflection probe";
		_reflectionProbe.gameObject.SetActive(value: true);
		_reflectionProbe.RenderProbe();
		return _reflectionProbe;
	}
}
