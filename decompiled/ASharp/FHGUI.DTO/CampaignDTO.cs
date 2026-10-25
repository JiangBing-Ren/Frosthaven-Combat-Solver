using UnityEngine;

namespace FHGUI.DTO;

public class CampaignDTO
{
	public string Id;

	public string NameKey;

	public string CaptionKey;

	public string DescriptionKey;

	public Sprite Illustration;

	public Sprite IllustrationActive;

	public bool IsAvailable;

	public override string ToString()
	{
		return $"CampaignDTOp[{Id}, Available{IsAvailable}]";
	}
}
