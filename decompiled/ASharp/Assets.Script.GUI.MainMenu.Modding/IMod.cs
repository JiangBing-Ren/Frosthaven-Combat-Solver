using UnityEngine;

namespace Assets.Script.GUI.MainMenu.Modding;

public interface IMod
{
	string Name { get; }

	string Description { get; }

	Texture2D Thumbnail { get; }

	string Version { get; }

	bool IsCustomMod { get; }

	int Rating { get; }

	bool IsValid { get; }

	string LastValidationResultsFile { get; }
}
