using System.Text;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public class GameToken : CustomDataToken
{
	public int GameModeID { get; set; }

	public string SaveName { get; private set; }

	public string SaveHash { get; private set; }

	public string HostPlayerID { get; private set; }

	public string HostAccountID { get; private set; }

	public string HostUsername { get; private set; }

	public uint DLCFlag { get; private set; }

	public bool FastUndoEnabled { get; private set; }

	public bool FastRedoEnabled { get; private set; }

	public int CampaignRules { get; set; }

	public float RegularSpeedMultiplier { get; private set; }

	public bool ChooseAdvantageModifiers { get; private set; }

	public bool ConfirmAttackAbilityBeforeFinishing { get; private set; }

	public bool WaitUntilSavePoint { get; set; }

	public bool IsIronMan { get; private set; }

	public GameToken(int gameModeID, string saveName, string saveHash, string hostPlayerID, string hostAccountID, string hostUsername, float regularSpeedMultiplier, bool chooseAdvantageModifiers, bool confirmAttackAbilityBeforeFinishing, uint dlcFlag, int campaignRules, bool isIronMan, bool waitUntilSavePoint = false, byte[] customData = null)
		: base(customData)
	{
		GameModeID = gameModeID;
		SaveName = saveName;
		SaveHash = saveHash;
		HostPlayerID = hostPlayerID;
		HostAccountID = hostAccountID;
		HostUsername = hostUsername;
		RegularSpeedMultiplier = regularSpeedMultiplier;
		ChooseAdvantageModifiers = chooseAdvantageModifiers;
		ConfirmAttackAbilityBeforeFinishing = confirmAttackAbilityBeforeFinishing;
		DLCFlag = dlcFlag;
		FastUndoEnabled = FastReload.FastUndoEnabled;
		FastRedoEnabled = FastReload.FastRedoEnabled;
		CampaignRules = campaignRules;
		WaitUntilSavePoint = waitUntilSavePoint;
		IsIronMan = isIronMan;
		Log.NetworkInfo(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat("--------------------------------------------------------------------------------------------\n" + "Creating the game data token.\n", "Game mode ID: ", gameModeID.ToString(), "\n"), "Save name: ", saveName, "\n"), "Save hash: ", saveHash, "\n"), "Host Player ID: ", HostPlayerID, "\n"), "Host Account ID: ", HostAccountID, "\n"), "Host Username: ", HostUsername, "\n"), "Regular Speed Multiplier: ", RegularSpeedMultiplier.ToString(), "\n"), "Confirm Advantage Modifiers: ", ChooseAdvantageModifiers.ToString(), "\n"), "Confirm Attack Abilities have finished: ", ConfirmAttackAbilityBeforeFinishing.ToString(), "\n"), "DLC Flag: ", DLCRegistry.GetDLCListAsStringFromFlag((DLCRegistry.EDLCKey)dlcFlag), "\n"), "Fast Undo Enabled: ", FastUndoEnabled.ToString(), "\n"), "Fast Redo Enabled: ", FastRedoEnabled.ToString(), "\n"), "Campaign rules: ", ((ERulesFlag)CampaignRules/*cast due to constrained. prefix*/).ToString(), "\n"), "Wait Until Save Point: ", WaitUntilSavePoint.ToString(), "\n"), "Is IronMan: ", IsIronMan.ToString(), "\n"), "--------------------------------------------------------------------------------------------"));
	}

	public GameToken()
	{
		GameModeID = 0;
		SaveName = string.Empty;
		SaveHash = string.Empty;
		HostPlayerID = string.Empty;
		HostAccountID = string.Empty;
		HostUsername = string.Empty;
		RegularSpeedMultiplier = 1f;
		ChooseAdvantageModifiers = false;
		ConfirmAttackAbilityBeforeFinishing = false;
		CampaignRules = 0;
		DLCFlag = 0u;
	}

	public override void Write(UdpPacket packet)
	{
		base.Write(packet);
		packet.WriteInt(GameModeID);
		packet.WriteString(SaveName, Encoding.UTF8);
		packet.WriteString(SaveHash, Encoding.UTF8);
		packet.WriteString(HostPlayerID, Encoding.UTF8);
		packet.WriteString(HostAccountID, Encoding.UTF8);
		packet.WriteString(HostUsername, Encoding.UTF8);
		packet.WriteFloat(RegularSpeedMultiplier);
		packet.WriteBool(ChooseAdvantageModifiers);
		packet.WriteBool(ConfirmAttackAbilityBeforeFinishing);
		packet.WriteUInt(DLCFlag);
		packet.WriteBool(FastUndoEnabled);
		packet.WriteBool(FastRedoEnabled);
		packet.WriteInt(CampaignRules);
		packet.WriteBool(WaitUntilSavePoint);
		packet.WriteBool(IsIronMan);
	}

	public override void Read(UdpPacket packet)
	{
		base.Read(packet);
		GameModeID = packet.ReadInt();
		SaveName = packet.ReadString(Encoding.UTF8);
		SaveHash = packet.ReadString(Encoding.UTF8);
		HostPlayerID = packet.ReadString(Encoding.UTF8);
		HostAccountID = packet.ReadString(Encoding.UTF8);
		HostUsername = packet.ReadString(Encoding.UTF8);
		RegularSpeedMultiplier = packet.ReadFloat();
		ChooseAdvantageModifiers = packet.ReadBool();
		ConfirmAttackAbilityBeforeFinishing = packet.ReadBool();
		DLCFlag = packet.ReadUInt();
		FastUndoEnabled = packet.ReadBool();
		FastRedoEnabled = packet.ReadBool();
		CampaignRules = packet.ReadInt();
		WaitUntilSavePoint = packet.ReadBool();
		IsIronMan = packet.ReadBool();
	}
}
