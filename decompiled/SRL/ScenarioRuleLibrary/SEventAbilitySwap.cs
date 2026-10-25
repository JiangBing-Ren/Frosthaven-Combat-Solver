using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEventAbilitySwap : SEventAbility
{
	public CAbilitySwap.ESwapState SwapState { get; private set; }

	public string FirstTargetName { get; private set; }

	public string SecondTargetName { get; private set; }

	public TileIndex FirstTargetStartLocation { get; private set; }

	public TileIndex SecondTargetStartLocation { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("SwapState", SwapState);
		info.AddValue("FirstTargetName", FirstTargetName);
		info.AddValue("SecondTargetName", SecondTargetName);
		info.AddValue("FirstTargetStartLocation", FirstTargetStartLocation);
		info.AddValue("SecondTargetStartLocation", SecondTargetStartLocation);
	}

	public SEventAbilitySwap(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		foreach (SerializationEntry item in info)
		{
			switch (item.Name)
			{
			case "SwapState":
				SwapState = (CAbilitySwap.ESwapState)info.GetValue("SwapState", typeof(CAbilitySwap.ESwapState));
				break;
			case "FirstTargetName":
				FirstTargetName = info.GetString("FirstTargetName");
				break;
			case "SecondTargetName":
				SecondTargetName = info.GetString("SecondTargetName");
				break;
			case "FirstTargetStartLocation":
				FirstTargetStartLocation = (TileIndex)info.GetValue("FirstTargetStartLocation", typeof(TileIndex));
				break;
			case "SecondTargetStartLocation":
				SecondTargetStartLocation = (TileIndex)info.GetValue("SecondTargetStartLocation", typeof(TileIndex));
				break;
			}
		}
	}

	public SEventAbilitySwap(SEventAbilityBaseData baseData, CAbilitySwap.ESwapState swapState, string firstTargetName, string secondTargetName, TileIndex firstTargetStartLocation, TileIndex secondTargetStartLocation)
		: base(baseData)
	{
		SwapState = swapState;
		FirstTargetName = firstTargetName;
		SecondTargetName = secondTargetName;
		FirstTargetStartLocation = firstTargetStartLocation;
		SecondTargetStartLocation = secondTargetStartLocation;
	}
}
