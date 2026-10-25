using System;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIUseSlot<T> : MonoBehaviour, IUIUseSlot
{
	[SerializeField]
	protected ExtendedButton button;

	[SerializeField]
	private GameObject selectedMask;

	[SerializeField]
	protected GameObject optionalHiglight;

	[SerializeField]
	private GameObject mandatoryHiglight;

	[SerializeField]
	private float disabledAlpha = 0.25f;

	[SerializeField]
	private GUIAnimator showAnimation;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	protected string selectAudioItem = "PlaySound_UIButtonSelect";

	[SerializeField]
	protected string unselectAudioItem = "PlaySound_UIButtonSelect";

	protected bool selected;

	protected bool interactable;

	protected CActor actor;

	private Action<T> onSelect;

	private Action<T> onUnselect;

	private Func<T, bool> isMandatoryChecker;

	public T element { get; private set; }

	public Selectable GetSelectable()
	{
		return GetComponent<Selectable>();
	}

	protected virtual void Init(CActor actor, T element, Action<T> onSelect = null, Action<T> onUnselect = null, Func<T, bool> isMandatoryChecker = null, bool isSelected = false, bool isInteractable = true)
	{
		this.actor = actor;
		selected = isSelected;
		this.element = element;
		this.onSelect = onSelect;
		this.onUnselect = onUnselect;
		this.isMandatoryChecker = isMandatoryChecker;
		SetInteractable(isInteractable);
		Refresh();
	}

	public virtual void Show()
	{
		Show(instant: false);
	}

	public void Show(bool instant)
	{
		gameObject.SetActive(value: true);
		CancelAnimations();
		if (instant)
		{
			showAnimation.GoToFinishState();
		}
		else
		{
			showAnimation.Play();
		}
	}

	public virtual void Hide()
	{
		CancelAnimations();
		gameObject.SetActive(value: false);
	}

	public virtual void Select()
	{
		selected = true;
		Refresh();
		onSelect?.Invoke(element);
	}

	public virtual void Unselect(bool forceUnselect = false)
	{
		if (selected || forceUnselect)
		{
			AudioControllerUtils.PlaySound(unselectAudioItem);
			ClearSelection(fromClick: true, forceUnselect);
			onUnselect?.Invoke(element);
		}
	}

	public virtual void ClearSelection(bool fromClick = false, bool forceClear = false)
	{
		if (selected || forceClear)
		{
			selected = false;
			Refresh();
		}
	}

	public bool IsSelected()
	{
		return selected;
	}

	public virtual void Refresh()
	{
		if (selectedMask != null)
		{
			selectedMask.SetActive(selected);
		}
		bool flag = isMandatoryChecker != null && isMandatoryChecker(element);
		if (mandatoryHiglight != null)
		{
			mandatoryHiglight.SetActive(flag && interactable && !selected);
		}
		if (optionalHiglight != null)
		{
			optionalHiglight.SetActive(selected && interactable);
		}
	}

	public virtual void OnPointerDown()
	{
		if (selected)
		{
			Unselect();
			return;
		}
		AudioControllerUtils.PlaySound(selectAudioItem);
		Select();
	}

	protected virtual void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			CancelAnimations();
		}
	}

	protected virtual void CancelAnimations()
	{
		showAnimation.Stop();
		showAnimation.GoToFinishState();
	}

	public void SetInteractable(bool interactable)
	{
		this.interactable = interactable && (!FFSNetwork.IsOnline || (actor != null && Choreographer.s_Choreographer.ActorOrHisSummonerIsUnderMyControl(actor)));
		if (button != null)
		{
			button.interactable = this.interactable;
		}
		canvasGroup.alpha = (this.interactable ? 1f : disabledAlpha);
		Refresh();
	}

	private void Update()
	{
		CheckButtonInteractability();
	}

	private void CheckButtonInteractability()
	{
		bool flag = interactable;
		if (!GameState.ThreadIsSleeping && (ScenarioRuleClient.IsProcessingOrMessagesQueued || Choreographer.s_Choreographer.m_MessageQueue.Count > 0))
		{
			flag = false;
		}
		if (FFSNetwork.IsOnline && GHClientCallbacks.CurrentStreamMode != GHClientCallbacks.EStreamMode.NotSet)
		{
			flag = false;
		}
		if (button != null)
		{
			if (button.interactable == flag)
			{
				return;
			}
			button.interactable = flag;
		}
		canvasGroup.alpha = (flag ? 1f : disabledAlpha);
		Refresh();
	}
}
