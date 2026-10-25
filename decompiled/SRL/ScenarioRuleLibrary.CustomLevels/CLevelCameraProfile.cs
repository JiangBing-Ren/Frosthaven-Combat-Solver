using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace ScenarioRuleLibrary.CustomLevels;

[Serializable]
public class CLevelCameraProfile : ISerializable
{
	public bool ShouldForceCamera { get; set; }

	public float CameraFieldOfView { get; set; }

	public CVector3 FocalPosition { get; set; }

	public CVector3 CameraPosition { get; set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("ShouldForceCamera", ShouldForceCamera);
		info.AddValue("CameraFieldOfView", CameraFieldOfView);
		info.AddValue("FocalPosition", FocalPosition);
		info.AddValue("CameraPosition", CameraPosition);
	}

	public CLevelCameraProfile(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				switch (item.Name)
				{
				case "ShouldForceCamera":
					ShouldForceCamera = info.GetBoolean("ShouldForceCamera");
					break;
				case "CameraFieldOfView":
					CameraFieldOfView = info.GetSingle("CameraFieldOfView");
					break;
				case "FocalPosition":
					FocalPosition = (CVector3)info.GetValue("FocalPosition", typeof(CVector3));
					break;
				case "CameraPosition":
					CameraPosition = (CVector3)info.GetValue("CameraPosition", typeof(CVector3));
					break;
				}
			}
			catch (Exception ex)
			{
				Log.ErrorWithReport("Exception while trying to deserialize CLevelCameraProfile entry " + item.Name + "\n" + ex.Message + "\n" + ex.StackTrace);
				throw ex;
			}
		}
	}

	public CLevelCameraProfile()
	{
		ShouldForceCamera = false;
		CameraFieldOfView = 0f;
		FocalPosition = new CVector3(0f, 0f, 0f);
		CameraPosition = new CVector3(0f, 0f, 0f);
	}

	public CLevelCameraProfile Copy()
	{
		return new CLevelCameraProfile
		{
			ShouldForceCamera = ShouldForceCamera,
			CameraFieldOfView = CameraFieldOfView,
			FocalPosition = FocalPosition,
			CameraPosition = CameraPosition
		};
	}
}
