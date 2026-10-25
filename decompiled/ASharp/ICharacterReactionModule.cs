using UnityEngine;

public interface ICharacterReactionModule
{
	GameObject GameObject { get; }

	void UpdateState();
}
