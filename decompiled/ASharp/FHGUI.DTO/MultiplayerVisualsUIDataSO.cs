using System;
using UnityEngine;

namespace FHGUI.DTO;

[CreateAssetMenu(fileName = "MultiplayerVisualsUIDataSO", menuName = "Def/MultiplayerVisualsUIDataSO")]
public class MultiplayerVisualsUIDataSO : ScriptableObject
{
	[Serializable]
	public struct PlayerVisuals
	{
		[ColorUsage(true, true)]
		public Color Color;

		public Sprite PartySlotFrame;

		public Sprite InitiativeSlotFrame;

		public Sprite InitiativeSlotInnerFrame;

		public Sprite InitiativeSlotSummonElement;
	}

	public PlayerVisuals Host = new PlayerVisuals
	{
		Color = new Color(255f, 220f, 102f)
	};

	public PlayerVisuals Client1 = new PlayerVisuals
	{
		Color = new Color(26f, 150f, 202f)
	};

	public PlayerVisuals Client2 = new PlayerVisuals
	{
		Color = new Color(29f, 171f, 147f)
	};

	public PlayerVisuals Client3 = new PlayerVisuals
	{
		Color = new Color(244f, 51f, 252f)
	};

	public PlayerVisuals Unknown = new PlayerVisuals
	{
		Color = Color.white
	};

	public PlayerVisuals Local => Host;

	public PlayerVisuals Remote => Client1;

	public PlayerVisuals GetPlayerVisuals(int id)
	{
		switch (id)
		{
		case 0:
		case 1:
			return Host;
		case 2:
			return Client1;
		case 3:
			return Client2;
		case 4:
			return Client3;
		default:
			return Unknown;
		}
	}
}
