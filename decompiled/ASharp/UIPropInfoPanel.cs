using System.Collections.Generic;
using System.Linq;
using AStar;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIPropInfoPanel : Singleton<UIPropInfoPanel>
{
	public enum EPropType
	{
		None,
		Trap,
		QuestItem,
		HazardousTerrain,
		DifficultTerrain
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TMP_Text propName;

	[SerializeField]
	private Transform conditionsHolder;

	[SerializeField]
	private AutoScrollRect conditionsScroll;

	[SerializeField]
	private GameObject uiConditionsPrefab;

	[SerializeField]
	private Sprite damageSprite;

	[SerializeField]
	private Sprite healSprite;

	[SerializeField]
	private Sprite moveSprite;

	private EPropType propType;

	private List<UIStatusEffect> conditions = new List<UIStatusEffect>();

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
			propType = EPropType.Trap;
			propName.text = LocalizationManager.GetTranslation(cObjectTrap.PrefabName);
			conditions.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			conditions.Clear();
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
			if (cObjectTrap.Damage || cObjectTrap.HealValue > 0 || (cObjectTrap.Conditions != null && cObjectTrap.Conditions.Count > 0) || (cObjectTrap.PositiveConditions != null && cObjectTrap.PositiveConditions.Count > 0))
			{
				conditionsScroll.StartAutoscroll();
			}
			else
			{
				conditionsScroll.StopAutoscroll();
			}
			window.Show();
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
				propType = EPropType.HazardousTerrain;
				propName.text = LocalizationManager.GetTranslation("HAZARDOUS_TERRAIN_TOOLTIP");
				conditions.ForEach((UIStatusEffect x) =>
				{
					Object.Destroy(x.gameObject);
				});
				conditions.Clear();
				CreateDamageEffect(ScenarioManager.HazardousTerrainDamage);
				if (cObjectHazardousTerrain.Owner != null)
				{
					CreateOwnerEffect(cObjectHazardousTerrain.Owner);
				}
				window.Show();
			}
		}
	}

	private void ShowDifficultTerrainHazardous()
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			propType = EPropType.HazardousTerrain;
			propName.text = LocalizationManager.GetTranslation("HAZARDOUS_DIFFICULT_TERRAIN_TOOLTIP");
			conditions.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			conditions.Clear();
			CreateDamageEffect(ScenarioManager.HazardousTerrainDamage);
			CreateMovementEffect(2);
			window.Show();
		}
	}

	private void ShowDifficultTerrainIce()
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			propType = EPropType.DifficultTerrain;
			propName.text = LocalizationManager.GetTranslation("FH/ICE_TERRAIN_TOOLTIP");
			conditions.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			conditions.Clear();
			CreateMovementEffect(2);
			window.Show();
		}
	}

	public void ShowQuestItem(string title, string description)
	{
		if ((!(TransitionManager.s_Instance != null) || TransitionManager.s_Instance.TransitionDone) && !Singleton<UIResultsManager>.Instance.IsShown)
		{
			propType = EPropType.QuestItem;
			propName.text = title;
			conditions.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			conditions.Clear();
			CreateQuestItemEffect(description);
			window.Show();
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
			propType = EPropType.DifficultTerrain;
			propName.text = text;
			conditions.ForEach((UIStatusEffect x) =>
			{
				Object.Destroy(x.gameObject);
			});
			conditions.Clear();
			CreateMovementEffect(2);
			window.Show();
		}
	}

	private void CreateNegativeEffect(CCondition.ENegativeCondition condition)
	{
		UIStatusEffect component = Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize(condition);
	}

	private void CreatePositiveEffect(CCondition.EPositiveCondition condition)
	{
		UIStatusEffect component = Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize(condition);
	}

	private void CreateDamageEffect(int damageAmount)
	{
		UIStatusEffect component = Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize("", damageSprite, UIInfoTools.DefInstance.negativeTextColor, "$Damage$: " + damageAmount);
	}

	private void CreateHealEffect(int healAmount)
	{
		UIStatusEffect component = Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize("", healSprite, UIInfoTools.DefInstance.positiveTextColor, "$Heal$: " + healAmount);
	}

	private void CreateMovementEffect(int moveAmount)
	{
		UIStatusEffect component = Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize("", moveSprite, UIInfoTools.DefInstance.basicTextColor, "$MoveCost$: " + moveAmount);
	}

	private void CreateQuestItemEffect(string description)
	{
		UIStatusEffect component = Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
		conditions.Add(component);
		component.Initialize(description, UIInfoTools.Instance.CarryableQuestItemIconSprite, UIInfoTools.DefInstance.neutralActionTextColor);
	}

	private void CreateOwnerEffect(CActor owner)
	{
		if (owner.CharacterClass != null)
		{
			ECharacter character = CrossReference.ClassIdToECharacterModel(owner.CharacterClass.CharacterID);
			CharacterConfigUI characterConfigUI = UIInfoTools.Instance.GetCharacterConfigUI(character);
			if (!(characterConfigUI == null))
			{
				UIStatusEffect component = Object.Instantiate(uiConditionsPrefab, conditionsHolder, worldPositionStays: false).GetComponent<UIStatusEffect>();
				conditions.Add(component);
				component.Initialize(string.Empty, characterConfigUI.iconClass, UIInfoTools.DefInstance.neutralActionTextColor, "$FH/GUI_PROP_CREATED_BY$ $" + owner.CharacterClass.LocKey + "$");
			}
		}
	}

	public void Hide()
	{
		propType = EPropType.None;
		conditionsScroll.StopAutoscroll();
		window.Hide();
	}

	public void Hide(params EPropType[] types)
	{
		if (types.Contains(propType))
		{
			Hide();
		}
	}
}
