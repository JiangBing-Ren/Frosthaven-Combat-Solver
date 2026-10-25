using System;
using System.Collections.Generic;
using System.Linq;
using DevLocker.GFrame.Input;
using DevLocker.GFrame.Input.UIScope;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHTabsGroup : MonoBehaviour
{
	[Serializable]
	public class TabActionEvent : UnityEvent<Button>
	{
	}

	[Tooltip("Should next/previous hotkeys wrap around the available toggles.")]
	public bool WrapAround = true;

	public HotkeyEventScopeElement NextHotkey;

	public HotkeyEventScopeElement PreviousHotkey;

	[Tooltip("List here all tab buttons. They should correspond to the Contents objects.\nHidden inactive tab gameobjects are allowed. You can re-activate them at any time.\nOpened tab is displayed as Active so it has distinguish look.\n")]
	public List<FHTabButton> Tabs;

	[Tooltip("Contents to be displayed when corresponding tab is active.")]
	public List<GameObject> Contents;

	[Tooltip("Which tab should be initially active?")]
	public FHTabButton StartTab;

	public bool ActivateInitialTabOnEnable = true;

	[Space]
	public TabActionEvent TabOpened;

	public TabActionEvent TabClosed;

	protected int m_CurrentIndex = -1;

	private List<Button> m_SubscribedTabButtons = new List<Button>();

	protected IPlayerContext m_PlayerContext;

	private bool m_EnabledOnce;

	public FHTabButton ActiveTab
	{
		get
		{
			if (m_CurrentIndex < 0)
			{
				return null;
			}
			return Tabs[m_CurrentIndex];
		}
	}

	public GameObject ActiveContent
	{
		get
		{
			if (m_CurrentIndex < 0)
			{
				return null;
			}
			return Contents[m_CurrentIndex];
		}
	}

	public GameObject GetContentByTab(FHTabButton tabButton)
	{
		return Contents[Tabs.IndexOf(tabButton)];
	}

	protected virtual void Awake()
	{
		if (StartTab == null)
		{
			StartTab = GetFirstEligible();
		}
		if ((bool)StartTab)
		{
			Reinitialize(StartTab);
		}
		NextHotkey?.OnPerformed.AddListener(OnNextAction);
		PreviousHotkey?.OnPerformed.AddListener(OnPreviousAction);
		m_PlayerContext = PlayerContextUtils.GetPlayerContextFor(gameObject);
	}

	public virtual void Reinitialize(FHTabButton nextStartTab)
	{
		OnValidate();
		m_SubscribedTabButtons.RemoveAll((Button b) => b == null);
		for (int num = 0; num < Tabs.Count; num++)
		{
			FHTabButton tabButton = Tabs[num];
			if (tabButton == null)
			{
				continue;
			}
			if (!m_SubscribedTabButtons.Contains(tabButton))
			{
				tabButton.onClick.AddListener(() =>
				{
					SwitchToTab(tabButton);
				});
				FHTabButton fHTabButton = tabButton;
				fHTabButton.OnInteractableStateChanged = (UnityAction)Delegate.Combine(fHTabButton.OnInteractableStateChanged, new UnityAction(ValidateSelected));
			}
			UIManager.RegisterLateActiveState(Contents[num], active: false);
		}
		m_CurrentIndex = -1;
		SwitchToTab(nextStartTab);
		StartTab = nextStartTab;
	}

	protected virtual void OnEnable()
	{
		if (ActivateInitialTabOnEnable && m_EnabledOnce)
		{
			m_CurrentIndex = -1;
			if (StartTab != null)
			{
				Reinitialize(StartTab);
			}
			else
			{
				FHTabButton firstEligible = GetFirstEligible();
				if ((bool)firstEligible)
				{
					Reinitialize(firstEligible);
				}
			}
		}
		m_EnabledOnce = true;
	}

	private FHTabButton GetFirstEligible()
	{
		FHTabButton fHTabButton = Tabs.Where((FHTabButton t) => t.gameObject.activeInHierarchy && t.interactable).FirstOrDefault((FHTabButton b) => b.IsFocused);
		if (fHTabButton == null)
		{
			fHTabButton = Tabs.FirstOrDefault((FHTabButton t) => t.gameObject.activeInHierarchy && t.interactable);
		}
		return fHTabButton;
	}

	protected virtual void OnValidate()
	{
		if (Tabs != null && Contents != null)
		{
			if (Tabs.Count != Contents.Count)
			{
				Debug.LogError($"\"{name}\" Tabs buttons {Tabs.Count} count is different from the Contents objects {Contents.Count}. Every tab button needs corresponding content object.", this);
			}
			else if ((bool)StartTab && !Tabs.Contains(StartTab))
			{
				Debug.LogError("\"" + name + "\" StartTab is not part of the original Tabs.", this);
			}
		}
	}

	public void SwitchToTab(FHTabButton tabButton)
	{
		SwitchToTab(Tabs.IndexOf(tabButton));
	}

	public virtual void SwitchToTab(int index)
	{
		if (m_CurrentIndex != index)
		{
			if (m_CurrentIndex >= 0)
			{
				UIManager.RegisterLateActiveState(ActiveContent, active: false);
				TabClosed.Invoke(ActiveTab);
			}
			else
			{
				ResetContent();
			}
			m_CurrentIndex = index;
			UpdateVisuals();
			UIManager.RegisterLateActiveState(ActiveContent, active: true);
			TabOpened.Invoke(ActiveTab);
			if (!ActiveTab.gameObject.activeInHierarchy)
			{
				Debug.LogWarning(string.Format("{0} \"{1}\" is trying to switch to a tab {2} \"{3}\" that is not active!", "TabsGroup", name, m_CurrentIndex, ActiveTab.name), this);
			}
		}
	}

	private void ValidateSelected()
	{
		if (!(ActiveTab == null) && !ActiveTab.interactable)
		{
			FHTabButton fHTabButton = Tabs.FirstOrDefault((FHTabButton b) => b.gameObject.activeInHierarchy);
			if (fHTabButton != null)
			{
				SwitchToTab(fHTabButton);
			}
		}
	}

	protected virtual void UpdateVisuals()
	{
		if (Tabs == null)
		{
			return;
		}
		for (int i = 0; i < Tabs.Count; i++)
		{
			FHTabButton fHTabButton = Tabs[i];
			if (!(fHTabButton == null))
			{
				fHTabButton.IsFocused = ActiveTab == fHTabButton;
			}
		}
	}

	private void ResetContent()
	{
		for (int i = 0; i < Contents.Count; i++)
		{
			UIManager.RegisterLateActiveState(Contents[i].gameObject, active: false);
		}
	}

	protected virtual void OnNextAction()
	{
		if (!m_PlayerContext.IsActive)
		{
			return;
		}
		List<FHTabButton> list = Tabs.Where((FHTabButton b) => b.gameObject.activeInHierarchy).ToList();
		if (list.Count != 0)
		{
			int num = list.IndexOf(ActiveTab);
			int num2 = (WrapAround ? ((num + 1) % list.Count) : Mathf.Clamp(num + 1, 0, list.Count - 1));
			if (num != num2)
			{
				ExecuteEvents.Execute(list[num2].gameObject, new PointerEventData(m_PlayerContext.EventSystem), ExecuteEvents.pointerClickHandler);
			}
		}
	}

	protected virtual void OnPreviousAction()
	{
		if (!m_PlayerContext.IsActive)
		{
			return;
		}
		List<FHTabButton> list = Tabs.Where((FHTabButton b) => b.gameObject.activeInHierarchy).ToList();
		if (list.Count != 0)
		{
			int num = list.IndexOf(ActiveTab);
			int num2 = (WrapAround ? (num - 1) : Mathf.Clamp(num - 1, 0, list.Count - 1));
			if (num2 < 0)
			{
				num2 = list.Count - 1;
			}
			if (num != num2)
			{
				ExecuteEvents.Execute(list[num2].gameObject, new PointerEventData(m_PlayerContext.EventSystem), ExecuteEvents.pointerClickHandler);
			}
		}
	}
}
