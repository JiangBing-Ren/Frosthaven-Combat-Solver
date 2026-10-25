using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Chronos;
using FH.LevelComponents.ProcGen;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class LevelEditorApparanceOverridePanel : MonoBehaviour
{
	private enum EApparanceOverrideType
	{
		Prop,
		Room,
		Wall,
		Scenario
	}

	public TextMeshProUGUI DescriptionText;

	public ConvenientDropdown BiomeDropDown;

	public ConvenientDropdown SubBiomeDropDown;

	public ConvenientDropdown ThemeDropDown;

	public ConvenientDropdown SubThemeDropDown;

	public ConvenientDropdown ToneDropDown;

	public ConvenientDropdown BiomeStateDropdown;

	public TMP_InputField SeedInput;

	public UnityEvent OnValueChanged;

	public GameObject BlockerObject;

	public TextMeshProUGUI BlockerText;

	private EApparanceOverrideType m_ApparanceOverrideType;

	private CObjectProp m_PropBeingDisplayed;

	private GameObject m_PropObjectBeingDisplayed;

	private CMap m_RoomBeingDisplayed;

	private int m_WallIndexBeingDisplayed;

	private ProceduralStyle[] m_ProceduralStylesBeingEdited;

	private CApparanceOverrideDetails m_CurrentApparanceOverride;

	private const float cPropObjectCheckTimeout = 10f;

	private IEnumerator m_PropSpawnCheckRoutine;

	private bool m_WaitingForOnEnableToResolveProp;

	private bool m_Initialised;

	private void EnsurePanelInitialised()
	{
		if (!m_Initialised)
		{
			List<ScenarioPossibleRoom.EBiome> source = GlobalSettings.Instance.ApparanceEnumFilter.AvailableBiomes().ToList();
			List<ScenarioPossibleRoom.ESubBiome> source2 = GlobalSettings.Instance.ApparanceEnumFilter.AvailableSubBiomes().ToList();
			List<ScenarioPossibleRoom.ETheme> source3 = GlobalSettings.Instance.ApparanceEnumFilter.AvailableThemes().ToList();
			List<ScenarioPossibleRoom.ESubTheme> source4 = GlobalSettings.Instance.ApparanceEnumFilter.AvailableSubThemes().ToList();
			List<ScenarioPossibleRoom.ETone> source5 = GlobalSettings.Instance.ApparanceEnumFilter.AvailableTones().ToList();
			List<string> options = Enum.GetNames(typeof(ScenarioPossibleRoom.EBiomeState)).ToList();
			BiomeDropDown.options.Clear();
			BiomeDropDown.AddOptions(source.Select((ScenarioPossibleRoom.EBiome b) => b.ToString()).ToList());
			SetDropdownLabel(BiomeDropDown, "EBiome");
			BiomeDropDown.OnValueChanged.AddListener((int i) =>
			{
				OnSetBiomePressed();
			});
			SubBiomeDropDown.options.Clear();
			SubBiomeDropDown.AddOptions(source2.Select((ScenarioPossibleRoom.ESubBiome sb) => sb.ToString()).ToList());
			SetDropdownLabel(SubBiomeDropDown, "ESubBiome");
			SubBiomeDropDown.OnValueChanged.AddListener((int i) =>
			{
				OnSetSubBiomePressed();
			});
			ThemeDropDown.options.Clear();
			ThemeDropDown.AddOptions(source3.Select((ScenarioPossibleRoom.ETheme t) => t.ToString()).ToList());
			SetDropdownLabel(ThemeDropDown, "ETheme");
			ThemeDropDown.OnValueChanged.AddListener((int i) =>
			{
				OnSetThemePressed();
			});
			SubThemeDropDown.options.Clear();
			SubThemeDropDown.AddOptions(source4.Select((ScenarioPossibleRoom.ESubTheme st) => st.ToString()).ToList());
			SetDropdownLabel(SubThemeDropDown, "ESubTheme");
			SubThemeDropDown.OnValueChanged.AddListener((int i) =>
			{
				OnSetSubThemePressed();
			});
			ToneDropDown.options.Clear();
			ToneDropDown.AddOptions(source5.Select((ScenarioPossibleRoom.ETone t) => t.ToString()).ToList());
			SetDropdownLabel(ToneDropDown, "ETone");
			ToneDropDown.OnValueChanged.AddListener((int i) =>
			{
				OnSetTonePressed();
			});
			BiomeStateDropdown.options.Clear();
			BiomeStateDropdown.AddOptions(options);
			SetDropdownLabel(BiomeStateDropdown, "EBiomeState");
			BiomeStateDropdown.OnValueChanged.AddListener((int i) =>
			{
				OnSetBiomeStatePressed();
			});
			m_Initialised = true;
		}
		static void SetDropdownLabel(ConvenientDropdown dropdown, object labelProvider)
		{
			dropdown.SetName(labelProvider.ToString().TrimStart('E'));
		}
	}

	private void Awake()
	{
		EnsurePanelInitialised();
	}

	public void SetDescriptionText(string desc)
	{
		DescriptionText.text = desc;
	}

	public void SetPropDisplayed(CObjectProp prop)
	{
		if (m_ApparanceOverrideType != EApparanceOverrideType.Prop)
		{
			m_RoomBeingDisplayed = null;
			m_WallIndexBeingDisplayed = -1;
		}
		SetDescriptionText("Set Apparance override at <b>Prop Level</b>");
		m_ApparanceOverrideType = EApparanceOverrideType.Prop;
		if (prop == null)
		{
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
			m_ProceduralStylesBeingEdited = null;
		}
		else if (m_PropBeingDisplayed == null || m_PropBeingDisplayed.PropGuid != prop.PropGuid || m_PropObjectBeingDisplayed == null)
		{
			if (m_PropSpawnCheckRoutine != null)
			{
				StopCoroutine(m_PropSpawnCheckRoutine);
				m_PropSpawnCheckRoutine = null;
			}
			m_PropSpawnCheckRoutine = ContinuouslyCheckForPropsObject(prop);
			m_WaitingForOnEnableToResolveProp = false;
			if (gameObject.activeInHierarchy)
			{
				StartCoroutine(m_PropSpawnCheckRoutine);
			}
			else
			{
				m_WaitingForOnEnableToResolveProp = true;
			}
		}
	}

	private void OnEnable()
	{
		if (m_WaitingForOnEnableToResolveProp && m_PropSpawnCheckRoutine != null)
		{
			m_WaitingForOnEnableToResolveProp = false;
			StartCoroutine(m_PropSpawnCheckRoutine);
		}
	}

	private IEnumerator ContinuouslyCheckForPropsObject(CObjectProp prop)
	{
		bool timedOut = false;
		float timeAtStartOfCheck = Timekeeper.instance.m_GlobalClock.unscaledTime;
		GameObject propGameObject = null;
		bool propGameObjectExists = false;
		BlockerObject.SetActive(value: true);
		BlockerText.text = "Locating Prop's Apparance Object";
		while (!propGameObjectExists && !timedOut)
		{
			yield return null;
			if (Timekeeper.instance.m_GlobalClock.unscaledTime - timeAtStartOfCheck > 10f)
			{
				timedOut = true;
				break;
			}
			propGameObject = LevelEditorController.MapSceneRoot.FindInChildren(prop.InstanceName);
			if (propGameObject != null)
			{
				propGameObjectExists = true;
				break;
			}
		}
		if (propGameObjectExists && !timedOut)
		{
			m_PropBeingDisplayed = prop;
			m_PropObjectBeingDisplayed = propGameObject;
			m_ProceduralStylesBeingEdited = m_PropObjectBeingDisplayed.GetComponentsInChildren<ProceduralStyle>();
			if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
			{
				BlockerObject.SetActive(value: true);
				BlockerText.text = "Unable to edit Prop's apparance settings";
				m_PropBeingDisplayed = null;
				m_PropObjectBeingDisplayed = null;
				m_ProceduralStylesBeingEdited = null;
			}
			else
			{
				BlockerObject.SetActive(value: false);
				BlockerText.text = "-";
			}
		}
		else
		{
			BlockerObject.SetActive(value: true);
			BlockerText.text = "Unable to edit Prop's apparance settings";
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
			m_ProceduralStylesBeingEdited = null;
		}
		RefreshPanelForCurrentOverride();
	}

	public void SetRoomDisplayed(CMap map)
	{
		if (m_ApparanceOverrideType != EApparanceOverrideType.Room)
		{
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
		}
		SetDescriptionText("Set Apparance override at <b>Room Level</b> <i>(NB: This will affect the look of all child components like props etc)</i>");
		m_ApparanceOverrideType = EApparanceOverrideType.Room;
		if (map == null)
		{
			m_RoomBeingDisplayed = null;
			m_ProceduralStylesBeingEdited = null;
		}
		else if (m_RoomBeingDisplayed == null || m_RoomBeingDisplayed.MapGuid != map.MapGuid || m_WallIndexBeingDisplayed >= 0)
		{
			m_WallIndexBeingDisplayed = -1;
			m_RoomBeingDisplayed = map;
			GameObject gameObject = GameObject.Find(m_RoomBeingDisplayed.MapInstanceName);
			if (gameObject != null)
			{
				m_ProceduralStylesBeingEdited = gameObject.GetComponents<ProceduralStyle>();
				if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
				{
					BlockerObject.SetActive(value: true);
					BlockerText.text = "Unable to edit Rooms's apparance settings";
					m_RoomBeingDisplayed = null;
					m_ProceduralStylesBeingEdited = null;
				}
				else
				{
					BlockerText.text = "-";
					BlockerObject.SetActive(value: false);
				}
			}
			else
			{
				m_RoomBeingDisplayed = null;
				m_ProceduralStylesBeingEdited = null;
			}
		}
		RefreshPanelForCurrentOverride();
	}

	public void SetWallDisplayed(CMap parentRoom, int wallIndex)
	{
		if (m_ApparanceOverrideType != EApparanceOverrideType.Wall)
		{
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
		}
		SetDescriptionText("Set Apparance override at <b>Wall Level</b>");
		m_ApparanceOverrideType = EApparanceOverrideType.Wall;
		if (parentRoom == null || wallIndex < 0)
		{
			m_RoomBeingDisplayed = null;
			m_ProceduralStylesBeingEdited = null;
			m_WallIndexBeingDisplayed = -1;
		}
		else if (m_RoomBeingDisplayed == null || m_RoomBeingDisplayed.MapGuid != parentRoom.MapGuid || m_WallIndexBeingDisplayed != wallIndex)
		{
			m_RoomBeingDisplayed = parentRoom;
			m_WallIndexBeingDisplayed = wallIndex;
			GameObject gameObject = GameObject.Find(m_RoomBeingDisplayed.MapInstanceName);
			if (gameObject != null)
			{
				ProceduralWall[] componentsInChildren = gameObject.GetComponentsInChildren<ProceduralWall>();
				if (m_WallIndexBeingDisplayed < componentsInChildren.Length)
				{
					ProceduralWall proceduralWall = componentsInChildren[m_WallIndexBeingDisplayed];
					m_ProceduralStylesBeingEdited = proceduralWall.GetComponents<ProceduralStyle>();
					if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
					{
						BlockerObject.SetActive(value: true);
						BlockerText.text = "Unable to edit Wall's apparance settings";
						m_RoomBeingDisplayed = null;
						m_ProceduralStylesBeingEdited = null;
						m_WallIndexBeingDisplayed = -1;
					}
					else
					{
						BlockerText.text = "-";
						BlockerObject.SetActive(value: false);
					}
				}
				else
				{
					m_RoomBeingDisplayed = null;
					m_ProceduralStylesBeingEdited = null;
					m_WallIndexBeingDisplayed = -1;
				}
			}
			else
			{
				m_RoomBeingDisplayed = null;
				m_ProceduralStylesBeingEdited = null;
				m_WallIndexBeingDisplayed = -1;
			}
		}
		RefreshPanelForCurrentOverride();
	}

	public void SetScenarioDisplayed()
	{
		if (m_ApparanceOverrideType != EApparanceOverrideType.Scenario)
		{
			m_PropBeingDisplayed = null;
			m_PropObjectBeingDisplayed = null;
			m_RoomBeingDisplayed = null;
			m_WallIndexBeingDisplayed = -1;
		}
		SetDescriptionText("Set Apparance override at <b>Scenario Level</b> <i>(NB: This will affect the look of all child components, like Rooms, props, walls, etc)</i>");
		m_ApparanceOverrideType = EApparanceOverrideType.Scenario;
		GameObject mapSceneRoot = LevelEditorController.MapSceneRoot;
		if (mapSceneRoot != null)
		{
			m_ProceduralStylesBeingEdited = mapSceneRoot.GetComponents<ProceduralStyle>();
			if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
			{
				BlockerObject.SetActive(value: true);
				BlockerText.text = "Unable to edit Scecnario's apparance settings";
				m_ProceduralStylesBeingEdited = null;
			}
			else
			{
				BlockerText.text = "-";
				BlockerObject.SetActive(value: false);
			}
		}
		else
		{
			m_ProceduralStylesBeingEdited = null;
		}
		RefreshPanelForCurrentOverride();
	}

	public void RefreshPanelForCurrentOverride()
	{
		EnsurePanelInitialised();
		m_CurrentApparanceOverride = null;
		if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0 || (m_ApparanceOverrideType == EApparanceOverrideType.Prop && m_PropBeingDisplayed == null) || (m_ApparanceOverrideType == EApparanceOverrideType.Room && m_RoomBeingDisplayed == null) || (m_ApparanceOverrideType == EApparanceOverrideType.Wall && (m_RoomBeingDisplayed == null || m_WallIndexBeingDisplayed < 0)))
		{
			BiomeDropDown.SetValueWithoutNotify(0);
			SubBiomeDropDown.SetValueWithoutNotify(0);
			ThemeDropDown.SetValueWithoutNotify(0);
			SubThemeDropDown.SetValueWithoutNotify(0);
			ToneDropDown.SetValueWithoutNotify(0);
			BiomeStateDropdown.SetValueWithoutNotify(0);
			SeedInput.SetTextWithoutNotify(string.Empty);
			return;
		}
		string text = string.Empty;
		switch (m_ApparanceOverrideType)
		{
		case EApparanceOverrideType.Prop:
			text = m_PropBeingDisplayed.PropGuid;
			break;
		case EApparanceOverrideType.Room:
			text = m_RoomBeingDisplayed.MapGuid;
			break;
		case EApparanceOverrideType.Wall:
			text = CApparanceOverrideDetails.GetWallGUID(m_RoomBeingDisplayed, m_WallIndexBeingDisplayed);
			break;
		case EApparanceOverrideType.Scenario:
			text = "ScenarioApparanceOverrideGUID";
			break;
		}
		m_CurrentApparanceOverride = SaveData.Instance.Global.CurrentEditorLevelData.GetApparanceOverrideDetailsForGUID(text);
		int objectCount;
		if (m_CurrentApparanceOverride == null)
		{
			m_CurrentApparanceOverride = new CApparanceOverrideDetails(text);
			m_CurrentApparanceOverride.OverrideSeed = m_ProceduralStylesBeingEdited[0].Seed;
			m_CurrentApparanceOverride.OverrideBiome = m_ProceduralStylesBeingEdited[0].Biome;
			m_CurrentApparanceOverride.OverrideSubBiome = m_ProceduralStylesBeingEdited[0].SubBiome;
			m_CurrentApparanceOverride.OverrideTheme = m_ProceduralStylesBeingEdited[0].Theme;
			m_CurrentApparanceOverride.OverrideSubTheme = m_ProceduralStylesBeingEdited[0].SubTheme;
			m_CurrentApparanceOverride.OverrideTone = m_ProceduralStylesBeingEdited[0].Tone;
			m_CurrentApparanceOverride.OverrideBiomeState = m_ProceduralStylesBeingEdited[0].BiomeState;
			m_ProceduralStylesBeingEdited[0].FetchObjectHierarchyValues(out var objectIndex, out objectCount);
			m_CurrentApparanceOverride.OverrideSiblingIndex = objectIndex;
		}
		if (m_CurrentApparanceOverride.OverrideSiblingIndex == -1)
		{
			m_ProceduralStylesBeingEdited[0].FetchObjectHierarchyValues(out var objectIndex2, out objectCount);
			m_CurrentApparanceOverride.OverrideSiblingIndex = objectIndex2;
		}
		BiomeDropDown.SetValueWithoutNotify(BiomeDropDown.options.FindIndex((ConvenientDropdown.Option o) => o.text == m_CurrentApparanceOverride.OverrideBiome.ToString()));
		SubBiomeDropDown.SetValueWithoutNotify(SubBiomeDropDown.options.FindIndex((ConvenientDropdown.Option o) => o.text == m_CurrentApparanceOverride.OverrideSubBiome.ToString()));
		ThemeDropDown.SetValueWithoutNotify(ThemeDropDown.options.FindIndex((ConvenientDropdown.Option o) => o.text == m_CurrentApparanceOverride.OverrideTheme.ToString()));
		SubThemeDropDown.SetValueWithoutNotify(SubThemeDropDown.options.FindIndex((ConvenientDropdown.Option o) => o.text == m_CurrentApparanceOverride.OverrideSubTheme.ToString()));
		ToneDropDown.SetValueWithoutNotify(ToneDropDown.options.FindIndex((ConvenientDropdown.Option o) => o.text == m_CurrentApparanceOverride.OverrideTone.ToString()));
		BiomeStateDropdown.SetValueWithoutNotify(BiomeStateDropdown.options.FindIndex((ConvenientDropdown.Option o) => o.text == m_CurrentApparanceOverride.OverrideBiomeState.ToString()));
		SeedInput.SetTextWithoutNotify(m_CurrentApparanceOverride.OverrideSeed.ToString());
	}

	public async void SaveOverrideToLevel()
	{
		OnValueChanged?.Invoke();
		Log.InfoFormat("Saving Override of type {0}, with GUID {1}", m_ApparanceOverrideType.ToString(), m_CurrentApparanceOverride.GUID);
		switch (m_ApparanceOverrideType)
		{
		case EApparanceOverrideType.Prop:
			if (m_PropBeingDisplayed != null)
			{
				LevelEditorController s_Instance = LevelEditorController.s_Instance;
				s_Instance.AddOrReplaceApparanceOverride(await m_CurrentApparanceOverride.DeepCopySerializableObject<CApparanceOverrideDetails>());
			}
			break;
		case EApparanceOverrideType.Room:
			if (m_RoomBeingDisplayed != null)
			{
				LevelEditorController s_Instance = LevelEditorController.s_Instance;
				s_Instance.AddOrReplaceApparanceOverride(await m_CurrentApparanceOverride.DeepCopySerializableObject<CApparanceOverrideDetails>());
			}
			break;
		case EApparanceOverrideType.Wall:
			if (m_RoomBeingDisplayed != null && m_WallIndexBeingDisplayed >= 0)
			{
				LevelEditorController s_Instance = LevelEditorController.s_Instance;
				s_Instance.AddOrReplaceApparanceOverride(await m_CurrentApparanceOverride.DeepCopySerializableObject<CApparanceOverrideDetails>());
			}
			break;
		case EApparanceOverrideType.Scenario:
		{
			LevelEditorController s_Instance = LevelEditorController.s_Instance;
			s_Instance.AddOrReplaceApparanceOverride(await m_CurrentApparanceOverride.DeepCopySerializableObject<CApparanceOverrideDetails>());
			break;
		}
		}
	}

	public void OnSetBiomePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.EBiome eBiome = ((ScenarioPossibleRoom.EBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.EBiome))).Single((ScenarioPossibleRoom.EBiome s) => s.ToString() == BiomeDropDown.options[BiomeDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].Biome = eBiome;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideBiome = eBiome;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetSubBiomePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.ESubBiome eSubBiome = ((ScenarioPossibleRoom.ESubBiome[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubBiome))).Single((ScenarioPossibleRoom.ESubBiome s) => s.ToString() == SubBiomeDropDown.options[SubBiomeDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].SubBiome = eSubBiome;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSubBiome = eSubBiome;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetThemePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.ETheme eTheme = ((ScenarioPossibleRoom.ETheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETheme))).Single((ScenarioPossibleRoom.ETheme s) => s.ToString() == ThemeDropDown.options[ThemeDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].Theme = eTheme;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideTheme = eTheme;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetSubThemePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.ESubTheme eSubTheme = ((ScenarioPossibleRoom.ESubTheme[])Enum.GetValues(typeof(ScenarioPossibleRoom.ESubTheme))).Single((ScenarioPossibleRoom.ESubTheme s) => s.ToString() == SubThemeDropDown.options[SubThemeDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].SubTheme = eSubTheme;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSubTheme = eSubTheme;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetTonePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.ETone eTone = ((ScenarioPossibleRoom.ETone[])Enum.GetValues(typeof(ScenarioPossibleRoom.ETone))).Single((ScenarioPossibleRoom.ETone s) => s.ToString() == ToneDropDown.options[ToneDropDown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].Tone = eTone;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideTone = eTone;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetBiomeStatePressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			ScenarioPossibleRoom.EBiomeState eBiomeState = ((ScenarioPossibleRoom.EBiomeState[])Enum.GetValues(typeof(ScenarioPossibleRoom.EBiomeState))).Single((ScenarioPossibleRoom.EBiomeState s) => s.ToString() == BiomeStateDropdown.options[BiomeStateDropdown.value].text);
			for (int num = 0; num < m_ProceduralStylesBeingEdited.Length; num++)
			{
				m_ProceduralStylesBeingEdited[num].BiomeState = eBiomeState;
				m_ProceduralStylesBeingEdited[num].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideBiomeState = eBiomeState;
			}
			SaveOverrideToLevel();
		}
	}

	public void OnSetCustomSeedPressed()
	{
		if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
		{
			return;
		}
		if (int.TryParse(SeedInput.text, out var result))
		{
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = result;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = result;
			}
		}
		SaveOverrideToLevel();
	}

	public void OnSeedFieldEndedEditing()
	{
		if (m_ProceduralStylesBeingEdited == null || m_ProceduralStylesBeingEdited.Length == 0)
		{
			return;
		}
		if (int.TryParse(SeedInput.text, out var result))
		{
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = result;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = result;
			}
		}
		SaveOverrideToLevel();
	}

	public void OnDecrementSeedPressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			int seed = m_ProceduralStylesBeingEdited[0].Seed - 1;
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = seed;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = m_ProceduralStylesBeingEdited[0].Seed;
			}
			SeedInput.SetTextWithoutNotify(m_ProceduralStylesBeingEdited[0].Seed.ToString());
			SaveOverrideToLevel();
		}
	}

	public void OnIncrementSeedPressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = m_ProceduralStylesBeingEdited[i].Seed + 1;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = m_ProceduralStylesBeingEdited[0].Seed;
			}
			SeedInput.SetTextWithoutNotify(m_ProceduralStylesBeingEdited[0].Seed.ToString());
			SaveOverrideToLevel();
		}
	}

	public void OnRandomiseSeedPressed()
	{
		if (m_ProceduralStylesBeingEdited != null && m_ProceduralStylesBeingEdited.Length != 0)
		{
			int num = SharedClient.GlobalRNG.Next();
			SeedInput.SetTextWithoutNotify(num.ToString());
			for (int i = 0; i < m_ProceduralStylesBeingEdited.Length; i++)
			{
				m_ProceduralStylesBeingEdited[i].Seed = num;
				m_ProceduralStylesBeingEdited[i].ForceValidate();
			}
			if (m_CurrentApparanceOverride != null)
			{
				m_CurrentApparanceOverride.OverrideSeed = num;
			}
			SaveOverrideToLevel();
		}
	}
}
