using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements;

public class UIDebriefingBattleStats : MonoBehaviour
{
	public struct Params
	{
		public DebriefingTableDTO TableData;
	}

	[Header("Components")]
	public UIDebriefingTable PersonalRewardsTable;

	public Params Data { get; private set; }

	public void Init(Params @params)
	{
		Data = @params;
		PersonalRewardsTable.Init(new UIDebriefingTable.Params
		{
			TableData = @params.TableData
		});
	}
}
