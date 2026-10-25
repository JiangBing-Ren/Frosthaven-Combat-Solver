using UnityEngine;
using UnityEngine.AddressableAssets;

namespace FH.Tools;

public class PreviewIconReferenceHolder : MonoBehaviour
{
	public AssetReferenceSprite PreviewIcon;

	public bool IsAssigned => PreviewIcon.RuntimeKeyIsValid();
}
