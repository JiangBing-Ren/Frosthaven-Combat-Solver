using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class UIPrefabTooltipTarget : UITooltipTarget
{
	public class TooltipInitParams
	{
		public GameObject Prefab;

		public UnityAction<GameObject> OnObjectCreatedCallback;
	}

	public bool SuppressMissingPrefabError;

	private GameObject prefab;

	private GameObject tooltipObject;

	private UnityAction<GameObject> onObjectCreatedCallback;

	public void Initialize(GameObject prefab, UnityAction<GameObject> onObjectCreatedCallback = null)
	{
		this.prefab = prefab;
		this.onObjectCreatedCallback = onObjectCreatedCallback;
	}

	protected override void ShowTooltip(float delay = -1f)
	{
		UITooltipManager.ResetContent();
		if (prefab != null)
		{
			tooltipObject = Object.Instantiate(prefab, UITooltipManager.GetTransform(), worldPositionStays: false);
			_ = tooltipObject.transform;
			onObjectCreatedCallback?.Invoke(tooltipObject);
			base.ShowTooltip(delay);
		}
		else if (!SuppressMissingPrefabError)
		{
			Log.GUIError("Trying to show a prefab based tooltip but the prefab is null.");
		}
	}

	public void Refresh()
	{
		if ((bool)tooltipObject)
		{
			onObjectCreatedCallback?.Invoke(tooltipObject);
		}
	}

	protected override void SetControls()
	{
		UITooltipManager.SetVerticalControls(autoAdjustHeight, tempIsPrefabTooltip: true);
	}

	public override void HideTooltip(float delay = -1f)
	{
		base.HideTooltip(delay);
		Object.Destroy(tooltipObject);
	}
}
