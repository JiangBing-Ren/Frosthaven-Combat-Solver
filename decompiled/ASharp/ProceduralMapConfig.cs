using UnityEngine;

public class ProceduralMapConfig : MonoBehaviour
{
	[Tooltip("Any distance between exit and entrance, anything more thatn optimal is great two and has same priority")]
	public int m_OptimalExitToEntranceDistance = 5;
}
