using System;
using System.Collections.Generic;
using MapRuleLibrary.Party;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class UIAdventurePartyAssemblyCharacterDisplay : MonoBehaviour
{
	[Serializable]
	private struct CharacterPosition
	{
		public ECharacter character;

		public Vector3 position;
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private List<UICharacterInformation> information;

	[SerializeField]
	private UICharacterSkinSelector skinSelector;

	[SerializeField]
	private UI3DCharacterDisplayEventsChannel characterDisplayEventsChannel;

	protected CMapCharacter character;

	private Action onHidden;

	private void Awake()
	{
		window.onTransitionComplete.AddListener((UIWindow window, UIWindow.VisualState state) =>
		{
			if (state == UIWindow.VisualState.Hidden)
			{
				skinSelector.Hide();
				onHidden?.Invoke();
			}
		});
	}

	public void Display(CMapCharacter characterData)
	{
		window.Show(instant: false);
		characterDisplayEventsChannel.Display(this, characterData.CharacterYMLData.Model, characterData.SkinId);
		foreach (UICharacterInformation item in information)
		{
			item.Display(characterData);
		}
		character = characterData;
		onHidden = null;
		skinSelector.Display(characterData, OnSelectedSkin);
	}

	private void OnSelectedSkin(string skin)
	{
		character.SkinId = skin;
		characterDisplayEventsChannel.Display(this, character.CharacterYMLData.Model, character.SkinId);
		SaveData.Instance.ManualSaveCurrentAdventureData();
	}

	public void Hide(bool instant = false)
	{
		onHidden = null;
		if (instant)
		{
			window.Hide(instant: true);
			characterDisplayEventsChannel.Hide(this);
			return;
		}
		onHidden = () =>
		{
			characterDisplayEventsChannel.Hide(this);
		};
		window.Hide();
	}
}
