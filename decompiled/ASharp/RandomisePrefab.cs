using System;
using System.Collections.Generic;
using SETUtil;
using UnityEngine;

[ExecuteAlways]
public class RandomisePrefab : MonoBehaviour
{
	public enum RotationIncrement
	{
		Degrees60 = 0,
		Degrees90 = 1,
		Degrees180 = 2,
		Arbitrary = 99
	}

	private List<GameObject> childObj = new List<GameObject>();

	public int Seed;

	[Tooltip("Turn this on if you want to preview the seed with the game running.")]
	public bool PreviewSeed;

	[Header("")]
	public bool RandomRotation = true;

	public RotationIncrement rotationIncrement;

	private int initialSeed;

	[Header(" ")]
	[Tooltip("WARNING: Doesn't multiply existing scale!")]
	public bool RandomScale;

	[Tooltip("Minimum uniform scale.\nWARNING: Doesn't multiply existing scale!")]
	[SerializeField]
	private float ScaleMin = 1f;

	[Tooltip("Maximum uniform scale.\nWARNING: Doesn't multiply existing scale!")]
	[SerializeField]
	private float ScaleMax = 1f;

	private void Awake()
	{
		initialSeed = Seed;
	}

	private void Start()
	{
		Randomise();
	}

	private void Update()
	{
		if (PreviewSeed && !Application.isPlaying && Seed != initialSeed)
		{
			Randomise();
			initialSeed = Seed;
			Debug.Log("Seed changed!");
		}
	}

	private void Randomise()
	{
		childObj.Clear();
		foreach (Transform item in base.gameObject.transform)
		{
			if (!(item == null))
			{
				childObj.Add(item.gameObject);
			}
		}
		if (childObj.Count == 0)
		{
			return;
		}
		int value = 0;
		System.Random random = new System.Random(Seed + (int)(base.gameObject.transform.position.x * 17f + base.gameObject.transform.position.z * 31f));
		if (childObj.Count > 1)
		{
			value = ((childObj.Count != 5) ? random.Next() : ((int)((base.gameObject.transform.position.x * 31f + base.gameObject.transform.position.z) * 17f) + Seed));
		}
		int index = Math.Abs(value) % childObj.Count;
		GameObject gameObject = null;
		foreach (GameObject item2 in childObj)
		{
			if (item2 != childObj[index])
			{
				item2.SetActive(value: false);
				continue;
			}
			item2.SetActive(value: true);
			gameObject = item2;
			if (RandomRotation)
			{
				if (childObj.Count != 5)
				{
					value = Seed * 13 + (int)(base.gameObject.transform.position.x * 31f + base.gameObject.transform.position.z * 17f);
				}
				int num = 0;
				switch (rotationIncrement)
				{
				case RotationIncrement.Degrees60:
					num = 60;
					break;
				case RotationIncrement.Degrees90:
					num = 90;
					break;
				case RotationIncrement.Degrees180:
					num = 180;
					break;
				case RotationIncrement.Arbitrary:
					num = 1;
					break;
				}
				int num2 = Math.Abs(value) % (360 / num);
				Vector3 eulers = Vector3.up * num * num2;
				gameObject.transform.Rotate(eulers, Space.World);
			}
			if (RandomScale)
			{
				float num3 = MathUtil.SeededRandomRange(random, ScaleMin, ScaleMax);
				gameObject.transform.localScale = new Vector3(num3, num3, num3);
			}
		}
	}
}
