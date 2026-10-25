using UdpKit;

namespace FFSNet;

public abstract class StatePropertyToken : IProtocolToken
{
	public int Revision { get; private set; }

	public virtual void Write(UdpPacket packet)
	{
		packet.WriteInt(Revision);
	}

	public virtual void Read(UdpPacket packet)
	{
		Revision = packet.ReadInt();
	}

	public void SetRevision(int allowedPlayerID, StatePropertyToken previousStatePropertyToken)
	{
		if (PlayerRegistry.MyPlayer.PlayerID.ID == allowedPlayerID)
		{
			Revision = ((previousStatePropertyToken == null) ? 1 : (previousStatePropertyToken.Revision + 1));
		}
	}

	public bool IsNewerRevision(StatePropertyToken tokenForComparison, bool orSameRevision = false)
	{
		if (tokenForComparison == null)
		{
			return true;
		}
		if (Revision <= tokenForComparison.Revision)
		{
			if (orSameRevision)
			{
				return Revision == tokenForComparison.Revision;
			}
			return false;
		}
		return true;
	}

	public abstract void Print(string customTitle);

	public string GetRevisionString()
	{
		return "Revision #" + Revision + ". ";
	}
}
