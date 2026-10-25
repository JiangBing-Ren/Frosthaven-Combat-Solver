using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHRL;

namespace FHGUI.Services;

public class PuzzleBookViewService : FHMapViewService
{
	public const string UI_SHOW_SPOILERS_TAG = "UI_TAGS_SHOW_SPOILERS";

	private PuzzleController PuzzleController => GameController.PuzzleController;

	public bool IsPuzzleBookUnlocked => _view.GameController.PuzzleController.IsPuzzleBookUnlocked;

	public bool HasPuzzles => _view.GameController.PuzzleController.UnlockedPuzzles.Any();

	public PuzzleBookViewService(FHMapView view)
		: base(view)
	{
	}

	public IEnumerable<PuzzleDTO> GetUnlockedPuzzles()
	{
		return PuzzleController.UnlockedPuzzles.Select(GetPuzzle);
	}

	public IEnumerable<PuzzleDTO> GetCompletedPuzzles()
	{
		return PuzzleController.CompletedPuzzles.Select(GetPuzzle);
	}

	public IEnumerable<PuzzleDTO> GetIncompletePuzzles()
	{
		return PuzzleController.IncompletePuzzles.Select(GetPuzzle);
	}

	public IEnumerable<PuzzleDTO> GetAllPuzzles()
	{
		return from p in DefRepo.Puzzles
			select GetPuzzle(p.Value) into p
			orderby PuzzleDef.PuzzleIDs.Order.IndexOf(p.Def.ID)
			select p;
	}

	public bool SolvePuzzle(PuzzleID puzzleID)
	{
		PuzzleDef puzzleDef = DefRepo.Puzzles[puzzleID];
		PuzzleState puzzleState = PuzzleController.GetPuzzleState(puzzleDef);
		if (!_view.IsPartyLeader || !PuzzleController.CanSolvePuzzle(puzzleState))
		{
			return false;
		}
		try
		{
			Choreographer.ExecuteCommand(FHRPC.Campaign.Puzzle.SolvePuzzle, puzzleDef);
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}

	public bool SolvePuzzle(PuzzleDef puzzleDef)
	{
		return SolvePuzzle(puzzleDef.ID);
	}

	public PuzzleDTO GetPuzzle(PuzzleState puzzleState)
	{
		return new PuzzleDTO(puzzleState, _view.GameController);
	}

	public PuzzleDTO GetPuzzle(PuzzleID puzzleID)
	{
		PuzzleDef def = DefRepo.Puzzles[puzzleID];
		PuzzleState puzzleState = PuzzleController.GetPuzzleState(def);
		if (puzzleState != null)
		{
			return new PuzzleDTO(puzzleState, _view.GameController);
		}
		return new PuzzleDTO(def, _view.GameController)
		{
			IsLocked = true
		};
	}

	public PuzzleDTO GetPuzzle(PuzzleDef puzzleDef)
	{
		PuzzleState puzzleState = PuzzleController.GetPuzzleState(puzzleDef);
		if (puzzleState != null)
		{
			return new PuzzleDTO(puzzleState, _view.GameController);
		}
		return new PuzzleDTO(puzzleDef, _view.GameController)
		{
			IsLocked = true
		};
	}
}
