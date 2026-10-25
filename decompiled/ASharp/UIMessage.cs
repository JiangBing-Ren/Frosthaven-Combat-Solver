using System;
using System.Collections;
using System.Text;
using GLOOM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UIMessage : MonoBehaviour
{
	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private Image image;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private GameObject paginationHolder;

	[SerializeField]
	private Button nextPage;

	[SerializeField]
	private Button previousPage;

	[SerializeField]
	private TextMeshProUGUI pageNumber;

	private Action onClose;

	public Button.ButtonClickedEvent OnClosePressed => closeButton.onClick;

	private void Awake()
	{
		closeButton.onClick.AddListener(Hide);
		nextPage.onClick.AddListener(() =>
		{
			GoToNextPage();
		});
		previousPage.onClick.AddListener(() =>
		{
			GoToNextPage(-1);
		});
	}

	public void Show(Message message, Action onClosed = null)
	{
		title.text = message.title;
		description.text = GenerateContent(message);
		image.sprite = message.image;
		onClose = onClosed;
		window.Show();
		description.pageToDisplay = 1;
		StartCoroutine(CheckPagination());
	}

	private IEnumerator CheckPagination()
	{
		yield return new WaitForEndOfFrame();
		pageNumber.text = "1/" + description.textInfo.pageCount;
		paginationHolder.SetActive(description.textInfo.pageCount > 1);
	}

	private string GenerateContent(Message message)
	{
		StringBuilder stringBuilder = new StringBuilder(message.information);
		if (message.content != null)
		{
			for (int i = 0; i < message.content.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.AppendLine($"<size=140%><font=\"MarcellusSC-Regular SDF\"><mark=#000000FF><color=\"white\">{message.content[i].title}</color></mark></font></size>");
				stringBuilder.AppendLine(GenerateContent(message.content[i]));
			}
		}
		return stringBuilder.ToString();
	}

	public void GoToNextPage(int increase = 1)
	{
		int pageToDisplay = Mathf.Clamp(description.pageToDisplay + increase, 1, description.textInfo.pageCount);
		description.pageToDisplay = pageToDisplay;
		pageNumber.text = description.pageToDisplay + "/" + description.textInfo.pageCount;
	}

	public void Hide()
	{
		window.Hide();
		onClose?.Invoke();
	}
}
