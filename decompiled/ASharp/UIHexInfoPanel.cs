using System.Collections.Generic;
using System.Linq;
using System.Text;
using AStar;
using FHGUI.Scenario.Services;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIHexInfoPanel : Singleton<UIHexInfoPanel>
{
	[SerializeField]
	private UIWindow m_Window;

	[Header("Text")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_Description;

	[Header("Effects")]
	[SerializeField]
	private Transform m_EffectsContainer;

	[SerializeField]
	private Transform m_EffectsListContainer;

	[SerializeField]
	private GameObject m_UIEffectPrefab;

	[SerializeField]
	private Sprite m_SpriteDamage;

	[SerializeField]
	private Sprite m_SpriteHeal;

	[SerializeField]
	private Sprite m_SpriteMove;

	private readonly List<UIStatusEffect> _effects = new List<UIStatusEffect>();

	private const string TITLE_SEPARATOR = " | ";

	private const string DESCRIPTION_SEPARATOR = "\n";

	private const string DESCRIPTION_ORIGIN_SEPARATOR = ": ";

	private readonly StringBuilder _sbTitle = new StringBuilder();

	private readonly StringBuilder _sbDescription = new StringBuilder();

	public bool IsTitleOnly
	{
		get
		{
			if (_effects.Count == 0)
			{
				return !m_Description.isActiveAndEnabled;
			}
			return false;
		}
	}

	private void Update()
	{
		bool flag = Singleton<ActorStatPanel>.IsInitialized && Singleton<ActorStatPanel>.InstanceFast.isActiveAndEnabled;
		UIManager.RegisterLateActiveState(m_EffectsContainer.gameObject, !flag);
	}

	public void Show(List<HexViewService.PropInfoParams> infoParams)
	{
		if (infoParams == null || infoParams.Count == 0)
		{
			Hide();
			return;
		}
		bool flag = infoParams.Count > 1;
		_sbTitle.Clear();
		_sbDescription.Clear();
		_effects.ForEach((UIStatusEffect x) =>
		{
			Object.Destroy(x.gameObject);
		});
		_effects.Clear();
		for (int num = 0; num < infoParams.Count; num++)
		{
			HexViewService.PropInfoParams propInfoParams = infoParams[num];
			bool flag2 = num > 0;
			if (propInfoParams.Title.IsNOTNullOrEmpty())
			{
				if (flag2)
				{
					_sbTitle.Append(" | ");
				}
				_sbTitle.Append(propInfoParams.Title);
			}
			bool flag3 = propInfoParams.Description.IsNOTNullOrEmpty();
			if (!string.IsNullOrEmpty(propInfoParams.EffectsParams?.QuestItemDescription))
			{
				flag3 = false;
			}
			if (flag3)
			{
				if (flag2)
				{
					_sbDescription.Append("\n");
				}
				if (flag)
				{
					_sbDescription.Append(propInfoParams.Title + ": ");
				}
				_sbDescription.Append(propInfoParams.Description);
			}
			if (propInfoParams.EffectsParams == null)
			{
				continue;
			}
			int count = _effects.Count;
			if (propInfoParams.EffectsParams.DamageValue > 0)
			{
				CreateDamageEffect(propInfoParams.EffectsParams.DamageValue);
			}
			if (propInfoParams.EffectsParams.HealValue > 0)
			{
				CreateHealEffect(propInfoParams.EffectsParams.HealValue);
			}
			if (propInfoParams.EffectsParams.MoveCost > 0)
			{
				CreateMovementEffect(propInfoParams.EffectsParams.MoveCost);
			}
			if (propInfoParams.EffectsParams.QuestItemDescription.IsNOTNullOrEmpty())
			{
				CreateQuestItemEffect(propInfoParams.EffectsParams.QuestItemDescription);
			}
			if (propInfoParams.EffectsParams.Conditions != null)
			{
				foreach (CCondition.ENegativeCondition condition in propInfoParams.EffectsParams.Conditions)
				{
					CreateNegativeEffect(condition);
				}
			}
			if (propInfoParams.EffectsParams.PositiveConditions != null)
			{
				foreach (CCondition.EPositiveCondition positiveCondition in propInfoParams.EffectsParams.PositiveConditions)
				{
					CreatePositiveEffect(positiveCondition);
				}
			}
			if (propInfoParams.EffectsParams.Owner != null)
			{
				CreateOwnerEffect(propInfoParams.EffectsParams.Owner);
			}
			if (flag)
			{
				for (int num2 = count; num2 < _effects.Count; num2++)
				{
					_effects[num2].AppendSource(propInfoParams.Title);
				}
			}
		}
		m_Title.text = _sbTitle.ToString();
		m_Description.text = _sbDescription.ToString();
		m_Description.gameObject.SetActive(!m_Description.text.IsNullOrEmpty());
		m_EffectsContainer.gameObject.SetActive(_effects.Count > 0);
		m_Window.Show();
	}

	public void ShowTrap(GameObject trap)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if ((TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone) || Singleton<UIResultsManager>.Instance.IsShown)
			{
				return;
			}
			if (trap == null)
			{
				Log.GUIError("Trying to display trap info but the trap object does not exist.");
				return;
			}
			if (trap.GetComponent<UnityGameEditorObject>() == null)
			{
				Log.GUIError("No UnityGameEditorObject script found on Trap");
				return;
			}
			CObjectTrap cObjectTrap = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.OfType<CObjectTrap>().SingleOrDefault((CObjectTrap s) => s.InstanceName == trap.name);
			if (cObjectTrap == null)
			{
				return;
			}
			m_Title.text = LocalizationManager.GetTranslation(cObjectTrap.PrefabName);
			_effects.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			_effects.Clear();
			if (cObjectTrap.Damage)
			{
				if (cObjectTrap.Owner != null || cObjectTrap.HasCustomDamageSet)
				{
					CreateDamageEffect(cObjectTrap.DamageValue);
				}
				else
				{
					CreateDamageEffect(ScenarioManager.TrapDamage);
				}
			}
			if (cObjectTrap.HealValue > 0)
			{
				CreateHealEffect(cObjectTrap.HealValue);
			}
			if (cObjectTrap.Conditions != null)
			{
				foreach (CCondition.ENegativeCondition condition in cObjectTrap.Conditions)
				{
					CreateNegativeEffect(condition);
				}
			}
			if (cObjectTrap.PositiveConditions != null)
			{
				foreach (CCondition.EPositiveCondition positiveCondition in cObjectTrap.PositiveConditions)
				{
					CreatePositiveEffect(positiveCondition);
				}
			}
			if (!cObjectTrap.Damage && cObjectTrap.HealValue <= 0 && (cObjectTrap.Conditions == null || cObjectTrap.Conditions.Count <= 0) && cObjectTrap.PositiveConditions != null)
			{
				_ = cObjectTrap.PositiveConditions.Count;
				_ = 0;
			}
			m_Window.Show();
		}
	}

	public void ShowHazardousTerrain(GameObject hazardousTerrain)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if ((TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone) || Singleton<UIResultsManager>.Instance.IsShown)
			{
				return;
			}
			if (hazardousTerrain == null)
			{
				Log.GUIError("Trying to display hazardous terrain info but the hazardous object does not exist.");
				return;
			}
			if (hazardousTerrain.GetComponent<UnityGameEditorObject>() == null)
			{
				Log.GUIError("No UnityGameEditorObject script found on hazardous terrain");
				return;
			}
			CObjectHazardousTerrain cObjectHazardousTerrain = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.OfType<CObjectHazardousTerrain>().SingleOrDefault((CObjectHazardousTerrain s) => s.InstanceName == hazardousTerrain.name);
			if (cObjectHazardousTerrain != null)
			{
				m_Title.text = LocalizationManager.GetTranslation("HAZARDOUS_TERRAIN_TOOLTIP");
				_effects.ForEach((UIStatusEffect x) =>
				{
					Object.Destroy(x.gameObject);
				});
				_effects.Clear();
				CreateDamageEffect(ScenarioManager.HazardousTerrainDamage);
				if (cObjectHazardousTerrain.Owner != null)
				{
					CreateOwnerEffect(cObjectHazardousTerrain.Owner);
				}
				m_Window.Show();
			}
		}
	}

	public void ShowDifficultTerrainHazardous()
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			m_Title.text = LocalizationManager.GetTranslation("HAZARDOUS_DIFFICULT_TERRAIN_TOOLTIP");
			_effects.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			_effects.Clear();
			CreateDamageEffect(ScenarioManager.HazardousTerrainDamage);
			CreateMovementEffect(2);
			m_Window.Show();
		}
	}

	public void ShowDifficultTerrainIce()
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			m_Title.text = LocalizationManager.GetTranslation("FH/ICE_TERRAIN_TOOLTIP");
			_effects.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			_effects.Clear();
			CreateMovementEffect(2);
			m_Window.Show();
		}
	}

	public void ShowQuestItem(string title, string description)
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			m_Title.text = title;
			_effects.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			_effects.Clear();
			CreateQuestItemEffect(description);
			m_Window.Show();
		}
	}

	public void ShowDifficultTerrain(GameObject difficultTerrain)
	{
		using (new CActiveBonus.ActiveBonusCalculationScope())
		{
			if ((TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone) || Singleton<UIResultsManager>.Instance.IsShown)
			{
				return;
			}
			if (difficultTerrain == null)
			{
				Log.GUIError("Trying to display hazardous terrain info but the difficult terrain object does not exist.");
				return;
			}
			if (difficultTerrain.GetComponent<UnityGameEditorObject>() == null)
			{
				Log.GUIError("No UnityGameEditorObject script found on difficult terrain");
				return;
			}
			CObjectDifficultTerrain cObjectDifficultTerrain = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.OfType<CObjectDifficultTerrain>().SingleOrDefault((CObjectDifficultTerrain s) => s.InstanceName == difficultTerrain.name);
			if (cObjectDifficultTerrain == null)
			{
				CObjectProp cObjectProp = ScenarioManager.CurrentScenarioState.GetPropsThreadSafe.SingleOrDefault((CObjectProp s) => s.InstanceName == difficultTerrain.name);
				if (cObjectProp != null)
				{
					CNode node = ScenarioManager.PathFinder.GetNode(cObjectProp.ArrayIndex);
					if (node != null && node.IsDifficultTerrain)
					{
						ShowDifficultTerrain(cObjectProp);
					}
				}
			}
			else
			{
				ShowDifficultTerrain(cObjectDifficultTerrain);
			}
		}
	}

	public void ShowDifficultTerrain(CObjectProp difficultTerrain)
	{
		if ((TransitionManager.s_Instance != null && !TransitionManager.s_Instance.TransitionDone) || Singleton<UIResultsManager>.Instance.IsShown)
		{
			return;
		}
		if (difficultTerrain == null)
		{
			Log.GUIError("Trying to display hazardous terrain info but the difficult terrain object does not exist.");
			return;
		}
		string text = ((!string.IsNullOrEmpty(difficultTerrain.PropCustomLocKey)) ? difficultTerrain.PropCustomLocKey : difficultTerrain.PropHealthDetails?.CustomLocKey);
		string translation = LocalizationManager.GetTranslation((!text.IsNullOrEmpty()) ? text : "DIFFICULT_TERRAIN_TOOLTIP");
		if (difficultTerrain.PropTile.IsIce)
		{
			ShowDifficultTerrainIce();
		}
		else if (difficultTerrain.PropTile.TreatAsHazardous)
		{
			ShowDifficultTerrainHazardous();
		}
		else
		{
			ShowDifficultTerrain(translation);
		}
	}

	public void ShowDifficultTerrain(string text)
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			m_Title.text = text;
			_effects.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			_effects.Clear();
			CreateMovementEffect(2);
			m_Window.Show();
		}
	}

	private void CreateNegativeEffect(CCondition.ENegativeCondition condition)
	{
		UIStatusEffect component = Object.Instantiate(m_UIEffectPrefab, m_EffectsListContainer, worldPositionStays: false).GetComponent<UIStatusEffect>();
		_effects.Add(component);
		component.Initialize(condition);
	}

	private void CreatePositiveEffect(CCondition.EPositiveCondition condition)
	{
		UIStatusEffect component = Object.Instantiate(m_UIEffectPrefab, m_EffectsListContainer, worldPositionStays: false).GetComponent<UIStatusEffect>();
		_effects.Add(component);
		component.Initialize(condition);
	}

	private void CreateDamageEffect(int damageAmount)
	{
		UIStatusEffect component = Object.Instantiate(m_UIEffectPrefab, m_EffectsListContainer, worldPositionStays: false).GetComponent<UIStatusEffect>();
		_effects.Add(component);
		component.Initialize("", m_SpriteDamage, UIInfoTools.DefInstance.negativeTextColor, damageAmount.ToString());
	}

	private void CreateHealEffect(int healAmount)
	{
		UIStatusEffect component = Object.Instantiate(m_UIEffectPrefab, m_EffectsListContainer, worldPositionStays: false).GetComponent<UIStatusEffect>();
		_effects.Add(component);
		component.Initialize("", m_SpriteHeal, UIInfoTools.DefInstance.positiveTextColor, healAmount.ToString());
	}

	private void CreateMovementEffect(int moveAmount)
	{
		UIStatusEffect component = Object.Instantiate(m_UIEffectPrefab, m_EffectsListContainer, worldPositionStays: false).GetComponent<UIStatusEffect>();
		_effects.Add(component);
		component.Initialize("", m_SpriteMove, UIInfoTools.DefInstance.basicTextColor, "$MoveCost$: " + moveAmount);
	}

	private void CreateQuestItemEffect(string description)
	{
		UIStatusEffect component = Object.Instantiate(m_UIEffectPrefab, m_EffectsListContainer, worldPositionStays: false).GetComponent<UIStatusEffect>();
		_effects.Add(component);
		component.Initialize(description, UIInfoTools.Instance.CarryableQuestItemIconSprite, UIInfoTools.DefInstance.neutralActionTextColor);
	}

	private void CreateOwnerEffect(CActor owner, string origin = null)
	{
		if (owner.CharacterClass != null)
		{
			ECharacter character = CrossReference.ClassIdToECharacterModel(owner.CharacterClass.CharacterID);
			CharacterConfigUI characterConfigUI = UIInfoTools.Instance.GetCharacterConfigUI(character);
			if (!(characterConfigUI == null))
			{
				UIStatusEffect component = Object.Instantiate(m_UIEffectPrefab, m_EffectsListContainer, worldPositionStays: false).GetComponent<UIStatusEffect>();
				_effects.Add(component);
				component.Initialize(string.Empty, characterConfigUI.iconClass, UIInfoTools.DefInstance.neutralActionTextColor, "$GUI_PROP_CREATED_BY$ $" + owner.CharacterClass.LocKey + "$");
			}
		}
	}

	public void Hide()
	{
		m_Window.Hide();
	}
}
