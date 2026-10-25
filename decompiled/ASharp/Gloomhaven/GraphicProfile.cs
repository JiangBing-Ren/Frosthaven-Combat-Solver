using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using UnityEngine;

namespace Gloomhaven;

[Serializable]
public class GraphicProfile : ISerializable
{
	protected bool OverridePixelLight;

	public int PixelLight;

	public TextureQualityRender TextureQuality;

	public AnisotropicFiltering AnisotropicTexture;

	public AntialiasingRender AntiAliasing;

	public bool SoftParticles;

	public bool RealtimeReflectionProbes;

	public ShadowQuality Shadows;

	public ShadowResolution ShadowResolution;

	public SkinWeights SkinWeights;

	public VSyncQuality VSync;

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("PixelLight", PixelLight);
		info.AddValue("TextureQuality", TextureQuality);
		info.AddValue("AnisotropicTexture", AnisotropicTexture);
		info.AddValue("PPAA", AntiAliasing);
		info.AddValue("RealtimeReflectionProbes", RealtimeReflectionProbes);
		info.AddValue("Shadows", Shadows);
		info.AddValue("ShadowResolution", ShadowResolution);
		info.AddValue("SkinWeights", SkinWeights);
		info.AddValue("VSync", VSync);
		info.AddValue("SoftParticles", SoftParticles);
	}

	public GraphicProfile()
	{
	}

	public GraphicProfile(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "PixelLight":
					PixelLight = info.GetInt32("PixelLight");
					break;
				case "TextureQuality":
					TextureQuality = (TextureQualityRender)info.GetValue("TextureQuality", typeof(TextureQualityRender));
					if (!Enum.IsDefined(typeof(TextureQualityRender), TextureQuality))
					{
						TextureQuality = TextureQualityRender.EIGHTHEN;
					}
					break;
				case "SoftParticles":
					SoftParticles = info.GetBoolean("SoftParticles");
					break;
				case "RealtimeReflectionProbes":
					RealtimeReflectionProbes = info.GetBoolean("RealtimeReflectionProbes");
					break;
				case "AnisotropicTexture":
					AnisotropicTexture = (AnisotropicFiltering)info.GetValue("AnisotropicTexture", typeof(AnisotropicFiltering));
					break;
				case "PPAA":
					AntiAliasing = (AntialiasingRender)info.GetValue("PPAA", typeof(AntialiasingRender));
					break;
				case "Shadows":
					Shadows = (ShadowQuality)info.GetValue("Shadows", typeof(ShadowQuality));
					break;
				case "ShadowResolution":
					ShadowResolution = (ShadowResolution)info.GetValue("ShadowResolution", typeof(ShadowResolution));
					break;
				case "SkinWeights":
					SkinWeights = (SkinWeights)info.GetValue("SkinWeights", typeof(SkinWeights));
					break;
				case "VSync":
					VSync = (VSyncQuality)info.GetValue("VSync", typeof(VSyncQuality));
					break;
				case "AA":
					AntiAliasing = AntialiasingRender.TAA;
					break;
				}
			}
			catch (Exception ex)
			{
				Log.GUIException(ex, "Exception while trying to deserialize GraphicProfile entry " + item.Name);
				throw ex;
			}
		}
	}

	public void Setup()
	{
		QualitySettings.anisotropicFiltering = AnisotropicTexture;
		QualitySettings.pixelLightCount = PixelLight;
		QualitySettings.shadowResolution = ShadowResolution;
		QualitySettings.shadows = Shadows;
		QualitySettings.realtimeReflectionProbes = RealtimeReflectionProbes;
		QualitySettings.softParticles = SoftParticles;
		QualitySettings.skinWeights = SkinWeights;
		QualitySettings.globalTextureMipmapLimit = (int)TextureQuality;
		QualitySettings.vSyncCount = (int)VSync;
		GraphicsController.SetAntiAliasing(AntiAliasing);
	}

	public void LoadCurrentValues()
	{
		AnisotropicTexture = QualitySettings.anisotropicFiltering;
		PixelLight = QualitySettings.pixelLightCount;
		ShadowResolution = QualitySettings.shadowResolution;
		Shadows = QualitySettings.shadows;
		RealtimeReflectionProbes = QualitySettings.realtimeReflectionProbes;
		SoftParticles = QualitySettings.softParticles;
		SkinWeights = QualitySettings.skinWeights;
		TextureQuality = (TextureQualityRender)QualitySettings.globalTextureMipmapLimit;
		VSync = (VSyncQuality)QualitySettings.vSyncCount;
		AntiAliasing = GraphicsController.GetAntiAliasing();
	}
}
