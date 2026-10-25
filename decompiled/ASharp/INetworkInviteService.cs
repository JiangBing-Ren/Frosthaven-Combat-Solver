using System.Collections.Generic;
using Assets.Script.Misc;

public interface INetworkInviteService
{
	List<IInvitePlayer> GetPlayersToInvite();

	ICallbackPromise SendInvite(IInvitePlayer player);
}
