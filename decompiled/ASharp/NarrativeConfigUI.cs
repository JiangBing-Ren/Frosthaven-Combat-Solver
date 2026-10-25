using System;
using UnityEngine;

[Serializable]
public class NarrativeConfigUI
{
	public string id;

	public Sprite picture;

	[Tooltip("Path to an image inside Resources folder")]
	public string pathPicture = "GUI/Loadouts/";
}
