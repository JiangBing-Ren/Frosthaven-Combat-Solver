using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Audio/Button", fileName = "Audio Button Profile")]
public class AudioButtonProfile : ScriptableObject
{
	public string mouseClickAudioItem;

	public string mouseDownAudioItem;

	public string mouseUpAudioItem;

	public string mouseEnterAudioItem;

	public string mouseExitAudioItem;

	public string nonInteractableMouseDownAudioItem;
}
