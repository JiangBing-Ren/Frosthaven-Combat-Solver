using System;
using System.Collections.Generic;
using FHGUI.DTO;
using FHGUI.Elements;
using FHGUI.Modules;
using FHGUI.States;
using UnityEngine.UI;

namespace FHGUI.Services;

public class InputActionHintsService : FHMapViewService
{
	public InputActionHintsService(FHMapView view)
		: base(view)
	{
	}

	public void Setup(InputActionHintsModule inputActionHints, FHMapViewState viewState)
	{
		InputActionHintsModule.Params obj = new InputActionHintsModule.Params
		{
			ButtonsParams = new List<ButtonsBarParam>()
		};
		foreach (ButtonsBarParam item in GetDefaultButtonsForViewState(viewState))
		{
			obj.ButtonsParams.Add(item);
		}
		inputActionHints.Init(obj);
	}

	private void TryOpenSettings()
	{
		if (!_view.InTransition && Singleton<ESCMenu>.IsInitialized)
		{
			Singleton<ESCMenu>.InstanceFast.GetComponent<UIWindow>().Show();
		}
	}

	private void TryCloseTab()
	{
		TryCloseTab(null);
	}

	private void TryCloseTab(Func<bool> blockingCondition = null)
	{
		if (!_view.InTransition && (blockingCondition == null || !blockingCondition()))
		{
			if (_view.GetService<QuickAccessBarViewService>().CurrentTab != MainViewSubCategories.None)
			{
				_view.GetService<QuickAccessBarViewService>().CloseCurrentTab();
			}
			else
			{
				_view.DecideViewStateFromGameState();
			}
		}
	}

	private IEnumerable<ButtonsBarParam> GetDefaultButtonsForViewState(FHMapViewState state)
	{
		if (!(state is TravellingMerchantViewState) && !(state is ShopViewState) && !(state is AlchemistViewState) && !(state is EnhancerViewState) && !(state is TownPerksViewState) && !(state is CraftingViewState) && !(state is EditCharacterViewState) && !(state is UnlockCardsViewState) && !(state is CharacterPerksViewState))
		{
			if (!(state is ManageRosterViewState))
			{
				if (!(state is HandManagementViewState))
				{
					if (!(state is ScenarioSelectionViewState) && !(state is OutpostVillageMapState))
					{
						if (!(state is BattleGoalSelectionViewState) && !(state is ChallengeSelectionViewState) && !(state is PetsSelectionViewState) && !(state is FavorsSelectionViewState) && !(state is ScenarioBriefingSummaryViewState))
						{
							if (state is DistributionViewState || state is EventViewState)
							{
								yield break;
							}
							if (!(state is NarrationViewState))
							{
								if (state is OutpostAttackViewState || state is BuildingMaterialDistributionViewState || state is CharacterEquipmentViewState)
								{
									yield break;
								}
								if (!(state is ProloguePartyCreationViewState))
								{
									if (!(state is GenericListModalViewState) && !(state is GenericModalViewState) && !(state is ImportFromPartyViewState) && !(state is InitViewState) && !(state is RenameCharacterViewState) && !(state is PersonalQuestSelectionViewState) && !(state is SpendStartingGoldViewState) && !(state is LinkingSectionViewState) && !(state is WorldTravelViewState) && !(state is ScenarioUnlockViewState) && !(state is WorldViewState) && !(state is ScenarioRewardsViewState))
									{
										Log.GUIWarning($"View State {state} is not listed. Please include desired behaviors");
									}
								}
								else
								{
									yield return Button_Settings_Close();
									yield return Button_Settings();
								}
							}
							else
							{
								yield return Button_Settings();
							}
						}
						else
						{
							yield return Button_Settings_Close();
							yield return Button_Settings();
						}
					}
					else
					{
						yield return Button_Settings_Close();
						yield return Button_Settings();
					}
					yield break;
				}
				yield return Button_Close(() =>
				{
					TryCloseTab(() => !((state as HandManagementViewState)?.IsHandValid(useTempCards: true, out var _) ?? false));
				});
				yield break;
			}
			yield return Button_Close(() =>
			{
				TryCloseTab(() => _view.SelectedCharacter == null);
			});
			yield return Button_Settings();
		}
		else
		{
			yield return Button_Close(TryCloseTab);
			yield return Button_Settings();
		}
	}

	public ButtonsBarParam Button_Close(Action callback)
	{
		return new ButtonsBarParam("GUI_BACK", InputFHManager.Controls.Shared.Close, callback);
	}

	public ButtonsBarParam Button_Back(Action callback)
	{
		return new ButtonsBarParam("GUI_BACK", InputFHManager.Controls.Generic.Back, callback);
	}

	public ButtonsBarParam Button_Settings()
	{
		return new ButtonsBarParam("GUI_Settings", InputFHManager.Controls.Shared.ShowSettings, TryOpenSettings);
	}

	public ButtonsBarParam Button_Settings_Close()
	{
		return new ButtonsBarParam("GUI_Settings", InputFHManager.Controls.Shared.Close, TryOpenSettings);
	}
}
