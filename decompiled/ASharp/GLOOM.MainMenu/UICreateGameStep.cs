using System;
using System.Threading.Tasks;
using Assets.Script.Misc;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UICreateGameStep : MonoBehaviour
{
	[SerializeField]
	protected UIWindow m_Window;

	[SerializeField]
	protected Button confirmButton;

	[SerializeField]
	protected Button cancelButton;

	protected Action m_OnConfirmed;

	protected Action m_OnCancelled;

	protected GameData m_Data;

	protected bool m_IsOpen;

	protected bool m_Initialized;

	protected virtual void InitCache()
	{
		if (!m_Initialized)
		{
			confirmButton.onClick.AddListener(Confirm);
			confirmButton.gameObject.AddComponent<UINonInteractableEvents>().NonInteractableClick.AddListener(ConfirmRejected);
			if (cancelButton != null)
			{
				cancelButton.onClick.AddListener(Cancel);
			}
			if (m_Window == null)
			{
				m_Window.GetComponent<UIWindow>();
			}
			m_Window.onHidden.AddListener(OnHidden);
			m_Initialized = true;
		}
	}

	private void Awake()
	{
		InitCache();
	}

	public virtual ICallbackPromise Show(IGameModeService service, GameData data, bool instant = false)
	{
		InitCache();
		CallbackPromise callbackPromise = new CallbackPromise();
		Setup(service, data, callbackPromise.Resolve, callbackPromise.Cancel);
		Show(instant);
		return callbackPromise;
	}

	protected virtual void Show(bool instant = false)
	{
		m_IsOpen = true;
		m_Window.Show(instant);
	}

	protected virtual void Setup(IGameModeService service, GameData data, Action onConfirmed, Action onCancelled = null)
	{
		m_IsOpen = true;
		m_Data = data;
		m_OnCancelled = onCancelled;
		m_OnConfirmed = onConfirmed;
		EnableConfirmationButton(enable: false);
	}

	public virtual void Hide(bool instant = false)
	{
		if (m_IsOpen)
		{
			m_IsOpen = false;
			m_Window.Hide(instant);
		}
	}

	private async void Confirm()
	{
		if (await Validate())
		{
			OnConfirmedStep();
		}
	}

	private async void ConfirmRejected()
	{
		await Validate();
	}

	protected virtual void OnConfirmedStep()
	{
		m_OnConfirmed?.Invoke();
	}

	protected virtual Task<bool> Validate()
	{
		return Task.FromResult(result: true);
	}

	protected virtual void Cancel()
	{
		Hide();
		m_OnCancelled();
	}

	private void OnHidden()
	{
		if (m_IsOpen)
		{
			Cancel();
		}
	}

	protected virtual void EnableConfirmationButton(bool enable)
	{
		confirmButton.interactable = enable;
	}
}
