using DevLocker.GFrame.Input;
using DevLocker.GFrame.Input.Contexts;
using DevLocker.GFrame.Input.UIScope;
using UnityEngine;
using UnityEngine.InputSystem;

public class CardsHandManagerInput : MonoBehaviour, FHControls.IScenarioDecksActions
{
	private void OnEnable()
	{
		InputFHManager.Controls.Enable(this, InputFHManager.Controls.ScenarioDecks);
		InputFHManager.Controls.ScenarioDecks.SetCallbacks(this);
	}

	private void OnDisable()
	{
		if (InputFHManager.Controls != null && !GloomUtility.IsAppQuitting)
		{
			InputFHManager.Controls.DisableAll(this);
			InputFHManager.Controls.ScenarioDecks.SetCallbacks(null);
		}
	}

	public void OnOpenViewAllCardsWindow(InputAction.CallbackContext context)
	{
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		if (((object)s_Instance != null && s_Instance.IsEditing) || InputFHManager.IsTextFieldFocused)
		{
			return;
		}
		bool flag = IsInActiveScope();
		if (Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			if (context.performed & flag)
			{
				Singleton<CardsHandManager>.Instance.ShowAllDecks(openByKey: false);
			}
			return;
		}
		if (context.started & flag)
		{
			Singleton<CardsHandManager>.Instance.ShowAllDecks(openByKey: true);
		}
		if (context.canceled)
		{
			Singleton<CardsHandPreviewWindow>.Instance.Cancel();
		}
	}

	public void OnViewHeroDeck1(InputAction.CallbackContext context)
	{
		ViewHeroDeck(context, 0);
	}

	public void OnViewHeroDeck2(InputAction.CallbackContext context)
	{
		ViewHeroDeck(context, 1);
	}

	public void OnViewHeroDeck3(InputAction.CallbackContext context)
	{
		ViewHeroDeck(context, 2);
	}

	public void OnViewHeroDeck4(InputAction.CallbackContext context)
	{
		ViewHeroDeck(context, 3);
	}

	private void ViewHeroDeck(InputAction.CallbackContext context, int handIndex)
	{
		LevelEditorController s_Instance = LevelEditorController.s_Instance;
		if (((object)s_Instance == null || !s_Instance.IsEditing) && !InputFHManager.IsTextFieldFocused)
		{
			bool flag = IsInActiveScope();
			if (context.started & flag)
			{
				Singleton<CardsHandManager>.Instance.ToggleViewHeroCards(handIndex);
			}
			if (context.canceled)
			{
				Singleton<CardsHandManager>.Instance.CloseViewHeroCards(handIndex);
			}
		}
	}

	private bool IsInActiveScope()
	{
		if (!Singleton<CardsHandManager>.Instance.isActiveAndEnabled)
		{
			return false;
		}
		UIScope uIScope = UIScope.FocusedScope(PlayerContextUIRootObject.GlobalPlayerContext);
		if (uIScope != null && uIScope.FocusLayer == null)
		{
			return uIScope.transform.IsChildOf(transform);
		}
		return false;
	}
}
