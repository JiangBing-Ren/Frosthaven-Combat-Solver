using FHGUI.DTO;
using UnityEngine;

namespace FHGUI.Elements;

public abstract class PersonalQuestStepUI : MonoBehaviour
{
	public abstract void Init(PersonalQuestStepDTO dto);
}
