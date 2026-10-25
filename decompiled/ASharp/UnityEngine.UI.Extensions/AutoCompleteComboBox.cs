using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions;

[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("UI/Extensions/AutoComplete ComboBox")]
public class AutoCompleteComboBox : MonoBehaviour
{
	[Serializable]
	public class SelectionChangedEvent : UnityEvent<string, bool>
	{
	}

	[Serializable]
	public class SelectionTextChangedEvent : UnityEvent<string>
	{
	}

	[Serializable]
	public class SelectionValidityChangedEvent : UnityEvent<bool>
	{
	}

	[SerializeField]
	private List<string> availableOptions;

	private bool _isPanelActive;

	private bool _hasDrawnOnce;

	public InputField _mainInput;

	private RectTransform _inputRT;

	private RectTransform _rectTransform;

	public RectTransform _overlayRT;

	public RectTransform _scrollPanelRT;

	public RectTransform _scrollBarRT;

	public RectTransform _slidingAreaRT;

	public RectTransform _container;

	private Canvas _canvas;

	private RectTransform _canvasRT;

	private ScrollRect _scrollRect;

	private List<string> _panelItems;

	private List<string> _prunedPanelItems;

	private Dictionary<string, GameObject> panelObjects;

	private GameObject itemTemplate;

	private Transform parent;

	[SerializeField]
	private int _itemsToDisplay;

	public bool SelectFirstItemOnStart;

	[SerializeField]
	[Tooltip("Change input text color based on matching items")]
	private bool _ChangeInputTextColorBasedOnMatchingItems;

	public Color ValidSelectionTextColor = Color.green;

	public Color MatchingItemsRemainingTextColor = Color.black;

	public Color NoItemsRemainingTextColor = Color.red;

	public AutoCompleteSearchType autocompleteSearchType = AutoCompleteSearchType.Linq;

	private bool _selectionIsValid;

	private bool initialized;

	public SelectionTextChangedEvent OnSelectionTextChanged;

	public SelectionValidityChangedEvent OnSelectionValidityChanged;

	public SelectionChangedEvent OnSelectionChanged;

	public string Text { get; private set; }

	public int ItemsToDisplay
	{
		get
		{
			return _itemsToDisplay;
		}
		set
		{
			_itemsToDisplay = value;
			RedrawPanel();
		}
	}

	public bool InputColorMatching
	{
		get
		{
			return _ChangeInputTextColorBasedOnMatchingItems;
		}
		set
		{
			_ChangeInputTextColorBasedOnMatchingItems = value;
			if (_ChangeInputTextColorBasedOnMatchingItems)
			{
				SetInputTextColor();
			}
		}
	}

	public List<string> AvailableOptions
	{
		set
		{
			availableOptions = value;
			if (initialized)
			{
				RebuildPanel();
				RedrawPanel();
			}
		}
	}

	public bool IsSelectionValid => _selectionIsValid;

	public void Awake()
	{
		initialized = Initialize();
	}

	public void Start()
	{
		if (SelectFirstItemOnStart && availableOptions.Count > 0)
		{
			ToggleDropdownPanel(directClick: false);
			OnItemClicked(availableOptions[0]);
		}
	}

	private bool Initialize()
	{
		bool result = true;
		try
		{
			parent = transform.parent;
			_rectTransform = GetComponent<RectTransform>();
			_inputRT = _mainInput.GetComponent<RectTransform>();
			_overlayRT.gameObject.SetActive(value: false);
			_canvas = GetComponentInParent<Canvas>();
			_canvasRT = _canvas.GetComponent<RectTransform>();
			_scrollRect = _scrollPanelRT.GetComponent<ScrollRect>();
			_scrollRect.scrollSensitivity = _rectTransform.sizeDelta.y / 2f;
			_scrollRect.movementType = ScrollRect.MovementType.Clamped;
			_scrollRect.content = _container;
			itemTemplate = _rectTransform.Find("ItemTemplate").gameObject;
			itemTemplate.SetActive(value: false);
		}
		catch (NullReferenceException exception)
		{
			Debug.LogException(exception);
			Log.GUIError("Something is setup incorrectly with the dropdownlist component causing a Null Refernece Exception");
			result = false;
		}
		panelObjects = new Dictionary<string, GameObject>();
		_prunedPanelItems = new List<string>();
		_panelItems = new List<string>();
		RebuildPanel();
		RedrawPanel();
		return result;
	}

	private void RebuildPanel()
	{
		_panelItems.Clear();
		_prunedPanelItems.Clear();
		foreach (KeyValuePair<string, GameObject> panelObject in panelObjects)
		{
			Object.Destroy(panelObject.Value);
		}
		panelObjects.Clear();
		foreach (string availableOption in availableOptions)
		{
			_panelItems.Add(availableOption);
		}
		List<GameObject> list = new List<GameObject>(panelObjects.Values);
		int num = 0;
		while (list.Count < availableOptions.Count)
		{
			GameObject gameObject = Object.Instantiate(itemTemplate);
			gameObject.name = "Item " + num;
			gameObject.transform.SetParent(_container, worldPositionStays: false);
			list.Add(gameObject);
			num++;
		}
		for (int i = 0; i < list.Count; i++)
		{
			list[i].SetActive(i <= availableOptions.Count);
			if (i < availableOptions.Count)
			{
				list[i].name = "Item " + i + " " + _panelItems[i];
				list[i].transform.Find("Text").GetComponent<TextMeshProUGUI>().text = _panelItems[i];
				Button component = list[i].GetComponent<Button>();
				component.onClick.RemoveAllListeners();
				string textOfItem = _panelItems[i];
				component.onClick.AddListener(() =>
				{
					OnItemClicked(textOfItem);
				});
				panelObjects[_panelItems[i]] = list[i];
			}
		}
		SetInputTextColor();
	}

	private void OnItemClicked(string item)
	{
		Text = item;
		_mainInput.text = Text;
		ToggleDropdownPanel(directClick: true);
	}

	private void RedrawPanel()
	{
		_scrollBarRT.gameObject.SetActive(_panelItems.Count > ItemsToDisplay);
		if (!_hasDrawnOnce || _rectTransform.sizeDelta != _inputRT.sizeDelta)
		{
			_hasDrawnOnce = true;
			_inputRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rectTransform.sizeDelta.y);
			_scrollPanelRT.SetParent(transform, worldPositionStays: true);
			_scrollPanelRT.anchoredPosition = new Vector2(0f, 0f - _rectTransform.sizeDelta.y);
			_overlayRT.SetParent(_canvas.transform, worldPositionStays: false);
			_overlayRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _canvasRT.sizeDelta.y);
			_overlayRT.SetParent(transform, worldPositionStays: true);
			_scrollPanelRT.SetParent(_overlayRT, worldPositionStays: true);
		}
		if (_panelItems.Count >= 1)
		{
			float num = _rectTransform.sizeDelta.y * (float)Mathf.Min(_itemsToDisplay, _panelItems.Count);
			_scrollPanelRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
			_scrollBarRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num);
			_slidingAreaRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num - _scrollBarRT.sizeDelta.y);
		}
	}

	public void OnValueChanged(string currText)
	{
		Text = currText;
		PruneItems(currText);
		RedrawPanel();
		if (_panelItems.Count == 0)
		{
			_isPanelActive = true;
			ToggleDropdownPanel(directClick: false);
		}
		else if (!_isPanelActive)
		{
			ToggleDropdownPanel(directClick: false);
		}
		bool flag = _panelItems.Contains(Text) != _selectionIsValid;
		_selectionIsValid = _panelItems.Contains(Text);
		OnSelectionChanged.Invoke(Text, _selectionIsValid);
		OnSelectionTextChanged.Invoke(Text);
		if (flag)
		{
			OnSelectionValidityChanged.Invoke(_selectionIsValid);
		}
		SetInputTextColor();
	}

	private void SetInputTextColor()
	{
		if (InputColorMatching)
		{
			if (_selectionIsValid)
			{
				_mainInput.textComponent.color = ValidSelectionTextColor;
			}
			else if (_panelItems.Count > 0)
			{
				_mainInput.textComponent.color = MatchingItemsRemainingTextColor;
			}
			else
			{
				_mainInput.textComponent.color = NoItemsRemainingTextColor;
			}
		}
	}

	public void ToggleDropdownPanel(bool directClick)
	{
		_isPanelActive = !_isPanelActive;
		_overlayRT.gameObject.SetActive(_isPanelActive);
		_ = _isPanelActive;
	}

	private void PruneItems(string currText)
	{
		if (autocompleteSearchType == AutoCompleteSearchType.Linq)
		{
			PruneItemsLinq(currText);
		}
		else
		{
			PruneItemsArray(currText);
		}
	}

	private void PruneItemsLinq(string currText)
	{
		currText = currText.ToLower();
		string[] array = _panelItems.Where((string x) => !x.ToLower().Contains(currText)).ToArray();
		foreach (string text in array)
		{
			panelObjects[text].SetActive(value: false);
			_panelItems.Remove(text);
			_prunedPanelItems.Add(text);
		}
		array = _prunedPanelItems.Where((string x) => x.ToLower().Contains(currText)).ToArray();
		foreach (string text2 in array)
		{
			panelObjects[text2].SetActive(value: true);
			_panelItems.Add(text2);
			_prunedPanelItems.Remove(text2);
		}
	}

	private void PruneItemsArray(string currText)
	{
		string value = currText.ToLower();
		for (int num = _panelItems.Count - 1; num >= 0; num--)
		{
			string text = _panelItems[num];
			if (!text.ToLower().Contains(value))
			{
				panelObjects[_panelItems[num]].SetActive(value: false);
				_panelItems.RemoveAt(num);
				_prunedPanelItems.Add(text);
			}
		}
		for (int num2 = _prunedPanelItems.Count - 1; num2 >= 0; num2--)
		{
			string text2 = _prunedPanelItems[num2];
			if (text2.ToLower().Contains(value))
			{
				panelObjects[_prunedPanelItems[num2]].SetActive(value: true);
				_prunedPanelItems.RemoveAt(num2);
				_panelItems.Add(text2);
			}
		}
	}
}
