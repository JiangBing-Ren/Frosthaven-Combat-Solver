using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.DTO;

public class LinkingSectionDTO
{
	public string ID;

	public string Caption;

	public string Text;

	public Sprite Illustration;

	public string Voiceover;

	public bool IsProceedInteractable;

	public List<UIMultiplayerUser.Data> ReadyUsers;

	public List<OutpostEventDTO.Option> Options = new List<OutpostEventDTO.Option>(2);

	public List<OutpostEventDTO.Effect> Effects = new List<OutpostEventDTO.Effect>(10);
}
