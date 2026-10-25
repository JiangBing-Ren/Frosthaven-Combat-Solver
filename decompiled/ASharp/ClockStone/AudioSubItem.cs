using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClockStone;

[Serializable]
public class AudioSubItem
{
	public AudioSubItemType SubItemType;

	public float Probability = 1f;

	public bool DisableOtherSubitems;

	public string ItemModeAudioID;

	public AudioClip Clip;

	public float Volume = 1f;

	public float PitchShift;

	public float Pan2D;

	public float Delay;

	public float RandomPitch;

	public float RandomVolume;

	public float RandomDelay;

	public float ClipStopTime;

	public float ClipStartTime;

	public float FadeIn;

	public float FadeOut;

	public bool RandomStartPosition;

	public List<string> individualSettings = new List<string>();

	private float _summedProbability = -1f;

	internal int _subItemID;

	[NonSerialized]
	private AudioItem _item;

	internal float _SummedProbability
	{
		get
		{
			return _summedProbability;
		}
		set
		{
			_summedProbability = value;
		}
	}

	public AudioItem item
	{
		get
		{
			return _item;
		}
		internal set
		{
			_item = value;
		}
	}

	public AudioSubItem()
	{
	}

	public AudioSubItem(AudioSubItem orig, AudioItem item)
	{
		SubItemType = orig.SubItemType;
		if (SubItemType == AudioSubItemType.Clip)
		{
			Clip = orig.Clip;
		}
		else if (SubItemType == AudioSubItemType.Item)
		{
			ItemModeAudioID = orig.ItemModeAudioID;
		}
		Probability = orig.Probability;
		DisableOtherSubitems = orig.DisableOtherSubitems;
		Clip = orig.Clip;
		Volume = orig.Volume;
		PitchShift = orig.PitchShift;
		Pan2D = orig.Pan2D;
		Delay = orig.Delay;
		RandomPitch = orig.RandomPitch;
		RandomVolume = orig.RandomVolume;
		RandomDelay = orig.RandomDelay;
		ClipStopTime = orig.ClipStopTime;
		ClipStartTime = orig.ClipStartTime;
		FadeIn = orig.FadeIn;
		FadeOut = orig.FadeOut;
		RandomStartPosition = orig.RandomStartPosition;
		for (int i = 0; i < orig.individualSettings.Count; i++)
		{
			individualSettings.Add(orig.individualSettings[i]);
		}
		this.item = item;
	}

	public override string ToString()
	{
		if (SubItemType == AudioSubItemType.Clip)
		{
			return "CLIP: " + Clip.name;
		}
		return "ITEM: " + ItemModeAudioID;
	}
}
