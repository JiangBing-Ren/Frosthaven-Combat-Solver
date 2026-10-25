using UnityEngine;

namespace ClockStone;

public static class AudioControllerHelper
{
	public static AudioSubItem[] _ChooseSubItems(AudioItem audioItem, AudioObject useExistingAudioObj)
	{
		return _ChooseSubItems(audioItem, audioItem.SubItemPickMode, useExistingAudioObj);
	}

	public static AudioSubItem _ChooseSingleSubItem(AudioItem audioItem, AudioPickSubItemMode pickMode, AudioObject useExistingAudioObj)
	{
		return _ChooseSubItems(audioItem, pickMode, useExistingAudioObj)[0];
	}

	public static AudioSubItem _ChooseSingleSubItem(AudioItem audioItem)
	{
		return _ChooseSingleSubItem(audioItem, audioItem.SubItemPickMode, null);
	}

	private static AudioSubItem[] _ChooseSubItems(AudioItem audioItem, AudioPickSubItemMode pickMode, AudioObject useExistingAudioObj)
	{
		if (audioItem.subItems == null)
		{
			return null;
		}
		int num = audioItem.subItems.Length;
		if (num == 0)
		{
			return null;
		}
		int num2 = 0;
		bool flag = (object)useExistingAudioObj != null;
		int num3 = ((!flag) ? audioItem._lastChosen : useExistingAudioObj._lastChosenSubItemIndex);
		if (num > 1)
		{
			switch (pickMode)
			{
			case AudioPickSubItemMode.Disabled:
				return null;
			case AudioPickSubItemMode.StartLoopSequenceWithFirst:
				num2 = (flag ? ((num3 + 1) % num) : 0);
				break;
			case AudioPickSubItemMode.Sequence:
				num2 = (num3 + 1) % num;
				break;
			case AudioPickSubItemMode.SequenceWithRandomStart:
				num2 = ((num3 != -1) ? ((num3 + 1) % num) : Random.Range(0, num));
				break;
			case AudioPickSubItemMode.Random:
				num2 = _ChooseRandomSubitem(audioItem, allowSameElementTwiceInRow: true, num3);
				break;
			case AudioPickSubItemMode.RandomNotSameTwice:
				num2 = _ChooseRandomSubitem(audioItem, allowSameElementTwiceInRow: false, num3);
				break;
			case AudioPickSubItemMode.AllSimultaneously:
			{
				AudioSubItem[] array = new AudioSubItem[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = audioItem.subItems[i];
				}
				return array;
			}
			case AudioPickSubItemMode.TwoSimultaneously:
				return new AudioSubItem[2]
				{
					_ChooseSingleSubItem(audioItem, AudioPickSubItemMode.RandomNotSameTwice, useExistingAudioObj),
					_ChooseSingleSubItem(audioItem, AudioPickSubItemMode.RandomNotSameTwice, useExistingAudioObj)
				};
			case AudioPickSubItemMode.RandomNotSameTwiceOddsEvens:
				num2 = _ChooseRandomSubitem(audioItem, allowSameElementTwiceInRow: false, num3, switchOddsEvens: true);
				break;
			}
		}
		if (flag)
		{
			useExistingAudioObj._lastChosenSubItemIndex = num2;
		}
		else
		{
			audioItem._lastChosen = num2;
		}
		return new AudioSubItem[1] { audioItem.subItems[num2] };
	}

	private static int _ChooseRandomSubitem(AudioItem audioItem, bool allowSameElementTwiceInRow, int lastChosen, bool switchOddsEvens = false)
	{
		int num = audioItem.subItems.Length;
		int result = 0;
		float num2 = 0f;
		float maxInclusive;
		if (!allowSameElementTwiceInRow)
		{
			if (lastChosen >= 0)
			{
				num2 = audioItem.subItems[lastChosen]._SummedProbability;
				if (lastChosen >= 1)
				{
					num2 -= audioItem.subItems[lastChosen - 1]._SummedProbability;
				}
			}
			else
			{
				num2 = 0f;
			}
			maxInclusive = 1f - num2;
		}
		else
		{
			maxInclusive = 1f;
		}
		float num3 = Random.Range(0f, maxInclusive);
		int i;
		for (i = 0; i < num - 1; i++)
		{
			float num4 = audioItem.subItems[i]._SummedProbability;
			if (switchOddsEvens && isOdd(i) == isOdd(lastChosen))
			{
				continue;
			}
			if (!allowSameElementTwiceInRow)
			{
				if (i == lastChosen && (num4 != 1f || !audioItem.subItems[i].DisableOtherSubitems))
				{
					continue;
				}
				if (i > lastChosen)
				{
					num4 -= num2;
				}
			}
			if (num4 > num3)
			{
				result = i;
				break;
			}
		}
		if (i == num - 1)
		{
			result = num - 1;
		}
		return result;
	}

	private static bool isOdd(int i)
	{
		return i % 2 != 0;
	}
}
