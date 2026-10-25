using TMPro;
using UnityEngine;

public class TextWrappingBalancer
{
	private const string openNobr = "<nobr>";

	private const string closeNobr = "</nobr>";

	public static string BalanceText(TextMeshProUGUI textBox, float containerWidth, float containerHeight = float.MaxValue)
	{
		string text = textBox.text;
		textBox.enableWordWrapping = true;
		textBox.ForceMeshUpdate();
		float num = textBox.GetPreferredValues(containerWidth, containerHeight).x;
		float y = textBox.GetPreferredValues(containerWidth, containerHeight).y;
		textBox.enableWordWrapping = false;
		if (containerWidth > 0f && num > containerWidth)
		{
			textBox.text = "A A";
			float preferredWidth = textBox.preferredWidth;
			textBox.text = "A";
			preferredWidth -= 2f * textBox.preferredWidth;
			float preferredHeight = textBox.preferredHeight;
			int num2 = Mathf.RoundToInt(y / preferredHeight);
			string text2 = text;
			int num3 = 0;
			string text3 = string.Empty;
			while (num2 > 1 && text2.Length > 0)
			{
				float num4 = Mathf.Round((num + preferredWidth) / (float)num2 - preferredWidth);
				int charIndex = Mathf.RoundToInt((text2.Length + 1) / num2) - 1;
				FindBreakOpportunity(textBox, text2, containerWidth, num4, forwardSearchDirection: false, charIndex, out var breakIndex, out var substrWidth);
				FindBreakOpportunity(textBox, text2, containerWidth, num4, forwardSearchDirection: true, breakIndex, out var breakIndex2, out var substrWidth2);
				FindBreakOpportunity(textBox, text2, containerWidth, num4, forwardSearchDirection: false, breakIndex2, out breakIndex, out substrWidth);
				if (breakIndex != 0)
				{
					if (!(containerWidth < substrWidth2) && breakIndex != breakIndex2)
					{
						num3 = ((Mathf.Abs(num4 - substrWidth) < Mathf.Abs(substrWidth2 - num4)) ? breakIndex : breakIndex2);
					}
					else
					{
						num3 = breakIndex;
					}
				}
				else
				{
					num3 = breakIndex2;
				}
				text3 = text3 + text2.Substring(0, num3).Replace("/ $/", "") + "\n";
				text2 = (textBox.text = text2.Substring(num3));
				num = textBox.preferredWidth;
				num2--;
			}
			textBox.text = text3 + text2;
		}
		return textBox.text;
	}

	private static void FindBreakOpportunity(TextMeshProUGUI textBox, string remainingText, float containerWidth, float desiredWidth, bool forwardSearchDirection, int charIndex, out int breakIndex, out float substrWidth)
	{
		float num = 0f;
		int num2 = (forwardSearchDirection ? 1 : (-1));
		while (true)
		{
			if (!isBreakOpportunity(remainingText, charIndex))
			{
				charIndex += num2;
				continue;
			}
			textBox.text = remainingText.Substring(0, charIndex);
			num = textBox.preferredWidth;
			if (!forwardSearchDirection)
			{
				if (num <= desiredWidth || num <= 0f || charIndex == 0)
				{
					break;
				}
			}
			else if (desiredWidth <= num || containerWidth <= num || charIndex == remainingText.Length)
			{
				break;
			}
			charIndex += num2;
		}
		breakIndex = charIndex;
		substrWidth = num;
	}

	private static bool isBreakOpportunity(string txt, int index)
	{
		if (index != 0 && index != txt.Length)
		{
			if (txt[index - 1] == ' ' && txt[index] != ' ')
			{
				return !WithinTags(txt, index);
			}
			return false;
		}
		return true;
	}

	private static bool WithinTags(string txt, int index)
	{
		bool result = false;
		int num = -1;
		int num2 = -1;
		for (int num3 = index - 1; num3 >= 0; num3--)
		{
			_ = txt[num3];
			if (txt[num3] == '>')
			{
				if (num3 >= 5 && index > 0 && txt.Substring(num3 - 5, "<nobr>".Length) == "<nobr>" && !txt.SubstringFromXToY(num3, index).Contains("</nobr>"))
				{
					result = true;
					break;
				}
				num2 = num3;
			}
			if (txt[num3] == '<' && num2 == -1)
			{
				num = num3;
				break;
			}
		}
		if (num != -1 && txt.Substring(index, txt.Length - index).Contains(">"))
		{
			result = true;
		}
		return result;
	}
}
