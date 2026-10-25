using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageLoader : MonoBehaviour
{
	public FileSelector selector;

	public Image img;

	public IEnumerator LoadImage(string path)
	{
		using UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + path);
		yield return uwr.SendWebRequest();
		if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
		{
			Debug.Log(uwr.error);
			yield break;
		}
		Texture2D content = DownloadHandlerTexture.GetContent(uwr);
		Sprite image = Sprite.Create(content, new Rect(0f, 0f, content.width, content.height), new Vector2(0.5f, 0.5f), 100f);
		yield return image;
		img.sprite = image;
		Debug.Log("[ImageLoader] Loaded image from " + path);
	}

	public void OnLoadClick()
	{
		StartCoroutine(LoadImage(selector.result));
	}
}
