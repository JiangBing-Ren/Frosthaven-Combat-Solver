using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FHGUI.Elements;

public class UIBuildingSection : MonoBehaviour
{
	[SerializeField]
	private TextLocalizedListener _titleLoc;

	[SerializeField]
	private TMP_Text _count;

	private List<UIConstructBuildingListElement> _buildings = new List<UIConstructBuildingListElement>(10);

	public void Init(int buildingCount)
	{
		_count.text = buildingCount.ToString();
	}
}
