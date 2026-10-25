using System.Collections.Generic;
using UnityEngine;

namespace FHGUI.Common.Glossary;

public interface IGlossaryProvider
{
	GameObject GameObject { get; }

	IEnumerable<UIGlossaryData> GetGlossary();
}
