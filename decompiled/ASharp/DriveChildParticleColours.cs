using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DriveChildParticleColours : MonoBehaviour
{
	public enum ColourElements
	{
		Default,
		Fire,
		Ice,
		Air,
		Earth,
		Dark,
		Light
	}

	public ColourElements activeElement;

	public List<Color> ElementColours;

	private ColourElements lastElement;

	private ParticleSystem[] childParticles;

	private Color currentColour;

	private void Start()
	{
		lastElement = activeElement;
		childParticles = GetComponentsInChildren<ParticleSystem>();
	}

	private void Update()
	{
		if (activeElement != lastElement)
		{
			lastElement = activeElement;
			if (activeElement == ColourElements.Default)
			{
				currentColour = ElementColours[0];
			}
			else if (activeElement == ColourElements.Fire)
			{
				currentColour = ElementColours[1];
			}
			else if (activeElement == ColourElements.Ice)
			{
				currentColour = ElementColours[2];
			}
			else if (activeElement == ColourElements.Air)
			{
				currentColour = ElementColours[3];
			}
			else if (activeElement == ColourElements.Earth)
			{
				currentColour = ElementColours[4];
			}
			else if (activeElement == ColourElements.Dark)
			{
				currentColour = ElementColours[5];
			}
			else if (activeElement == ColourElements.Light)
			{
				currentColour = ElementColours[6];
			}
			ParticleSystem[] array = childParticles;
			for (int i = 0; i < array.Length; i++)
			{
				ParticleSystem.MainModule main = array[i].main;
				main.startColor = currentColour;
				Debug.Log("colorSwitch");
			}
		}
	}
}
