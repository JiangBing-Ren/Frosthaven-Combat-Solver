using System.Text;
using FFSNet;
using UdpKit;

public sealed class ChooseAbilityToken : IProtocolToken
{
	public string AbilityID { get; private set; }

	public string SelectedOptionID { get; set; }

	public bool Selected { get; private set; }

	public ChooseAbilityToken(IAbility ability, bool selected, IOption selectedOption = null)
	{
		AbilityID = ability.ID;
		Selected = selected;
		if (selected)
		{
			SelectedOptionID = selectedOption?.ID;
		}
	}

	public ChooseAbilityToken()
	{
		AbilityID = string.Empty;
		SelectedOptionID = null;
		Selected = true;
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteString(AbilityID, Encoding.UTF8);
		packet.WriteBool(Selected);
		packet.WriteString(SelectedOptionID, Encoding.UTF8);
	}

	public void Read(UdpPacket packet)
	{
		AbilityID = packet.ReadString(Encoding.UTF8);
		Selected = packet.ReadBool();
		SelectedOptionID = packet.ReadString(Encoding.UTF8);
	}
}
