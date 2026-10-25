using Libs.GameConsole;
using UnityEngine;

namespace FH.Tools;

public class ToggleRuntimeInspector : MonoBehaviour
{
	[SerializeField]
	private GameObject[] _elements;

	private static bool _enabled;

	private void Awake()
	{
		Object.DontDestroyOnLoad(gameObject);
		Toggle(_enabled);
	}

	public static ToggleRuntimeInspector Instance()
	{
		return Object.FindObjectOfType<ToggleRuntimeInspector>();
	}

	[ConsoleCommand(Command = "DebugToggleRuntimeInspector")]
	public static void DebugToggleRuntimeInspector(IConsole console)
	{
		ToggleRuntimeInspector toggleRuntimeInspector = Instance();
		if ((bool)toggleRuntimeInspector)
		{
			toggleRuntimeInspector.Toggle();
		}
	}

	private void Toggle(bool? state = null)
	{
		_enabled = state ?? (!_enabled);
		GameObject[] elements = _elements;
		for (int i = 0; i < elements.Length; i++)
		{
			elements[i].SetActive(_enabled);
		}
	}
}
