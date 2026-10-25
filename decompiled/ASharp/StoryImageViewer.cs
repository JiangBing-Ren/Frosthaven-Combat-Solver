using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StoryImageViewer : MonoBehaviour
{
	private class LoadImageToken
	{
		private Dictionary<string, Sprite> loadedImages = new Dictionary<string, Sprite>();

		private List<string> loadingImages = new List<string>();

		public void Load(List<string> imagesIds)
		{
			foreach (string imagesId in imagesIds)
			{
				Load(imagesId);
			}
		}

		private void Load(string imageId)
		{
			if (imageId.IsNullOrEmpty())
			{
				return;
			}
			if (loadedImages.ContainsKey(imageId))
			{
				Log.GUIInfo("Already loaded " + imageId);
				return;
			}
			NarrativeConfigUI narrativeImageConfig = UIInfoTools.Instance.GetNarrativeImageConfig(imageId);
			if (narrativeImageConfig != null && !(narrativeImageConfig.picture != null))
			{
				if (loadingImages.Contains(imageId))
				{
					Log.GUIInfo("Already loading " + imageId);
					return;
				}
				Log.GUIInfo("Start loading " + narrativeImageConfig.pathPicture);
				loadingImages.Add(imageId);
				CoroutineHelper.RunCoroutine(LoadImageAsync(narrativeImageConfig.pathPicture, imageId));
			}
		}

		private IEnumerator LoadImageAsync(string path, string imageID)
		{
			path = path.Replace("\\", "/");
			byte[] bytes = null;
			string path2 = Path.Combine(Application.streamingAssetsPath, path + ".png");
			if (File.Exists(path2))
			{
				Task<byte[]> asyncRead = File.ReadAllBytesAsync(path2);
				while (!asyncRead.IsCompleted)
				{
					yield return null;
				}
				if (asyncRead.IsCompletedSuccessfully)
				{
					bytes = asyncRead.Result;
				}
				else
				{
					Log.GUIError($"Error loading image ID: '{imageID}' Path: '{path}' Exception: {asyncRead.Exception}");
				}
			}
			else
			{
				Log.GUIError("File not found while trying to load image ID: '" + imageID + "' Path: '" + path + "'");
			}
			if (bytes == null)
			{
				loadingImages.Remove(imageID);
				Log.GUIError("Unable to load image from path " + path);
				yield break;
			}
			Texture2D texture2D = new Texture2D(2, 2);
			texture2D.LoadImage(bytes);
			Sprite value = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
			loadingImages.Remove(imageID);
			loadedImages[imageID] = value;
		}

		private IEnumerator WaitForImageLoad(string imageID, Action<Sprite> onLoaded)
		{
			while (loadingImages.Contains(imageID))
			{
				yield return null;
			}
			if (loadedImages.ContainsKey(imageID))
			{
				onLoaded(loadedImages[imageID]);
			}
			else
			{
				onLoaded(null);
			}
		}

		public void Get(string imageId, Action<Sprite> onLoaded)
		{
			if (imageId.IsNullOrEmpty())
			{
				onLoaded(null);
			}
			else if (loadedImages.ContainsKey(imageId))
			{
				Log.GUIInfo("Get loaded image " + imageId);
				onLoaded(loadedImages[imageId]);
			}
			else if (loadingImages.Contains(imageId))
			{
				Log.GUIInfo("Get loading image " + imageId);
				CoroutineHelper.RunCoroutine(WaitForImageLoad(imageId, onLoaded));
			}
			else
			{
				NarrativeConfigUI narrativeImageConfig = UIInfoTools.Instance.GetNarrativeImageConfig(imageId);
				onLoaded((narrativeImageConfig?.picture != null) ? narrativeImageConfig.picture : null);
			}
		}

		public void Free()
		{
			loadedImages.Clear();
			loadingImages.Clear();
		}

		public void Cancel(string imageId)
		{
			if (imageId.IsNOTNullOrEmpty() && loadingImages.Contains(imageId))
			{
				Log.GUIInfo("Cancel loading image " + imageId);
				loadingImages.Remove(imageId);
			}
		}

		public bool AreAllImagesLoaded()
		{
			return loadingImages.Count == 0;
		}
	}

	[SerializeField]
	private GameObject container;

	[SerializeField]
	private Image imageHolder;

	[SerializeField]
	private GUIAnimator loadImagesAnimator;

	[SerializeField]
	private GUIAnimator showImageAnimator;

	private Action onShownImage;

	private LoadImageToken token;

	private Action onLoaded;

	private string currentImage;

	private void Awake()
	{
		loadImagesAnimator.OnAnimationFinished.AddListener(WaitLoadImage);
		loadImagesAnimator.OnAnimationStarted.AddListener(() =>
		{
			Log.GUIInfo("Start load images anim");
		});
		loadImagesAnimator.OnAnimationStopped.AddListener(() =>
		{
			Log.GUIInfo("Load images anim stopped");
		});
		if (showImageAnimator != null)
		{
			showImageAnimator.OnAnimationFinished.AddListener(() =>
			{
				onShownImage?.Invoke();
			});
		}
	}

	public void LoadImages(List<string> images, Action onLoaded)
	{
		this.onLoaded = onLoaded;
		if (images.Count > 0)
		{
			Log.GUIInfo("LoadImages");
			if (loadImagesAnimator.IsPlaying)
			{
				Log.GUIError("Stop LoadImages Animator");
				loadImagesAnimator.Stop();
			}
			imageHolder.gameObject.SetActive(value: false);
			container.SetActive(value: true);
			token = new LoadImageToken();
			token.Load(images);
			loadImagesAnimator.Play();
		}
		else
		{
			Hide();
		}
	}

	public void Show(string image, Action onShown = null)
	{
		if (currentImage == image || image.IsNullOrEmpty())
		{
			onShown?.Invoke();
			return;
		}
		token.Cancel(currentImage);
		currentImage = image;
		onShownImage = onShown;
		token.Get(image, (Sprite sprite) =>
		{
			if (!(currentImage != image))
			{
				if (sprite == null)
				{
					onShown?.Invoke();
				}
				else
				{
					imageHolder.sprite = sprite;
					imageHolder.gameObject.SetActive(value: true);
					container.SetActive(value: true);
					if (showImageAnimator != null)
					{
						Log.GUIInfo("Show image " + currentImage);
						showImageAnimator.Play();
					}
					else
					{
						onShown?.Invoke();
					}
				}
			}
		});
	}

	public void Hide()
	{
		container?.SetActive(value: false);
		OnHidden();
	}

	private void OnDisable()
	{
		OnHidden();
	}

	private void OnHidden()
	{
		StopAllCoroutines();
		currentImage = null;
		loadImagesAnimator.Stop();
		showImageAnimator?.Stop();
		token?.Free();
	}

	private void WaitLoadImage()
	{
		Log.GUIInfo("WaitLoadImage");
		StartCoroutine(CheckLoaded());
	}

	private IEnumerator CheckLoaded()
	{
		yield return new WaitUntil(token.AreAllImagesLoaded);
		onLoaded?.Invoke();
	}
}
