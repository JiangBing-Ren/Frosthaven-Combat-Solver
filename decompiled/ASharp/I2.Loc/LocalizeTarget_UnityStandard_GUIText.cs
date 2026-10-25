using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc;

public class LocalizeTarget_UnityStandard_GUIText : LocalizeTarget<Text>
{
	private TextAnchor mAlignment_RTL = TextAnchor.MiddleRight;

	private TextAnchor mAlignment_LTR = TextAnchor.MiddleLeft;

	private bool mAlignmentWasRTL;

	private bool mInitializeAlignment = true;

	static LocalizeTarget_UnityStandard_GUIText()
	{
		AutoRegister();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void AutoRegister()
	{
		LocalizationManager.RegisterTarget(new LocalizeTargetDesc_Type<Text, LocalizeTarget_UnityStandard_GUIText>
		{
			Name = "GUIText",
			Priority = 100
		});
	}

	public override eTermType GetPrimaryTermType(Localize cmp)
	{
		return eTermType.Text;
	}

	public override eTermType GetSecondaryTermType(Localize cmp)
	{
		return eTermType.Font;
	}

	public override bool CanUseSecondaryTerm()
	{
		return true;
	}

	public override bool AllowMainTermToBeRTL()
	{
		return true;
	}

	public override bool AllowSecondTermToBeRTL()
	{
		return false;
	}

	public override void GetFinalTerms(Localize cmp, string Main, string Secondary, out string primaryTerm, out string secondaryTerm)
	{
		primaryTerm = (mTarget ? mTarget.text : null);
		secondaryTerm = ((string.IsNullOrEmpty(Secondary) && mTarget.font != null) ? mTarget.font.name : null);
	}

	public override void DoLocalize(Localize cmp, string mainTranslation, string secondaryTranslation)
	{
		Font secondaryTranslatedObj = cmp.GetSecondaryTranslatedObj<Font>(ref mainTranslation, ref secondaryTranslation);
		if (secondaryTranslatedObj != null && mTarget.font != secondaryTranslatedObj)
		{
			mTarget.font = secondaryTranslatedObj;
		}
		if (mInitializeAlignment)
		{
			mInitializeAlignment = false;
			mAlignment_LTR = (mAlignment_RTL = mTarget.alignment);
			if (LocalizationManager.IsRight2Left && mAlignment_RTL == TextAnchor.MiddleRight)
			{
				mAlignment_LTR = TextAnchor.MiddleLeft;
			}
			if (!LocalizationManager.IsRight2Left && mAlignment_LTR == TextAnchor.MiddleLeft)
			{
				mAlignment_RTL = TextAnchor.MiddleRight;
			}
		}
		if (mainTranslation != null && mTarget.text != mainTranslation)
		{
			if (cmp.CorrectAlignmentForRTL && mTarget.alignment != TextAnchor.MiddleCenter)
			{
				mTarget.alignment = (LocalizationManager.IsRight2Left ? mAlignment_RTL : mAlignment_LTR);
			}
			mTarget.text = mainTranslation;
		}
	}
}
