using System;
using UnityEngine;

public class AnimFXTrigger : MonoBehaviour
{
	public class AnimFXTriggerHitEventArgs : EventArgs
	{
		public CharacterManager[] m_Targets;

		public AnimFXTriggerHitEventArgs(CharacterManager[] targets)
		{
			m_Targets = targets;
		}
	}

	[NonSerialized]
	public Texture2D m_ColourTex;

	private VFXLookup vfxLookup;

	public static event EventHandler<AnimFXTriggerHitEventArgs> AnimFXTriggerHit;

	private void OnEnable()
	{
		vfxLookup = gameObject.GetComponent<VFXLookup>();
		if (vfxLookup == null)
		{
			Log.Error("AnimFXTrigger for " + gameObject.name + " was unable to find VFXLookup component");
		}
		_ = vfxLookup.CharacterBodyMesh == null;
	}

	protected virtual void OnAnimFXTriggerHit(AnimFXTriggerHitEventArgs e)
	{
		if (AnimFXTriggerHit != null)
		{
			AnimFXTriggerHit(this, e);
		}
	}

	[Obsolete]
	public void TriggerFX(UnityEngine.Object basicEffect)
	{
		Log.Warning("TriggerFX: Should not be called, coroutine must be invoked instead");
	}

	[Obsolete]
	public void TriggerHitEffectTarget(UnityEngine.Object hitEffectTargetObject)
	{
		Log.Warning("TriggerHitEffectTarget: Should not be called, coroutine must be invoked instead");
	}

	public void TriggerProgressChoreographer(UnityEngine.Object actorEventsObject)
	{
		(actorEventsObject as VFXShared.ActorEventsObject)?.ActorEvents?.ProgressChoreographer();
	}
}
