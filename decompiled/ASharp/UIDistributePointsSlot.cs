using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDistributePointsSlot : MonoBehaviour
{
	[SerializeField]
	private FHButton slotButton;

	[SerializeField]
	private Image portrait;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private Color defaultTitleColor;

	[SerializeField]
	private UIDistributePointsCounter slider;

	[SerializeField]
	private FHButton addPointButton;

	[SerializeField]
	private FHButton removePointButton;

	[SerializeField]
	private Image removedAllPointsHighlight;

	[SerializeField]
	private Color removedAllPointsTitleColor;

	[SerializeField]
	private TextMeshProUGUI optionText;

	[SerializeField]
	private List<UIDistributePointsActorInfo> extraInfo;

	private Action onAddedPoint;

	private Action onRemovedPoint;

	private Action<bool> onHoveredSlot;

	private int currentPoints;

	private bool m_IsRewards;

	public IDistributePointsActor Actor { get; private set; }

	public UIDistributePointsCounter Counter => slider;

	public Selectable Selectable => slotButton;

	private void Awake()
	{
		addPointButton.onClick.AddListener(AddPointDelegate);
		removePointButton.onClick.AddListener(RemovePointDelegate);
		if (slotButton == null)
		{
			slotButton = GetComponent<FHButton>();
		}
		slotButton.PointerEntered += () =>
		{
			OnHoveredSlot(hovered: true);
		};
		slotButton.PointerExited += () =>
		{
			OnHoveredSlot(hovered: false);
		};
	}

	public virtual void Init(IDistributePointsActor actor, int maxPoints, int currentPoints, Action onAddedPoint, Action onRemovedPoint, Action<bool> onHoveredSlot, int assignedPoints = 0, bool isRewards = false, string optionName = null)
	{
		this.onAddedPoint = onAddedPoint;
		this.onRemovedPoint = onRemovedPoint;
		this.onHoveredSlot = onHoveredSlot;
		this.currentPoints = currentPoints;
		Actor = actor;
		titleText.text = actor.Name;
		if (optionText != null)
		{
			optionText.text = optionName;
		}
		portrait.sprite = actor.Portrait;
		slider.Setup(maxPoints, currentPoints);
		for (int i = 0; i < extraInfo.Count; i++)
		{
			extraInfo[i].Display(actor);
		}
		RefreshAssignedPoints(assignedPoints);
		EnableAddPoints(enable: false);
		EnableRemovePoints(enable: false);
		m_IsRewards = isRewards;
	}

	private void RemovePointDelegate()
	{
		RemovePoint(isProxyCall: false);
	}

	private void AddPointDelegate()
	{
		AddPoint(isProxyCall: false);
	}

	public void RemovePoint(bool isProxyCall)
	{
		if (isProxyCall || removePointButton.interactable)
		{
			onRemovedPoint?.Invoke();
		}
	}

	public void AddPoint(bool isProxyCall)
	{
		if (isProxyCall || addPointButton.interactable)
		{
			onAddedPoint?.Invoke();
		}
	}

	private void OnHoveredSlot(bool hovered)
	{
		onHoveredSlot?.Invoke(hovered);
	}

	public void RefreshAssignedPoints(int points)
	{
		slider.SetExtendedPoints(points);
		if (removedAllPointsHighlight != null)
		{
			removedAllPointsHighlight.enabled = currentPoints + points == 0;
		}
		titleText.color = ((currentPoints + points == 0) ? removedAllPointsTitleColor : defaultTitleColor);
		for (int i = 0; i < extraInfo.Count; i++)
		{
			extraInfo[i].RefreshAssignedPoints(currentPoints, points);
		}
	}

	public void EnableAddPoints(bool enable)
	{
		if (!m_IsRewards || !FFSNetwork.IsOnline || FFSNetwork.IsHost)
		{
			addPointButton.interactable = enable;
			addPointButton.SetAsFaded(!enable);
		}
		else
		{
			addPointButton.interactable = false;
		}
	}

	public void EnableRemovePoints(bool enable)
	{
		if (!m_IsRewards || !FFSNetwork.IsOnline || FFSNetwork.IsHost)
		{
			removePointButton.interactable = enable;
			removePointButton.SetAsFaded(!enable);
		}
		else
		{
			removePointButton.interactable = false;
		}
	}
}
