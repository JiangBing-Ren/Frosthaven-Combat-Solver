using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

[RequireComponent(typeof(UIWindow))]
public class FHUISaveGameCreateNew : UISubmenuGOWindow
{
	[SerializeField]
	public TMP_InputField newSaveNameInput;

	[SerializeField]
	public FHButton closeButton;

	[Header("Confirm")]
	[SerializeField]
	private FHButton saveButton;

	[SerializeField]
	private FHButton cancelButton;

	public Action<string> OnConfirm;

	protected override void AwakeInit()
	{
		base.AwakeInit();
		saveButton.onClick.AddListener(HandleOnContinue);
		cancelButton.onClick.AddListener(Hide);
		closeButton.onClick.AddListener(Hide);
		newSaveNameInput.onValueChanged.AddListener(ValidateName);
		newSaveNameInput.onSubmit.AddListener((string s) =>
		{
			HandleOnContinue();
		});
		showAnimator.OnAnimationFinished.AddListener(() =>
		{
			if (newSaveNameInput.text.IsNullOrEmpty())
			{
				newSaveNameInput.ActivateInputField();
				newSaveNameInput.Select();
			}
		});
	}

	public void Init()
	{
		Clear();
		Show();
	}

	private void ValidateName(string arg0)
	{
	}

	private void HandleOnContinue()
	{
		OnConfirm?.Invoke(newSaveNameInput.text);
	}

	private void Clear()
	{
		newSaveNameInput.text = string.Empty;
	}
}
