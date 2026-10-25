using System;
using FHGUI;
using FHGUI.Common;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class MonsterCardSkin : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener _title;

	[SerializeField]
	private TextLocalizedListener _monsterName;

	[SerializeField]
	private TextMeshProUGUI _initiativeText;

	[SerializeField]
	private Image _monsterPortrait;

	[SerializeField]
	private MonsterUIDataSO _monsterDB;

	[SerializeField]
	private RectTransform _contentHolder;

	[SerializeField]
	private TMP_Text _faction;

	[SerializeField]
	private GameObject _factionContainer;

	private UIImageSettings _monsterPortraitDefaultPos;

	[field: SerializeField]
	public CActor.EType MonsterType { get; private set; }

	public RectTransform Content => _contentHolder;

	public Image MonsterPortrait => _monsterPortrait;

	private void Awake()
	{
		RectTransform rectTransform = _monsterPortrait.transform as RectTransform;
		_monsterPortraitDefaultPos = new UIImageSettings(rectTransform);
	}

	public void InitCardSkin(MonsterCardUIContext context)
	{
		CMonsterClass cMonsterClass = context.EnemyForBaseStats?.MonsterClass ?? context.FallbackMonsterClass;
		CActor.EType eType = context.EnemyForBaseStats?.Type ?? CActor.EType.Unknown;
		RectTransform rt = (RectTransform)_monsterPortrait.transform;
		if (cMonsterClass != null)
		{
			string modelName = cMonsterClass.Models[context.EnemyForBaseStats?.ChosenModelIndex ?? 0];
			MonsterConfigUI monsterConfigUI = _monsterDB.GetConfig(cMonsterClass.ID, modelName) ?? UIInfoTools.Instance.GetMonsterConfig(cMonsterClass.ID, modelName);
			_title.enabled = true;
			_monsterName.enabled = true;
			string textKey = cMonsterClass.LocKey;
			if (!string.IsNullOrEmpty(context.EnemyForBaseStats?.CustomLocKey))
			{
				textKey = context.EnemyForBaseStats?.CustomLocKey;
			}
			_monsterName.SetTextKey(textKey);
			if (monsterConfigUI != null)
			{
				_monsterPortrait.enabled = true;
				_monsterPortrait.sprite = monsterConfigUI.Portrait;
				monsterConfigUI.CardHeaderSettings.ApplyRectSettings(rt);
			}
			else
			{
				_monsterPortraitDefaultPos.ApplyRectSettings(rt);
				_monsterPortrait.enabled = false;
			}
			_factionContainer.SetActive(context.ShowFaction && (eType == CActor.EType.Enemy || eType == CActor.EType.Enemy2));
			_faction.text = GetFactionText(eType);
		}
		else
		{
			_title.enabled = false;
			_monsterName.enabled = false;
			_factionContainer.SetActive(value: false);
			_monsterPortraitDefaultPos.ApplyRectSettings(rt);
			_monsterPortrait.enabled = false;
		}
		_initiativeText.text = context.CardYML.Initiative.ToString();
		_title.SetTextKey(context.CardYML.Name);
	}

	private string GetFactionText(CActor.EType actorType)
	{
		return actorType switch
		{
			CActor.EType.Player => string.Empty, 
			CActor.EType.Enemy => "I", 
			CActor.EType.HeroSummon => string.Empty, 
			CActor.EType.Ally => string.Empty, 
			CActor.EType.Enemy2 => "II", 
			CActor.EType.Neutral => string.Empty, 
			CActor.EType.Unknown => "", 
			_ => throw new ArgumentOutOfRangeException("actorType", actorType, null), 
		};
	}
}
