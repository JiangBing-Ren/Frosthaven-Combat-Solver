using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary;

[Serializable]
public class SEvent : ISerializable
{
	public List<ElementInfusionBoardManager.EElement> Elements;

	public ESEType Type { get; private set; }

	public string Text { get; private set; }

	public int Round { get; private set; }

	public int EventOrder { get; private set; }

	public string CurrentPhaseActorName { get; private set; }

	public string CurrentPhaseActorGuid { get; private set; }

	public bool m_DoNotSerialize { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("Type", Type);
		info.AddValue("Text", Text);
		info.AddValue("Round", Round);
		info.AddValue("CurrentPhaseActorName", CurrentPhaseActorName);
		info.AddValue("CurrentPhaseActorGuid", CurrentPhaseActorGuid);
		info.AddValue("Elements", Elements);
		info.AddValue("EventOrder", EventOrder);
	}

	public SEvent(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "EventOrder":
					EventOrder = info.GetInt32("EventOrder");
					break;
				case "Type":
					Type = (ESEType)info.GetValue("Type", typeof(ESEType));
					break;
				case "Text":
					Text = info.GetString("Text");
					break;
				case "Round":
					Round = info.GetInt32("Round");
					break;
				case "CurrentPhaseActorName":
					CurrentPhaseActorName = info.GetString("CurrentPhaseActorName");
					break;
				case "CurrentPhaseActorGuid":
					CurrentPhaseActorGuid = info.GetString("CurrentPhaseActorGuid");
					break;
				case "Elements":
					Elements = (List<ElementInfusionBoardManager.EElement>)info.GetValue("Elements", typeof(List<ElementInfusionBoardManager.EElement>));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize SEvent entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public SEvent(ESEType type, string text = "", bool doNotSerialize = false)
	{
		Type = type;
		Text = text;
		m_DoNotSerialize = doNotSerialize;
		Round = ((ScenarioManager.CurrentScenarioState == null) ? 1 : ScenarioManager.CurrentScenarioState.RoundNumber);
		CurrentPhaseActorName = ((GameState.InternalCurrentActor != null) ? GameState.InternalCurrentActor.GetPrefabName() : "");
		CurrentPhaseActorGuid = ((GameState.InternalCurrentActor != null) ? GameState.InternalCurrentActor.ActorGuid : "");
		Elements = ElementInfusionBoardManager.GetAvailableElements();
		EventOrder = (ScenarioManager.CurrentScenarioState?.ScenarioEventLog?.SerializedEventCount).GetValueOrDefault();
	}
}
