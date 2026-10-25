using System.Collections;
using Chronos;
using UnityEngine;
using UnityEngine.UI;

public class UIPromotionTransition : MonoBehaviour
{
	[SerializeField]
	private Image imageHolder;

	[SerializeField]
	private GUIAnimator transitionAnimator;

	[SerializeField]
	private float delay = 5f;

	public Sprite[] images;

	private int currentImage;

	private bool isPlaying;

	private void Awake()
	{
		transitionAnimator.OnAnimationFinished.AddListener(Wait);
	}

	public void StartTransition()
	{
		if (!isPlaying)
		{
			isPlaying = true;
			currentImage = 0;
			imageHolder.sprite = images[currentImage];
			if (images.Length > 1)
			{
				Wait();
			}
		}
	}

	private void Wait()
	{
		StartCoroutine(ShowNextImage());
	}

	private IEnumerator ShowNextImage()
	{
		yield return Timekeeper.instance.WaitForSeconds(delay);
		currentImage = (currentImage + 1) % images.Length;
		imageHolder.sprite = images[currentImage];
		transitionAnimator.Play();
	}

	private void OnDisable()
	{
		isPlaying = false;
		transitionAnimator.Stop();
	}

	public void StopTransition()
	{
		isPlaying = false;
		StopAllCoroutines();
		transitionAnimator.Stop(goToEnd: true);
	}
}
