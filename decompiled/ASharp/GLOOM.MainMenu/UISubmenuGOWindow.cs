using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class UISubmenuGOWindow : MonoBehaviour, IExternallyAwaked
{
	[SerializeField]
	protected UIWindow window;

	[SerializeField]
	protected GUIAnimator showAnimator;

	public UnityEvent OnHidden = new UnityEvent();

	private bool m_Awaken;

	private bool m_Destroyed;

	private void Awake()
	{
		ExternalAwake();
	}

	public void ExternalAwake()
	{
		if (!m_Awaken)
		{
			m_Awaken = true;
			AwakeInit();
		}
	}

	private void OnDestroy()
	{
		ExternalDestroy();
	}

	public void ExternalDestroy()
	{
		if (!m_Destroyed)
		{
			m_Destroyed = true;
			AwakenDestroy();
		}
	}

	protected virtual void AwakeInit()
	{
		window.onHidden.AddListener(OnCompleteHidden);
		window.onTransitionComplete.AddListener((UIWindow _, UIWindow.VisualState state) =>
		{
			if (state == UIWindow.VisualState.Shown)
			{
				OnShown();
			}
		});
	}

	protected virtual void AwakenDestroy()
	{
	}

	public virtual void Show()
	{
		if (gameObject.activeSelf && window.IsOpen)
		{
			showAnimator.Play();
			return;
		}
		gameObject.SetActive(value: true);
		window.Show();
	}

	protected virtual void OnShown()
	{
		showAnimator.Play();
	}

	public void ToggleVisibility(bool on)
	{
		if (on)
		{
			Show();
		}
		else
		{
			Hide();
		}
	}

	public void Hide()
	{
		if (gameObject.activeSelf)
		{
			showAnimator.Stop();
			window.Hide();
		}
	}

	protected virtual void OnCompleteHidden()
	{
		showAnimator.Stop();
		gameObject.SetActive(value: false);
		OnHidden.Invoke();
	}

	protected virtual void OnDisable()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			showAnimator.Stop();
		}
	}
}
