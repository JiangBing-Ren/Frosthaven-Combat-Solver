using System.Collections.Generic;
using UnityEngine;

namespace Assets.Script.GUI.Configuration;

[CreateAssetMenu(menuName = "UI Config/Keyboard")]
public class KeyboardConfig : ScriptableObject
{
	public string Language;

	public List<KeyboardRow> Rows;
}
