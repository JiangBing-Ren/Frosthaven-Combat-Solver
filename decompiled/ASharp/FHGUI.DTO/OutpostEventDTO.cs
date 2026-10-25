using System;
using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.DTO;

public class OutpostEventDTO
{
	public class Option
	{
		public uint ID;

		public string Text;

		public bool Interactable;

		public List<UIMultiplayerUser.Data> SuggestingUsers;

		public Action Action;
	}

	public class Effect
	{
		public bool Hidden;

		public string Text;
	}

	public string ID;

	public string Text;

	public Sprite Icon;

	public Sprite Illustration;

	public Sprite FadeInIllustration;

	public string EventType;

	public List<string> Voiceovers = new List<string>();

	public bool LastInChain;

	public bool IsProceedInteractable;

	public List<UIMultiplayerUser.Data> ReadyUsers;

	public string OptionPromp;

	public List<Option> Options = new List<Option>(2);

	public List<Effect> Effects = new List<Effect>(10);

	public bool NoButtonsAtAll;

	public string Voiceover
	{
		get
		{
			if (Voiceovers != null && Voiceovers.Count > 0)
			{
				return Voiceovers[0];
			}
			return null;
		}
		set
		{
			Voiceovers.Clear();
			Voiceovers.Add(value);
		}
	}
}
