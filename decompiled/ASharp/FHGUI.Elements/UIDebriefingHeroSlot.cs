using System.Collections;
using DG.Tweening;
using FFSNet;
using FHGUI.Common;
using FHGUI.DTO;
using GLOOM;
using Libs.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class UIDebriefingHeroSlot : MonoBehaviour
{
	public struct Params
	{
		public BattleResultDTO.HeroStats HeroStats;

		public NetworkPlayer NetworkPlayer;
	}

	[Header("Components")]
	[SerializeField]
	public CanvasGroup CanvasGroup;

	[SerializeField]
	public UITextTooltipTarget UserIconTooltip;

	[SerializeField]
	public TMP_Text HeroName;

	[Header("Badge")]
	[SerializeField]
	public GameObject BadgeContainer;

	[SerializeField]
	public Image BadgeIcon;

	[SerializeField]
	public TextLocalizedListener BadgeName;

	[Header("EXP")]
	[SerializeField]
	public GameObject XPContainer;

	[SerializeField]
	public TMP_Text HeroLevel;

	[SerializeField]
	public TMP_Text HeroXP;

	[SerializeField]
	public GameObject HeroLevelUpEffect;

	[SerializeField]
	public Image PreviousHeroXP;

	[SerializeField]
	public Image CurrentHeroXP;

	[Header("Multiplayer")]
	[SerializeField]
	public MultiplayerUserState MultiplayerUserState;

	[SerializeField]
	public TMP_Text MultiplayerUserVoteState;

	[Header("Tooltips")]
	[SerializeField]
	private UITextTooltipTarget m_TooltipTarget;

	public Params Data { get; private set; }

	public void Init(Params @params)
	{
		Data = @params;
		if ((bool)MultiplayerUserState)
		{
			bool flag = @params.NetworkPlayer != null && @params.HeroStats != null;
			MultiplayerUserState.gameObject.SetActive(flag);
			if (flag)
			{
				MultiplayerUserState.Show(@params.NetworkPlayer);
			}
		}
		if ((bool)UserIconTooltip)
		{
			UserIconTooltip.SetText((@params.NetworkPlayer != null) ? @params.NetworkPlayer.Username : string.Empty);
		}
		if ((bool)HeroName)
		{
			HeroName.gameObject.SetActive(@params.HeroStats != null);
			if (@params.HeroStats != null)
			{
				HeroName.text = @params.HeroStats.CharacterDTO.Name;
			}
		}
		UpdateBadge();
		UpdateXPBar();
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public void Show(float delay = 0f)
	{
		gameObject.SetActive(value: true);
		PlayShowEffect(delay);
	}

	private void PlayShowEffect(float delay)
	{
		float duration = 0.6f;
		CanvasGroup.alpha = 0f;
		CanvasGroup.DOFade(1f, duration).SetDelay(delay);
		RectTransform obj = CanvasGroup.transform as RectTransform;
		obj.pivot = new Vector2(0.69f, 0.5f);
		obj.DOPivot(new Vector2(0.5f, 0.5f), duration).SetDelay(delay);
		if (Data.HeroStats != null)
		{
			float num = 1.4f;
			float progress = Data.HeroStats.ProjectedLevelExperience.Progress;
			CurrentHeroXP.fillAmount = PreviousHeroXP.fillAmount;
			CurrentHeroXP.DOFillAmount(progress, num).SetDelay(delay);
			StartCoroutine(PlayLevelUpVFX(num + delay));
		}
	}

	private IEnumerator PlayLevelUpVFX(float delay)
	{
		if (Data.HeroStats.CharacterDTO.Level < Data.HeroStats.ProjectedLevel)
		{
			Scene sceneByName = SceneManager.GetSceneByName("FHPartyView");
			FHPartyViewScene partyView = sceneByName.GetComponent<FHPartyViewScene>(inChildren: true);
			if (partyView == null)
			{
				Log.Error("Cannot findFHPartyViewScene");
				yield break;
			}
			yield return new WaitForSeconds(delay);
			partyView.PlayVFX3D(Data.HeroStats.CharacterDTO.Class, "LevelUpQuestDebriefing");
			UIContextualTutorialModule.OnEvent("LevelUpDebriefing", gameObject);
		}
	}

	private void Update()
	{
		UpdatePlayerVoteState();
	}

	private void UpdatePlayerVoteState()
	{
		bool flag = Singleton<UIReadyToggle>.Instance.IsVisible && Data.NetworkPlayer != null && Data.HeroStats != null;
		MultiplayerUserVoteState.gameObject.SetActive(flag);
		if (flag)
		{
			string sourceText = (Singleton<UIReadyToggle>.Instance.PlayerReadiedState.ContainsKey(Data.NetworkPlayer) ? FHUILocFormatting.Color(LocalizationManager.GetTranslation("GUI_READY"), FHUIColors.VOTE_READY) : FHUILocFormatting.Color(LocalizationManager.GetTranslation("GUI_VOTE_PENDING"), FHUIColors.VOTE_PENDING));
			MultiplayerUserVoteState.SetText(sourceText);
		}
	}

	private void UpdateBadge()
	{
		bool flag = Data.HeroStats == null;
		BadgeContainer.SetActive(!flag);
		if (!flag)
		{
			if ((bool)BadgeIcon)
			{
				BadgeIcon.sprite = Data.HeroStats.Badge.Icon;
			}
			if ((bool)BadgeName)
			{
				BadgeName.SetTextKey(Data.HeroStats.Badge.NameKey);
			}
			if ((bool)m_TooltipTarget)
			{
				m_TooltipTarget.SetText(LocalizationManager.GetTranslation($"HeroPerformanceBadge_{Data.HeroStats.Badge.Category}_Tooltip"));
			}
		}
	}

	private void UpdateXPBar()
	{
		bool flag = Data.HeroStats == null;
		XPContainer.SetActive(!flag);
		if (!flag)
		{
			bool flag2 = Data.HeroStats.CharacterDTO.Level < Data.HeroStats.ProjectedLevel;
			if ((bool)HeroLevelUpEffect)
			{
				HeroLevelUpEffect.SetActive(flag2);
			}
			if ((bool)HeroLevel)
			{
				HeroLevel.text = Data.HeroStats.ProjectedLevel.ToString();
			}
			if ((bool)PreviousHeroXP)
			{
				float fillAmount = (flag2 ? 0f : Data.HeroStats.CharacterDTO.Experience.Progress);
				PreviousHeroXP.fillAmount = fillAmount;
			}
			if ((bool)CurrentHeroXP)
			{
				float progress = Data.HeroStats.ProjectedLevelExperience.Progress;
				CurrentHeroXP.fillAmount = progress;
			}
			if ((bool)HeroXP)
			{
				HeroXP.text = $"{Data.HeroStats.ProjectedLevelExperience.Value}/{Data.HeroStats.ProjectedLevelExperience.Max}";
			}
		}
	}
}
