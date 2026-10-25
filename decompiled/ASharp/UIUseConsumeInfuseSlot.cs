using System;
using System.Collections.Generic;
using System.Linq;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class UIUseConsumeInfuseSlot<T> : UIUseSlot<T>
{
	internal class InfuseElementController : IElementHolder
	{
		private UIUseOption elementUI;

		private IInfuseElement elementInfuse;

		public ElementInfusionBoardManager.EElement RequiredElement
		{
			get
			{
				if (!elementInfuse.IsAnyElement)
				{
					return elementInfuse.SelectedElement;
				}
				return ElementInfusionBoardManager.EElement.Any;
			}
		}

		public ElementInfusionBoardManager.EElement? SelectedElement
		{
			get
			{
				if (elementInfuse.SelectedElement != ElementInfusionBoardManager.EElement.Any)
				{
					return elementInfuse.SelectedElement;
				}
				return null;
			}
		}

		public InfuseElementController(IInfuseElement elementInfuse, UIUseOption elementUI = null)
		{
			this.elementUI = elementUI;
			this.elementInfuse = elementInfuse;
			elementUI?.Clear();
		}

		public void SetSelectedElement(ElementInfusionBoardManager.EElement? element)
		{
			if (!element.HasValue)
			{
				elementUI?.Clear();
				elementInfuse.ResetElementToInitial();
			}
			else
			{
				elementUI?.SetOption(UIInfoTools.Instance.GetElementPickerSelectedSprite(element.Value));
				elementInfuse.PickElement(element.Value);
			}
		}
	}

	internal class ConsumeElementController : IElementHolder
	{
		private UIUseOption elementUI;

		private ElementInfusionBoardManager.EElement? selectedElement;

		public ElementInfusionBoardManager.EElement RequiredElement { get; }

		public ElementInfusionBoardManager.EElement? SelectedElement => selectedElement;

		public ConsumeElementController(UIUseOption elementUI, ElementInfusionBoardManager.EElement element)
		{
			this.elementUI = elementUI;
			RequiredElement = element;
			SetSelectedElement(null);
		}

		public void SetSelectedElement(ElementInfusionBoardManager.EElement? element)
		{
			selectedElement = element;
			if (elementUI != null)
			{
				if (!element.HasValue)
				{
					elementUI.SetOption((RequiredElement == ElementInfusionBoardManager.EElement.Any) ? null : UIInfoTools.Instance.GetElementPickerSelectedSprite(RequiredElement));
				}
				else
				{
					elementUI.SetOption(UIInfoTools.Instance.GetElementPickerSelectedSprite(element.Value));
				}
			}
		}
	}

	[SerializeField]
	private Image unfocusImage;

	[SerializeField]
	private Color unfocusedColor = Color.gray;

	[Header("Element")]
	[SerializeField]
	private UIElementPicker elementPicker;

	[SerializeField]
	private List<UIUseOption> infuseElements;

	[SerializeField]
	private List<UIUseOption> consumeElements;

	protected List<IElementHolder> infusions = new List<IElementHolder>();

	protected List<IElementHolder> consumes = new List<IElementHolder>();

	private InfuseElementPickController infusePickerController;

	private MultiElementPickController consumePickerController;

	protected bool hovered;

	protected virtual void Awake()
	{
	}

	private void InitElementPickers()
	{
		infusePickerController = new InfuseElementPickController(elementPicker).SetOnSelectedAllElements((List<ElementInfusionBoardManager.EElement> elements) =>
		{
			Select();
		}).SetOnSelectedElement((int index, ElementInfusionBoardManager.EElement element) =>
		{
			SelectElement(element);
		});
		infusePickerController.SetOnClosePicker(OnCloseInfusionPicker).SetOnOpenPicker(OnOpenPicker);
		infusePickerController.ClosePicker();
		consumePickerController = new MultiElementPickController(elementPicker).SetOnSelectedAllElements((List<ElementInfusionBoardManager.EElement> elements) =>
		{
			Select();
		}).SetOnSelectedElement((int index, ElementInfusionBoardManager.EElement element) =>
		{
			SelectElement(element);
		});
		consumePickerController.SetOnClosePicker(OnClosePicker).SetOnOpenPicker(OnOpenPicker);
		consumePickerController.ClosePicker();
	}

	protected void Init(CActor actor, T element, Action<T> onSelect = null, Action<T> onUnselect = null, Func<T, bool> isMandatoryChecker = null, bool isSelected = false, bool isInteractable = true, List<IInfuseElement> infusions = null, List<ElementInfusionBoardManager.EElement> consumes = null, List<ElementInfusionBoardManager.EElement> excludedConsumes = null)
	{
		InitElementPickers();
		base.Init(actor, element, onSelect, onUnselect, isMandatoryChecker, isSelected, isInteractable);
		CreateInfusions(infusions);
		CreateConsumes(consumes, excludedConsumes);
	}

	protected virtual void OnCloseInfusionPicker()
	{
		if (!infusePickerController.AreAllAnySelected())
		{
			consumePickerController.Cancel();
		}
		OnClosePicker();
	}

	protected virtual void OnClosePicker()
	{
		if (!(gameObject == null) && gameObject.activeSelf)
		{
			if ((bool)unfocusImage)
			{
				unfocusImage.color = UIInfoTools.Instance.White;
			}
			CheckShowTooltip();
			if (Singleton<InputFHManager>.Instance.UsingGamepad)
			{
				EventSystem.current.SetSelectedGameObject(gameObject);
			}
		}
	}

	protected virtual void OnOpenPicker()
	{
		if ((bool)unfocusImage)
		{
			unfocusImage.color = unfocusedColor;
		}
		CheckShowTooltip();
	}

	public void SetInfusions(List<ElementInfusionBoardManager.EElement> elements)
	{
		int num = 0;
		for (int i = 0; i < infusions.Count; i++)
		{
			if (infusions[i].RequiredElement == ElementInfusionBoardManager.EElement.Any)
			{
				infusions[i].SetSelectedElement((num < elements.Count) ? new ElementInfusionBoardManager.EElement?(elements[num]) : ((ElementInfusionBoardManager.EElement?)null));
				num++;
			}
		}
	}

	public void SetConsumes(List<ElementInfusionBoardManager.EElement> elements)
	{
		int num = 0;
		for (int i = 0; i < consumes.Count; i++)
		{
			if (consumes[i].RequiredElement == ElementInfusionBoardManager.EElement.Any)
			{
				consumes[i].SetSelectedElement((num < elements.Count) ? new ElementInfusionBoardManager.EElement?(elements[num]) : ((ElementInfusionBoardManager.EElement?)null));
				num++;
			}
		}
	}

	public void SelectElement(ElementInfusionBoardManager.EElement selectedElement)
	{
		if (element is IAbility ability)
		{
			ability.SelectedElements.Clear();
			ability.SelectedElements.Add(selectedElement);
		}
	}

	public override void Select()
	{
		if (consumes.Count > 0 && consumePickerController != null)
		{
			if (consumePickerController.IsSelecting())
			{
				consumePickerController.ClosePicker();
				return;
			}
			if (!consumePickerController.Pick())
			{
				return;
			}
		}
		if (infusions.Count > 0)
		{
			if (infusePickerController.IsSelecting())
			{
				infusePickerController.ClosePicker();
				return;
			}
			if (!infusePickerController.Pick())
			{
				return;
			}
		}
		selected = true;
		IAbility ability = element as IAbility;
		if (consumes.Count > 0)
		{
			Consume(consumes.Select((IElementHolder it) => it.SelectedElement.Value).ToList(), active: true);
		}
		if (ability != null && ability.Consumes != null && ability.Consumes.Count > 0 && !ability.Consumes.Any((ElementInfusionBoardManager.EElement consume) => consume == ElementInfusionBoardManager.EElement.Any))
		{
			ability.SelectedElements.Clear();
			ability.SelectedElements.AddRange(ability.Consumes);
			Consume(ability.Consumes, active: true);
		}
		else if (ability?.Consumes != null && ability.Consumes.Any((ElementInfusionBoardManager.EElement x) => x == ElementInfusionBoardManager.EElement.Any))
		{
			ability.SelectedElements.AddRange(ability.Consumes.Where((ElementInfusionBoardManager.EElement x) => x != ElementInfusionBoardManager.EElement.Any && !ability.SelectedElements.Contains(x)));
		}
		if (infusions.Count > 0 && (ability == null || !ability.Abilities.Any((CAbility x) => x is CAbilityInfuse)))
		{
			Infuse(infusions.Select((IElementHolder it) => it.SelectedElement.Value).ToList(), active: true);
		}
		base.Select();
	}

	public void Deselect()
	{
		base.Unselect();
	}

	public List<ElementInfusionBoardManager.EElement> GetSelectedInfusions()
	{
		return (from it in infusions
			where it.SelectedElement.HasValue
			select it.SelectedElement.Value).ToList();
	}

	public List<ElementInfusionBoardManager.EElement> GetSelectedConsumes()
	{
		return (from it in consumes
			where it.SelectedElement.HasValue
			select it.SelectedElement.Value).ToList();
	}

	protected virtual void Infuse(List<ElementInfusionBoardManager.EElement> infusions, bool active)
	{
		if (infusions.Count != 0)
		{
			if (active)
			{
				ElementInfusionBoardManager.Infuse(infusions, actor);
			}
			else
			{
				ElementInfusionBoardManager.UndoInfuse(infusions, actor);
			}
		}
	}

	protected virtual void Consume(List<ElementInfusionBoardManager.EElement> consumes, bool active)
	{
		if (consumes.Count != 0)
		{
			InfusionBoardUI.Instance?.MarkElementsDirty();
		}
	}

	public override void ClearSelection(bool fromClick = false, bool forceClear = false)
	{
		if (!selected && !forceClear)
		{
			return;
		}
		base.ClearSelection(fromClick, forceClear);
		if (element is CBaseCard { ActionHasHappened: not false })
		{
			ElementInfusionBoardManager.SaveState();
		}
		if (element is IAbility { Consumes: not null } ability && ability.Consumes.Count == 1 && ability.Consumes[0] != ElementInfusionBoardManager.EElement.Any)
		{
			Consume(ability.Consumes, active: false);
		}
		else
		{
			Consume((from it in consumes
				where it.SelectedElement.HasValue
				select it.SelectedElement.Value).ToList(), active: false);
		}
		foreach (IElementHolder consume in consumes)
		{
			consume.SetSelectedElement(null);
		}
		Infuse((from it in infusions
			where it.SelectedElement.HasValue
			select it.SelectedElement.Value).ToList(), active: false);
		ElementInfusionBoardManager.RestoreState();
		foreach (IElementHolder infusion in infusions)
		{
			infusion.SetSelectedElement(null);
		}
	}

	protected virtual void CreateInfusions(List<IInfuseElement> elements)
	{
		infusions.Clear();
		List<ElementInfusionBoardManager.EElement> list = new List<ElementInfusionBoardManager.EElement>();
		if (elements == null)
		{
			HelperTools.NormalizePool(ref infuseElements, infuseElements[0].gameObject, infuseElements[0].transform.parent, 0);
		}
		else
		{
			HelperTools.NormalizePool(ref infuseElements, infuseElements[0].gameObject, infuseElements[0].transform.parent, elements.Count((IInfuseElement it) => it.IsAnyElement));
			int num = 0;
			for (int num2 = 0; num2 < elements.Count; num2++)
			{
				UIUseOption elementUI = null;
				if (elements[num2].IsAnyElement)
				{
					elementUI = infuseElements[num];
					num++;
				}
				if (elements[num2] is UIUseAbilitiesBar.DummyInfusion dummyInfusion)
				{
					List<ElementInfusionBoardManager.EElement> first = Enum.GetValues(typeof(ElementInfusionBoardManager.EElement)).Cast<ElementInfusionBoardManager.EElement>().ToList();
					list.AddRange(first.Except(dummyInfusion.Elements));
				}
				infusions.Add(new InfuseElementController(elements[num2], elementUI));
			}
		}
		infusePickerController.Setup(infusions);
		infusePickerController.SetExcludedElements(list);
	}

	protected virtual void CreateConsumes(List<ElementInfusionBoardManager.EElement> elements, List<ElementInfusionBoardManager.EElement> excludedElements)
	{
		consumes.Clear();
		if (elements == null)
		{
			HelperTools.NormalizePool(ref consumeElements, consumeElements[0].gameObject, consumeElements[0].transform.parent, 0);
		}
		else
		{
			HelperTools.NormalizePool(ref consumeElements, consumeElements[0].gameObject, consumeElements[0].transform.parent, elements.Count);
			for (int i = 0; i < elements.Count; i++)
			{
				consumes.Add(new ConsumeElementController(consumeElements[i], elements[i]));
			}
		}
		consumePickerController.Setup(consumes);
		consumePickerController.SetExcludedElements(excludedElements);
	}

	public void HideConsumeElements()
	{
		foreach (UIUseOption consumeElement in consumeElements)
		{
			consumeElement.gameObject.SetActive(value: false);
		}
	}

	public virtual void OnPointerEnter()
	{
		hovered = true;
		CheckShowTooltip();
	}

	public virtual void OnPointerExit()
	{
		hovered = false;
		ShowTooltip(show: false);
	}

	public override void Hide()
	{
		OnPointerExit();
		base.Hide();
	}

	protected virtual void CheckShowTooltip()
	{
		if (gameObject.activeSelf)
		{
			if (elementPicker.IsOpen)
			{
				ShowTooltip(show: false);
			}
			else
			{
				ShowTooltip(hovered);
			}
		}
	}

	protected abstract void ShowTooltip(bool show);

	public void SetAnyConsume(ElementInfusionBoardManager.EElement? element, int consumeIndex = 0)
	{
		if (consumeIndex < consumes.Count)
		{
			List<IElementHolder> list = consumes.FindAll((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any);
			if (consumeIndex < list.Count)
			{
				list[consumeIndex].SetSelectedElement(element);
			}
		}
	}

	public void SetAnyConsume(List<ElementInfusionBoardManager.EElement> elements)
	{
		List<IElementHolder> list = consumes.FindAll((IElementHolder it) => it.RequiredElement == ElementInfusionBoardManager.EElement.Any);
		if (elements.Count == list.Count)
		{
			for (int num = 0; num < list.Count; num++)
			{
				list[num].SetSelectedElement(elements[num]);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		if (!GloomUtility.IsAppQuitting)
		{
			consumePickerController?.OnDestroy();
			infusePickerController?.OnDestroy();
		}
	}
}
