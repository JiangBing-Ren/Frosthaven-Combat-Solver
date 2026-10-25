using System.Collections.Generic;
using System.Linq;
using FHGUI.DTO;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.PuzzleBook;

public class PuzzleListContainer : ListContainerBase<UIPuzzleRow, PuzzleListContainer.PuzzleListParams, PuzzleDTO>
{
	public struct PuzzleListParams
	{
		public PuzzleDTO Selected;

		public UIPuzzleRow.PuzzleUIEventHandler OnSelected;
	}

	[SerializeField]
	private ScrollRect _scrollRect;

	private UIPuzzleRow.PuzzleUIEventHandler _onSelect;

	public UIPuzzleRow SelectedPuzzleRow { get; set; }

	protected override void InitList(PuzzleListParams @params, IList<PuzzleDTO> listElementParams)
	{
		base.InitList(@params, listElementParams);
		_onSelect = @params.OnSelected;
	}

	private void OnDisable()
	{
		if (SelectedPuzzleRow != null)
		{
			SelectedPuzzleRow.Select(selected: false);
		}
		SelectedPuzzleRow = null;
		_onSelect = null;
	}

	protected override void InitElement(UIPuzzleRow row, PuzzleDTO puzzleData, PuzzleListParams @params)
	{
		row.Init(puzzleData, (UIPuzzleRow puzzleRow) =>
		{
			OnSelectPuzzle(puzzleRow);
		});
	}

	protected override void AfterInit_Impl(PuzzleListParams @params)
	{
		UIPuzzleRow uIPuzzleRow = null;
		uIPuzzleRow = ((@params.Selected != null) ? Elements.FirstOrDefault((UIPuzzleRow p) => p.Puzzle?.Id == @params.Selected.Id) : Elements.LastOrDefault((UIPuzzleRow p) => p.Puzzle?.IsSelectable ?? false));
		if (!(uIPuzzleRow == null))
		{
			SelectPuzzle(uIPuzzleRow, propagate: false);
		}
	}

	private void OnSelectPuzzle(UIPuzzleRow row, bool propagate = true)
	{
		if (!(SelectedPuzzleRow == row))
		{
			if (SelectedPuzzleRow != null)
			{
				SelectedPuzzleRow.Select(selected: false);
			}
			SelectedPuzzleRow = row;
			SelectedPuzzleRow.Select(selected: true);
			if (propagate)
			{
				_onSelect?.Invoke(SelectedPuzzleRow);
			}
		}
	}

	public void SelectPuzzle(UIPuzzleRow row, bool propagate = true)
	{
		OnSelectPuzzle(row, propagate);
	}
}
