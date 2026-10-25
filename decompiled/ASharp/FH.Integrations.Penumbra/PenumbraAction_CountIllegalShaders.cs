using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Libs.Penumbra;
using UnityEngine;
using UnityEngine.Scripting;

namespace FH.Integrations.Penumbra;

[Preserve]
internal class PenumbraAction_CountIllegalShaders : PenumbraAction
{
	private const string KEY = "Forbidden Shaders";

	private static string[] _forbiddenShaders = new string[8] { "Universal Render Pipeline/VR", "Universal Render Pipeline/2D", "Universal Render Pipeline/Autodesk Interactive", "Universal Render Pipeline/Lit", "Universal Render Pipeline/Unlit", "Universal Render Pipeline/Nature", "Universal Render Pipeline/Complex Lit", "Universal Render Pipeline/Simple Lit" };

	public override int Priority => -10;

	public override Filtering Binding => new Filtering(Filtering.Type.FilterContains, "ScenarioProcessing");

	public override IEnumerator RunCrt(PenumbraActionParams parameters, PenumbraJob runningJob, PenumbraJobResults outResults)
	{
		Renderer[] array = UnityEngine.Object.FindObjectsOfType<Renderer>();
		HashSet<Shader> hashSet = new HashSet<Shader>();
		try
		{
			HashSet<Shader> hashSet2 = new HashSet<Shader>();
			Renderer[] array2 = array;
			foreach (Renderer renderer in array2)
			{
				if (!renderer)
				{
					continue;
				}
				Material[] sharedMaterials = renderer.sharedMaterials;
				foreach (Material material in sharedMaterials)
				{
					if ((bool)material)
					{
						hashSet.Add(material.shader);
						if ((bool)material.shader && _forbiddenShaders.Contains(material.shader.name))
						{
							hashSet2.Add(material.shader);
							parameters.Logger.Log($"[Assert Failed] Forbidden Shader: {material.shader.name} in {renderer}");
						}
					}
				}
			}
			outResults.Add(new PenumbraMeasurement("Forbidden Shaders")
			{
				BigValue = hashSet2.Count,
				IsAccepted = (hashSet2.Count == 0)
			});
		}
		catch (Exception ex)
		{
			parameters.LogActionError(this, ex.Message);
		}
		yield break;
	}
}
