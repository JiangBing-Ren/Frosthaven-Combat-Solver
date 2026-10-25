using UnityEngine;

namespace FHGUI.Common.Tooltips;

public interface ITooltip<T>
{
	GameObject GameObject { get; }

	void Show(T data);

	void Hide();
}
