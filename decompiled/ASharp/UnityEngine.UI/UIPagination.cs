namespace UnityEngine.UI;

[AddComponentMenu("UI/Pagination", 82)]
public class UIPagination : MonoBehaviour
{
	public Button buttonPrev;

	public Button buttonNext;

	public Transform pagesContainer;

	private int activePage;

	private void Start()
	{
		if (buttonPrev != null)
		{
			buttonPrev.onClick.AddListener(OnPrevClick);
		}
		if (buttonNext != null)
		{
			buttonNext.onClick.AddListener(OnNextClick);
		}
		if (pagesContainer != null && pagesContainer.childCount > 0)
		{
			for (int i = 0; i < pagesContainer.childCount; i++)
			{
				if (pagesContainer.GetChild(i).gameObject.activeSelf)
				{
					activePage = i;
					break;
				}
			}
		}
		UpdatePagesVisibility();
	}

	private void UpdatePagesVisibility()
	{
		if (pagesContainer != null && pagesContainer.childCount > 0)
		{
			for (int i = 0; i < pagesContainer.childCount; i++)
			{
				pagesContainer.GetChild(i).gameObject.SetActive((i == activePage) ? true : false);
			}
		}
	}

	private void OnPrevClick()
	{
		if (enabled && !(pagesContainer == null))
		{
			if (activePage == 0)
			{
				activePage = pagesContainer.childCount - 1;
			}
			else
			{
				activePage--;
			}
			UpdatePagesVisibility();
		}
	}

	private void OnNextClick()
	{
		if (enabled && !(pagesContainer == null))
		{
			if (activePage == pagesContainer.childCount - 1)
			{
				activePage = 0;
			}
			else
			{
				activePage++;
			}
			UpdatePagesVisibility();
		}
	}
}
