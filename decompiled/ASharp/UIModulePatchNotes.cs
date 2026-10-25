using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIModulePatchNotes : MonoBehaviour
{
	public UIWindow Window;

	public FHButton ButtonClose;

	public TMP_ColorGradient TextGradientSeen;

	public TMP_ColorGradient TextGradientUnseen;

	private List<UIElementPatchNotesContainer> ___notes;

	private IReadOnlyList<UIElementPatchNotesContainer> Notes => ___notes ?? (___notes = GetComponentsInChildren<UIElementPatchNotesContainer>(includeInactive: true).ToList());

	public int CurrentPatchNotesVersion => Notes.Select((UIElementPatchNotesContainer s) => s.PatchNotesVersion).Max();

	private void Awake()
	{
		ButtonClose.onClick.RemoveAllListeners();
		ButtonClose.onClick.AddListener(Hide);
	}

	public void ShowNotes(int lastSeenVersion)
	{
		foreach (UIElementPatchNotesContainer note in Notes)
		{
			bool flag = lastSeenVersion < note.PatchNotesVersion;
			note.TextHeader.colorGradientPreset = (flag ? TextGradientUnseen : TextGradientSeen);
		}
		Window.Show();
		ButtonClose.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		Window.Hide();
	}
}
