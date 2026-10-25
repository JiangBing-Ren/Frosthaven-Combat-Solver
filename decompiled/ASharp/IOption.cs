using UnityEngine;

public interface IOption
{
	string ID { get; }

	string GetPickerText();

	Sprite GetPickerIcon();

	string GetSelectedText();
}
