using System;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSymbolTab : MonoBehaviour
{
	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	public TextMeshProUGUI selectedNumberText;

	[SerializeField]
	private InteractabilityIsolatedUIControl m_IsolatedControl;

	[SerializeField]
	private Image unselectedImage;

	[SerializeField]
	private Image selectedImage;

	[SerializeField]
	private GameObject exhaustedMask;

	[SerializeField]
	private float exhaustedOppacity = 0.6f;

	[SerializeField]
	private CanvasGroup tabCanvasGroup;

	[Header("Multiplayer")]
	[SerializeField]
	private Color otherPlayerIconColor;

	[SerializeField]
	private Image highlightImage;

	[SerializeField]
	private float highlightScale = 1.2f;

	[SerializeField]
	private float highlightAlpha = 0.2f;

	private bool selectFromOutside;

	private int selectedNumber;

	private CPlayerActor playerActor;

	private Action<CPlayerActor> onClickAction;

	public CPlayerActor PlayerActor => playerActor;

	public bool IsSelected => toggle.isOn;

	public void Init(ToggleGroup toggleGroup, CPlayerActor playerActor, Action<CPlayerActor> onClickAction)
	{
		this.playerActor = playerActor;
		m_IsolatedControl.ControlSecondIdentifier = playerActor.ActorGuid;
		CharacterTabSkin characterTabSkin = UIInfoTools.Instance.GetCharacterTabSkin(playerActor.CharacterClass.DefaultModel, playerActor.CharacterClass.CharacterYML.CustomCharacterConfig);
		unselectedImage.sprite = characterTabSkin.normalIcon;
		selectedImage.sprite = characterTabSkin.selectedIcon;
		toggle.group = toggleGroup;
		this.onClickAction = onClickAction;
		UpdateSelectedCardsNum(0);
		RefreshPlayerOnlineState();
		RefreshDead();
	}

	public void RefreshPlayerOnlineState()
	{
		Image image = unselectedImage;
		Color color = (selectedImage.color = ((!FFSNetwork.IsOnline || playerActor.IsUnderMyControl()) ? UIInfoTools.Instance.White : otherPlayerIconColor));
		image.color = color;
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		if (FFSNetwork.IsOnline && playerActor.IsUnderMyControl() && !toggle.isOn && !playerActor.IsDead && playerActor.SelectedRoundCardsCount > selectedNumber)
		{
			highlightImage.gameObject.SetActive(value: true);
		}
		else
		{
			highlightImage.gameObject.SetActive(value: false);
		}
	}

	public void UpdateSelectedCardsNum(int number)
	{
		selectedNumber = number;
		if (PlayerActor.CharacterClass.ShortRest)
		{
			selectedNumberText.text = "-";
		}
		else if (PlayerActor.CharacterClass.LongRest)
		{
			selectedNumberText.text = PlayerActor.Initiative().ToString();
		}
		else
		{
			selectedNumberText.text = $"{number}/{playerActor.MaxRoundCardsCount}";
		}
		RefreshHighlight();
	}

	public void Select()
	{
		selectFromOutside = true;
		toggle.isOn = true;
	}

	public void Click()
	{
		toggle.isOn = true;
	}

	public void OnClick(bool isOn)
	{
		if (isOn && !selectFromOutside)
		{
			onClickAction(playerActor);
		}
		if (selectFromOutside)
		{
			selectFromOutside = false;
		}
		RefreshHighlight();
	}

	public void SetInteractable(bool interactable)
	{
		Log.NetworkInfo((interactable ? "Enabling" : "Disabling") + " card hand tab for " + playerActor.CharacterClass.ID);
		toggle.interactable = interactable;
	}

	public void SetHighlightProgress(float percent)
	{
		if (!playerActor.IsDead)
		{
			highlightImage.SetAlpha((1f - percent) * highlightAlpha);
			float num = (highlightScale - 1f) * percent + 1f;
			highlightImage.transform.localScale = new Vector3(num, num, 1f);
		}
	}

	public void RefreshDead()
	{
		RefreshHighlight();
		Image image = unselectedImage;
		Material material = (selectedImage.material = (playerActor.IsDead ? UIInfoTools.Instance.greyedOutMaterial : null));
		image.material = material;
		exhaustedMask.SetActive(playerActor.IsDead);
		selectedNumberText.gameObject.SetActive(!playerActor.IsDead);
		tabCanvasGroup.alpha = (playerActor.IsDead ? exhaustedOppacity : 1f);
	}
}
