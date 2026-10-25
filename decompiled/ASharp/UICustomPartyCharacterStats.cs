using UnityEngine;

public class UICustomPartyCharacterStats : MonoBehaviour
{
	protected CMapCharacterService character;

	[SerializeField]
	private GameObject levelUpMask;

	public virtual void Setup(CMapCharacterService character)
	{
		ClearEvents();
		this.character = character;
		character.AddCallbackOnLevelUpAvailable(RefreshLevelUp);
		RefreshLevelUp();
	}

	private void OnEnable()
	{
		if (character != null)
		{
			character.AddCallbackOnLevelUpAvailable(RefreshLevelUp);
		}
	}

	private void OnDisable()
	{
		if (character != null)
		{
			ClearEvents();
		}
	}

	public virtual void Clear()
	{
		if (character != null)
		{
			ClearEvents();
			character = null;
		}
	}

	protected virtual void ClearEvents()
	{
		character?.RemoveCallbackOnLevelUpAvailable(RefreshLevelUp);
	}

	public virtual void Show()
	{
	}

	public virtual void Hide()
	{
	}

	public void RefreshLevelUp()
	{
		if (character == null)
		{
			Log.GUIError("Trying to RefreshLevelUp for a null character");
		}
		else if (character.CanLevelup())
		{
			EnableUIForLevelingUp();
		}
		else
		{
			DisableUIForLevelingUp();
		}
	}

	public virtual void DisableUIForLevelingUp()
	{
		levelUpMask.SetActive(value: false);
	}

	public virtual void EnableUIForLevelingUp()
	{
		levelUpMask.SetActive(value: true);
	}
}
