using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaginationHandler : MonoBehaviour
{
	[SerializeField]
	public List<GameObject> pages;

	[SerializeField]
	private TextMeshProUGUI pageText;

	[SerializeField]
	private Button nextPage;

	[SerializeField]
	private Button previousPage;

	[SerializeField]
	private bool autohide;

	public bool forceHide;

	public Action<int, int> OnPageChanged;

	public int currentPage = -1;

	private void Awake()
	{
		nextPage?.onClick.AddListener(() =>
		{
			GoNextPage();
		});
		previousPage?.onClick.AddListener(() =>
		{
			GoPreviousPage();
		});
	}

	private void Start()
	{
		if (forceHide)
		{
			gameObject.SetActive(value: false);
		}
		else if (autohide)
		{
			gameObject.SetActive(pages.Count > 1);
		}
		OpenPage(1);
	}

	public void OpenPage(int page)
	{
		page = Mathf.Clamp(page, 1, pages.Count);
		if (pages.Count > 0)
		{
			for (int i = 0; i < pages.Count; i++)
			{
				pages[i].SetActive(value: false);
			}
			pages[page - 1].SetActive(value: true);
			if (currentPage != page)
			{
				OnPageChanged?.Invoke(page, pages.Count);
			}
			currentPage = page;
			pageText.text = page + "/" + pages.Count;
		}
	}

	public void GoNextPage()
	{
		OpenPage(currentPage + 1);
	}

	public void GoPreviousPage()
	{
		OpenPage(currentPage - 1);
	}

	public void Init(List<GameObject> pages, bool forceHide = false)
	{
		this.forceHide = forceHide;
		this.pages = pages;
		Start();
	}
}
