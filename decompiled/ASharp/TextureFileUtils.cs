using System;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public static class TextureFileUtils
{
	public static void SaveTextureToFile(Texture source, string filePath, int width, int height, bool asynchronous = true, Material blitMat = null, Action<bool> done = null)
	{
		if (!(source is Texture2D) && !(source is RenderTexture))
		{
			done?.Invoke(obj: false);
			return;
		}
		if (width < 0 || height < 0)
		{
			width = source.width;
			height = source.height;
		}
		RenderTexture resizeRT = RenderTexture.GetTemporary(width, height, 0);
		if (blitMat != null)
		{
			Graphics.Blit(source, resizeRT, blitMat);
		}
		else
		{
			Graphics.Blit(source, resizeRT);
		}
		NativeArray<byte> narray = new NativeArray<byte>(width * height * 4, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		AsyncGPUReadbackRequest asyncGPUReadbackRequest = AsyncGPUReadback.RequestIntoNativeArray(ref narray, resizeRT, 0, (AsyncGPUReadbackRequest request) =>
		{
			if (!request.hasError)
			{
				NativeArray<byte> nativeArray = ImageConversion.EncodeNativeArrayToPNG(narray, resizeRT.graphicsFormat, (uint)width, (uint)height);
				File.WriteAllBytes(filePath, nativeArray.ToArray());
				nativeArray.Dispose();
			}
			narray.Dispose();
			done?.Invoke(!request.hasError);
		});
		if (!asynchronous)
		{
			asyncGPUReadbackRequest.WaitForCompletion();
		}
	}

	public static Texture2D LoadTextureFromFile(string path, int initialWidth = 1, int initialHeight = 1)
	{
		Texture2D texture2D = new Texture2D(initialWidth, initialHeight);
		texture2D.LoadImage(File.ReadAllBytes(path));
		texture2D.name = Path.GetFileName(path);
		return texture2D;
	}
}
