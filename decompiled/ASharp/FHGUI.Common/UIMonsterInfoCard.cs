using TMPro;
using UnityEngine;

namespace FHGUI.Common;

public class UIMonsterInfoCard : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _name;

	[SerializeField]
	private Sprite _image;

	[SerializeField]
	private UIBaseStatsView _stats;
}
