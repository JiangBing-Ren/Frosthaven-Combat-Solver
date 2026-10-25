using UnityEngine;

namespace Chronos;

public class TerrainTimeline : ComponentTimeline<Terrain>
{
	private float _wavingGrassSpeed;

	public float wavingGrassSpeed
	{
		get
		{
			return _wavingGrassSpeed;
		}
		set
		{
			_wavingGrassSpeed = value;
			AdjustProperties();
		}
	}

	public TerrainTimeline(Timeline timeline, Terrain component)
		: base(timeline, component)
	{
	}

	public override void CopyProperties(Terrain source)
	{
		_wavingGrassSpeed = source.terrainData.wavingGrassSpeed;
	}

	public override void AdjustProperties(float timeScale)
	{
	}
}
