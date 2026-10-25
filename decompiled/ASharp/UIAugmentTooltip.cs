using System.Collections.Generic;
using System.Text;
using GLOOM;
using ScenarioRuleLibrary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAugmentTooltip : MonoBehaviour
{
	[SerializeField]
	public TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	public Transform descriptionContainer;

	[SerializeField]
	public LayoutGroup layoutGroupPadding;

	private GameObject _currentDescriptionObject;

	private CActionAugmentation _consumeData;

	public void Initialize(ConsumeButton consumeButton)
	{
		GameObject descriptionObjectTemplate = (consumeButton.IsUsingDescription ? consumeButton.Description.transform.GetChild(1).gameObject : null);
		Initialize(consumeButton.AbilityConsume.ConsumeData, descriptionObjectTemplate);
	}

	public void Initialize(CActionAugmentation consumeData, GameObject descriptionObjectTemplate = null)
	{
		if (_consumeData == consumeData)
		{
			return;
		}
		_consumeData = consumeData;
		StringBuilder stringBuilder = new StringBuilder();
		List<ElementInfusionBoardManager.EElement> elements = _consumeData.Elements;
		if (_consumeData.Trohpies > 0)
		{
			string term = "FH/GUI_TROPHIES";
			if (_consumeData.Trohpies == 1)
			{
				term = "FH/GUI_TROPHY";
			}
			stringBuilder.Append(string.Format(LocalizationManager.GetTranslation(term), _consumeData.Trohpies));
			if (elements.Count > 0)
			{
				stringBuilder.Append((elements.Count == 1) ? (" " + LocalizationManager.GetTranslation("AND") + " ") : ", ");
			}
		}
		if (_consumeData.Resonance > 0)
		{
			string term2 = "FH/GUI_RESONANCE";
			stringBuilder.Append(string.Format(LocalizationManager.GetTranslation(term2), _consumeData.Resonance));
			if (elements.Count > 0)
			{
				stringBuilder.Append((elements.Count == 1) ? (" " + LocalizationManager.GetTranslation("AND") + " ") : ", ");
			}
		}
		for (int i = 0; i < elements.Count; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append((i == elements.Count - 1) ? (LocalizationManager.GetTranslation("AND") + " ") : ", ");
			}
			stringBuilder.Append("<color=#" + ColorUtility.ToHtmlStringRGB(UIInfoTools.Instance.GetElementHighlightColor(elements[i])) + ">");
			stringBuilder.Append(LocalizationManager.GetTranslation($"GUI_ELEMENT_{elements[i]}"));
			stringBuilder.Append($" <sprite name={elements[i]}></color>");
		}
		string term3 = "GUI_CONSUME_ELEMENT";
		if (_consumeData.CostAbility != null)
		{
			term3 = "FH/GUI_USE";
		}
		else if (consumeData.Elements == null || consumeData.Elements.Count == 0)
		{
			term3 = "GUI_EFFECTS";
		}
		titleText.text = string.Format(LocalizationManager.GetTranslation(term3), stringBuilder);
		titleText.GetComponent<LayoutElement>().enabled = false;
		titleText.ForceMeshUpdate();
		descriptionText.gameObject.SetActive(value: false);
		if (_currentDescriptionObject != null)
		{
			Object.Destroy(_currentDescriptionObject.gameObject);
		}
		float num = titleText.preferredWidth;
		if (descriptionObjectTemplate != null)
		{
			_currentDescriptionObject = Object.Instantiate(descriptionObjectTemplate, descriptionContainer);
			Rect rect = ((RectTransform)descriptionObjectTemplate.transform).rect;
			((RectTransform)_currentDescriptionObject.transform).sizeDelta = new Vector2(rect.width, rect.height);
			if (_currentDescriptionObject.TryGetComponent<TMP_Text>(out var component))
			{
				component.ForceMeshUpdate();
			}
			num = Mathf.Max(rect.width, num);
		}
		UpdateWidth(num);
	}

	public void Initialize(string titleLoc, string textLoc, bool translated = false)
	{
		_consumeData = null;
		if (_currentDescriptionObject != null)
		{
			Object.Destroy(_currentDescriptionObject.gameObject);
		}
		titleText.text = (translated ? titleLoc : LocalizationManager.GetTranslation(titleLoc));
		descriptionText.text = (translated ? textLoc : LocalizationManager.GetTranslation(textLoc));
		descriptionText.gameObject.SetActive(value: true);
		titleText.ForceMeshUpdate();
		descriptionText.ForceMeshUpdate();
		float x = titleText.GetPreferredValues(titleText.text).x;
		float x2 = descriptionText.GetPreferredValues(descriptionText.text).x;
		float targetWidth = Mathf.Max(x, x2);
		UpdateWidth(targetWidth);
	}

	private void UpdateWidth(float targetWidth)
	{
		RectTransform rectTransform = (RectTransform)transform;
		float x = targetWidth + (float)layoutGroupPadding.padding.left + (float)layoutGroupPadding.padding.right;
		rectTransform.sizeDelta = new Vector2(x, rectTransform.sizeDelta.y);
	}
}
