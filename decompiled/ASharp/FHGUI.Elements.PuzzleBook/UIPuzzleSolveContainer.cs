using System.Collections;
using DG.Tweening;
using FFHGUI.Elements;
using FHGUI.DTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements.PuzzleBook;

public class UIPuzzleSolveContainer : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _solveText;

	[SerializeField]
	private ScrollRect _scroll;

	[SerializeField]
	private UINarrationComponent _narration;

	[SerializeField]
	private FHToggle _muteButton;

	[SerializeField]
	private FHButton _replayButton;

	private Tween _tween;

	private Coroutine _crt;

	private void Awake()
	{
		_muteButton.onValueChanged.AddListener(OnMuteToggle);
		_replayButton.onClick.AddListener(PlayNarration);
	}

	private void PlayNarration()
	{
		_narration.RestartNarration();
		AnimateScroll();
	}

	private void OnMuteToggle(bool arg0)
	{
		_narration.ToggleMute();
	}

	public void Init(PuzzleDTO puzzle, bool autoPlayNarration = true)
	{
		_muteButton.SetIsOnWithoutNotify(value: false);
		if (((object)puzzle != null && !puzzle.IsCompleted) || puzzle.RewardText.IsNullOrEmpty())
		{
			Hide();
			return;
		}
		gameObject.SetActive(value: true);
		_solveText.text = puzzle.RewardText;
		_scroll.ScrollToTop();
		_narration.Stop();
		if (puzzle.RewardSectionID.IsNOTNullOrEmpty())
		{
			_narration.Init("Event_" + puzzle.RewardSectionID + "_Text", new UINarrationComponent.NarrationAssetBundleParams
			{
				AssetBundleDirName = "eventnarrations",
				AssetBundleName = "misc_eventnarrations"
			}, loadSFX: true, autoPlayNarration);
			AnimateScroll();
		}
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
			yield return new WaitForSeconds(wait);
			_tween = _scroll.DOVerticalNormalizedPos(0f, totalDuration - wait);
			yield return _tween;
		}
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	private void OnDisable()
	{
		_narration.Cleanup();
		StopScrollAnimation();
	}

	private void StopScrollAnimation()
	{
		_tween?.Kill();
		_tween = null;
		if (_crt != null)
		{
			StopCoroutine(_crt);
		}
		_crt = null;
	}
}
