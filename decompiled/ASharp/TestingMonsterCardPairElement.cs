using System.Diagnostics;
using System.IO;
using System.Linq;
using Libs.Utils;
using ScenarioRuleLibrary.YML;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestingMonsterCardPairElement : MonoBehaviour
{
	public Button ButtonOpenYml;

	public RectTransform ContainerPrefab;

	public RectTransform ContainerGenerated;

	public void Init(MonsterCardYMLData card, GameObject cardFromPrefabGO, GameObject cardFromGenerationGO)
	{
		Nest(cardFromPrefabGO.transform, ContainerPrefab);
		Nest(cardFromGenerationGO.transform, ContainerGenerated);
		ButtonOpenYml.GetComponentInChildren<TMP_Text>().text = $"{card.ID}\n\n{card.FileName}";
		ButtonOpenYml.onClick.RemoveAllListeners();
		ButtonOpenYml.onClick.AddListener(() =>
		{
			string text = Directory.GetFiles(YMLLoading.GetRulesetPath(), Path.GetFileName(card.FileName), SearchOption.AllDirectories).FirstOrDefault();
			if (text.IsNOTNullOrEmpty())
			{
				Process.Start(text);
			}
		});
	}

	private static void Nest(Transform target, Transform parent)
	{
		foreach (Transform item in parent.GetChildren().Reverse().ToList())
		{
			Object.Destroy(item.gameObject);
		}
		Vector2 vector = new Vector2(0f, 1f);
		RectTransform rectTransform = (RectTransform)target;
		rectTransform.SetParent(parent);
		rectTransform.anchorMin = vector;
		rectTransform.anchorMax = vector;
		rectTransform.pivot = vector;
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.localScale = Vector3.one;
	}
}
