using UnityEngine;

public class ModDataView
{
	public string Name { get; set; }

	public string Description { get; set; }

	public Texture2D Thumbnail { get; set; }

	public ModDataView(string name, string description, Texture2D thumbnail)
	{
		Name = name;
		Description = description;
		Thumbnail = thumbnail;
	}
}
