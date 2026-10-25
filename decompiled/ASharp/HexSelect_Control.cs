using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;

public class HexSelect_Control : MonoBehaviour, IHexControl
{
	private bool m_Initialised;

	private IHexControl.HexType m_CachedType;

	private IHexControl.HexMode m_CachedMode;

	private bool m_CachedHover;

	private bool m_Hover;

	private Color? m_ColorOverrideCached;

	private bool m_CachedW_On;

	private bool m_CachedNw_On;

	private bool m_CachedNe_On;

	private bool m_CachedE_On;

	private bool m_CachedSw_On;

	private bool m_CachedSe_On;

	public bool nw_On;

	public bool ne_On;

	public bool e_On;

	public bool se_On;

	public bool sw_On;

	public bool w_On;

	public bool borderParticleBitsByDefault = true;

	private int m_HoverFlagsCached;

	private int m_HoverFlags;

	private bool enableRayCast = true;

	private float hexBaseRotation;

	public bool disableCrossHair;

	private bool TargetFrame;

	private bool[] isOn;

	private float sparkRateMin;

	private float sparkRateMax;

	private float borderFlameIntensity;

	private float borderLineIntensity;

	private float hexIntensity;

	private float targetFrameIntensity;

	public float cursorIntensity = 0.2f;

	private Color _hexColor;

	[Header("Hex Material Settings")]
	public float reachHexIntensity = 0.25f;

	public float reachBorderFlameIntensity = 0.2f;

	public float reachBorderLineIntensity = 0.7f;

	public float reachAllHexIntensity = 0.25f;

	public float reachAllBorderFlameIntensity = 0.2f;

	public float reachAllBorderLineIntensity = 0.7f;

	public float targetHexIntensity = 0.35f;

	public float targetBorderFlameIntensity = 0.35f;

	public float targetBorderLineIntensity = 0.5f;

	public float selectedHexIntensity = 0.35f;

	public float selectedBorderFlameIntensity = 0.35f;

	public float selectedBorderLineIntensity = 0.35f;

	public float selectedTargetFrameIntensity = 0.88f;

	[Header("Hover Material Adjustments")]
	public float hoverReachHexIntensityAdd = 0.2f;

	public float hoverReachBorderLineIntensityAdd = -0.2f;

	public float hoverTargetHexIntensityAdd = 0.2f;

	public float hoverTargetBorderLineIntensityAdd;

	public float hoverSelectedHexIntensityAdd = 0.2f;

	public float hoverSelectedBorderLineIntensityAdd;

	[Header("Negative Effect Hex Colours")]
	public Color negativeEffectReachColour = new Color(1f, 0.8f, 0.6f, 1f);

	public Color negativeEffectReachColourHover = new Color(0.8f, 0.8f, 0.6f, 1f);

	public Color negativeEffectPossibleTargetColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color negativeEffectPossibleTargetColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color negativeEffectSelectedColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color negativeEffectSelectedColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color negativeEffectSelectedAoEWithoutTargetColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color negativeEffectSelectedAoEWithoutTargetColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	[Header("Positive Effect Hex Colours")]
	public Color positiveEffectReachColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectReachColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectPossibleTargetColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectPossibleTargetColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectSelectedColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectSelectedColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectSelectedAoEWithoutTargetColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color positiveEffectSelectedAoEWithoutTargetColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	[Header("Move Hex Colours")]
	public Color moveReachColour = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color moveReachColourHover = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color movePossibleTargetColour = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color movePossibleTargetColourHover = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color moveSelectedColour = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color moveSelectedColourHover = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	[Header("Difficult Terrain Hex Colours")]
	public Color difficultTerrainReachColour = new Color(1f, 0.8f, 0.6f, 1f);

	public Color difficultTerrainReachColourHover = new Color(0.8f, 0.8f, 0.6f, 1f);

	public Color difficultTerrainPossibleTargetColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color difficultTerrainPossibleTargetColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color difficultTerrainSelectedColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color difficultTerrainSelectedColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color difficultTerrainSelectedAoEWithoutTargetColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color difficultTerrainSelectedAoEWithoutTargetColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	[Header("Icy Terrain  Hex Colours")]
	public Color icyTerrainReachColour = new Color(1f, 0.8f, 0.6f, 1f);

	public Color icyTerrainReachColourHover = new Color(0.8f, 0.8f, 0.6f, 1f);

	public Color icyTerrainPossibleTargetColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color icyTerrainPossibleTargetColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color icyTerrainSelectedColour = new Color(1f, 0.6f, 0.4f, 1f);

	public Color icyTerrainSelectedColourHover = new Color(1f, 0.6f, 0.4f, 1f);

	public Color icyTerrainSelectedAoEWithoutTargetColour = new Color(0.6f, 0.7f, 1f, 1f);

	public Color icyTerrainSelectedAoEWithoutTargetColourHover = new Color(0.6f, 0.7f, 1f, 1f);

	[Header("Other Hex Colours")]
	public Color invalidPlacementColour = new Color(0.8f, 0.8f, 0.8f, 0.8f);

	public Color dungeonExitColour = new Color(0.5f, 0.8f, 0.3f, 0.8f);

	public Color activeBonusConditionColour = new Color(0.5f, 0.8f, 0.3f, 0.8f);

	public Color objectiveColour = new Color(0.5f, 0.8f, 0.3f, 0.8f);

	public Color elevatedTerrainColour = new Color(1f, 0.5f, 0f, 1f);

	[Header("Child components to control")]
	public MeshRenderer HexProjector;

	public MeshRenderer unseenGroundPlane;

	public ParticleSystem[] particleBits;

	[Header("!Same order as - ScenarioManager.EAdjacentPosition")]
	public ParticleSystem[] particleHover;

	private Material m_Material;

	private ParticleSystem.Particle[] m_Particles = new ParticleSystem.Particle[1000];

	public GameObject GameObject => gameObject;

	public IHexControl.HexType Type { get; set; }

	public IHexControl.HexMode Mode { get; set; }

	public bool Hover
	{
		get
		{
			return m_Hover;
		}
		set
		{
			bool hover = m_Hover;
			m_Hover = value;
			if (m_Initialised && hover != m_Hover)
			{
				HexUpdate();
			}
		}
	}

	public Color? ColorOverride { get; set; }

	private Color HexColour
	{
		get
		{
			return ColorOverride ?? _hexColor;
		}
		set
		{
			_hexColor = value;
		}
	}

	private void Start()
	{
		RefreshHexUI();
		EnsureMaterialInstance();
	}

	private void OnDestroy()
	{
		if ((bool)m_Material)
		{
			Object.Destroy(m_Material);
		}
		m_Material = null;
	}

	private void EnsureMaterialInstance()
	{
		if (!m_Material)
		{
			m_Material = new Material(HexProjector.sharedMaterial);
		}
	}

	public void SetNeighbors(CClientTile currentTile, HashSet<CClientTile> activeTiles, List<CClientTile> hoveredTiles = null)
	{
		w_On = (nw_On = (ne_On = (e_On = (sw_On = (se_On = true)))));
		m_HoverFlags = -1;
		for (ScenarioManager.EAdjacentPosition eAdjacentPosition = ScenarioManager.EAdjacentPosition.ELeft; eAdjacentPosition <= ScenarioManager.EAdjacentPosition.EBottomRight; eAdjacentPosition++)
		{
			CTile adjacentTile = ScenarioManager.GetAdjacentTile(currentTile.m_Tile.m_ArrayIndex.X, currentTile.m_Tile.m_ArrayIndex.Y, eAdjacentPosition);
			if (adjacentTile == null)
			{
				continue;
			}
			CClientTile item = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[adjacentTile.m_ArrayIndex.X, adjacentTile.m_ArrayIndex.Y];
			if (hoveredTiles != null && hoveredTiles.Contains(item))
			{
				m_HoverFlags &= ~(1 << (int)(eAdjacentPosition - 1));
			}
			if (activeTiles == null || activeTiles.Contains(item))
			{
				switch (eAdjacentPosition)
				{
				case ScenarioManager.EAdjacentPosition.ELeft:
					w_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.ETopLeft:
					nw_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.ETopRight:
					ne_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.ERight:
					e_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.EBottomLeft:
					sw_On = false;
					break;
				case ScenarioManager.EAdjacentPosition.EBottomRight:
					se_On = false;
					break;
				}
			}
		}
	}

	public void SetAllNeighborsOn()
	{
		w_On = (nw_On = (ne_On = (e_On = (sw_On = (se_On = true)))));
	}

	[ContextMenu("Refresh")]
	public void DEBUG_Refresh()
	{
		RefreshHexUI(forceUpdate: false);
	}

	[ContextMenu("Hard Refresh")]
	public void DEBUG_HardRefresh()
	{
		RefreshHexUI();
	}

	public void RefreshHexUI(bool forceUpdate = true)
	{
		if (forceUpdate || EvaluateCacheChanged() || !m_Initialised)
		{
			m_Initialised = true;
			isOn = new bool[6];
			isOn.SetValue(nw_On, 0);
			isOn.SetValue(ne_On, 1);
			isOn.SetValue(e_On, 2);
			isOn.SetValue(se_On, 3);
			isOn.SetValue(sw_On, 4);
			isOn.SetValue(w_On, 5);
			EnsureMaterialInstance();
			HexProjector.sharedMaterial = m_Material;
			m_Material.SetFloat("_CrossHair", (!disableCrossHair) ? 1 : 0);
			HexUpdate();
		}
	}

	private bool EvaluateCacheChanged()
	{
		if (m_CachedW_On != w_On || m_CachedNw_On != nw_On || m_CachedNe_On != ne_On || m_CachedE_On != e_On || m_CachedSe_On != se_On || m_CachedSw_On != sw_On || m_CachedHover != Hover || m_CachedMode != Mode || m_CachedType != Type || m_HoverFlags != m_HoverFlagsCached || m_ColorOverrideCached != ColorOverride)
		{
			m_CachedW_On = w_On;
			m_CachedNw_On = nw_On;
			m_CachedNe_On = ne_On;
			m_CachedE_On = e_On;
			m_CachedSe_On = se_On;
			m_CachedSw_On = sw_On;
			m_CachedHover = Hover;
			m_CachedMode = Mode;
			m_CachedType = Type;
			m_HoverFlagsCached = m_HoverFlags;
			m_ColorOverrideCached = ColorOverride;
			return true;
		}
		return false;
	}

	public void HexUpdate()
	{
		HexSettings();
		BorderParticleAdjustment();
		ProjectorMaterialAdjustment();
	}

	private void HexSettings()
	{
		switch (Type)
		{
		case IHexControl.HexType.NegativeEffect:
			switch (Mode)
			{
			case IHexControl.HexMode.Reach:
			case IHexControl.HexMode.ReachAllHexes:
				if (Hover)
				{
					HexColour = negativeEffectReachColourHover;
				}
				else
				{
					HexColour = negativeEffectReachColour;
				}
				break;
			case IHexControl.HexMode.PossibleTarget:
				if (Hover)
				{
					HexColour = negativeEffectPossibleTargetColourHover;
				}
				else
				{
					HexColour = negativeEffectPossibleTargetColour;
				}
				break;
			case IHexControl.HexMode.Selected:
				if (Hover)
				{
					HexColour = negativeEffectSelectedColourHover;
				}
				else
				{
					HexColour = negativeEffectSelectedColour;
				}
				break;
			case IHexControl.HexMode.SelectedAOEWithoutTarget:
				if (Hover)
				{
					HexColour = negativeEffectSelectedAoEWithoutTargetColourHover;
				}
				else
				{
					HexColour = negativeEffectSelectedAoEWithoutTargetColour;
				}
				break;
			}
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		case IHexControl.HexType.PositiveEffect:
			switch (Mode)
			{
			case IHexControl.HexMode.Reach:
			case IHexControl.HexMode.ReachAllHexes:
				if (Hover)
				{
					HexColour = positiveEffectReachColour;
				}
				else
				{
					HexColour = positiveEffectReachColourHover;
				}
				break;
			case IHexControl.HexMode.PossibleTarget:
				if (Hover)
				{
					HexColour = positiveEffectPossibleTargetColour;
				}
				else
				{
					HexColour = positiveEffectPossibleTargetColourHover;
				}
				break;
			case IHexControl.HexMode.Selected:
				if (Hover)
				{
					HexColour = positiveEffectSelectedColour;
				}
				else
				{
					HexColour = positiveEffectSelectedColourHover;
				}
				break;
			case IHexControl.HexMode.SelectedAOEWithoutTarget:
				if (Hover)
				{
					HexColour = positiveEffectSelectedAoEWithoutTargetColour;
				}
				else
				{
					HexColour = positiveEffectSelectedAoEWithoutTargetColourHover;
				}
				break;
			}
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		case IHexControl.HexType.Move:
			switch (Mode)
			{
			case IHexControl.HexMode.Reach:
			case IHexControl.HexMode.ReachAllHexes:
				if (Hover)
				{
					HexColour = moveReachColour;
				}
				else
				{
					HexColour = moveReachColourHover;
				}
				break;
			case IHexControl.HexMode.PossibleTarget:
				if (Hover)
				{
					HexColour = movePossibleTargetColour;
				}
				else
				{
					HexColour = movePossibleTargetColourHover;
				}
				break;
			case IHexControl.HexMode.Selected:
				if (Hover)
				{
					HexColour = moveSelectedColour;
				}
				else
				{
					HexColour = moveSelectedColourHover;
				}
				break;
			}
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		case IHexControl.HexType.InvalidPlacement:
			HexColour = invalidPlacementColour;
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		case IHexControl.HexType.ExitDungeonRevealed:
			HexColour = dungeonExitColour;
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		case IHexControl.HexType.ExitDungeonUnrevealed:
			HexColour = dungeonExitColour;
			unseenGroundPlane.gameObject.SetActive(value: true);
			break;
		case IHexControl.HexType.ActiveBonusCondition:
			HexColour = activeBonusConditionColour;
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		case IHexControl.HexType.DifficultTerrain:
			switch (Mode)
			{
			case IHexControl.HexMode.Reach:
			case IHexControl.HexMode.ReachAllHexes:
				if (Hover)
				{
					HexColour = difficultTerrainReachColourHover;
				}
				else
				{
					HexColour = difficultTerrainReachColour;
				}
				break;
			case IHexControl.HexMode.PossibleTarget:
				if (Hover)
				{
					HexColour = difficultTerrainPossibleTargetColourHover;
				}
				else
				{
					HexColour = difficultTerrainPossibleTargetColour;
				}
				break;
			case IHexControl.HexMode.Selected:
				if (Hover)
				{
					HexColour = difficultTerrainSelectedColourHover;
				}
				else
				{
					HexColour = difficultTerrainSelectedColour;
				}
				break;
			case IHexControl.HexMode.SelectedAOEWithoutTarget:
				if (Hover)
				{
					HexColour = difficultTerrainSelectedAoEWithoutTargetColourHover;
				}
				else
				{
					HexColour = difficultTerrainSelectedAoEWithoutTargetColour;
				}
				break;
			}
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		case IHexControl.HexType.IcyTerrain:
			switch (Mode)
			{
			case IHexControl.HexMode.Reach:
			case IHexControl.HexMode.ReachAllHexes:
				if (Hover)
				{
					HexColour = icyTerrainReachColourHover;
				}
				else
				{
					HexColour = icyTerrainReachColour;
				}
				break;
			case IHexControl.HexMode.PossibleTarget:
				if (Hover)
				{
					HexColour = icyTerrainPossibleTargetColourHover;
				}
				else
				{
					HexColour = icyTerrainPossibleTargetColour;
				}
				break;
			case IHexControl.HexMode.Selected:
				if (Hover)
				{
					HexColour = icyTerrainSelectedColourHover;
				}
				else
				{
					HexColour = icyTerrainSelectedColour;
				}
				break;
			case IHexControl.HexMode.SelectedAOEWithoutTarget:
				if (Hover)
				{
					HexColour = icyTerrainSelectedAoEWithoutTargetColourHover;
				}
				else
				{
					HexColour = icyTerrainSelectedAoEWithoutTargetColour;
				}
				break;
			}
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		case IHexControl.HexType.Objective:
			HexColour = objectiveColour;
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		case IHexControl.HexType.ElevatedTerrain:
			HexColour = elevatedTerrainColour;
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		default:
			HexColour = moveReachColour;
			unseenGroundPlane.gameObject.SetActive(value: false);
			break;
		}
		switch (Mode)
		{
		case IHexControl.HexMode.Reach:
			hexIntensity = reachHexIntensity;
			borderFlameIntensity = reachBorderFlameIntensity;
			borderLineIntensity = reachBorderLineIntensity;
			sparkRateMin = 15f;
			sparkRateMax = 30f;
			TargetFrame = false;
			break;
		case IHexControl.HexMode.ReachAllHexes:
			hexIntensity = reachAllHexIntensity;
			borderFlameIntensity = reachAllBorderFlameIntensity;
			borderLineIntensity = reachAllBorderLineIntensity;
			sparkRateMin = 15f;
			sparkRateMax = 30f;
			TargetFrame = false;
			break;
		case IHexControl.HexMode.PossibleTarget:
		case IHexControl.HexMode.SelectedAOEWithoutTarget:
			hexIntensity = targetHexIntensity;
			borderFlameIntensity = targetBorderFlameIntensity;
			borderLineIntensity = targetBorderLineIntensity;
			sparkRateMin = 50f;
			sparkRateMax = 70f;
			TargetFrame = false;
			break;
		case IHexControl.HexMode.Selected:
			hexIntensity = selectedHexIntensity;
			borderFlameIntensity = selectedBorderFlameIntensity;
			borderLineIntensity = selectedBorderLineIntensity;
			sparkRateMin = 90f;
			sparkRateMax = 120f;
			targetFrameIntensity = selectedTargetFrameIntensity;
			TargetFrame = true;
			break;
		case IHexControl.HexMode.Cursor:
			HexColour = moveReachColour;
			hexIntensity = cursorIntensity;
			borderFlameIntensity = 0f;
			borderLineIntensity = 0.15f;
			sparkRateMin = 15f;
			sparkRateMax = 30f;
			TargetFrame = false;
			w_On = (nw_On = (ne_On = (e_On = (sw_On = (se_On = false)))));
			break;
		}
		if (ColorOverride.HasValue)
		{
			borderFlameIntensity *= ColorOverride.Value.a;
			borderLineIntensity *= ColorOverride.Value.a;
			hexIntensity *= ColorOverride.Value.a;
			targetFrameIntensity *= ColorOverride.Value.a;
		}
	}

	private void BorderParticleAdjustment()
	{
		for (int i = 0; i < 6; i++)
		{
			ParticleSystem obj = particleBits[i];
			ParticleSystem.MainModule main = obj.main;
			main.startColor = new ParticleSystem.MinMaxGradient(new Color(HexColour.r, positiveEffectReachColour.g, HexColour.b, 1f));
			ParticleSystem.EmissionModule emission = obj.emission;
			emission.rateOverTime = new ParticleSystem.MinMaxCurve(sparkRateMin, sparkRateMax);
			if ((borderParticleBitsByDefault && isOn[i] && Mode != IHexControl.HexMode.Cursor) || Mode == IHexControl.HexMode.PossibleTarget || Mode == IHexControl.HexMode.SelectedAOEWithoutTarget || Mode == IHexControl.HexMode.Selected || (Hover && (Mode == IHexControl.HexMode.Reach || Mode == IHexControl.HexMode.ReachAllHexes)))
			{
				particleBits[i].gameObject.SetActive(value: true);
			}
			else
			{
				particleBits[i].gameObject.SetActive(value: false);
			}
			if (Hover)
			{
				Color hexColour = HexColour;
				if (Type == IHexControl.HexType.Move)
				{
					hexColour.a = 0.25f;
				}
				ParticleSystem.MainModule main2 = particleHover[i].main;
				main2.startColor = new ParticleSystem.MinMaxGradient(hexColour);
				int particles = particleHover[i].GetParticles(m_Particles);
				for (int j = 0; j < particles; j++)
				{
					m_Particles[j].startColor = hexColour;
				}
				particleHover[i].SetParticles(m_Particles, particles);
			}
			bool active = (m_HoverFlags & (1 << i)) > 0 && Hover;
			if (Mode == IHexControl.HexMode.Reach || Mode == IHexControl.HexMode.ReachAllHexes)
			{
				particleHover[i].gameObject.SetActive(value: false);
			}
			else
			{
				particleHover[i].gameObject.SetActive(active);
			}
		}
	}

	private void ProjectorMaterialAdjustment()
	{
		if (Hover)
		{
			if (Mode == IHexControl.HexMode.Reach || Mode == IHexControl.HexMode.ReachAllHexes)
			{
				hexIntensity += hoverReachHexIntensityAdd;
				borderLineIntensity += hoverReachBorderLineIntensityAdd;
			}
			else if (Mode == IHexControl.HexMode.PossibleTarget || Mode == IHexControl.HexMode.SelectedAOEWithoutTarget)
			{
				hexIntensity += hoverTargetHexIntensityAdd;
				borderLineIntensity += hoverTargetBorderLineIntensityAdd;
			}
			else if (Mode == IHexControl.HexMode.Selected)
			{
				hexIntensity += hoverSelectedHexIntensityAdd;
				borderLineIntensity += hoverSelectedBorderLineIntensityAdd;
			}
		}
		m_Material.SetColor("_HexColour", HexColour);
		m_Material.SetFloat("_HexIntensity", hexIntensity);
		m_Material.SetFloat("_BorderLineIntensity", borderLineIntensity);
		m_Material.SetFloat("_BorderFlameIntensity", borderFlameIntensity);
		m_Material.SetFloat("_TargetFrameIntensity", targetFrameIntensity);
		float num = 0f;
		num = ((!TargetFrame) ? 0f : 1f);
		m_Material.SetFloat("_CrossHair", num);
		if (nw_On || Mode == IHexControl.HexMode.PossibleTarget || Mode == IHexControl.HexMode.Selected || Mode == IHexControl.HexMode.SelectedAOEWithoutTarget || Hover)
		{
			m_Material.SetInt("_NW_On", 1);
		}
		else
		{
			m_Material.SetInt("_NW_On", 0);
		}
		if (ne_On || Mode == IHexControl.HexMode.PossibleTarget || Mode == IHexControl.HexMode.Selected || Mode == IHexControl.HexMode.SelectedAOEWithoutTarget || Hover)
		{
			m_Material.SetInt("_NE_On", 1);
		}
		else
		{
			m_Material.SetInt("_NE_On", 0);
		}
		if (e_On || Mode == IHexControl.HexMode.PossibleTarget || Mode == IHexControl.HexMode.Selected || Mode == IHexControl.HexMode.SelectedAOEWithoutTarget || Hover)
		{
			m_Material.SetInt("_E_On", 1);
		}
		else
		{
			m_Material.SetInt("_E_On", 0);
		}
		if (se_On || Mode == IHexControl.HexMode.PossibleTarget || Mode == IHexControl.HexMode.Selected || Mode == IHexControl.HexMode.SelectedAOEWithoutTarget || Hover)
		{
			m_Material.SetInt("_SE_On", 1);
		}
		else
		{
			m_Material.SetInt("_SE_On", 0);
		}
		if (sw_On || Mode == IHexControl.HexMode.PossibleTarget || Mode == IHexControl.HexMode.Selected || Mode == IHexControl.HexMode.SelectedAOEWithoutTarget || Hover)
		{
			m_Material.SetInt("_SW_On", 1);
		}
		else
		{
			m_Material.SetInt("_SW_On", 0);
		}
		if (w_On || Mode == IHexControl.HexMode.PossibleTarget || Mode == IHexControl.HexMode.Selected || Mode == IHexControl.HexMode.SelectedAOEWithoutTarget || Hover)
		{
			m_Material.SetInt("_W_On", 1);
		}
		else
		{
			m_Material.SetInt("_W_On", 0);
		}
	}
}
