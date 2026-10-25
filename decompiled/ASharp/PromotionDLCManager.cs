using System.Collections.Generic;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.UI;

public class PromotionDLCManager : Singleton<PromotionDLCManager>
{
	private class PromotionDLCState
	{
		private Sprite[] images;

		private int currentImage;

		public DLCRegistry.EDLCKey DLC { get; }

		public bool HasSeveralImages
		{
			get
			{
				if (!images.IsNullOrEmpty())
				{
					return images.Length > 1;
				}
				return false;
			}
		}

		public PromotionDLCState(DLCRegistry.EDLCKey dlc)
		{
			DLC = dlc;
			images = UIInfoTools.Instance.GetDLCPromotionSprites(dlc);
			currentImage = -1;
		}

		public Sprite GetImage()
		{
			currentImage = (currentImage + 1) % images.Length;
			return images[currentImage];
		}
	}

	[SerializeField]
	private UIWindow window;

	[SerializeField]
	private UIPromotionDLCSlot buyDLCPrefab;

	[SerializeField]
	private GUIAnimator imageTransitionAnimator;

	[SerializeField]
	private UIPromotionDLCSlot comingSoonDLCPrefab;

	private List<PromotionDLCState> dlcToShow = new List<PromotionDLCState>();

	private int currentDlc;

	protected override void AwakeInit()
	{
		window.onHidden.AddListener(imageTransitionAnimator.Stop);
		dlcToShow.Clear();
		DLCRegistry.EDLCKey[] dLCKeys = DLCRegistry.DLCKeys;
		foreach (DLCRegistry.EDLCKey dlc in dLCKeys)
		{
			if (UIInfoTools.Instance.GetDLCState(dlc) != DLCConfig.EDLCState.Unavailable)
			{
				dlcToShow.Add(new PromotionDLCState(dlc));
			}
		}
		if (dlcToShow.IsNullOrEmpty())
		{
			gameObject.SetActive(value: false);
			return;
		}
		buyDLCPrefab.OnClick.AddListener(GoToStore);
		comingSoonDLCPrefab.OnClick.AddListener(GoToStore);
		imageTransitionAnimator.OnAnimationFinished.AddListener(ShowNextPromotion);
	}

	private void ShowNextPromotion()
	{
		ShowAvailableDLC(currentDlc + 1);
	}

	private void ShowAvailableDLC(int dlcIndex)
	{
		currentDlc = dlcIndex % dlcToShow.Count;
		PromotionDLCState promotionDLCState = dlcToShow[currentDlc];
		if (UIInfoTools.Instance.GetDLCState(promotionDLCState.DLC) == DLCConfig.EDLCState.Available)
		{
			comingSoonDLCPrefab.gameObject.SetActive(value: false);
			buyDLCPrefab.SetDLC(promotionDLCState.DLC, promotionDLCState.GetImage());
			buyDLCPrefab.gameObject.SetActive(value: true);
		}
		else
		{
			buyDLCPrefab.gameObject.SetActive(value: false);
			comingSoonDLCPrefab.SetDLC(promotionDLCState.DLC, promotionDLCState.GetImage());
			comingSoonDLCPrefab.gameObject.SetActive(value: true);
		}
		if (dlcToShow.Count > 1 || promotionDLCState.HasSeveralImages)
		{
			imageTransitionAnimator.Play();
		}
	}

	private void GoToStore(DLCRegistry.EDLCKey dlc)
	{
		PlatformLayer.Instance.NewPlatformLayer.OpenStore();
	}

	public void Hide()
	{
		imageTransitionAnimator.Stop();
		window.Hide();
	}

	public void Show()
	{
		if (dlcToShow.Count != 0)
		{
			window.Show();
			ShowAvailableDLC(0);
		}
	}

	private void OnDisable()
	{
		imageTransitionAnimator.Stop();
	}
}
