using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FH.Tools;
using Libs.Penumbra;
using UnityEngine;
using UnityEngine.Scripting;

namespace FH.Integrations.Penumbra;

[Preserve]
internal class PenumbraAction_CountCharacters : PenumbraAction
{
	private const string KEY_CHARACTERS = "Characters";

	private const string KEY_CHARACTER_VERTS = "Character Vertices";

	private const string KEY_TOTALVERTS_ENV = "ENV Vertices";

	private const int MAX_CHARACTERS_BUDGET = 49;

	private const int MAX_CHARACTERS_VERT_BUDGET = 1290000;

	public override int Priority => -10;

	public override Filtering Binding => new Filtering(Filtering.Type.FilterContains, "ScenarioProcessing");

	public override IEnumerator RunCrt(PenumbraActionParams parameters, PenumbraJob runningJob, PenumbraJobResults outResults)
	{
		GameObject gameObject = GeneralHelpersRuntime.FindCharactersBoard();
		if (!gameObject)
		{
			parameters.LogActionError(this, "No characters board on " + runningJob.JobContextName);
			outResults.Add(new PenumbraMeasurement("Characters")
			{
				BigValue = -1,
				IsAccepted = false
			});
			yield break;
		}
		try
		{
			int childCount = gameObject.transform.childCount;
			outResults.Add(new PenumbraMeasurement("Characters")
			{
				BigValue = childCount,
				IsAccepted = (childCount <= 49)
			});
			IEnumerable<int> enumerable = (from a in gameObject.GetComponentsInChildren<MeshFilter>()
				select a.sharedMesh ? a.sharedMesh.vertexCount : 0).Concat(from a in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>()
				select a.sharedMesh ? a.sharedMesh.vertexCount : 0);
			long num = 0L;
			foreach (int item in enumerable)
			{
				num += item;
			}
			outResults.Add(new PenumbraMeasurement("Character Vertices")
			{
				BigValue = num,
				IsAccepted = (num <= 1290000),
				SoftLimit = true
			});
			ILogger logger = parameters.Logger;
			long num2 = 0L;
			_ = UnityEngine.Object.FindObjectsOfType<Renderer>().Length;
			try
			{
				MeshFilter[] array = UnityEngine.Object.FindObjectsOfType<MeshFilter>();
				foreach (MeshFilter meshFilter in array)
				{
					if ((bool)meshFilter.sharedMesh)
					{
						num2 += meshFilter.sharedMesh.vertexCount;
					}
				}
			}
			catch (Exception message)
			{
				logger.LogError("Couldn't read vertices.", message);
			}
			SkinnedMeshRenderer[] array2 = UnityEngine.Object.FindObjectsOfType<SkinnedMeshRenderer>();
			_ = array2.Length;
			try
			{
				SkinnedMeshRenderer[] array3 = array2;
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in array3)
				{
					if ((bool)skinnedMeshRenderer.sharedMesh)
					{
						num2 += skinnedMeshRenderer.sharedMesh.vertexCount;
					}
				}
			}
			catch (Exception message2)
			{
				logger.LogError("Couldn't read vertices.", message2);
			}
			long num4 = num2 - num;
			outResults.Add(new PenumbraMeasurement("ENV Vertices")
			{
				BigValue = num4,
				IsAccepted = (num4 <= 9209000),
				SoftLimit = true
			});
		}
		catch (Exception context)
		{
			parameters.LogActionError(this, context);
		}
	}
}
