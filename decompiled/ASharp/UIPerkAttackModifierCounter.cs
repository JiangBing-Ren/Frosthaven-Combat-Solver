using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPerkAttackModifierCounter : MonoBehaviour
{
	[SerializeField]
	private Image counter;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Color addColor;

	[SerializeField]
	private Color removeColor;

	public bool IsActive { get; private set; }

	public void ShowToAdd()
	{
		IsActive = false;
		counter.enabled = false;
		text.text = "+";
		text.color = addColor;
		text.enabled = true;
		ShowCancelled(cancelled: false);
	}

	public void ShowToRemove()
	{
		IsActive = false;
		counter.enabled = false;
		text.text = "-";
		text.color = removeColor;
		text.enabled = true;
		ShowCancelled(cancelled: false);
	}

	public void ShowActive()
	{
		IsActive = true;
		counter.enabled = true;
		text.enabled = false;
		ShowCancelled(cancelled: false);
	}

	public void ShowCancelled(bool cancelled)
	{
		counter.material = (cancelled ? UIInfoTools.Instance.greyedOutMaterial : null);
		text.material = (cancelled ? UIInfoTools.Instance.greyedOutMaterial : null);
	}
}
