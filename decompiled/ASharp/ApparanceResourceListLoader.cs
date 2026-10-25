using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ApparanceResourceListLoader : MonoBehaviour, IApparanceResourceListLoaderAsync
{
	[Serializable]
	[SerializeField]
	public class NameReference
	{
		public AssetReference Reference;

		public string Name;
	}

	[SerializeField]
	private List<NameReference> _references = new List<NameReference>();

	private Dictionary<string, ApparanceResourceList> _loadedPackets = new Dictionary<string, ApparanceResourceList>();

	private HashSet<string> _loadingPackets = new HashSet<string>();

	public ApparanceResourceList Load(string asset_path)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asset_path);
		return GetResourceList(fileNameWithoutExtension);
	}

	public void UnloadAll()
	{
		_loadedPackets.Clear();
		foreach (NameReference reference in _references)
		{
			if (reference.Reference.IsValid() && reference.Reference.Asset != null)
			{
				reference.Reference.ReleaseAsset();
			}
		}
	}

	private ApparanceResourceList GetResourceList(string name)
	{
		if (_loadedPackets.TryGetValue(name, out var value))
		{
			return value;
		}
		AssetReference assetReference = null;
		foreach (NameReference reference in _references)
		{
			if (name == reference.Name)
			{
				assetReference = reference.Reference;
				break;
			}
		}
		if (assetReference == null)
		{
			return null;
		}
		ApparanceResourceList apparanceResourceList = assetReference.LoadAssetAsync<ApparanceResourceList>().WaitForCompletion();
		if (apparanceResourceList != null)
		{
			_loadedPackets.Add(name, apparanceResourceList);
		}
		return apparanceResourceList;
	}

	public ApparanceResourceList LoadCheck(string asset_path)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asset_path);
		if (_loadedPackets.TryGetValue(fileNameWithoutExtension, out var value))
		{
			return value;
		}
		return null;
	}

	public bool LoadAsync(string asset_path, Action<ApparanceResourceList> onCompleted)
	{
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asset_path);
		if (_loadingPackets.Contains(fileNameWithoutExtension))
		{
			return false;
		}
		GetResourceListAsync(fileNameWithoutExtension, onCompleted);
		return true;
	}

	private async void GetResourceListAsync(string name, Action<ApparanceResourceList> onCompleted)
	{
		AssetReference assetReference = null;
		foreach (NameReference reference in _references)
		{
			if (name == reference.Name)
			{
				assetReference = reference.Reference;
				break;
			}
		}
		if (assetReference == null)
		{
			Log.Error("Reference " + name + " is null!");
		}
		AsyncOperationHandle<ApparanceResourceList> handle = assetReference.LoadAssetAsync<ApparanceResourceList>();
		await handle.Task;
		_loadingPackets.Add(name);
		ApparanceResourceList result = handle.Result;
		if (result != null)
		{
			_loadedPackets.Add(name, result);
		}
		_loadingPackets.Remove(name);
		onCompleted(handle.Result);
	}
}
