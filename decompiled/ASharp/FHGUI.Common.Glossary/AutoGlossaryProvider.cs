using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.Common.Glossary;

public class AutoGlossaryProvider : MonoBehaviour, IGlossaryProvider
{
	[SerializeField]
	private bool _checkChildren = true;

	public GameObject GameObject => gameObject;

	public IEnumerable<UIGlossaryData> GetGlossary()
	{
		return LocalizationGlossaryUtils.GetGlossaryContents(gameObject, _checkChildren);
	}
}
