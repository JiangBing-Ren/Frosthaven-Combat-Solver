using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements;

public class UIDebriefingHeroRewards : MonoBehaviour
{
	public struct Params
	{
		public DebriefingTableDTO TableData;
	}

	[Header("Components")]
	[SerializeField]
	public UIDebriefingTable StatTable;

	public Params Data { get; private set; }

	public void Init(Params @params)
	{
		Data = @params;
		StatTable.Init(new UIDebriefingTable.Params
		{
			TableData = @params.TableData
		});
	}
}
