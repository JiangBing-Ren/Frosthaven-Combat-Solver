using System;
using System.Linq;
using FFSNet;
using UdpKit;

public sealed class ControllableStateRevisionToken : IProtocolToken
{
	public struct ControllableStateRevisionData
	{
		public int ControllableID;

		public int ControllerID;

		public ulong RevisionHash;
	}

	private int controllablesCount;

	public ControllableStateRevisionData[] Data { get; set; }

	public ControllableStateRevisionToken(NetworkPlayer controller)
	{
		controllablesCount = controller.MyControllables.Count();
		Data = new ControllableStateRevisionData[controllablesCount];
		Log.NetworkInfo("Creating a ControllableStateRevisionsToken. " + controller.Username + "'s controllable count: " + controllablesCount);
		for (int i = 0; i < controllablesCount; i++)
		{
			NetworkControllable networkControllable = controller.MyControllables.ElementAt(i);
			Data[i].ControllableID = networkControllable.ID;
			Data[i].ControllerID = networkControllable.Controller.PlayerID.ID;
		}
	}

	public ControllableStateRevisionToken()
	{
		controllablesCount = 0;
		Data = new ControllableStateRevisionData[controllablesCount];
	}

	public void Write(UdpPacket packet)
	{
		packet.WriteInt(controllablesCount);
		for (int i = 0; i < controllablesCount; i++)
		{
			packet.WriteInt(Data[i].ControllableID);
			packet.WriteInt(Data[i].ControllerID);
			packet.WriteULong(Data[i].RevisionHash);
		}
	}

	public void Read(UdpPacket packet)
	{
		controllablesCount = packet.ReadInt();
		Data = new ControllableStateRevisionData[controllablesCount];
		for (int i = 0; i < controllablesCount; i++)
		{
			Data[i].ControllableID = packet.ReadInt();
			Data[i].ControllerID = packet.ReadInt();
			Data[i].RevisionHash = packet.ReadULong();
		}
	}

	public bool IsSameRevision(NetworkPlayer targetController)
	{
		for (int i = 0; i < controllablesCount; i++)
		{
			if (targetController != PlayerRegistry.GetPlayer(Data[i].ControllerID))
			{
				Log.NetworkWarning("State revisions do not match. Incorrect controller detected.");
				return false;
			}
			try
			{
				if (ControllableRegistry.GetControllable(Data[i].ControllableID) == null)
				{
					Log.NetworkWarning("State revisions do not match. Incorrect controllable detected.");
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.NetworkWarning("Exception getting NetworkEntity from ControllableID: " + Data[i].ControllableID + ".\n" + ex.Message + "\n" + ex.StackTrace);
				return false;
			}
		}
		Log.NetworkInfo("Comparison check for state revisions successfully completed. State revisions match!");
		return true;
	}

	public void PrintControllableStateRevisionData()
	{
		for (int i = 0; i < controllablesCount; i++)
		{
			Log.NetworkInfo("Controllable State Revision Hash (ControllableID: " + Data[i].ControllableID + ", ControllerID: " + Data[i].ControllerID + "): " + Data[i].RevisionHash);
		}
	}
}
