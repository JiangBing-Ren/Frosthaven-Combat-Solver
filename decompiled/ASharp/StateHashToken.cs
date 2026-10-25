using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class StateHashToken : StatePropertyToken
{
	public string importantStateHash;

	public string fullStateHash;

	public int round;

	public CPhase.PhaseType phaseType;

	public ECharacter playerID;

	public int actionID;

	public int lastGameActionID;

	public StateHashToken(string importantStateHash, string fullStateHash, int round, CPhase.PhaseType phaseType, ECharacter playerID, int actionID)
	{
		this.importantStateHash = importantStateHash;
		this.fullStateHash = fullStateHash;
		this.round = round;
		this.phaseType = phaseType;
		this.playerID = playerID;
		this.actionID = actionID;
		lastGameActionID = 0;
	}

	public StateHashToken(StateHashToken token, int lastGameActionID)
	{
		importantStateHash = token.importantStateHash;
		fullStateHash = token.fullStateHash;
		round = token.round;
		phaseType = token.phaseType;
		playerID = token.playerID;
		actionID = token.actionID;
		this.lastGameActionID = lastGameActionID;
	}

	public StateHashToken()
	{
		importantStateHash = "";
		fullStateHash = "";
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteString(importantStateHash);
		packet.WriteString(fullStateHash);
		packet.WriteInt(round);
		packet.WriteInt((int)phaseType);
		packet.WriteInt((int)playerID);
		packet.WriteInt(actionID);
		packet.WriteInt(lastGameActionID);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		importantStateHash = packet.ReadString();
		fullStateHash = packet.ReadString();
		round = packet.ReadInt();
		phaseType = (CPhase.PhaseType)packet.ReadInt();
		playerID = (ECharacter)packet.ReadInt();
		actionID = packet.ReadInt();
		lastGameActionID = packet.ReadInt();
	}

	public override string ToString()
	{
		return $"{importantStateHash}+{fullStateHash} @ R{round} {phaseType} ({playerID} #{actionID})";
	}

	public override void Print(string customTitle = "")
	{
		Log.NetworkInfo(customTitle + GetRevisionString() + " state hash: \"" + importantStateHash + "+" + fullStateHash + "\"");
	}

	public bool IsSameCheckpoint(StateHashToken token)
	{
		if (token != null && token.phaseType == phaseType && token.round == round && token.playerID == playerID)
		{
			return token.actionID == actionID;
		}
		return false;
	}
}
