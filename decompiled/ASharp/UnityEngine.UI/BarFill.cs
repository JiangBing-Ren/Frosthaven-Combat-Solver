namespace UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BarFill : MonoBehaviour
{
	[SerializeField]
	private Image fillImage;

	public void SetFillAmount(float amount)
	{
		if (fillImage == null)
		{
			Log.GUIError("No Image Component attached to BarFill.");
		}
		else
		{
			fillImage.fillAmount = amount;
		}
	}
}
