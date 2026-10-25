using System;
using System.Linq;
using ScenarioRuleLibrary.YML;
using UnityEngine;

public class UIPerkAttackModifier : UIAttackModifier<UIPerkAttackModifierCounter>
{
	[SerializeField]
	private GameObject cancelledOverlay;

	[SerializeField]
	private GameObject activeOverlay;

	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private UINewNotificationTip newNotification;

	private Color defaultTextColor;

	private Action<AttackModifierYMLData, bool> onHovered;

	public bool isRemover { get; private set; }

	private void Awake()
	{
		button.onMouseEnter.AddListener(() =>
		{
			OnHovered(hovered: true);
		});
		button.onMouseExit.AddListener(() =>
		{
			OnHovered(hovered: false);
		});
	}

	public void Init(AttackModifierYMLData modifier, int counters, bool isNew, Action<AttackModifierYMLData, bool> onHovered, bool remover)
	{
		base.Init(modifier, counters);
		this.onHovered = onHovered;
		defaultTextColor = textValue.color;
		isRemover = remover;
		if (isNew)
		{
			newNotification.Show();
		}
		else
		{
			newNotification.Hide();
		}
		SetCancelled(isCancelled: false);
		SetActive(active: false);
	}

	public override void Init(AttackModifierYMLData modifier, int counters)
	{
		Init(modifier, counters, isNew: false, null, remover: false);
	}

	public override void InitEmpty()
	{
		base.InitEmpty();
		defaultTextColor = textValue.color;
		SetCancelled(isCancelled: false);
		SetActive(active: false);
	}

	public void SetCancelled(bool isCancelled)
	{
		if (cancelledOverlay != null)
		{
			cancelledOverlay.SetActive(isCancelled);
		}
		textValue.color = (isCancelled ? UIInfoTools.DefInstance.greyedOutTextColor : defaultTextColor);
		imageValue.material = (isCancelled ? UIInfoTools.Instance.greyedOutMaterial : null);
		foreach (UIPerkAttackModifierCounter attackModCounter in attackModCounters)
		{
			attackModCounter.ShowCancelled(isCancelled);
		}
	}

	public void SetActive(bool active)
	{
		activeOverlay.SetActive(active);
	}

	public void UpdateCounters(int counters, int previewAdd, int previewRemove)
	{
		highlight.enabled = false;
		UpdateCounters(counters + previewAdd);
		for (int i = 0; i < Mathf.Min(previewRemove, counters); i++)
		{
			attackModCounters[i].ShowToRemove();
		}
		for (int j = previewRemove; j < counters; j++)
		{
			attackModCounters[j].ShowActive();
		}
		for (int k = counters; k < counters + previewAdd; k++)
		{
			attackModCounters[k].ShowToAdd();
		}
	}

	public override void UpdateCounters(int counters)
	{
		base.UpdateCounters(counters);
		for (int i = 0; i < counters; i++)
		{
			attackModCounters[i].ShowActive();
		}
	}

	public int GetActiveCounters()
	{
		return attackModCounters.Count((UIPerkAttackModifierCounter it) => it.IsActive);
	}

	private void OnHovered(bool hovered)
	{
		if (hovered)
		{
			newNotification.Hide();
		}
		onHovered?.Invoke(ModifierData, hovered);
	}
}
