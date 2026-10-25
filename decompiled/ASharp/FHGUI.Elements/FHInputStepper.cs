using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FHGUI.Elements;

public class FHInputStepper : MonoBehaviour
{
	[SerializeField]
	private Button _left;

	[SerializeField]
	private Button _right;

	[Header("Optional")]
	[SerializeField]
	private Button _middle;

	[SerializeField]
	private Image _fill;

	[SerializeField]
	private TMP_Text _value;

	private int _min;

	private int _max;

	private Action<int> _onIncrease;

	private Action<int> _onDecrease;

	public int Min;

	public int Max = 10;

	public int CurrentValue { get; private set; }

	private void Awake()
	{
		_left.onClick.AddListener(Reduce);
		_right.onClick.AddListener(Increment);
		if (_middle != null)
		{
			_middle.onClick.AddListener(Increment);
		}
	}

	private void OnEnable()
	{
		Init(Min, Max);
	}

	private void OnDisable()
	{
		_onIncrease = null;
		_onDecrease = null;
	}

	public void Init(int minValue, int maxValue, int currentValue = 0, Action<int> onIncrease = null, Action<int> onDecrease = null)
	{
		SetValues(currentValue, minValue, maxValue);
		_onIncrease = onIncrease;
		_onDecrease = onDecrease;
	}

	private void SetValues(int currentValue, int minValue, int maxValue)
	{
		CurrentValue = currentValue;
		_min = minValue;
		_max = maxValue;
		if (_fill != null)
		{
			_fill.fillAmount = (float)currentValue / (float)(maxValue - minValue);
		}
		if (_value != null)
		{
			_value.text = currentValue.ToString();
		}
		_left.interactable = currentValue > minValue;
		_right.interactable = currentValue < maxValue;
	}

	private void Increment()
	{
		if (CurrentValue != _max)
		{
			CurrentValue++;
			SetValues(CurrentValue, _min, _max);
			_onIncrease?.Invoke(CurrentValue);
		}
	}

	private void Reduce()
	{
		if (CurrentValue != _min)
		{
			CurrentValue--;
			SetValues(CurrentValue, _min, _max);
			_onDecrease?.Invoke(CurrentValue);
		}
	}
}
