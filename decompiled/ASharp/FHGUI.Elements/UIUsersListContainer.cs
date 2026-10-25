using System.Collections.Generic;

namespace FHGUI.Elements;

public class UIUsersListContainer : ListContainerBase<UIMultiplayerUser, bool, UIMultiplayerUser.Data>
{
	public void Init(List<UIMultiplayerUser.Data> users)
	{
		Init(listParams: true, users);
	}

	protected override void InitElement(UIMultiplayerUser listElement, UIMultiplayerUser.Data elementParams, bool listParams)
	{
		listElement.Show(elementParams);
	}
}
