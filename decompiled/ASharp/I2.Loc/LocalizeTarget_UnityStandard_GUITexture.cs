using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc;

public class LocalizeTarget_UnityStandard_GUITexture : LocalizeTarget<RawImage>
{
	static LocalizeTarget_UnityStandard_GUITexture()
	{
		AutoRegister();
	}

	[RuntimeInitializeOnLoadMethod]
	private static void AutoRegister()
	{
		LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<RawImage, LocalizeTarget_UnityStandard_GUITexture>
		{
			Name = "GUITexture",
			Priority = 100
		});
	}

	public override eTermType GetPrimaryTermType(Localize cmp)
	{
		return eTermType.Texture;
	}

	public override eTermType GetSecondaryTermType(Localize cmp)
	{
		return eTermType.Text;
	}

	public override bool CanUseSecondaryTerm()
	{
		return false;
	}

	public override bool AllowMainTermToBeRTL()
	{
		return false;
	}

	public override bool AllowSecondTermToBeRTL()
	{
		return false;
	}

	public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
	{
		primaryTerm = (mTarget.texture ? mTarget.texture.name : string.Empty);
		secondaryTerm = null;
	}

	public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
	{
		Texture texture = mTarget.texture;
		if (texture == null || texture.name != mainTranslation)
		{
			mTarget.texture = cmp.FindTranslatedObject<Texture>(mainTranslation);
		}
	}
}
