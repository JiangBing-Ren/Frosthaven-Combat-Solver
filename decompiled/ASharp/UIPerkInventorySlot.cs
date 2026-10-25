using System;
using System.Collections.Generic;
using System.Linq;
using GLOOM;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPerkInventorySlot : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Button button;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private Transform holderPoints;

	[SerializeField]
	private UIPerkCheckbox pointPrefab;

	[SerializeField]
	private UIPerkModifiersTooltip tooltip;

	[SerializeField]
	private float alphaInactive = 0.4f;

	[SerializeField]
	private Color colorTitleActive;

	[Header("Hover")]
	[SerializeField]
	private float hoverFadeDuration = 0.3f;

	[SerializeField]
	protected Image background;

	private List<UIPerkCheckbox> pointsPool = new List<UIPerkCheckbox>();

	private List<CharacterPerk> perks;

	private Action<bool, UIPerkInventorySlot> onHoverCallback;

	private Action<UIPerkInventorySlot> onSelectedCallback;

	private UIPerkCheckbox previewedCheck;

	private Color defaultColor;

	protected ICharacterService character;

	public CharacterPerk Perk => perks[0];

	public int TotalCounters => perks.Count;

	public int ActiveCounters => perks.Count((CharacterPerk it) => it.IsActive);

	protected virtual void Awake()
	{
		defaultColor = title.color;
		button.onClick.AddListener(Select);
	}

	public virtual void Init(ICharacterService character, List<CharacterPerk> perks, Action<bool, UIPerkInventorySlot> onHoverCallback, Action<UIPerkInventorySlot> onSelectedCallback, bool isInteractable = true)
	{
		this.character = character;
		this.perks = perks;
		this.onHoverCallback = onHoverCallback;
		this.onSelectedCallback = onSelectedCallback;
		title.text = LocalizationManager.GetTranslation(perks[0].Perk.Name);
		description.text = LocalizationManager.GetTranslation(perks[0].Perk.Description);
		tooltip.Initialize(perks[0]);
		background.sprite = UIInfoTools.Instance.GetNewAdventureCharacterPortrait(character.CharacterModel, highlighted: true, character.Class.CustomCharacterConfig);
		HelperTools.NormalizePool(ref pointsPool, pointPrefab.gameObject, holderPoints, perks.Count);
		RefreshCounters();
		PreviewSelect(preview: false);
		if (FFSNetwork.IsOnline)
		{
			button.interactable = character.IsMapCharacterUnderMyControl & isInteractable;
		}
		else
		{
			button.interactable = isInteractable;
		}
	}

	public virtual void OnHover(bool hovered)
	{
		onHoverCallback(hovered, this);
		PreviewSelect(hovered);
	}

	protected virtual void PreviewSelect(bool preview)
	{
		if (preview)
		{
			previewedCheck = pointsPool.Take(perks.Count).FirstOrDefault((UIPerkCheckbox it) => !it.isOn);
			if (previewedCheck != null)
			{
				previewedCheck.Highlight(highlight: true);
			}
		}
		else if (previewedCheck != null)
		{
			previewedCheck.Highlight(highlight: false);
			previewedCheck = null;
		}
	}

	private void Select()
	{
		if (perks.FirstOrDefault((CharacterPerk it) => !it.IsActive) != null)
		{
			onSelectedCallback(this);
		}
		else
		{
			AudioControllerUtils.PlaySound(UIInfoTools.Instance.InvalidOptionAudioItem);
		}
	}

	public virtual void RefreshCounters()
	{
		bool flag = character.PerkPoints > 0;
		for (int i = 0; i < perks.Count; i++)
		{
			pointsPool[i].isOn = perks[i].IsActive;
			flag |= perks[i].IsActive;
		}
		canvasGroup.alpha = (flag ? 1f : alphaInactive);
		title.color = (flag ? colorTitleActive : defaultColor);
		if (previewedCheck != null)
		{
			PreviewSelect(preview: true);
		}
	}
}
