using System;
using ScenarioRuleLibrary;

[AttributeUsage(AttributeTargets.Method)]
public class MessageHandlerAttribute : Attribute
{
	public CMessageData.MessageType MessageType { get; private set; }

	public MessageHandlerAttribute(CMessageData.MessageType messageType)
	{
		MessageType = messageType;
	}
}
