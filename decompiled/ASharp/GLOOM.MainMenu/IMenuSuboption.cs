using UnityEngine;

namespace GLOOM.MainMenu;

public interface IMenuSuboption
{
	string NameLocKey { get; }

	Sprite Icon { get; }

	Sprite IconHighlight { get; }

	bool IsInteractable { get; }
}
