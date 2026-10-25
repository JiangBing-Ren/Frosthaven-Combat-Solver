using System.Collections;
using DG.Tweening;
using FFHGUI.Elements;
using FHGUI.DTO;
using FHGUI.Modules;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.PuzzleBook;

public class UIPuzzleInfo : MonoBehaviour
{
	[SerializeField]
	private Image _puzzleImage;

	[SerializeField]
	private TMP_Text _description;

	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private FHButton _solveButton;

	[SerializeField]
	private GameObject _requirementsMet;

	[SerializeField]
	private ScrollRect _scroll;

	[SerializeField]
	private UINarrationComponent _narration;

	[SerializeField]
	private FHToggle _playButton;

	[SerializeField]
	private FHButton _replayButton;

	[SerializeField]
	private FHToggle _showSpoilersButton;

	[SerializeField]
	private TMP_Text _showSpoilersButtonText;

	private Tween _tween;

	private Coroutine _crt;

	private PuzzleBookModule.PuzzleBookModuleContext _context;

	public PuzzleDTO Puzzle { get; private set; }

	private void Awake()
	{
		_solveButton.onClick.AddListener(OnSolve);
		_playButton.onValueChanged.AddListener(ToggleNarration);
		_replayButton.onClick.AddListener(PlayNarration);
		_showSpoilersButton.onValueChanged.AddListener(ToggleShowSpoilers);
	}

	private void ToggleShowSpoilers(bool arg0)
	{
		_context.OnToggleSpoilers(arg0);
		_context.ShowSpoilers = arg0;
		Image puzzleImage = _puzzleImage;
		Sprite sprite;
		if (Puzzle.IsCompleted)
		{
			sprite = (_context.ShowSpoilers ? UIInfoTools.Instance.GetPuzzleCompletedImage(Puzzle.Def.ID) : UIInfoTools.Instance.GetPuzzleImage(Puzzle.Def.ID));
		}
		else
		{
			sprite = UIInfoTools.Instance.GetPuzzleImage(Puzzle.Def.ID);
		}
		puzzleImage.sprite = sprite;
	}

	private void OnDisable()
	{
		Puzzle = null;
		_context = null;
		_narration.Cleanup();
		StopScrollAnimation();
	}

	public void Init(PuzzleDTO puzzle, PuzzleBookModule.PuzzleBookModuleContext context)
	{
		PuzzleDTO puzzle2 = Puzzle;
		Puzzle = puzzle;
		_context = context;
		if (puzzle == null)
		{
			SetEmpty();
			return;
		}
		SetData();
		if (!(puzzle2 == puzzle))
		{
			ResetNarration();
			bool flag = (object)puzzle != null && puzzle.IsCompleted && puzzle.RewardSectionID.IsNOTNullOrEmpty();
			_playButton.SetTooltipText(LocalizationManager.GetTranslation("GUI_NARRATION_PAUSE"), LocalizationManager.GetTranslation("GUI_NARRATION_PLAY"), string.Empty);
			_playButton.gameObject.SetActive(flag);
			_replayButton.gameObject.SetActive(flag && _narration.Progress > 0f);
			if (flag)
			{
				_narration.Init("Event_" + puzzle.RewardSectionID + "_Text", new UINarrationComponent.NarrationAssetBundleParams
				{
					AssetBundleDirName = "eventnarrations",
					AssetBundleName = "misc_eventnarrations"
				}, loadSFX: true, autoPlay: false, useGlobalMute: false, null, OnNarrationEnd);
			}
		}
	}

	private void OnNarrationEnd()
	{
		_replayButton.gameObject.SetActive(value: true);
		_playButton.SetIsOnWithoutNotify(value: false);
		StopScrollAnimation();
		_scroll.verticalNormalizedPosition = 0f;
	}

	private void SetEmpty()
	{
		_description.text = string.Empty;
		_name.text = string.Empty;
		_solveButton.gameObject.SetActive(value: false);
		_puzzleImage.enabled = false;
		_narration.Stop();
		_playButton.gameObject.SetActive(value: false);
		_showSpoilersButton.gameObject.SetActive(value: false);
		_replayButton.gameObject.SetActive(value: false);
	}

	private void SetData()
	{
		_description.text = (Puzzle.IsCompleted ? Puzzle.RewardText : Puzzle.Description);
		_name.text = Puzzle.Name;
		_solveButton.gameObject.SetActive(value: true);
		_solveButton.interactable = Puzzle.CanSolve;
		_requirementsMet.SetActive(Puzzle.CanSolve);
		_puzzleImage.enabled = true;
		Image puzzleImage = _puzzleImage;
		Sprite sprite;
		if (Puzzle.IsCompleted)
		{
			sprite = (_context.ShowSpoilers ? UIInfoTools.Instance.GetPuzzleCompletedImage(Puzzle.Def.ID) : UIInfoTools.Instance.GetPuzzleImage(Puzzle.Def.ID));
		}
		else
		{
			sprite = UIInfoTools.Instance.GetPuzzleImage(Puzzle.Def.ID);
		}
		puzzleImage.sprite = sprite;
		_showSpoilersButton.SetIsOnWithoutNotify(_context.ShowSpoilers);
		_showSpoilersButton.gameObject.SetActive(Puzzle.IsCompleted);
	}

	private void UpdateContext(PuzzleBookModule.PuzzleBookModuleContext context)
	{
		_context = context;
		SetData();
	}

	private void AnimateScroll()
	{
		StopScrollAnimation();
		_crt = StartCoroutine(AnimateScroll_Crt());
	}

	private IEnumerator AnimateScroll_Crt()
	{
		float totalDuration = _narration.TotalLength;
		if (totalDuration != 0f)
		{
			float y = _scroll.content.sizeDelta.y;
			float num = ((y > 0f) ? ((transform as RectTransform).sizeDelta.y / y) : 0f);
			_scroll.ScrollToTop();
			float num2 = 0.75f;
			float wait = num * totalDuration * num2;
			_scroll.scrollSensitivity = 0f;
			yield return new WaitForSeconds(wait);
			_tween = _scroll.DOVerticalNormalizedPos(0f, totalDuration - wait).SetEase(Ease.Linear);
			yield return _tween;
		}
	}

	private void StopScrollAnimation()
	{
		_tween?.Kill();
		_tween = null;
		_scroll.scrollSensitivity = 0.2f;
		if (_crt != null)
		{
			StopCoroutine(_crt);
		}
		_crt = null;
	}

	private void PauseScrollAnimation()
	{
		_tween?.Pause();
		_scroll.scrollSensitivity = 0.2f;
	}

	private void UnpauseScrollAnimation()
	{
		_tween?.Play();
		_scroll.scrollSensitivity = 0f;
	}

	private void PlayNarration()
	{
		_narration.PlayNarration();
		AnimateScroll();
	}

	private void StopNarration()
	{
		_narration.Stop();
		StopScrollAnimation();
	}

	private void PauseNarration()
	{
		_narration.Pause();
		PauseScrollAnimation();
	}

	private void UnpauseNarration()
	{
		_narration.UnPause();
		UnpauseScrollAnimation();
	}

	private void ResetNarration()
	{
		_narration.Cleanup();
		_playButton.SetIsOnWithoutNotify(value: false);
		StopScrollAnimation();
	}

	private void ToggleNarration(bool active)
	{
		if (active)
		{
			if (_narration.HasEnded)
			{
				_narration.RestartNarration();
			}
			else if (_narration.Progress > 0f)
			{
				UnpauseNarration();
			}
			else
			{
				PlayNarration();
			}
		}
		else if (_narration.Progress > 0f)
		{
			PauseNarration();
		}
	}

	private void OnSolve()
	{
		PuzzleDTO puzzle = Puzzle;
		if ((object)puzzle != null && puzzle.CanSolve)
		{
			_context?.SolveEvent?.Invoke(Puzzle);
		}
	}
}
