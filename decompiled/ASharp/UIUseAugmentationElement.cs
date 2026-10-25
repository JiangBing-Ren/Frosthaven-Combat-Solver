using UnityEngine;
using UnityEngine.UI;

public class UIUseAugmentationElement : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image selectedMask;

	[SerializeField]
	private UIUseOption consumeElement;

	[SerializeField]
	private Color unfocusedColor = Color.gray;

	[SerializeField]
	private GUIAnimator showAnimator;

	private bool selected;

	private bool showSelectorOption;

	private bool hovered;

	private Vector3 positionConsumedElement;

	private void Awake()
	{
		positionConsumedElement = (consumeElement.transform as RectTransform).anchoredPosition;
		showAnimator.OnAnimationFinished.AddListener(() =>
		{
			if (showSelectorOption)
			{
				(consumeElement.transform as RectTransform).anchoredPosition = positionConsumedElement;
				consumeElement.Show();
				consumeElement.transform.SetParent(transform.parent);
			}
		});
	}

	public void Init(Sprite image, bool showSelectorOption)
	{
		this.showSelectorOption = showSelectorOption;
		icon.sprite = image;
		hovered = false;
		selected = false;
		Refresh();
		Focus(focus: true);
	}

	public void Focus(bool focus)
	{
		icon.color = (focus ? UIInfoTools.Instance.White : unfocusedColor);
	}

	private void SetSelected(bool selected)
	{
		this.selected = selected;
		Refresh();
	}

	public void SetSelected(Sprite option)
	{
		consumeElement.SetOption(option);
		SetSelected(option != null);
	}

	public void SetSelected(string option)
	{
		consumeElement.SetOption(option);
		SetSelected(option != null);
	}

	public void ShowHovered(bool hovered)
	{
		this.hovered = hovered;
		Refresh();
	}

	private void Refresh()
	{
		if (selected)
		{
			selectedMask.enabled = true;
		}
		else if (hovered)
		{
			selectedMask.enabled = true;
		}
		else
		{
			selectedMask.enabled = false;
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
		consumeElement.transform.SetParent(transform);
		consumeElement.Hide();
	}

	private void OnDisable()
	{
		consumeElement.Hide();
		showAnimator.Stop();
	}

	public void Show()
	{
		consumeElement.Hide();
		consumeElement.transform.SetParent(transform);
		showAnimator.Stop();
		showAnimator.Play();
	}

	public void OnConsumed()
	{
		if (!selected)
		{
			SetSelected(selected: true);
		}
	}
}
