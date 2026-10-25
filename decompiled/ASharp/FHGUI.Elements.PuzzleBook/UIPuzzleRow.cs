using FHGUI.DTO;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Elements.PuzzleBook;

public class UIPuzzleRow : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public delegate void PuzzleUIInteractEventHandler(UIPuzzleRow row, bool interaction);

	public delegate void PuzzleUIEventHandler(UIPuzzleRow row);

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TMP_Text _title;

	[SerializeField]
	private FHButton _button;

	[Header("States")]
	[SerializeField]
	private Color _selectedTextColor;

	[SerializeField]
	private Color _normalTextColor;

	[SerializeField]
	private Color _completedTextColor;

	[SerializeField]
	private GameObject _highlight;

	[Space]
	[SerializeField]
	private GameObject _currentPuzzleState;

	[SerializeField]
	private GameObject _completedState;

	[SerializeField]
	private GameObject _lockedState;

	[SerializeField]
	private GameObject _selectedState;

	private PuzzleUIEventHandler _onClick;

	private PuzzleUIInteractEventHandler _onHover;

	private PuzzleUIInteractEventHandler _onSelect;

	public PuzzleDTO Puzzle { get; private set; }

	public bool Selected { get; private set; }

	public bool Hovered { get; private set; }

	private void Awake()
	{
		_button.onClick.AddListener(OnClick);
	}

	private void OnDisable()
	{
		_onClick = null;
		_onHover = null;
		_onSelect = null;
	}

	private void OnClick()
	{
		_onClick?.Invoke(this);
	}

	public void Init(PuzzleDTO puzzle, PuzzleUIEventHandler onClick = null, PuzzleUIInteractEventHandler onSelect = null, PuzzleUIInteractEventHandler onHover = null)
	{
		Puzzle = puzzle;
		_onClick = onClick;
		_onHover = onHover;
		_onSelect = onSelect;
		_button.interactable = !Puzzle.IsLocked && !Puzzle.IsDisabled;
		TMP_Text title = _title;
		string text;
		if (Puzzle.IsLocked)
		{
			text = LocalizationManager.GetTranslation("GUI_PUZZLE_LOCKED");
		}
		else
		{
			text = (Puzzle.IsDisabled ? LocalizationManager.GetTranslation("GUI_COMING_SOON") : puzzle.Name);
		}
		title.text = text;
		UpdateState();
	}

	public void Select(bool selected)
	{
		if (Selected != selected)
		{
			Selected = selected;
			_onSelect?.Invoke(this, selected);
			UpdateState();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		_onHover?.Invoke(this, interaction: true);
		UpdateState();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_onHover?.Invoke(this, interaction: false);
		UpdateState();
	}

	private void UpdateState()
	{
		bool flag = Selected || Hovered;
		if (_highlight != null)
		{
			_highlight.SetActive(flag);
		}
		TMP_Text title = _title;
		Color color;
		if (flag)
		{
			color = _selectedTextColor;
		}
		else
		{
			color = (Puzzle.IsCompleted ? _completedTextColor : _normalTextColor);
		}
		title.color = color;
		_lockedState.SetActive(Puzzle.IsLocked || Puzzle.IsDisabled);
		_completedState.SetActive(Puzzle.IsCompleted);
		if (_selectedState != null)
		{
			_selectedState.SetActive(Selected);
		}
		_currentPuzzleState.SetActive(!Puzzle.IsLocked && !Puzzle.IsCompleted && !Puzzle.IsDisabled);
	}
}
