using System.Collections.Generic;
using Assets.Script.Misc;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace GLOOM.MainMenu;

public class UIEnableDLCAnimator : MonoBehaviour
{
	[SerializeField]
	private GUIAnimator enableDLCAnimator;

	[SerializeField]
	private TextLocalizedListener titleBanner;

	[SerializeField]
	private List<Image> icons;

	[SerializeField]
	private List<Image> glows;

	private CallbackPromise promise;

	private void Awake()
	{
		enableDLCAnimator.OnAnimationFinished.AddListener(Finish);
	}

	public ICallbackPromise Play(DLCRegistry.EDLCKey dlc)
	{
		GloomUtility.GetEnumCategory(dlc);
		titleBanner.SetTextKey(dlc.ToString());
		enableDLCAnimator.Play();
		gameObject.SetActive(value: true);
		promise = new CallbackPromise();
		return promise;
	}

	[ContextMenu("Play")]
	private void Play()
	{
		enableDLCAnimator.Play();
		gameObject.SetActive(value: true);
	}

	private void Finish()
	{
		promise?.Resolve();
		Stop();
	}

	public void Stop()
	{
		if (gameObject.activeSelf)
		{
			gameObject.SetActive(value: false);
			enableDLCAnimator.Stop();
			if (promise != null && promise.IsPending)
			{
				promise.Cancel();
			}
		}
	}

	private void OnDisable()
	{
		enableDLCAnimator.Stop();
	}
}
