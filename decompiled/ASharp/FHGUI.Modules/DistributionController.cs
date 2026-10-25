using System;
using TMPro;
using UnityEngine;

namespace FHGUI.Modules;

public class DistributionController : MonoBehaviour
{
	public FHButton LessButton;

	public FHButton MoreButton;

	public TMP_Text Label;

	private int _totalValue;

	private int _mod;

	private int _maxMod;

	private string _name;

	private bool _isGain;

	public Action<int> OnDistributionChanged;

	public void Awake()
	{
		LessButton.onClick.AddListener(OnLess);
		MoreButton.onClick.AddListener(OnMore);
	}

	private void OnLess()
	{
		OnDistributionChanged?.Invoke(-1);
	}

	private void OnMore()
	{
		OnDistributionChanged?.Invoke(1);
	}

	public void Init(string name, int value, int mod, int maxMod, bool isGain, int price)
	{
		_isGain = isGain;
		_name = name;
		_totalValue = value + mod * (isGain ? 1 : (-1));
		_mod = mod;
		_maxMod = maxMod;
		string text;
		if (_mod != 0)
		{
			text = (isGain ? "+" : "-");
		}
		else
		{
			text = "";
		}
		string text2 = ((price >= 0) ? (price + " gold") : "");
		Label.text = $"{_name}\n{_totalValue} {text2} ({text}{mod})";
	}

	public void Refresh(bool isDone)
	{
		if (!_isGain)
		{
			LessButton.interactable = _mod > 0;
			MoreButton.interactable = _totalValue > 0 && !isDone && Math.Abs(_mod) < _maxMod;
		}
		else
		{
			LessButton.interactable = _mod > 0;
			MoreButton.interactable = !isDone && Math.Abs(_mod) < _maxMod;
		}
	}

	public void SetEnabled(bool enabled)
	{
		MoreButton.gameObject.SetActive(enabled);
		LessButton.gameObject.SetActive(enabled);
	}
}
