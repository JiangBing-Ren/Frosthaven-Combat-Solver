using UnityEngine;
using UnityEngine.Serialization;

public class RoomVisibilityTracker : MonoBehaviour
{
	[SerializeField]
	[FormerlySerializedAs("m_CentralTile")]
	private TileBehaviour _centralTile;

	public static bool RunningAutomationTests { get; internal set; }

	public TileBehaviour CentralTile => _centralTile;
}
