using System.Collections.Generic;
using System.Linq;
using FHGUI;
using FHGUI.DTO;
using FHGUI.Elements;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class TestingPartyScene : MonoBehaviour
{
	public Camera UICamera;

	public Canvas Canvas;

	public FHPartyViewScene Scene;

	public EditorSceneDirectPlaySpawner Spawner;

	public ECharacter[] Characters = new ECharacter[4]
	{
		ECharacter.Blinkblade,
		ECharacter.BannerSpear,
		ECharacter.Boneshaper,
		ECharacter.None
	};

	private void Awake()
	{
		if (SceneManager.sceneCount > 1)
		{
			Cleanup();
			return;
		}
		UniversalAdditionalCameraData component = Scene.Camera.GetComponent<UniversalAdditionalCameraData>();
		component.cameraStack.Clear();
		component.cameraStack.Add(UICamera);
		Spawner.gameObject.SetActive(value: true);
		Scene.Show(new FHPartyViewScene.PartySceneParams
		{
			SlotData = new List<PartySlotDTO>
			{
				new PartySlotDTO
				{
					Character = Characters.ElementAt(0),
					Owned = true
				},
				new PartySlotDTO
				{
					Character = Characters.ElementAt(1),
					Owned = true
				},
				new PartySlotDTO
				{
					Character = Characters.ElementAt(2),
					Owned = false
				},
				new PartySlotDTO
				{
					Character = Characters.ElementAt(3),
					Owned = false
				}
			}
		});
	}

	private IEnumerable<UIPartySlotHints.HintedActionParams> SlotToHintedActions(PartySlotDTO partyslot)
	{
		bool isSlowOwner = partyslot.Owned;
		if (partyslot.IsEmpty)
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HintKey = "GUI_CREATE_CHARACTER_NAME_CONFIRM",
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, "GUI_CREATE_CHARACTER_NAME_CONFIRM"),
				ColorSchemeID = "PositiveAction",
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					Debug.Log("Create");
				}
			};
		}
		else
		{
			yield return new UIPartySlotHints.HintedActionParams
			{
				HintKey = (isSlowOwner ? "GUI_EDIT_CHARACTER" : "GUI_VIEW_CHARACTER"),
				HitLocalizedOverride = FHUILocFormatting.GetActionIconFormat(FHInputEventElement.UserInteractionType.LeftClick, isSlowOwner ? "GUI_EDIT_CHARACTER" : "GUI_VIEW_CHARACTER"),
				ColorSchemeID = "PositiveAction",
				InteractionType = FHInputEventElement.UserInteractionType.LeftClick,
				ActionCallback = () =>
				{
					Debug.Log(isSlowOwner ? "Edit" : "View");
				}
			};
		}
	}

	private void Cleanup()
	{
		Object.Destroy(UICamera.gameObject);
		Object.Destroy(Canvas.gameObject);
		Object.Destroy(gameObject);
	}
}
