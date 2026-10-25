using System.Collections.Generic;
using FH.LevelComponents.ProcGen;
using FH.LevelComponents.TopDownProjection;
using UnityEngine;

namespace FH.PostProcessing;

public sealed class KronosFogOfWar
{
	private const int TARGET_RES = 256;

	private const int CEL_REVEALED = 0;

	private const int CEL_UNREVEALED = 1;

	private static int _fowRevealedWorldMask = Shader.PropertyToID("_Fow_Revealed_WorldMask");

	private static int _fowUnrevealedWorldMask = Shader.PropertyToID("_Fow_Unrevealed_WorldMask");

	private static int _fowViewAreaSize = Shader.PropertyToID("_Fow_ViewAreaSize");

	private static int _fowViewAreaPivot = Shader.PropertyToID("_Fow_ViewAreaPivot");

	private static int _fowFadeValue = Shader.PropertyToID("_Fow_FadeValue");

	private List<Cel> _cels;

	public void Init(MultiplaneSystem multiplaneSystem, IEnumerable<ProceduralMapTile> maps, Bounds levelBounds, bool enabled)
	{
		Shader.SetGlobalTexture(_fowRevealedWorldMask, null);
		Shader.SetGlobalTexture(_fowUnrevealedWorldMask, null);
		KronosFogOfWarSettingsDef kronosFogOfWarSettingsDef = Resources.Load<KronosFogOfWarSettingsDef>("Settings/KronosFogOfWarSettingsDef");
		Material overrideMaterial = kronosFogOfWarSettingsDef.OverrideMaterial;
		List<Material> fowMapPostProcessing = kronosFogOfWarSettingsDef.FowMapPostProcessing;
		if (_cels == null)
		{
			_cels = new List<Cel>
			{
				multiplaneSystem.NewCel(new CelParams
				{
					LevelBounds = levelBounds,
					Resolution = Vector2Int.one * 256,
					TextureFormat = RenderTextureFormat.ARGBHalf,
					DepthBufferPrecision = 16,
					ReplacementMaterial = overrideMaterial,
					PostProcessing = fowMapPostProcessing,
					Content = new KronosScenarioCelContent(maps, ProceduralMapTile.Visibility.Revealed),
					MinClipHeight = -1f,
					CullingMask = 0
				}),
				multiplaneSystem.NewCel(new CelParams
				{
					LevelBounds = levelBounds,
					Resolution = Vector2Int.one * 256,
					TextureFormat = RenderTextureFormat.ARGBHalf,
					DepthBufferPrecision = 16,
					ReplacementMaterial = overrideMaterial,
					PostProcessing = fowMapPostProcessing,
					Content = new KronosScenarioCelContent(maps, ProceduralMapTile.Visibility.Unrevealed),
					MinClipHeight = -1f,
					CullingMask = 0
				})
			};
		}
		Shader.SetGlobalTexture(_fowRevealedWorldMask, _cels[0].RenderTextureTarget);
		Shader.SetGlobalTexture(_fowUnrevealedWorldMask, _cels[1].RenderTextureTarget);
		Shader.SetGlobalVector(_fowViewAreaSize, Vector2.one * _cels[0].CelParams.OrthographicAreaSize);
		Shader.SetGlobalVector(_fowViewAreaPivot, new Vector2(levelBounds.center.x, levelBounds.center.z));
		SetFowMaskActive(enabled);
	}

	public static void SetFowMaskActive(bool enabled)
	{
		Shader.SetGlobalFloat(_fowFadeValue, enabled ? 1 : 0);
	}

	public void DiscardCel()
	{
		if (_cels != null)
		{
			foreach (Cel cel in _cels)
			{
				cel?.Dispose();
			}
		}
		_cels = null;
	}

	public void OnMapUpdate()
	{
		if (_cels == null)
		{
			return;
		}
		foreach (Cel cel in _cels)
		{
			if (cel != null && !cel.MarkForRemoval)
			{
				cel.RequestRefresh();
			}
		}
	}
}
