using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilityMove : SEventAbility
{
	public CAbilityMove.EMoveState MoveState { get; private set; }

	public int TilesMoved { get; private set; }

	public int ActualMoved { get; private set; }

	public bool Jumped { get; private set; }

	public TileIndex MovedFromPoint { get; private set; }

	public TileIndex MovedToPoint { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("MoveState", MoveState);
		info.AddValue("TilesMoved", TilesMoved);
		info.AddValue("ActualMoved", TilesMoved);
		info.AddValue("Jumped", Jumped);
		info.AddValue("MovedFromPoint", MovedFromPoint);
		info.AddValue("MovedToPoint", MovedToPoint);
	}

	public SEventAbilityMove(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "MoveState":
					MoveState = (CAbilityMove.EMoveState)info.GetValue("MoveState", typeof(CAbilityMove.EMoveState));
					break;
				case "TilesMoved":
					TilesMoved = info.GetInt32("TilesMoved");
					break;
				case "ActualMoved":
					ActualMoved = info.GetInt32("ActualMoved");
					break;
				case "Jumped":
					Jumped = info.GetBoolean("Jumped");
					break;
				case "MovedFromPoint":
					MovedFromPoint = (TileIndex)info.GetValue("MovedFromPoint", typeof(TileIndex));
					break;
				case "MovedToPoint":
					MovedToPoint = (TileIndex)info.GetValue("MovedToPoint", typeof(TileIndex));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEventAbilityMove entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEventAbilityMove(SEventAbilityBaseData baseData, CAbilityMove.EMoveState moveState, int tilesMoved, int actualMoved, TileIndex movedFromPoint, TileIndex movedToPoint, bool jump)
		: base(baseData)
	{
		MoveState = moveState;
		TilesMoved = tilesMoved;
		ActualMoved = actualMoved;
		MovedFromPoint = movedFromPoint;
		MovedToPoint = movedToPoint;
		Jumped = jump;
	}
}
