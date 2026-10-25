using System;
using System.Collections;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class PhaseBannerHandler : Singleton<PhaseBannerHandler>
{
	private enum PhaseBanner
	{
		NONE,
		ENEMY_TURN,
		PLAYER_TURN,
		START_ROUND,
		END_ROUND,
		EXHAUSTED,
		DEATH,
		PRE_DEATH
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TextMeshProUGUI phaseText;

	[SerializeField]
	private Image interactionBlock;

	[SerializeField]
	private GUIAnimator showAnimation;

	[Header("Phase Colors")]
	[SerializeField]
	private Color roundColor;

	[SerializeField]
	private Color playerTurnColor;

	[SerializeField]
	private Color enemyTurnColor;

	[SerializeField]
	private Color dieColor;

	private string lastShownEnemyTurn;

	private Action onClosed;

	private PhaseBanner currentPhaseShown;

	private bool isEnabled;

	protected override void AwakeInit()
	{
		isEnabled = true;
		window.onHidden.AddListener(OnHidden);
		window.onShown.AddListener(() =>
		{
			Log.GUIInfo($"Banner: On window show {currentPhaseShown}. Is already playing " + showAnimation.IsPlaying);
			showAnimation.Stop();
			showAnimation.Play();
		});
		showAnimation.OnAnimationFinished.AddListener(() =>
		{
			Log.GUIInfo($"Banner: Finished show banner animation phase {currentPhaseShown}");
			window.Hide();
		});
		showAnimation.OnAnimationStopped.AddListener(() =>
		{
			Log.GUIInfo($"Banner: Stopped show banner animation {currentPhaseShown}");
		});
		showAnimation.OnAnimationStarted.AddListener(() =>
		{
			Log.GUIInfo($"Banner: Started show banner animation {currentPhaseShown}");
		});
	}

	private void SetData(string text, Color color, bool lockInteraction = false, Action onClosed = null)
	{
		phaseText.text = text;
		phaseText.color = color;
		this.onClosed = onClosed;
		interactionBlock.enabled = lockInteraction;
	}

	private void ShowPhase(string text, Color color, bool lockInteraction = false, Action onClosed = null)
	{
		if (!isEnabled)
		{
			Log.GUIInfo($"Banner: Skipped show phase {text} since it's disabled (phase {currentPhaseShown})");
			return;
		}
		if (window.IsOpen)
		{
			Log.GUIInfo($"Banner: Trigger on closed {text} since window is open (phase {currentPhaseShown})");
			onClosed?.Invoke();
		}
		SetData(text, color, lockInteraction, onClosed);
		Show();
	}

	private void Show()
	{
		if (!isEnabled)
		{
			Log.GUIInfo($"Banner: Skipped show since it's disabled (phase {currentPhaseShown})");
			return;
		}
		StopAllCoroutines();
		if (HasToWaitoToShow())
		{
			UIManager.Instance.StartCoroutine(WaitToShow());
			return;
		}
		Log.GUIInfo($"Banner: Show {currentPhaseShown} isOpen {window.IsOpen}");
		if (window.IsOpen)
		{
			showAnimation.Stop();
			showAnimation.Play();
		}
		window.Show();
	}

	public void ResetAndHide()
	{
		if (!isEnabled)
		{
			Log.GUIInfo($"Banner: Skipped reset and hide since it's disabled {currentPhaseShown}");
			return;
		}
		Log.GUIInfo($"Banner: ResetAndHide {currentPhaseShown} isOpen {window.IsOpen}");
		window.Hide();
	}

	private bool HasToWaitoToShow()
	{
		if (TransitionManager.s_Instance != null)
		{
			return !TransitionManager.s_Instance.TransitionDone;
		}
		return false;
	}

	private IEnumerator WaitToShow()
	{
		if (HasToWaitoToShow())
		{
			yield return new WaitUntil(() => TransitionManager.s_Instance.TransitionDone);
		}
		window.Show();
	}

	private void OnHidden()
	{
		interactionBlock.enabled = false;
		Log.GUIInfo($"Banner: Hidden banner phase {currentPhaseShown}");
		currentPhaseShown = PhaseBanner.NONE;
		StopAllCoroutines();
		showAnimation.Stop();
		Action action = onClosed;
		onClosed = null;
		action?.Invoke();
	}

	private void SetPhase(PhaseBanner phase)
	{
		currentPhaseShown = phase;
		if (phase == PhaseBanner.END_ROUND || phase == PhaseBanner.START_ROUND || phase == PhaseBanner.PLAYER_TURN)
		{
			lastShownEnemyTurn = null;
		}
	}

	public void Disable()
	{
		Log.GUIInfo($"Banner: Disable {currentPhaseShown} isOpen {window.IsOpen}");
		isEnabled = false;
	}

	public void ShowStartRound(int round, Action onClosed = null)
	{
		SetPhase(PhaseBanner.START_ROUND);
		ShowPhase(string.Format(LocalizationManager.GetTranslation("GUI_START_ROUND_BANNER"), round), roundColor, lockInteraction: true, onClosed);
	}

	public void ShowEnemyTurn(string enemyName, Action onClosed = null)
	{
		if (lastShownEnemyTurn == enemyName)
		{
			Log.GUIInfo("Banner: End immediately since it's the same enemy");
			onClosed?.Invoke();
		}
		else
		{
			lastShownEnemyTurn = enemyName;
			SetPhase(PhaseBanner.ENEMY_TURN);
			ShowPhase(string.Format(LocalizationManager.GetTranslation("GUI_COMBATLOG_START_TURN"), enemyName), enemyTurnColor, lockInteraction: true, onClosed);
		}
	}

	public void ShowPlayerTurn(string playerName, string subtext = null)
	{
		SetPhase(PhaseBanner.PLAYER_TURN);
		string text = string.Format(LocalizationManager.GetTranslation("GUI_COMBATLOG_START_TURN"), playerName);
		if (subtext.IsNOTNullOrEmpty())
		{
			text = text + "\n" + $"<size=24><font=\"FH_Sarala-Regular\"><color=#d0d0d0>{subtext}</color></font></size>";
		}
		ShowPhase(text, playerTurnColor);
	}

	public void ShowDie()
	{
		SetPhase(PhaseBanner.DEATH);
		Show();
	}

	public void ShowExhausted(string name, Action callback = null)
	{
		if (currentPhaseShown == PhaseBanner.PRE_DEATH)
		{
			SetData(string.Format(LocalizationManager.GetTranslation("GUI_PLAYER_EXHAUSTED_BANNER"), name), dieColor, lockInteraction: true, () =>
			{
				callback?.Invoke();
			});
		}
		else
		{
			ShowPhase(string.Format(LocalizationManager.GetTranslation("GUI_PLAYER_EXHAUSTED_BANNER"), name), dieColor, lockInteraction: true, callback);
		}
		SetPhase(PhaseBanner.EXHAUSTED);
	}

	public void ShowPreDie(string name, Action onClosed = null)
	{
		string translation = LocalizationManager.GetTranslation("GUI_PLAYER_EXHAUSTED_BANNER");
		string translation2 = LocalizationManager.GetTranslation(name);
		SetData(string.Format(translation, translation2), dieColor, lockInteraction: true, onClosed);
		SetPhase(PhaseBanner.PRE_DEATH);
	}

	public bool IsShowingDeath()
	{
		if (currentPhaseShown != PhaseBanner.DEATH && currentPhaseShown != PhaseBanner.EXHAUSTED)
		{
			return currentPhaseShown == PhaseBanner.PRE_DEATH;
		}
		return true;
	}
}
