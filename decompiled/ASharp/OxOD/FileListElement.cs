using UnityEngine;
using UnityEngine.UI;

namespace OxOD;

public class FileListElement : MonoBehaviour
{
	public Image icon;

	public Text elementName;

	public Text size;

	public FileDialog instance;

	public bool isFile;

	public string data;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void OnClick()
	{
		if (!isFile)
		{
			instance.OpenDir(data);
		}
		else
		{
			instance.SelectFile(data);
		}
	}

	public void OnDoubleClick()
	{
		if (isFile)
		{
			instance.SelectFile(data, withoutConfirm: true);
		}
	}
}
