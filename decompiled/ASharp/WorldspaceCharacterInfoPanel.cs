using DG.Tweening;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;

public class WorldspaceCharacterInfoPanel : MonoBehaviour
{
	public TMP_Text CharacterNameText;

	[HideInInspector]
	public ActorBehaviour m_ActorBehavior;

	[Header("Notifications")]
	[SerializeField]
	private Vector3 m_NotificationEndPosition;

	[SerializeField]
	private float m_NotificationDuration;

	[SerializeField]
	private float m_NotificationFadeDuration;

	[SerializeField]
	private TMP_Text m_PositiveNotifier;

	[SerializeField]
	private TMP_Text m_NegativeNotifer;

	private bool? m_IsPlayingPositive = false;

	private bool? m_IsPlayingNegative = false;

	public void Init(ActorBehaviour actorBehaviour)
	{
		m_ActorBehavior = actorBehaviour;
		CActor actor = m_ActorBehavior.Actor;
		if (actor.IsEnemyByDefault())
		{
			CharacterNameText.text = LocalizationManager.GetTranslation(actor.ActorLocKey()) + " " + actor.ID;
		}
		else
		{
			CharacterNameText.text = LocalizationManager.GetTranslation(actor.ActorLocKey());
			CharacterNameText.color = UIInfoTools.Instance.Orange;
		}
		ShowCharacterName(show: false);
	}

	public void ShowCharacterName(bool show)
	{
		CharacterNameText.enabled = show;
	}

	public void NotifyHPIncrease(int amount)
	{
		if (m_IsPlayingPositive.Value)
		{
			Log.GUIInfo("PositiveNotifer Animation in progress");
			return;
		}
		m_PositiveNotifier.text = $"+{amount}";
		PlayHPNotifier(m_PositiveNotifier, m_IsPlayingPositive);
	}

	public void NotifyHPDecrease(int amount)
	{
		if (m_IsPlayingNegative.Value)
		{
			Log.GUIInfo("NegativeNotifier Animation in progress");
			return;
		}
		m_NegativeNotifer.text = $"-{Mathf.Abs(amount)}";
		PlayHPNotifier(m_NegativeNotifer, m_IsPlayingNegative);
	}

	private void PlayHPNotifier(TMP_Text notifier, bool? isPlayingFlagRef)
	{
		Transform transformCached = notifier.transform;
		transformCached.localPosition = Vector3.zero;
		notifier.alpha = 1f;
		isPlayingFlagRef = true;
		Vector3 originalScale = transformCached.localScale;
		Color originalColor = notifier.color;
		notifier.transform.DOLocalMove(m_NotificationEndPosition, m_NotificationDuration + m_NotificationFadeDuration).SetEase(Ease.OutQuad);
		DOTween.To((float value) =>
		{
			transformCached.localScale = originalScale * value;
		}, 1f, 1.2f, m_NotificationDuration).SetTarget(gameObject).SetEase(Ease.OutQuad);
		DOTween.To((float value) =>
		{
			notifier.color = Color.Lerp(Color.white, originalColor, value);
		}, 0f, 1f, m_NotificationDuration).SetTarget(gameObject).SetEase(Ease.OutQuad);
		DOTween.To((float value) =>
		{
			transformCached.localScale = originalScale * value;
		}, 1.2f, 0.5f, m_NotificationFadeDuration).SetTarget(gameObject).SetDelay(m_NotificationDuration)
			.SetEase(Ease.OutQuad);
		DOTween.To((float value) =>
		{
			notifier.alpha = value;
		}, 1f, 0f, m_NotificationFadeDuration).SetDelay(m_NotificationDuration).SetEase(Ease.OutQuad)
			.OnComplete(() =>
			{
				transformCached.localScale = originalScale;
				isPlayingFlagRef = false;
			});
	}
}
