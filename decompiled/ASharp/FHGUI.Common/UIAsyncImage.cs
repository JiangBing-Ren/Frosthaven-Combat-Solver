using System;
using System.Collections;
using SETUtil.Extend;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace FHGUI.Common;

[RequireComponent(typeof(Image))]
[ExecuteAlways]
public class UIAsyncImage : MonoBehaviour
{
	private struct AsyncImageRequest
	{
		public AssetReferenceSprite SpriteAsset;

		public Sprite FallbackImage;

		public Action<Sprite> Callback;
	}

	private Image _image;

	private AsyncOperationHandle<Sprite> _handle;

	private Coroutine _coroutine;

	private AsyncImageRequest? _request;

	private int _reqCount;

	private void Awake()
	{
		EnsureImage();
	}

	private void OnEnable()
	{
		if (_request.HasValue)
		{
			Init(_request.Value.SpriteAsset, _request.Value.FallbackImage, _request.Value.Callback);
			_request = null;
		}
	}

	private void EnsureImage()
	{
		_image = this.GetOrAdd<Image>();
	}

	public void Init(AssetReferenceSprite spriteAsset, Sprite fallbackImage = null, Action<Sprite> onLoaded = null)
	{
		if (_coroutine != null)
		{
			StopCoroutine(_coroutine);
			_coroutine = null;
		}
		if (!isActiveAndEnabled)
		{
			_request = new AsyncImageRequest
			{
				SpriteAsset = spriteAsset,
				FallbackImage = fallbackImage,
				Callback = onLoaded
			};
		}
		else
		{
			_coroutine = StartCoroutine(InitCrt(spriteAsset, fallbackImage, onLoaded));
		}
	}

	private IEnumerator InitCrt(AssetReferenceSprite spriteAsset, Sprite fallbackImage = null, Action<Sprite> onLoaded = null)
	{
		EnsureImage();
		_reqCount++;
		int currentCount = _reqCount;
		Release();
		if (spriteAsset == null)
		{
			_image.sprite = fallbackImage;
			_image.enabled = fallbackImage != null;
			yield break;
		}
		_image.enabled = false;
		_handle = spriteAsset.LoadAssetAsync();
		AsyncOperationHandle<Sprite> handle = _handle;
		if (!handle.IsDone)
		{
			yield return handle;
		}
		if (!handle.IsValid() || _reqCount != currentCount)
		{
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			yield break;
		}
		try
		{
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				Sprite sprite = ((handle.Result == null) ? fallbackImage : handle.Result);
				_image.enabled = sprite != null;
				onLoaded?.Invoke(sprite);
				_image.sprite = sprite;
			}
			else if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			_coroutine = null;
		}
		catch (Exception)
		{
			Log.GUIError($"Cannot load sprite {spriteAsset?.RuntimeKey} in {gameObject.name}");
		}
	}

	private void Release()
	{
		if (_handle.IsValid())
		{
			Addressables.Release(_handle);
		}
		_handle = default;
	}

	private void OnDisable()
	{
		if (_coroutine != null)
		{
			StopCoroutine(_coroutine);
			_coroutine = null;
		}
		Release();
	}
}
