using System;
using FHGUI.Modules;

namespace FHGUI.Modals;

[Serializable]
public struct ModalsCatalogue
{
	public TransferResourcesModal TransferResources;

	public FHInfoModal GenericInfo;

	public CardInfoModal CardUnlockModal;

	public StoryEventViewModule StoryEventModule;
}
