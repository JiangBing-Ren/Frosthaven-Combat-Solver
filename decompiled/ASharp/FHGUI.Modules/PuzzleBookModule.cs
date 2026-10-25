using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using FHGUI.Elements.PuzzleBook;
using UnityEngine;

namespace FHGUI.Modules;

public class PuzzleBookModule : FHMapViewModule
{
	public class PuzzleBookModuleContext
	{
		public PuzzleEventHandler SolveEvent;

		public PuzzleEventHandler SelectEvent;

		public Action<bool> NarrationMutedEvent;

		public Action<bool> OnToggleSpoilers;

		public PuzzleDTO SelectedElement;

		public bool JustSolved;

		public bool ShowSpoilers;
	}

	[SerializeField]
	private PuzzleListContainer _puzzleContainer;

	[SerializeField]
	private PuzzleStepListContainer _puzzleStepsContainer;

	[SerializeField]
	private UIPuzzleInfo _puzzleInfo;

	private readonly List<PuzzleDTO> _puzzles = new List<PuzzleDTO>();

	private Action _onClose;

	private PuzzleBookModuleContext _context;

	public PuzzleDTO SelectedPuzzle => _puzzleContainer.SelectedPuzzleRow?.Puzzle;

	protected override void OnHide()
	{
		base.OnHide();
		_onClose = null;
		_context = null;
	}

	public void Init(IEnumerable<PuzzleDTO> puzzles, PuzzleBookModuleContext context, Action onClose = null)
	{
		_puzzles.Clear();
		_puzzles.AddRange(puzzles);
		_onClose = onClose;
		_context = context;
		if (!_puzzles.Any())
		{
			_puzzleInfo.Init(null, _context);
			_puzzleStepsContainer.Init(null, Array.Empty<PersonalQuestStepDTO>());
			return;
		}
		_puzzleContainer.Init(new PuzzleListContainer.PuzzleListParams
		{
			OnSelected = OnPuzzleSelected,
			Selected = _context?.SelectedElement
		}, _puzzles);
		_puzzleStepsContainer.Init(null, _puzzleContainer.SelectedPuzzleRow.Puzzle.Steps);
		_puzzleInfo.Init(_puzzleContainer.SelectedPuzzleRow.Puzzle, _context);
	}

	private void OnPuzzleSelected(UIPuzzleRow row)
	{
		_puzzleInfo.Init(row.Puzzle, _context);
		_puzzleStepsContainer.Init(null, row.Puzzle.Steps);
		_context?.SelectEvent?.Invoke(row.Puzzle);
	}

	public void Close()
	{
		_onClose?.Invoke();
		gameObject.SetActive(value: false);
	}
}
