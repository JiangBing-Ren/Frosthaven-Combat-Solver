using UnityEngine;

public abstract class APartyDisplayUI : Singleton<APartyDisplayUI>
{
	[SerializeField]
	protected CanvasGroup canvasGroup;

	public bool EnableInteraction
	{
		set
		{
			canvasGroup.blocksRaycasts = value;
		}
	}

	public abstract APartyCharacterUI SelectedCharacter { get; }

	public abstract void CloseWindows();

	public abstract void OpenCardsPanel(string character, float durationOpen = -1f);

	public virtual void ToggleCardsPanel(string character)
	{
	}

	public abstract void UpdatePartyLevel();
}
