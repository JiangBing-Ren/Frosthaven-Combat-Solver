using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GLOOM;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniAbilityCard : MonoBehaviour
{
	[Serializable]
	public struct StateDependantObjects
	{
		public CardPileType Type;

		public GameObject[] Objects;
	}

	[Serializable]
	public struct ActiveBonusSprite
	{
		public CActiveBonus.EActiveBonusDurationType Type;

		public Sprite Sprite;
	}

	private enum SelectionMode
	{
		Skin,
		Glow
	}

	[Header("Internal references")]
	[SerializeField]
	public TextMeshProUGUI initiativeText;

	[SerializeField]
	public TextMeshProUGUI initiativeTextAlt;

	[SerializeField]
	public Image initiativeBackground;

	[SerializeField]
	public TextMeshProUGUI titleText;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private GameObject activeBonusContainer;

	[SerializeField]
	private TextMeshProUGUI activeBonusRemainingText;

	[SerializeField]
	private Image activeBonusDurationImage;

	[SerializeField]
	private ActiveBonusSprite[] activeBonusSprites;

	[SerializeField]
	private Image miniAbilityBackgroundImage;

	[SerializeField]
	private Image highlightImage;

	[SerializeField]
	private GUIAnimator warningAnimation;

	[SerializeField]
	private Image glowImage;

	[SerializeField]
	private float glowAnimationDuration = 1f;

	[SerializeField]
	private Color defaultGlowColor = new Color32(143, 58, 44, byte.MaxValue);

	[SerializeField]
	private Color burnGlowColorFrom;

	[SerializeField]
	private Color burnGlowColorTo;

	[SerializeField]
	private Material m_InactiveMaterial;

	[SerializeField]
	private Material m_TakenMaterial;

	[SerializeField]
	private Image[] m_InactiveImages;

	[SerializeField]
	private Image[] m_HoverImages;

	[SerializeField]
	private StateDependantObjects[] m_StateDependantObjects;

	[SerializeField]
	private MiniCardEffects effects;

	public RectTransform HighlightRect;

	[SerializeField]
	private List<MiniAbilityCardIndicator> indicators;

	private AbilityCardUISkin cardSkin;

	private MiniAbilityCardIndicator currentIndicator;

	[SerializeField]
	private bool isLongRest;

	[SerializeField]
	private GameObject unfocusedMask;

	[SerializeField]
	private GameObject loseIndicator;

	private SelectionMode selectionMode;

	private Tween colorGlowAnim;

	private Action onGlow;

	private Color? _activeBonusStartingColor;

	private CardPileType _currentDisplayType = CardPileType.None;

	private Tween lostAnimation;

	private void Awake()
	{
		Clear();
	}

	private void OnDestroy()
	{
		cardSkin = null;
		onGlow = null;
	}

	private void CancelGlowAnimation()
	{
		if (colorGlowAnim != null)
		{
			colorGlowAnim.Kill();
		}
		colorGlowAnim = null;
	}

	public void EnableGlowSelected(Color? glowColor = null, Action onGlow = null)
	{
		CancelGlowAnimation();
		selectionMode = SelectionMode.Glow;
		glowImage.color = glowColor ?? defaultGlowColor;
		glowImage.enabled = false;
		glowImage.gameObject.SetActive(value: true);
		this.onGlow = onGlow;
	}

	public void EnableGlowBurnSelected()
	{
		EnableGlowSelected(burnGlowColorFrom, () =>
		{
			CancelGlowAnimation();
			glowImage.color = burnGlowColorFrom;
			colorGlowAnim = glowImage.DOColor(burnGlowColorTo, glowAnimationDuration).SetLoops(-1, LoopType.Yoyo);
		});
	}

	public void DisableGlowSelected()
	{
		onGlow = null;
		CancelGlowAnimation();
		selectionMode = SelectionMode.Skin;
		glowImage.enabled = false;
		glowImage.gameObject.SetActive(value: false);
	}

	public void SetDisplayMode(CardPileType type, CAbilityCard abilityCard, bool isHighlighted, bool isHovered = false, int selectedIndex = -1)
	{
		CancelGlowAnimation();
		_currentDisplayType = type;
		if (cardSkin != null)
		{
			miniAbilityBackgroundImage.sprite = cardSkin.GetPreviewBackground(type, isHighlighted, isLongRest);
			if (!isLongRest)
			{
				Color previewTextColor = cardSkin.GetPreviewTextColor(type, isHighlighted);
				titleText.color = previewTextColor;
				initiativeText.color = previewTextColor;
			}
			if (initiativeBackground != null && cardSkin.initiativeBackground != null)
			{
				initiativeBackground.sprite = cardSkin.initiativeBackground;
			}
		}
		bool flag = abilityCard != null && abilityCard.ActiveBonuses.Count > 0;
		if (iconImage != null)
		{
			iconImage.enabled = flag;
		}
		if (selectedIndex > -1)
		{
			ShowSelectedNumber(selectedIndex + 1);
		}
		else if (flag)
		{
			CActiveBonus activeBonus = abilityCard.ActiveBonuses[0];
			int remaining = activeBonus.Remaining;
			bool flag2 = remaining > 0;
			Sprite sprite = ((!flag2) ? activeBonusSprites.FirstOrDefault((ActiveBonusSprite abs) => abs.Type == activeBonus.Duration).Sprite : null);
			SetActiveChargesParams(isActive: true, _activeBonusStartingColor, flag2 ? remaining.ToString() : string.Empty, sprite);
			if (iconImage != null)
			{
				if (activeBonus.Layout == null)
				{
					UIActiveAbility.CheckForLayoutToGenerate(activeBonus);
				}
				iconImage.sprite = UIInfoTools.Instance.GetActiveAbilityIcon(activeBonus);
				iconImage.enabled = iconImage.sprite != null;
			}
		}
		else
		{
			SetActiveChargesParams(isActive: false);
		}
		bool flag3 = isHovered || type == CardPileType.Round;
		currentIndicator?.Highlight(flag3);
		if (selectionMode == SelectionMode.Glow)
		{
			glowImage.enabled = flag3;
			if (glowImage.enabled)
			{
				onGlow?.Invoke();
			}
		}
		else if (effects != null)
		{
			switch (type)
			{
			case CardPileType.Active:
				effects.RestoreCard();
				break;
			case CardPileType.Lost:
				effects.ToggleEffect(active: false, MiniCardEffects.FXTask.LostMode);
				break;
			case CardPileType.Discarded:
				effects.ToggleEffect(active: false, MiniCardEffects.FXTask.DiscardMode);
				break;
			case CardPileType.Round:
				effects.RestoreCard();
				break;
			case CardPileType.Permalost:
				effects.ToggleEffect(active: false, MiniCardEffects.FXTask.LostMode);
				break;
			default:
				effects.RestoreCard();
				break;
			}
		}
		Image[] hoverImages = m_HoverImages;
		for (int num = 0; num < hoverImages.Length; num++)
		{
			hoverImages[num].enabled = isHovered;
		}
		Material material = null;
		switch (type)
		{
		case CardPileType.Discarded:
		case CardPileType.Lost:
		case CardPileType.Permalost:
			material = m_InactiveMaterial;
			break;
		case CardPileType.Taken:
			material = m_TakenMaterial;
			break;
		default:
			material = null;
			break;
		}
		hoverImages = m_InactiveImages;
		for (int num = 0; num < hoverImages.Length; num++)
		{
			hoverImages[num].material = material;
		}
		Dictionary<GameObject, bool> dictionary = new Dictionary<GameObject, bool>();
		StateDependantObjects[] stateDependantObjects = m_StateDependantObjects;
		for (int num = 0; num < stateDependantObjects.Length; num++)
		{
			StateDependantObjects stateDependantObjects2 = stateDependantObjects[num];
			bool value = stateDependantObjects2.Type == type;
			GameObject[] objects = stateDependantObjects2.Objects;
			foreach (GameObject key in objects)
			{
				if (!(dictionary.TryGetValue(key, out var value2) & value2))
				{
					dictionary[key] = value;
				}
			}
		}
		foreach (KeyValuePair<GameObject, bool> item in dictionary)
		{
			item.Key.SetActive(item.Value);
		}
	}

	public void MakeMiniCard(string cardName, AbilityCardYMLData cardData, bool isLongRest = false)
	{
		this.isLongRest = isLongRest;
		SetCardName(cardName);
		SetInitiativeText(cardData.Initiative.ToString());
	}

	public void SetInitiativeText(string text)
	{
		if (initiativeText != null)
		{
			initiativeText.text = text;
		}
		if (initiativeTextAlt != null)
		{
			initiativeTextAlt.text = text;
		}
	}

	public void SetSkin(AbilityCardUISkin skin)
	{
		cardSkin = skin;
	}

	public MiniAbilityCard GenerateCopy(Transform parent)
	{
		MiniAbilityCard miniAbilityCard = UnityEngine.Object.Instantiate(this, parent);
		miniAbilityCard.SetSkin(cardSkin);
		miniAbilityCard.ShowWarning(show: false);
		return miniAbilityCard;
	}

	public IEnumerator LostAnimation(bool active, float cardHighlightTime)
	{
		if (highlightImage == null)
		{
			yield break;
		}
		if (lostAnimation != null)
		{
			lostAnimation.Kill();
			lostAnimation = null;
		}
		if (active)
		{
			highlightImage.enabled = true;
			lostAnimation = highlightImage.DOFade(1f, cardHighlightTime).SetUpdate(isIndependentUpdate: true).OnComplete(() =>
			{
				lostAnimation = null;
			});
			yield break;
		}
		lostAnimation = highlightImage.DOFade(0f, cardHighlightTime).SetUpdate(isIndependentUpdate: true).OnComplete(() =>
		{
			lostAnimation = null;
		});
		yield return new WaitForSecondsRealtime(cardHighlightTime);
		highlightImage.enabled = false;
	}

	public void CancelLostAnimation()
	{
		if (!(highlightImage == null))
		{
			if (lostAnimation != null)
			{
				lostAnimation.Kill();
				lostAnimation = null;
			}
			StopAllCoroutines();
			highlightImage.enabled = false;
			Color color = highlightImage.color;
			color.a = 0f;
			highlightImage.color = color;
		}
	}

	public void DisplayIndicator(bool active, IndicatorType indicatorType = IndicatorType.General)
	{
		if (active)
		{
			if (currentIndicator == null || currentIndicator.type != indicatorType)
			{
				if (currentIndicator != null)
				{
					currentIndicator.Hide();
				}
				currentIndicator = indicators.FirstOrDefault((MiniAbilityCardIndicator it) => it.type == indicatorType);
			}
			currentIndicator?.Show();
			if (!isLongRest)
			{
				GetComponent<RectTransform>().anchoredPosition = new Vector3(currentIndicator.offset, 0f, 0f);
			}
		}
		else
		{
			if (currentIndicator != null)
			{
				currentIndicator.Hide();
			}
			currentIndicator = null;
			if (!isLongRest)
			{
				GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);
			}
		}
	}

	public void SetCardName(string cardName)
	{
		titleText.text = LocalizationManager.GetTranslation(cardName);
	}

	public void ShowWarning(bool show)
	{
		if (show)
		{
			warningAnimation?.Play();
		}
		else
		{
			warningAnimation?.Stop();
		}
	}

	private void OnDisable()
	{
		CancelGlowAnimation();
		ShowWarning(show: false);
	}

	public void Clear()
	{
		ShowWarning(show: false);
		if (effects != null)
		{
			effects.RestoreCard();
		}
		DisplayIndicator(active: false);
		DisableGlowSelected();
	}

	public void ShowSelectedNumber(int number)
	{
		SetActiveChargesParams(isActive: true, cardSkin?.GetPreviewTextColor(_currentDisplayType, isHighlighted: false), GetSelectedText(number - 1));
	}

	private void SetActiveChargesParams(bool isActive, Color? textColor = null, string text = null, Sprite sprite = null)
	{
		if (activeBonusContainer == null)
		{
			return;
		}
		if (isActive)
		{
			isActive = sprite != null || text.IsNOTNullOrEmpty();
		}
		activeBonusContainer.SetActive(isActive);
		if (isActive)
		{
			Color valueOrDefault = _activeBonusStartingColor.GetValueOrDefault();
			if (!_activeBonusStartingColor.HasValue)
			{
				valueOrDefault = activeBonusRemainingText.color;
				_activeBonusStartingColor = valueOrDefault;
			}
			if (textColor.HasValue)
			{
				activeBonusRemainingText.color = textColor.Value;
			}
			activeBonusRemainingText.text = text;
			activeBonusDurationImage.sprite = sprite;
			activeBonusDurationImage.enabled = sprite != null;
		}
	}

	private string GetSelectedText(int index)
	{
		return index switch
		{
			0 => "I", 
			1 => "II", 
			2 => "III", 
			3 => "IV", 
			_ => index.ToString(), 
		};
	}

	public void SetUnfocused(bool unfocused)
	{
		if (unfocusedMask != null)
		{
			unfocusedMask.SetActive(unfocused);
		}
	}

	public void SetLoseIndicatorVisibility(bool isVisible)
	{
		if (loseIndicator != null)
		{
			loseIndicator.SetActive(isVisible);
		}
	}
}
