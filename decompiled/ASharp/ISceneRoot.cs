using UnityEngine;

public interface ISceneRoot
{
	Transform Root { get; }

	void Show(object data = null);

	void Hide();

	bool ShouldRefresh(object data = null);
}
