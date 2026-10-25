using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestingTMPSpriteAtlas : MonoBehaviour
{
	public TMP_SpriteAsset Atlas;

	public RectTransform Container;

	public TMP_Text TextPrototype;

	public TMP_InputField InputField_OffsetY;

	public Button ButttonApply;

	private List<TMP_Text> _texts = new List<TMP_Text>();

	public float PercentY = 0.86f;

	private void Awake()
	{
		_texts = Container.GetComponentsInChildren<TMP_Text>().ToList();
		InputField_OffsetY.onSubmit.AddListener((string v) =>
		{
			PercentY = int.Parse(v);
		});
		InputField_OffsetY.onValueChanged.AddListener((string arg) =>
		{
			PercentY = int.Parse(arg);
		});
		InputField_OffsetY.SetValue(PercentY.ToString(CultureInfo.InvariantCulture));
		ButttonApply.onClick.AddListener(MassAdjust);
	}

	private void Start()
	{
		UIHelperUtil.UpdateUIList(Atlas.spriteCharacterTable.Count, _texts, TextPrototype, Container);
		for (int i = 0; i < _texts.Count; i++)
		{
			_texts[i].text = "Testing with some text and <sprite name=" + Atlas.spriteCharacterTable[i].name + "> 5 and more text";
		}
	}

	private void MassAdjust()
	{
	}
}
