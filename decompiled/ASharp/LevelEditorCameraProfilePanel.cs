using System;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.CustomLevels;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorCameraProfilePanel : MonoBehaviour
{
	public Toggle ShouldForceCameraToggle;

	[Space]
	public LayoutElement FocalPositionElement;

	public InputField FocalPositionXInput;

	public InputField FocalPositionYInput;

	public InputField FocalPositionZInput;

	[Space]
	public LayoutElement CameraPositionElement;

	public InputField CameraPositionXInput;

	public InputField CameraPositionYInput;

	public InputField CameraPositionZInput;

	[Space]
	public LayoutElement FOVElement;

	public InputField FOVInput;

	[Space]
	public LayoutElement ButtonsElement;

	private CLevelCameraProfile m_ProfileEditing;

	private bool m_IgnoreToggleEvents;

	public void InitForProfile(CLevelCameraProfile profileToSet)
	{
		m_ProfileEditing = profileToSet;
		RefreshUIWithCurrentProfile();
	}

	public void RefreshUIWithCurrentProfile()
	{
		m_IgnoreToggleEvents = true;
		if (ShouldForceCameraToggle != null)
		{
			ShouldForceCameraToggle.isOn = m_ProfileEditing.ShouldForceCamera;
		}
		m_IgnoreToggleEvents = false;
		if (m_ProfileEditing != null)
		{
			if (m_ProfileEditing.FocalPosition != null)
			{
				FocalPositionXInput.text = m_ProfileEditing.FocalPosition.X.ToString();
				FocalPositionYInput.text = m_ProfileEditing.FocalPosition.Y.ToString();
				FocalPositionZInput.text = m_ProfileEditing.FocalPosition.Z.ToString();
			}
			if (m_ProfileEditing.CameraPosition != null)
			{
				CameraPositionXInput.text = m_ProfileEditing.CameraPosition.X.ToString();
				CameraPositionYInput.text = m_ProfileEditing.CameraPosition.Y.ToString();
				CameraPositionZInput.text = m_ProfileEditing.CameraPosition.Z.ToString();
			}
			FOVInput.text = m_ProfileEditing.CameraFieldOfView.ToString();
			FocalPositionElement.gameObject.SetActive(ShouldForceCameraToggle == null || m_ProfileEditing.ShouldForceCamera);
			CameraPositionElement.gameObject.SetActive(ShouldForceCameraToggle == null || m_ProfileEditing.ShouldForceCamera);
			FOVElement.gameObject.SetActive(ShouldForceCameraToggle == null || m_ProfileEditing.ShouldForceCamera);
			ButtonsElement.gameObject.SetActive(ShouldForceCameraToggle == null || m_ProfileEditing.ShouldForceCamera);
		}
	}

	public void OnShouldForceCameraToggled(bool value)
	{
		if (!m_IgnoreToggleEvents)
		{
			FocalPositionElement.gameObject.SetActive(value);
			CameraPositionElement.gameObject.SetActive(value);
			FOVElement.gameObject.SetActive(value);
			ButtonsElement.gameObject.SetActive(value);
			m_ProfileEditing.ShouldForceCamera = value;
		}
	}

	public void OnApplyPressed()
	{
		try
		{
			if (m_ProfileEditing != null)
			{
				float x = float.Parse(FocalPositionXInput.text);
				float y = float.Parse(FocalPositionYInput.text);
				float z = float.Parse(FocalPositionZInput.text);
				float x2 = float.Parse(CameraPositionXInput.text);
				float y2 = float.Parse(CameraPositionYInput.text);
				float z2 = float.Parse(CameraPositionZInput.text);
				float cameraFieldOfView = float.Parse(FOVInput.text);
				if (ShouldForceCameraToggle != null)
				{
					m_ProfileEditing.ShouldForceCamera = ShouldForceCameraToggle.isOn;
				}
				else
				{
					m_ProfileEditing.ShouldForceCamera = true;
				}
				m_ProfileEditing.FocalPosition = new CVector3(x, y, z);
				m_ProfileEditing.CameraPosition = new CVector3(x2, y2, z2);
				m_ProfileEditing.CameraFieldOfView = cameraFieldOfView;
			}
		}
		catch (Exception ex)
		{
			Log.Error("Failed to parse camera position values, please check that no fields are empty - " + ex.Message);
		}
	}

	public void OnGetFromCurrentPressed()
	{
		Vector3 targetFocalPoint = CameraController.s_CameraController.m_TargetFocalPoint;
		Vector3 position = CameraController.s_CameraController.m_Camera.transform.position;
		FocalPositionXInput.text = targetFocalPoint.x.ToString();
		FocalPositionYInput.text = targetFocalPoint.y.ToString();
		FocalPositionZInput.text = targetFocalPoint.z.ToString();
		CameraPositionXInput.text = position.x.ToString();
		CameraPositionYInput.text = position.y.ToString();
		CameraPositionZInput.text = position.z.ToString();
		FOVInput.text = CameraController.s_CameraController.m_Camera.fieldOfView.ToString();
	}

	public void OnSetFromMessagePressed()
	{
		CameraController.s_CameraController.SetCameraWithMessageProfile(m_ProfileEditing);
	}
}
