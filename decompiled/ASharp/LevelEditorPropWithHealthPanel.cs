using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorPropWithHealthPanel : MonoBehaviour
{
	[Header("Prop Max Health")]
	public TMP_InputField PropHealthForPartySize1;

	public TMP_InputField PropHealthForPartySize2;

	public TMP_InputField PropHealthForPartySize3;

	public TMP_InputField PropHealthForPartySize4;

	[Space]
	public TMP_InputField PropObjectCustomLoc;

	[Space]
	public TMP_Dropdown PropActorTypeDropDown;

	public TMP_InputField PropActorClassField;

	public TMP_Dropdown PropDeathBehaviourDropDown;

	public Toggle PropIgnoredByAIFocusToggle;

	public Toggle PropIgnoredByHeroSummonFocusToggle;

	[Space]
	public TMP_Dropdown PropActorIconDropDown;

	public Image PropActorIcon;

	public GameObject PropIconSelectedObject;

	private CObjectProp m_PropCurrentlyDisplayed;

	public void SetPropDisplayed(CObjectProp propToDisplay)
	{
		m_PropCurrentlyDisplayed = propToDisplay;
		if (m_PropCurrentlyDisplayed == null)
		{
			return;
		}
		if (m_PropCurrentlyDisplayed.PropHealthDetails == null)
		{
			m_PropCurrentlyDisplayed.PropHealthDetails = new PropHealthDetails();
		}
		PropHealthForPartySize1.text = m_PropCurrentlyDisplayed.PropHealthDetails.MaxHealthPerPartySize[0].ToString();
		PropHealthForPartySize2.text = m_PropCurrentlyDisplayed.PropHealthDetails.MaxHealthPerPartySize[1].ToString();
		PropHealthForPartySize3.text = m_PropCurrentlyDisplayed.PropHealthDetails.MaxHealthPerPartySize[2].ToString();
		PropHealthForPartySize4.text = m_PropCurrentlyDisplayed.PropHealthDetails.MaxHealthPerPartySize[3].ToString();
		PropObjectCustomLoc.text = m_PropCurrentlyDisplayed.PropHealthDetails.CustomLocKey;
		PropActorTypeDropDown.ClearOptions();
		PropActorTypeDropDown.AddOptions(CActor.Types.Select((CActor.EType t) => t.ToString()).ToList());
		PropActorTypeDropDown.value = CActor.Types.IndexOf(m_PropCurrentlyDisplayed.PropHealthDetails.ActorType);
		PropActorClassField.text = m_PropCurrentlyDisplayed.PropHealthDetails.ActorClass;
		PropDeathBehaviourDropDown.ClearOptions();
		PropDeathBehaviourDropDown.AddOptions(PropHealthDetails.DeathActions.Select((PropHealthDetails.EDeathAction t) => t.ToString()).ToList());
		PropDeathBehaviourDropDown.value = PropHealthDetails.DeathActions.IndexOf(m_PropCurrentlyDisplayed.PropHealthDetails.DeathAction);
		PropIgnoredByAIFocusToggle.SetValue(m_PropCurrentlyDisplayed.PropHealthDetails.IgnoredByAIFocus);
		PropIgnoredByHeroSummonFocusToggle.SetValue(m_PropCurrentlyDisplayed.PropHealthDetails.IgnoredByHeroSummonFocus);
		PropActorIconDropDown.ClearOptions();
		PropActorIconDropDown.AddOptions(new List<string> { "NONE" });
		PropActorIconDropDown.value = 0;
		PropActorIconDropDown.AddOptions((from i in UIInfoTools.Instance.actorPortraits
			where i != null
			select i.name).ToList());
		if (!string.IsNullOrEmpty(m_PropCurrentlyDisplayed.PropHealthDetails.ActorSpriteName))
		{
			int num = UIInfoTools.Instance.actorPortraits.FindIndex((Sprite s) => s.name == m_PropCurrentlyDisplayed.PropHealthDetails.ActorSpriteName);
			if (num > -1)
			{
				PropActorIconDropDown.value = num + 1;
			}
		}
		OnPortraitSpriteDropDownChanged(PropActorIconDropDown.value);
	}

	public void ApplyToProp()
	{
		if (int.TryParse(PropHealthForPartySize1.text, out var result))
		{
			m_PropCurrentlyDisplayed.PropHealthDetails.MaxHealthPerPartySize[0] = result;
		}
		if (int.TryParse(PropHealthForPartySize2.text, out result))
		{
			m_PropCurrentlyDisplayed.PropHealthDetails.MaxHealthPerPartySize[1] = result;
		}
		if (int.TryParse(PropHealthForPartySize3.text, out result))
		{
			m_PropCurrentlyDisplayed.PropHealthDetails.MaxHealthPerPartySize[2] = result;
		}
		if (int.TryParse(PropHealthForPartySize4.text, out result))
		{
			m_PropCurrentlyDisplayed.PropHealthDetails.MaxHealthPerPartySize[3] = result;
		}
		m_PropCurrentlyDisplayed.PropHealthDetails.CustomLocKey = PropObjectCustomLoc.text;
		m_PropCurrentlyDisplayed.PropHealthDetails.ActorType = CActor.Types[PropActorTypeDropDown.value];
		m_PropCurrentlyDisplayed.PropHealthDetails.ActorClass = PropActorClassField.text;
		m_PropCurrentlyDisplayed.PropHealthDetails.DeathAction = PropHealthDetails.DeathActions[PropDeathBehaviourDropDown.value];
		if (!string.IsNullOrWhiteSpace(m_PropCurrentlyDisplayed.PropHealthDetails.ActorClass) && MonsterClassManager.MonsterAndObjectClasses.All((CMonsterClass c) => c.ID != m_PropCurrentlyDisplayed.PropHealthDetails.ActorClass))
		{
			Log.GameplayError("Trying to create prop with health using unknown actor class " + m_PropCurrentlyDisplayed.PropHealthDetails.ActorClass + "!");
		}
		m_PropCurrentlyDisplayed.PropHealthDetails.IgnoredByAIFocus = PropIgnoredByAIFocusToggle.isOn;
		m_PropCurrentlyDisplayed.PropHealthDetails.IgnoredByHeroSummonFocus = PropIgnoredByHeroSummonFocusToggle.isOn;
		if (PropActorIconDropDown.value > 0 && UIInfoTools.Instance.actorPortraits.Length > PropActorIconDropDown.value - 1)
		{
			Sprite sprite = UIInfoTools.Instance.actorPortraits[PropActorIconDropDown.value - 1];
			m_PropCurrentlyDisplayed.PropHealthDetails.ActorSpriteName = sprite.name;
		}
		else
		{
			m_PropCurrentlyDisplayed.PropHealthDetails.ActorSpriteName = string.Empty;
		}
	}

	public void OnPortraitSpriteDropDownChanged(int value)
	{
		if (value > 0 && UIInfoTools.Instance.actorPortraits.Length > value - 1)
		{
			PropIconSelectedObject.SetActive(value: false);
			PropActorIcon.gameObject.SetActive(value: true);
			PropActorIcon.sprite = UIInfoTools.Instance.actorPortraits[value - 1];
		}
		else
		{
			PropIconSelectedObject.SetActive(value: true);
			PropActorIcon.gameObject.SetActive(value: false);
			PropActorIcon.sprite = null;
		}
	}
}
