using Apparance.Net;
using FH.ApparanceBaking;
using UnityEngine;
using UnityEngine.Events;

namespace FH.LevelComponents.ProcGen;

[ExecuteInEditMode]
public class ProceduralProp : ProceduralBase, IProceduralContent, IContentUpdateMonitor
{
	private ParameterCollection Parameters;

	public UnityAction PlacementCompleteAction;

	public void Start()
	{
		((IProceduralContent)this).RebuildContent();
	}

	private int GetAdditionalOptions()
	{
		UnityGameEditorDoorProp component = GetComponent<UnityGameEditorDoorProp>();
		if (component != null)
		{
			return (int)component.LockType();
		}
		return 0;
	}

	private void Rebuild()
	{
		Parameters = ParameterCollection.CreateEmpty();
		Parameters.BeginWrite();
		int num = 0;
		num = ((!(GetRoom() != null)) ? GetScenarioSeed() : GetRoomSeed());
		ProceduralStyle proceduralStyle = GloomUtility.EnsureComponent<ProceduralStyle>(gameObject);
		if ((bool)proceduralStyle)
		{
			ParameterCollection style_struct = Parameters.WriteListBegin(3);
			proceduralStyle.WriteStyleParameters(style_struct, GetScenarioSeed(), num, proceduralStyle.Seed);
			Parameters.WriteListEnd();
			Apparance.Net.Vector3 vector_value = default;
			if (GetRoom() != null)
			{
				vector_value = Conversion.AVfromUV(GetRoom().transform.position);
			}
			Parameters.WriteVector3(vector_value, 4);
			int additionalOptions = GetAdditionalOptions();
			Parameters.WriteInteger(additionalOptions, 5);
		}
		Parameters.EndWrite();
	}

	private void Apply()
	{
		ApparanceEntity component = GetComponent<ApparanceEntity>();
		if ((bool)component && component.enabled)
		{
			component.PartialParameterOverride = Parameters;
			component.IsPopulated = false;
			component.NotifyPropertyChanged();
			component.IsPopulated = true;
			component.NotifyPropertyChanged();
			apparanceDebug.RegisterEntity(component);
		}
	}

	void IProceduralContent.RebuildContent()
	{
		Rebuild();
		Apply();
		if ((bool)EnvironmentLoader.Instance && EnvironmentLoader.Instance.IsApparanceRunning())
		{
			BakedProp component = GetComponent<BakedProp>();
			if (component != null)
			{
				component.Hide();
			}
		}
	}

	public override void NotifyContentPlacementComplete()
	{
		PlacementCompleteAction?.Invoke();
		base.NotifyContentPlacementComplete();
	}

	public override void NotifyContentPlacementStarted()
	{
		base.NotifyContentPlacementStarted();
	}

	public override void NotifyContentRemovalComplete()
	{
		base.NotifyContentRemovalComplete();
	}

	public override void NotifyContentRemovalStarted()
	{
		base.NotifyContentRemovalStarted();
	}
}
