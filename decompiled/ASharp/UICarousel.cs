using FHGUI.Common;
using UnityEngine;
using UnityEngine.UI;

public class UICarousel : MonoBehaviour
{
	[SerializeField]
	private ScrollRect _scroll;

	[SerializeField]
	private FHButton _previous;

	[SerializeField]
	private FHButton _next;

	[SerializeField]
	private bool _wrapAround;

	[SerializeField]
	private bool _showArrowsWhenJustOneElement;

	[Header("Pagination")]
	[SerializeField]
	private UIPagination _pagination;

	private int _selectedIndex;

	private int _childCount;

	public int Current => _selectedIndex;

	public int Total => _childCount;

	private void Awake()
	{
		_previous.onClick.AddListener(OnPrevious);
		_next.onClick.AddListener(OnNext);
	}

	private void Start()
	{
		Init();
	}

	private void OnTransformChildrenChanged()
	{
		Init();
	}

	private void OnDisable()
	{
		_childCount = -1;
	}

	private void OnPageClick(int page)
	{
		SetPage(page);
	}

	private void OnNext()
	{
		int num = _scroll.content.ActiveChildCount();
		if (_selectedIndex + 1 < num || _wrapAround)
		{
			int selectedIndex = _selectedIndex;
			selectedIndex++;
			selectedIndex %= num;
			SetPage(selectedIndex);
		}
	}

	private void OnPrevious()
	{
		int num = _scroll.content.ActiveChildCount();
		if (_selectedIndex > 0 || _wrapAround)
		{
			int selectedIndex = _selectedIndex;
			selectedIndex--;
			selectedIndex = ((selectedIndex < 0) ? (num - 1) : selectedIndex);
			SetPage(selectedIndex);
		}
	}

	private void RefreshState()
	{
		int num = _scroll.content.ActiveChildCount();
		_previous.interactable = num > 0 && (_selectedIndex > 0 || _wrapAround);
		_next.interactable = num > 0 && (_selectedIndex < num - 1 || _wrapAround);
		bool active = num > 1 || _showArrowsWhenJustOneElement;
		_previous.gameObject.SetActive(active);
		_next.gameObject.SetActive(active);
	}

	public void SetPage(int index, bool forceRefresh = false)
	{
		int num = _scroll.content.ActiveChildCount();
		index = Mathf.Clamp(index, 0, num - 1);
		if (index == _selectedIndex && !forceRefresh)
		{
			return;
		}
		_selectedIndex = index;
		if (num != 0)
		{
			ScrollTo(_selectedIndex);
			if (_pagination != null)
			{
				_pagination.SetCurrentPage(index);
			}
		}
		RefreshState();
	}

	private void ScrollTo(int index)
	{
		Canvas.ForceUpdateCanvases();
		ScrollRect scroll = _scroll;
		Transform child = _scroll.content.GetChild(index);
		bool flag = true;
		RectTransform content = scroll.content;
		RectTransform viewport = scroll.viewport;
		Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(content, child);
		Vector2 size = content.rect.size;
		Vector2 size2 = viewport.rect.size;
		Vector2 normalizedPosition = scroll.normalizedPosition;
		if (scroll.horizontal && size.x > size2.x)
		{
			float num = size.x - size2.x;
			float num2 = ((!flag) ? bounds.min.x : (bounds.center.x - size2.x * 0.5f));
			normalizedPosition.x = Mathf.Clamp01(num2 / num);
		}
		if (scroll.vertical && size.y > size2.y)
		{
			float num3 = size.y - size2.y;
			float num4 = ((!flag) ? (size.y - bounds.max.y) : (size.y - bounds.center.y - size2.y * 0.5f));
			normalizedPosition.y = Mathf.Clamp01(num4 / num3);
		}
		scroll.normalizedPosition = normalizedPosition;
	}

	public void Init()
	{
		int num = _scroll.content.ActiveChildCount();
		if (_childCount != num)
		{
			_childCount = num;
			if (_pagination != null)
			{
				_pagination.InitPages(_childCount, OnPageClick);
			}
			SetPage(0, forceRefresh: true);
		}
		RefreshState();
	}
}
