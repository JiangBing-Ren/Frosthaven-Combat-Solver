using UnityEngine.SceneManagement;

namespace FH.ProcBackgroundsPrototype;

public interface IBackgroundPostProcessor
{
	void Process(Scene scene, GameBackgrounds.WorldContext context);
}
