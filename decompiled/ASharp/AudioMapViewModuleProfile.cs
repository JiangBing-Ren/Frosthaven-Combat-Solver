using UnityEngine;

[CreateAssetMenu(menuName = "UI Config/Audio/MapViewModule", fileName = "Audio MapViewModule Profile")]
public class AudioMapViewModuleProfile : ScriptableObject
{
	public string mouseEnterAudioItem;

	public string mouseExitAudioItem;

	public string OnShow;

	public string OnHide;

	public string mouseClickAudioItem;

	public string mouseDownAudioItem;

	public string mouseUpAudioItem;
}
