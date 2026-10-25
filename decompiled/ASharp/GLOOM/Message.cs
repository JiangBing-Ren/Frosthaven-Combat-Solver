using System.Collections.Generic;
using UnityEngine;

namespace GLOOM;

public struct Message(string title, Sprite image, string information = null, List<Message> content = null)
{
	public string title = title;

	public Sprite image = image;

	public string information = information;

	public List<Message> content = content;

	public Message(string title, string information)
		: this(title, null, information)
	{
	}
}
