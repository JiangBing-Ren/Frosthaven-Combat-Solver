using System.Linq;
using FHGUI.DTO;
using FHGUI.Modules;
using FHGUI.Services;
using UnityEngine.Scripting;

namespace FHGUI.States;

public class PuzzleBookViewState : FHMapViewState
{
	private PuzzleDTO _selectedPuzzle;

	private readonly PuzzleBookModule.PuzzleBookModuleContext _context;

	public override StateCategory Category => StateCategory.WorldMap;

	public override FHMapViewLayer.LayerType TargetLayerType { get; set; } = FHMapViewLayer.LayerType.Modal;

	[Preserve]
	private PuzzleBookModule PuzzleBookModule => View.Modules.WorldMap.PuzzleBook;

	private QuickAccessBarViewService QuickAccessBarService => View.GetService<QuickAccessBarViewService>();

	private PuzzleBookViewService PuzzleBookService => View.GetService<PuzzleBookViewService>();

	private UIElementTagsViewService TagsViewService => View.GetService<UIElementTagsViewService>();

	public PuzzleBookViewState()
	{
		_context = new PuzzleBookModule.PuzzleBookModuleContext
		{
			SolveEvent = SolveEvent,
			SelectEvent = SelectEvent,
			NarrationMutedEvent = NarrationMutedEvent,
			OnToggleSpoilers = OnToggleSpoilers,
			SelectedElement = null,
			JustSolved = false,
			ShowSpoilers = TagsViewService.GetGameTagsByKey("UI_TAGS_SHOW_SPOILERS").Any()
		};
	}

	private void OnToggleSpoilers(bool showSpoilers)
	{
		if (showSpoilers)
		{
			TagsViewService.AddPlayerTag("UI_TAGS_SHOW_SPOILERS", "1", 1);
		}
		else
		{
			TagsViewService.RemovePlayerTag("UI_TAGS_SHOW_SPOILERS", "1");
		}
	}

	private void NarrationMutedEvent(bool muted)
	{
		if (!(SaveData.Instance == null))
		{
			SaveData.Instance.Global.MuteStoryVoiceOver = muted;
			if (!SaveData.Instance.IsSavingData)
			{
				SaveData.Instance.SaveGlobalData();
			}
		}
	}

	public override void OnShow()
	{
		SavePayload();
		QuickAccessBarService.Setup(MainViewMainCategories.WorldMap, MainViewSubCategories.PuzzleBook);
		PuzzleBookModule.Init(PuzzleBookService.GetAllPuzzles(), _context, OnClose);
		LoadPayload();
	}

	private void SelectEvent(PuzzleDTO puzzle)
	{
	}

	private void SolveEvent(PuzzleDTO puzzle)
	{
		if (PuzzleBookService.SolvePuzzle(puzzle.Def))
		{
			_context.JustSolved = true;
		}
	}

	private void OnClose()
	{
		PopThisState();
	}

	public override void OnHide()
	{
		ClearPayload();
	}

	protected override void SavePayload()
	{
		_context.SelectedElement = PuzzleBookModule.SelectedPuzzle;
		_context.ShowSpoilers = TagsViewService.GetPlayerTagsByKey("UI_TAGS_SHOW_SPOILERS").Any();
	}

	protected override void LoadPayload()
	{
		if (_context.JustSolved)
		{
			_context.JustSolved = false;
		}
	}

	protected override void ClearPayload()
	{
		_context.SelectedElement = null;
		_context.JustSolved = false;
	}
}
