using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPartyCharacter2HandSlot : MonoBehaviour
{
	[SerializeField]
	private ExtendedButton button;

	[SerializeField]
	private List<GameObject> elementsToDisable;

	[SerializeField]
	private List<GameObject> elementsToEnable;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private ExtendedButton button1;

	[SerializeField]
	private ExtendedButton button2;

	private void Awake()
	{
		button.onClick.AddListener(Select);
		button.onMouseEnter.AddListener(OnHovered);
		button.onMouseExit.AddListener(OnUnhovered);
	}

	private void Select()
	{
		button1.onClick.Invoke();
	}

	public void Enable(bool enable)
	{
		CanvasGroup canvasGroup = this.canvasGroup;
		bool interactable = (this.canvasGroup.blocksRaycasts = !enable);
		canvasGroup.interactable = interactable;
		button.enabled = enable;
		for (int i = 0; i < elementsToDisable.Count; i++)
		{
			elementsToDisable[i].SetActive(!enable);
		}
		for (int j = 0; j < elementsToEnable.Count; j++)
		{
			elementsToEnable[j].SetActive(enable);
		}
	}

	private void OnUnhovered()
	{
		button1.OnPointerExit(new PointerEventData(EventSystem.current));
		button2.OnPointerExit(new PointerEventData(EventSystem.current));
	}

	private void OnHovered()
	{
		button1.OnPointerEnter(new PointerEventData(EventSystem.current));
		button2.OnPointerEnter(new PointerEventData(EventSystem.current));
	}
}
