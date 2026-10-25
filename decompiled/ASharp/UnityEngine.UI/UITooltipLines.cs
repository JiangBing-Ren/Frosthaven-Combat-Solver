using System.Collections.Generic;
using TMPro;

namespace UnityEngine.UI;

public class UITooltipLines
{
	public enum LineStyle
	{
		Title,
		Attribute,
		Description,
		Keyword
	}

	public class Line
	{
		public string left;

		public string right;

		public bool isComplete;

		public RectOffset padding;

		public LineStyle style;

		public TMP_SpriteAsset spriteAsset;

		public Line(string left, string right, bool isComplete, RectOffset padding, LineStyle style, TMP_SpriteAsset spriteAsset)
		{
			this.left = left;
			this.right = right;
			this.isComplete = isComplete;
			this.padding = padding;
			this.style = style;
			this.spriteAsset = spriteAsset;
		}
	}

	public class ImageLine : Line
	{
		public int[] attributes;

		public Sprite[] images;

		public ImageLine(string left, int[] attributes, Sprite[] images, bool isComplete, RectOffset padding, LineStyle style)
			: base(left, "", isComplete, padding, style, null)
		{
			this.attributes = attributes;
			this.images = images;
		}
	}

	public class Lines : List<Line>
	{
	}

	public Lines lineList = new Lines();

	public void AddLine(string leftContent, string rightContent, TMP_SpriteAsset spriteAsset = null)
	{
		lineList.Add(new Line(leftContent, rightContent, isComplete: true, new RectOffset(), LineStyle.Attribute, spriteAsset));
	}

	public void AddLine(string leftContent, string rightContent, RectOffset padding, TMP_SpriteAsset spriteAsset = null)
	{
		lineList.Add(new Line(leftContent, rightContent, isComplete: true, padding, LineStyle.Attribute, spriteAsset));
	}

	public void AddLine(string content, TMP_SpriteAsset spriteAsset = null)
	{
		lineList.Add(new Line(content, string.Empty, isComplete: true, new RectOffset(), LineStyle.Attribute, spriteAsset));
	}

	public void AddLine(string content, RectOffset padding, TMP_SpriteAsset spriteAsset = null)
	{
		lineList.Add(new Line(content, string.Empty, isComplete: true, padding, LineStyle.Attribute, spriteAsset));
	}

	public void AddLine(string content, RectOffset padding, LineStyle style, TMP_SpriteAsset spriteAsset = null)
	{
		lineList.Add(new Line(content, string.Empty, isComplete: true, padding, style, spriteAsset));
	}

	public void AddLine(string leftContent, string rightContent, RectOffset padding, LineStyle style, TMP_SpriteAsset spriteAsset = null)
	{
		lineList.Add(new Line(leftContent, rightContent, isComplete: true, padding, style, spriteAsset));
	}

	public void AddAttributeLine(string leftContent, int[] attributes, Sprite[] images)
	{
		lineList.Add(new ImageLine(leftContent, attributes, images, isComplete: true, new RectOffset(), LineStyle.Attribute));
	}

	public void AddColumn(string content, TMP_SpriteAsset spriteAsset = null)
	{
		if (lineList.Count == 0)
		{
			lineList.Add(new Line(content, string.Empty, isComplete: false, new RectOffset(), LineStyle.Attribute, spriteAsset));
			return;
		}
		Line line = lineList[lineList.Count - 1];
		if (!line.isComplete)
		{
			line.right = content;
			line.isComplete = true;
		}
		else
		{
			lineList.Add(new Line(content, string.Empty, isComplete: false, new RectOffset(), LineStyle.Attribute, spriteAsset));
		}
	}
}
