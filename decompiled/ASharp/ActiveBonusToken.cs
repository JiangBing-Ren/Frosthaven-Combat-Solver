using System.Collections.Generic;
using System.Text;
using FFSNet;
using ScenarioRuleLibrary;
using UdpKit;

public sealed class ActiveBonusToken : IProtocolToken
{
	private int selectedOptionsCount;

	public int BaseCardID { get; private set; }

	public int ActiveBonusID { get; private set; }

	public string AbilityName { get; private set; }

	public string[] SelectedOptionsID { get; set; }

	public int SelectedElementID { get; private set; }

	public bool Selected { get; private set; }

	public ActiveBonusToken(CActiveBonus activeBonus, bool selected, ElementInfusionBoardManager.EElement? toggledElement = null, List<IOption> selectedOptions = null)
	{
		BaseCardID = activeBonus.BaseCard.ID;
		ActiveBonusID = activeBonus.ID;
		AbilityName = activeBonus.Ability.Name;
		selectedOptionsCount = 0;
		Selected = selected;
		if (selected)
		{
			selectedOptionsCount = selectedOptions?.Count ?? 0;
			SelectedElementID = (int)(toggledElement.HasValue ? toggledElement.Value : ElementInfusionBoardManager.EElement.Fire);
		}
		SelectedOptionsID = new string[selectedOptionsCount];
		for (int i = 0; i < selectedOptionsCount; i++)
		{
			SelectedOptionsID[i] = selectedOptions[i].ID;
		}
	}

	public ActiveBonusToken()
	{
		BaseCardID = 0;
		ActiveBonusID = 0;
		AbilityName = string.Empty;
		selectedOptionsCount = 0;
		SelectedOptionsID = new string[selectedOptionsCount];
		SelectedElementID = 0;
		Selected = true;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(BaseCardID);
		packet.WriteInt(ActiveBonusID);
		packet.WriteString(AbilityName, Encoding.UTF8);
		packet.WriteInt(SelectedElementID);
		packet.WriteBool(Selected);
		packet.WriteInt(selectedOptionsCount);
		for (int i = 0; i < selectedOptionsCount; i++)
		{
			packet.WriteString(SelectedOptionsID[i], Encoding.ASCII);
		}
	}

	public void Read(UdpPacket packet)
	{
		BaseCardID = packet.ReadInt();
		ActiveBonusID = packet.ReadInt();
		AbilityName = packet.ReadString(Encoding.UTF8);
		SelectedElementID = packet.ReadInt();
		Selected = packet.ReadBool();
		selectedOptionsCount = packet.ReadInt();
		SelectedOptionsID = new string[selectedOptionsCount];
		for (int i = 0; i < selectedOptionsCount; i++)
		{
			SelectedOptionsID[i] = packet.ReadString(Encoding.ASCII);
		}
	}
}
