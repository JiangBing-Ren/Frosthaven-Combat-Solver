using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FH.LevelComponents.EnvironmentFX;
using FH.LevelComponents.ProcGen;
using IKPn.Extras;
using SETUtil;
using SETUtil.Extend;
using SETUtil.Materials;
using UnityEngine;
using UnityEngine.Rendering;

namespace FH.Tools;

public class PropSpecialCaseAdjuster2000 : MonoBehaviour
{
	private const float F_TOLERANCE = 0.1f;

	private bool DontTouchMeAgain;

	[ContextMenu("Adjust Props")]
	public void AdjustProp()
	{
		if (base.gameObject.name.Contains("IMP_Hanging"))
		{
			base.transform.position = new Vector3(base.transform.position.x, 0.1f, base.transform.position.z);
		}
		else if (base.gameObject.name.Contains("CagedVermling"))
		{
			Vector3 vector = new Vector3(18.915188f, -0.06928253f, 5.956549f) - base.transform.position;
			vector.y = 0f;
			if (vector != Vector3.zero)
			{
				Quaternion rotation = Quaternion.LookRotation(vector);
				base.transform.rotation = rotation;
			}
		}
		else if (base.gameObject.name == "EN_WFW_Scenario038_Blocker")
		{
			if (IsPositionMatch(base.transform.position, new Vector3(27.48827f, 0f, 6.1163716f)))
			{
				base.gameObject.name += "_Adjusted";
				base.transform.SetPositionAndRotation(new Vector3(27.66971f, 0f, 6.0566764f), new Quaternion(0f, -0.9333235f, 0f, 0.35903656f));
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(27.366306f, 0f, 11.981661f)))
			{
				base.gameObject.name += "_Adjusted";
				base.transform.SetPositionAndRotation(new Vector3(27.49571f, 0f, 11.856353f), new Quaternion(0f, -0.24349812f, 0f, 0.96990156f));
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(17.258026f, 0f, 6.1056175f)))
			{
				base.gameObject.name += "_Adjusted";
				base.transform.SetPositionAndRotation(new Vector3(17.332443f, 0f, 6.0346766f), new Quaternion(0f, 0.9909748f, 0f, 0.13404894f));
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(17.049038f, 0f, 11.981661f)))
			{
				base.gameObject.name += "_Adjusted";
				base.transform.SetPositionAndRotation(new Vector3(17.174442f, 0f, 11.945353f), new Quaternion(0f, -0.3220252f, 0f, -0.9467312f));
			}
		}
		else if (base.gameObject.name.Contains("Difficult") && HasParentContaining("ThinDoor") && base.transform.position.y < 0f)
		{
			base.transform.position = new Vector3(base.transform.position.x, 0f, base.transform.position.z);
		}
		else if (base.gameObject.name.Contains("DeckFront") && !DontTouchMeAgain)
		{
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name.Equals("CargoScaleFix") && !(transform.parent == null))
				{
					Vector3 lossyScale = transform.parent.lossyScale;
					Vector3 b = new Vector3(Mathf.Approximately(lossyScale.x, 0f) ? 1f : (1f / lossyScale.x), Mathf.Approximately(lossyScale.y, 0f) ? 1f : (1f / lossyScale.y), Mathf.Approximately(lossyScale.z, 0f) ? 1f : (1f / lossyScale.z));
					Vector3 localScale = transform.localScale;
					transform.localScale = Vector3.Scale(localScale, b);
				}
			}
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("MysteriousMachine") && !DontTouchMeAgain)
		{
			base.transform.localRotation = Quaternion.identity;
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("CV_Wall") && HasParentContaining("FH_11:fe3253cc-8ed8-403b-b709-8389f9a07973") && HasParentContaining("Wall 3") && !base.gameObject.name.Contains("_Moved") && !DontTouchMeAgain)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.x -= 0.8f;
			base.transform.localPosition = localPosition;
			base.gameObject.name += "_Moved";
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name == "EN_CV_WallThin_2x_B" && HasParentContaining("FH_11:fe3253cc-8ed8-403b-b709-8389f9a07973") && HasParentContaining("Wall 4") && IsPositionMatch(base.transform.position, new Vector3(9.317493f, -0.09736753f, -21.224802f)))
		{
			base.gameObject.name += "_Moved";
			base.transform.SetPositionAndRotation(new Vector3(9.317493f, -0.1f, -21.79421f), new Quaternion(0f, 0.7071067f, 1f / 8192f, 0.7071069f));
		}
		else if (base.gameObject.name == "EN_CV_WallThin_2x_A" && HasParentContaining("G:89fe768e-bfc8-4547-b0ff-e7296b9e5e4f") && IsPositionMatch(base.transform.position, new Vector3(9.597496f, -0.09736753f, -17.418657f)))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("EN_CV_Pillar") && IsPositionMatch(base.transform.position, new Vector3(13.071653f, -1.4700003f, -10.354115f)) && HasParentContaining("FH_11_01:9b1db770-2b31-406a-aa34-9455c24c901f"))
		{
			base.transform.SetPositionAndRotation(new Vector3(12.846333f, -1.4700003f, -10.445217f), new Quaternion(0f, 0.28995088f, 0f, 0.9570416f));
			base.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
		}
		else if (base.gameObject.name.Contains("Lshape") && IsPositionMatch(base.transform.position, new Vector3(12.896575f, -0.00999999f, -10.424181f)) && HasParentContaining("FH_11_01:9b1db770-2b31-406a-aa34-9455c24c901f"))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name == "EN_CV_Wall_2x_C" && IsPositionMatch(base.transform.position, new Vector3(14.732707f, -1.8795895E-08f, -10.635564f)) && HasParentContaining("FH_11_01:9b1db770-2b31-406a-aa34-9455c24c901f"))
		{
			base.transform.SetPositionAndRotation(new Vector3(14.653951f, -1.8795895E-08f, -10.78062f), new Quaternion(0f, -0.96592593f, 0f, 0.25881898f));
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("CV_WallThin_2x_A") && IsPositionMatch(base.transform.position, new Vector3(9.808053f, -0.09736753f, -6.15866f)))
		{
			base.transform.localScale = Vector3.one;
			base.gameObject.name += "_ScaleReset";
		}
		else if (base.gameObject.name == "EN_CV_DoorThickFrame_A" && !base.gameObject.name.Contains("_Rotated") && IsPositionMatch(base.transform.position, new Vector3(14.498796f, 0f, -16.458065f)) && HasParentContaining("FH_11:fe3253cc-8ed8-403b-b709-8389f9a07973"))
		{
			base.transform.rotation = new Quaternion(8.605841E-05f, 0.75965655f, 6.67309E-06f, 0.6503246f);
			base.gameObject.name += "_Rotated";
		}
		else if (base.gameObject.name.Contains("CV_DoorThickFrame") && Application.isPlaying && !DontTouchMeAgain)
		{
			Transform parent = base.transform.parent;
			if (parent != null)
			{
				Transform transform2 = null;
				Transform transform3 = null;
				foreach (Transform item in parent)
				{
					string text = item.name;
					if (text.IndexOf("Sshape", StringComparison.OrdinalIgnoreCase) < 0)
					{
						continue;
					}
					if (text.Contains("_CV_"))
					{
						if (transform2 == null && !text.Contains("_Duplicated"))
						{
							transform2 = item;
						}
					}
					else if (transform3 == null)
					{
						transform3 = item;
					}
				}
				if (transform2 != null && transform3 != null)
				{
					transform3.gameObject.SetActive(value: false);
					GameObject gameObject = UnityEngine.Object.Instantiate(transform2.gameObject, transform2.position, transform2.rotation, parent);
					PropSpecialCaseAdjuster2000 component = gameObject.GetComponent<PropSpecialCaseAdjuster2000>();
					if (component != null)
					{
						SceneUtil.SmartDestroy(component);
					}
					gameObject.transform.rotation = transform2.rotation * Quaternion.Euler(0f, 180f, 0f);
					gameObject.name = transform2.name + "_Duplicated";
				}
			}
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("CV_Pillar") && IsPositionMatch(base.transform.position, new Vector3(15.587588f, -1.4700003f, -16.888832f)) && HasParentContaining("FH_11:fe3253cc-8ed8-403b-b709-8389f9a07973"))
		{
			base.transform.SetPositionAndRotation(new Vector3(16.296696f, -1.4700003f, -17.489054f), new Quaternion(0f, -0.86602557f, 0f, 0.49999973f));
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name == "EN_DoorFrame_A" && IsPositionMatch(base.transform.position, new Vector3(9.534701f, 0.02332997f, -19.476532f)) && HasParentContaining("G:89fe768e-bfc8-4547-b0ff-e7296b9e5e4f"))
		{
			Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform5 in componentsInChildren)
			{
				if (transform5 == base.transform)
				{
					continue;
				}
				string text2 = transform5.gameObject.name;
				if (!text2.Contains("_Moved"))
				{
					if (text2.Contains("_Left"))
					{
						Vector3 localPosition2 = transform5.localPosition;
						localPosition2.x += 0.5f;
						transform5.localPosition = localPosition2;
						transform5.gameObject.name = text2 + "_Moved";
					}
					else if (text2.Contains("_Right"))
					{
						Vector3 localPosition3 = transform5.localPosition;
						localPosition3.x -= 0.5f;
						transform5.localPosition = localPosition3;
						transform5.gameObject.name = text2 + "_Moved";
					}
				}
			}
		}
		else if (base.gameObject.name.Contains("EN_CV_Clutter") && (HasParentContaining("FH_11:fe3253cc-8ed8-403b-b709-8389f9a07973") || HasParentContaining("G:89fe768e-bfc8-4547-b0ff-e7296b9e5e4f") || HasParentContaining("K:1e6cbe20-f19e-43c3-be62-da0bc4348f78") || HasParentContaining("M:6f94bd7d-e9a3-41f4-8626-64f16dce3dd1") || HasParentContaining("B:28cc48ec-c9dd-4ad4-b20b-490673b15794")))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(10.596098f, 0.08750849f, -17.24918f)) || IsPositionMatch(base.transform.position, new Vector3(10.941261f, 0.03110662f, -17.71782f)))
			{
				base.gameObject.name += "_TurnedOffManually";
				base.gameObject.SetActive(value: false);
				return;
			}
			float num = 2.25f;
			Transform transform6 = GetParentContaining("FH_11:fe3253cc-8ed8-403b-b709-8389f9a07973") ?? GetParentContaining("G:89fe768e-bfc8-4547-b0ff-e7296b9e5e4f") ?? GetParentContaining("K:1e6cbe20-f19e-43c3-be62-da0bc4348f78") ?? GetParentContaining("M:6f94bd7d-e9a3-41f4-8626-64f16dce3dd1") ?? GetParentContaining("B:28cc48ec-c9dd-4ad4-b20b-490673b15794");
			if (transform6 != null)
			{
				foreach (Transform item2 in transform6)
				{
					if (item2.gameObject.name.IndexOf("TerrainThorns", StringComparison.OrdinalIgnoreCase) >= 0 && (item2.position - base.transform.position).sqrMagnitude <= num)
					{
						if (!base.gameObject.name.Contains("TurnedOffForOverlappingIcySpikes"))
						{
							base.gameObject.name += "_TurnedOffForOverlappingIcySpikes";
						}
						base.gameObject.SetActive(value: false);
						break;
					}
				}
			}
		}
		else if ((base.gameObject.name.Contains("CV_WallTiny") || base.gameObject.name.Contains("CV_Pillar")) && HasParentContaining("M:6f94bd7d-e9a3-41f4-8626-64f16dce3dd1"))
		{
			float num2 = 4f;
			Transform parentContaining = GetParentContaining("M:6f94bd7d-e9a3-41f4-8626-64f16dce3dd1");
			if (parentContaining != null)
			{
				bool flag = false;
				foreach (Transform item3 in parentContaining)
				{
					if (item3.gameObject.name.IndexOf("TerrainThorns", StringComparison.OrdinalIgnoreCase) >= 0 && (base.transform.position - item3.position).sqrMagnitude <= num2)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					Vector3 vector2 = base.transform.position - parentContaining.position;
					vector2.Normalize();
					base.transform.position += vector2 * 0.5f;
					base.gameObject.name += "_MovedAwayFromCentre";
				}
			}
		}
		else if (base.gameObject.name.Contains("B:28cc48ec-c9dd-4ad4-b20b-490673b15794"))
		{
			Transform transform9 = base.transform.Find("Walls/Wall 1/Generated Content");
			if (transform9 != null)
			{
				Transform[] componentsInChildren = transform9.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform10 in componentsInChildren)
				{
					if (transform10.name.Contains("EN_CV_Wall_2x_J"))
					{
						transform10.name += "_TurnedOffForOverlappingIcySpike";
						transform10.gameObject.SetActive(value: false);
						break;
					}
				}
			}
			Transform transform11 = base.transform.Find("Walls/Wall 4/Generated Content");
			if (transform11 != null)
			{
				Transform[] componentsInChildren = transform11.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform12 in componentsInChildren)
				{
					if (transform12.name.Contains("EN_CV_WallNarrow_1x_A") && IsPositionMatch(transform12.position, new Vector3(-1.5097709f, -0.035000112f, 3.1187844f)))
					{
						transform12.name += "_TurnedOff_ForIcySpike";
						transform12.gameObject.SetActive(value: false);
					}
					else if (transform12.name.Contains("EN_CV_Stalacton_A"))
					{
						if (IsPositionMatch(transform12.position, new Vector3(-1.3137703f, -0.204f, -0.36541462f)))
						{
							transform12.name += "_MovedForIcySpike";
							transform12.SetPositionAndRotation(new Vector3(-1.7117705f, -0.204f, 1.4795854f), new Quaternion(0f, -0.6087612f, 0f, 0.7933537f));
						}
					}
					else if (transform12.name.Contains("Point Light") && IsPositionMatch(transform12.position, new Vector3(-1.0977707f, 2.577f, 1.1765857f)))
					{
						transform12.name += "_MovedForIcySpike";
						transform12.position = new Vector3(-0.58777046f, 2.577f, -0.24541426f);
					}
				}
			}
		}
		else if (base.gameObject.name.Contains("M:6f94bd7d-e9a3-41f4-8626-64f16dce3dd1"))
		{
			Transform transform13 = base.transform.Find("Walls/Wall 5/Generated Content");
			if (transform13 != null)
			{
				Transform[] componentsInChildren = transform13.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform14 in componentsInChildren)
				{
					if (transform14.name.Contains("EN_CV_Wall_2x_B") && IsPositionMatch(transform14.position, new Vector3(0.4863124f, -0.15999998f, 4.228897f)))
					{
						transform14.position = new Vector3(0.4893167f, -0.15999998f, 4.430702f);
					}
				}
			}
		}
		else if (base.gameObject.name == "Terrain_Difficult_CV" && (HasParentContaining("FH_11:fe3253cc-8ed8-403b-b709-8389f9a07973") || HasParentContaining("G:89fe768e-bfc8-4547-b0ff-e7296b9e5e4f") || HasParentContaining("K:1e6cbe20-f19e-43c3-be62-da0bc4348f78") || HasParentContaining("M:6f94bd7d-e9a3-41f4-8626-64f16dce3dd1") || HasParentContaining("B:28cc48ec-c9dd-4ad4-b20b-490673b15794")))
		{
			base.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
		}
		else if (base.gameObject.name == "EN_SHP_Hatch_A")
		{
			base.transform.rotation = new Quaternion(0f, 1f, 0f, 0f);
		}
		else if (base.gameObject.name.Contains("RockColumn"))
		{
			Vector3 vector3 = new Vector3(6.8781776f, 0f, 0f) - base.transform.position;
			vector3.y = 0f;
			if (vector3 != Vector3.zero)
			{
				Quaternion rotation2 = Quaternion.LookRotation(vector3);
				base.transform.rotation = rotation2;
			}
			SimpleRotate[] componentsInChildren2 = GetComponentsInChildren<SimpleRotate>(includeInactive: true);
			foreach (SimpleRotate obj in componentsInChildren2)
			{
				FieldInfo field = typeof(SimpleRotate).GetField("vector", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null)
				{
					Vector3 vector4 = (Vector3)field.GetValue(obj);
					float num3 = UnityEngine.Random.Range(1f, 1.2f) * (float)((UnityEngine.Random.value > 0.5f) ? 1 : (-1));
					vector4.y *= num3;
					field.SetValue(obj, vector4);
				}
			}
		}
		else if (base.gameObject.name.Contains("MR_Bay_H") && !DontTouchMeAgain)
		{
			float num4 = 25f;
			foreach (Transform item4 in base.transform.parent)
			{
				if (item4.name != "Full")
				{
					continue;
				}
				foreach (Transform item5 in item4)
				{
					string text3 = item5.gameObject.name;
					if (text3.IndexOf("debris", StringComparison.OrdinalIgnoreCase) >= 0 && !((item5.position - base.transform.position).sqrMagnitude > num4) && base.transform.InverseTransformPoint(item5.position).z < 0f)
					{
						item5.gameObject.name = text3 + "_TurnedOffDebrisOverShafts";
						item5.gameObject.SetActive(value: false);
					}
				}
			}
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name == "EN_CV_Pillar_A" && IsPositionMatch(base.transform.position, new Vector3(-3.2908726f, -1.4700003f, -2.5985293f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_Moved";
			base.transform.position = new Vector3(-3.199089f, -1.4700003f, -5.9911175f);
		}
		else if (base.gameObject.name == "EN_CV_WallThin_2x_A" && IsPositionMatch(base.transform.position, new Vector3(-3.0990887f, -0.09736753f, -4.252961f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name == "EN_CV_WallNarrow_1x_A" && IsPositionMatch(base.transform.position, new Vector3(-3.0787992f, -0.034999985f, -3.1319487f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name == "EN_CV_WallThin_2x_C" && IsPositionMatch(base.transform.position, new Vector3(-2.719317f, -0.09736753f, 3.3555365f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_Adjusted_Right";
			base.transform.SetPositionAndRotation(new Vector3(-2.6503172f, -0.09736753f, 3.4460006f), new Quaternion(0f, 0.65843594f, 0f, 0.7526369f));
			base.transform.localScale = new Vector3(0.7714393f, 1f, 0.44574478f);
		}
		else if (base.gameObject.name == "EN_DoorFrame_A" && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if ((base.gameObject.name.Contains("CV_Hex_Hshape") || base.gameObject.name.Contains("CV_Hex_Jshape")) && IsPositionMatch(base.transform.position, new Vector3(-3.439087f, 0f, -2.9783375f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (Application.isPlaying && base.gameObject.name.Contains("CV_Hex_Fshape") && IsPositionMatch(base.transform.position, new Vector3(-4.29875f, -3.874302E-07f, -4.467396f)) && HasParentContaining("FS111:") && !base.gameObject.name.Contains("_Handled"))
		{
			base.gameObject.name += "_Handled";
			InstantiateDuplicate(new Vector3(-3.4192667f, -3.874302E-07f, -2.985f), new Quaternion(0f, 0f, 0f, 1f)).name = base.gameObject.name + "_Duplicate";
		}
		else if (base.gameObject.name == "EN_CV_Pillar_A" && IsPositionMatch(base.transform.position, new Vector3(-3.3225074f, -1.4700003f, 2.6301649f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_Moved";
			base.transform.position = new Vector3(-3.0090885f, -1.4700003f, 5.7711177f);
		}
		else if (base.gameObject.name.Contains("CV_WallThin_2x_C") && IsPositionMatch(base.transform.position, new Vector3(-3.0990887f, -0.09736753f, 4.251125f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name == "EN_CV_WallNarrow_1x_A" && IsPositionMatch(base.transform.position, new Vector3(-3.077756f, -0.034999985f, 3.1319487f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name == "EN_CV_WallThin_2x_A" && IsPositionMatch(base.transform.position, new Vector3(-2.719317f, -0.09736753f, -3.354773f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_Adjusted_Left";
			base.transform.SetPositionAndRotation(new Vector3(-2.719316f, -0.09736753f, -3.6300006f), new Quaternion(0f, 0.7933532f, 0f, 0.6087618f));
			base.transform.localScale = new Vector3(0.6f, 1f, 0.5f);
		}
		else if (base.gameObject.name.Contains("CV_Hex") && IsPositionMatch(base.transform.position, new Vector3(-3.439087f, 0f, 2.9783378f)) && HasParentContaining("FS111:"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (Application.isPlaying && base.gameObject.name.Contains("CV_Hex") && IsPositionMatch(base.transform.position, new Vector3(-4.299009f, -3.874302E-07f, 4.4675446f)) && HasParentContaining("FS111:") && !base.gameObject.name.Contains("_Handled"))
		{
			base.gameObject.name += "_Handled";
			InstantiateDuplicate(new Vector3(-3.439087f, 0f, 2.9783375f), new Quaternion(0f, -1.0000001f, 0f, 0f)).name = base.gameObject.name + "_Duplicate";
		}
		else if (base.gameObject.name.Contains("DoorFrame_BoneVariants") && !DontTouchMeAgain)
		{
			if (HasParentContaining("FH_01_01"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(3.4390888f, -0.03999996f, -8.935015f)))
				{
					ToggleChildren("EN_MNT_DoorFrame_Bone_Thin_A", wantedState: true);
					ToggleChildren("EN_MNT_DoorFrame_Bone_Thin_B", wantedState: false);
					base.transform.localRotation = Quaternion.LookRotation(Vector3.left);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(8.597722f, -0.03999996f, -8.935015f)))
				{
					ToggleChildren("EN_MNT_DoorFrame_Bone_Thin_B", wantedState: true);
					ToggleChildren("EN_MNT_DoorFrame_Bone_Thin_A", wantedState: false);
					base.transform.localRotation = Quaternion.LookRotation(Vector3.left);
				}
			}
			else if (HasParentContaining("G:"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(3.4390888f, -0.03999996f, -0f)))
				{
					ToggleChildren("EN_MNT_DoorFrame_Bone_Thin_B", wantedState: true);
					ToggleChildren("EN_MNT_DoorFrame_Bone_Thin_A", wantedState: false);
					base.transform.localRotation = Quaternion.LookRotation(Vector3.right);
				}
			}
			else if (HasParentContaining("FH_02_06b:") && IsPositionMatch(base.transform.position, new Vector3(8.597722f, -0.03999996f, 0f)))
			{
				ToggleChildren("EN_MNT_DoorFrame_Bone_Thin_A", wantedState: true);
				ToggleChildren("EN_MNT_DoorFrame_Bone_Thin_B", wantedState: false);
				base.transform.localRotation = Quaternion.LookRotation(Vector3.right);
			}
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("SpawnerTunnel_3hex"))
		{
			base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, 240f, base.transform.localEulerAngles.z);
		}
		else if (base.gameObject.name.Contains("SpawnerTunnel_2hex"))
		{
			base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, 60f, base.transform.localEulerAngles.z);
		}
		else if (base.gameObject.name.Contains("BigCogWheel") && !DontTouchMeAgain)
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("MR_Wall_1x_A") && HasParentContaining("Wall 4") && HasParentContaining("D:94d43154-ffaf-4acd-9208-bd526bd99a08"))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("MR_Lamp") && ((HasParentContaining("D:94d43154-ffaf-4acd-9208-bd526bd99a08") && (HasParentContaining("Wall 3") || HasParentContaining("Wall 5"))) || (HasParentContaining("M:e888fe01-a6a9-4d44-b149-ea0441b6e327") && HasParentContaining("Wall 7")) || (HasParentContaining("K:f7c10db0-4ae9-4203-aece-f25db96a61a2") && HasParentContaining("Wall 9"))))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("MR_Lamp") && HasParentContaining("J:d865523d-f2e1-4077-aeed-d192841f2729") && IsPositionMatch(base.transform.position, new Vector3(3.2779112f, 2.0824494f, -12.403499f)))
		{
			base.transform.SetPositionAndRotation(new Vector3(3.2102504f, 2.0824494f, -12.557768f), new Quaternion(0f, -0.9067395f, 0f, 0.42169148f));
		}
		else if (base.gameObject.name.Contains("SupportRailColumns") && !DontTouchMeAgain)
		{
			base.transform.localRotation = Quaternion.identity;
			if (base.gameObject.name.ToLower().Contains("1hex"))
			{
				base.transform.localPosition = Vector3.zero;
			}
			else if (base.gameObject.name.ToLower().Contains("2hex"))
			{
				base.transform.position = new Vector3(9.447267f, base.transform.position.y, base.transform.position.z);
			}
			if (HasParentContaining("HexObstacle"))
			{
				Transform parentContaining2 = GetParentContaining("HexObstacle");
				if (parentContaining2 != null)
				{
					parentContaining2.localRotation = Quaternion.identity;
				}
			}
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("VLT_Light_Lantern") && (HasParentContaining("FH_09_09:20c5a3ce-9403-49e7-b1aa-9f420d1dd7a7") || HasParentContaining("A:120be751-16c1-4b96-aab0-863ab4a2f877")) && (IsPositionMatch(base.transform.position, new Vector3(2.8344316f, 2.6933634f, -13.392287f)) || IsPositionMatch(base.transform.position, new Vector3(2.8344276f, 2.6933634f, -13.909144f))))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("VLT_Candle") && IsPositionMatch(base.transform.position, new Vector3(8.291722f, 0.12999979f, -10.520259f)))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("EN_VLT_Dirt") && (HasParentContaining("FH_09_09:20c5a3ce-9403-49e7-b1aa-9f420d1dd7a7") || HasParentContaining("A:120be751-16c1-4b96-aab0-863ab4a2f877") || HasParentContaining("A:70aae230-fda2-4f7a-897f-dd05ab90f72f")))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(-2.893155f, 0.0226541f, -10.337073f)) && base.gameObject.name.Contains("DirtA"))
			{
				base.transform.position = new Vector3(10.098455f, 0.094999f, 0.1515961f);
				base.gameObject.name += "_DecalMoved";
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(-2.893155f, 0.0226541f, -10.337073f)) && base.gameObject.name.Contains("DirtB"))
			{
				base.transform.position = new Vector3(0.2564554f, 0.05f, -9.239404f);
				base.gameObject.name += "_DecalMoved";
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(7.3276997f, 0.02437904f, -9.935186f)) || IsPositionMatch(base.transform.position, new Vector3(-2.9500551f, 0.0235279f, -10.354652f)) || IsPositionMatch(base.transform.position, new Vector3(-2.893155f, 0.0226541f, -10.337073f)) || IsPositionMatch(base.transform.position, new Vector3(-2.4321432f, 0.02015626f, -9.79562f)) || IsPositionMatch(base.transform.position, new Vector3(-2.2599401f, 0.02378883f, -9.832329f)) || IsPositionMatch(base.transform.position, new Vector3(6.729402f, -0.009f, -13.006131f)) || IsPositionMatch(base.transform.position, new Vector3(2.376955f, 0.02110635f, 1.2439228f)) || IsPositionMatch(base.transform.position, new Vector3(2.4443579f, 0.02161904f, 1.7960231f)) || IsPositionMatch(base.transform.position, new Vector3(2.0352836f, 0.02454743f, 1.2507492f)) || IsPositionMatch(base.transform.position, new Vector3(2.1395526f, 0.02492912f, 2.086975f)) || IsPositionMatch(base.transform.position, new Vector3(13.2734165f, 0.02034779f, 1.630574f)) || IsPositionMatch(base.transform.position, new Vector3(13.53156f, 0.02289732f, 2.5386748f)) || IsPositionMatch(base.transform.position, new Vector3(13.126549f, 0.02138204f, 2.6692188f)) || IsPositionMatch(base.transform.position, new Vector3(4.2340226f, 0.02438377f, -16.459532f)) || IsPositionMatch(base.transform.position, new Vector3(4.6924825f, 0.02132019f, -16.955328f)) || IsPositionMatch(base.transform.position, new Vector3(4.859304f, 0.02421026f, -16.998737f)) || IsPositionMatch(base.transform.position, new Vector3(4.6560516f, 0.02450061f, -17.03111f)) || IsPositionMatch(base.transform.position, new Vector3(-2.6856008f, 0.0248614f, -17.211327f)) || IsPositionMatch(base.transform.position, new Vector3(-2.3716726f, 0.02078485f, -16.884007f)) || IsPositionMatch(base.transform.position, new Vector3(-3.0442495f, 0.0202327f, -16.663952f)) || IsPositionMatch(base.transform.position, new Vector3(12.701115f, 0.02310011f, 5.6410046f)) || IsPositionMatch(base.transform.position, new Vector3(13.26032f, 0.02068417f, 8.059931f)) || IsPositionMatch(base.transform.position, new Vector3(13.627961f, 0.0249389f, 7.249154f)) || IsPositionMatch(base.transform.position, new Vector3(13.403489f, 0.0210613f, 7.76935f)) || IsPositionMatch(base.transform.position, new Vector3(-3.0643568f, 0.02306015f, -17.24201f)) || IsPositionMatch(base.transform.position, new Vector3(-3.2980156f, 0.02439503f, -17.017975f)) || IsPositionMatch(base.transform.position, new Vector3(-2.7458854f, 0.02213879f, -14.34601f)) || IsPositionMatch(base.transform.position, new Vector3(-2.748391f, 0.02365521f, -14.29234f)))
			{
				base.gameObject.SetActive(value: false);
				base.gameObject.name += "_DecalTurnedOff";
			}
		}
		else if (base.gameObject.name.Contains("EN_VLT_Dirt") && (HasParentContaining("I:0b7b3ab9-ea53-43d7-8682-a27faf994f33") || HasParentContaining("L:f3594f2d-8ba3-4ae0-863f-000841f05e0d") || HasParentContaining("D:0cc93083-3414-4cc3-ac02-b766645d9214") || HasParentContaining("H:289ed255-0a5b-4d0f-864d-8def6313c9eb")))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(-14.049629f, 0.02344309f, 6.353739f)) || IsPositionMatch(base.transform.position, new Vector3(-14.060329f, 0.02311437f, 6.748398f)) || IsPositionMatch(base.transform.position, new Vector3(-13.329631f, 0.02053627f, 6.4736795f)) || IsPositionMatch(base.transform.position, new Vector3(-13.670385f, 0.02015547f, 6.6464624f)) || IsPositionMatch(base.transform.position, new Vector3(-13.715626f, 0.02124947f, 6.8144917f)) || IsPositionMatch(base.transform.position, new Vector3(-5.7899966f, 0.02266506f, -0.40820837f)) || IsPositionMatch(base.transform.position, new Vector3(-5.076662f, 0.02242235f, 6.080124f)) || IsPositionMatch(base.transform.position, new Vector3(-4.983825f, 0.02425755f, 6.521492f)) || IsPositionMatch(base.transform.position, new Vector3(-4.9165635f, 0.02459998f, 6.3983464f)) || IsPositionMatch(base.transform.position, new Vector3(-4.8527904f, 0.02239673f, 6.539383f)) || IsPositionMatch(base.transform.position, new Vector3(-4.613737f, 0.02271029f, 6.3976536f)) || IsPositionMatch(base.transform.position, new Vector3(-3.6867576f, 0.02462573f, 2.507748f)) || IsPositionMatch(base.transform.position, new Vector3(-3.911706f, 0.02118969f, 3.0105906f)) || IsPositionMatch(base.transform.position, new Vector3(-2.9627752f, 0.02149019f, 2.3192494f)) || IsPositionMatch(base.transform.position, new Vector3(-1.1927912f, 0.0233622f, 13.779606f)) || IsPositionMatch(base.transform.position, new Vector3(-0.50891256f, 0.02014401f, 13.644834f)) || IsPositionMatch(base.transform.position, new Vector3(-0.5246861f, 0.02355057f, 13.610423f)) || IsPositionMatch(base.transform.position, new Vector3(1.7915592f, 0.02252613f, 13.881935f)) || IsPositionMatch(base.transform.position, new Vector3(2.4065862f, 0.02004964f, 13.203956f)) || IsPositionMatch(base.transform.position, new Vector3(2.2350488f, 0.02465145f, 13.6481695f)) || IsPositionMatch(base.transform.position, new Vector3(2.68484f, 0.02218565f, 13.405605f)) || IsPositionMatch(base.transform.position, new Vector3(2.1046338f, 0.02481008f, 13.999571f)) || IsPositionMatch(base.transform.position, new Vector3(2.9556131f, 0.02244207f, 14.057627f)) || IsPositionMatch(base.transform.position, new Vector3(2.7906983f, 0.02080144f, 13.956724f)) || IsPositionMatch(base.transform.position, new Vector3(3.1626077f, 0.02251579f, 13.274618f)) || IsPositionMatch(base.transform.position, new Vector3(8.652471f, 0.02495966f, 9.117525f)) || IsPositionMatch(base.transform.position, new Vector3(8.341867f, 0.02193481f, 9.458972f)) || IsPositionMatch(base.transform.position, new Vector3(8.432184f, 0.02423641f, 9.15552f)) || IsPositionMatch(base.transform.position, new Vector3(15.219993f, 0.02485638f, -0.2560744f)) || IsPositionMatch(base.transform.position, new Vector3(15.473888f, 0.02492742f, 0.118803f)) || IsPositionMatch(base.transform.position, new Vector3(15.633253f, 0.02302169f, 9.005835f)) || IsPositionMatch(base.transform.position, new Vector3(16.175894f, 0.02495872f, -0.3556089f)))
			{
				base.gameObject.name += "_TurnedOff";
				base.gameObject.SetActive(value: false);
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(-4.7197323f, 0.02461195f, -0.05696678f)))
			{
				base.transform.SetPositionAndRotation(new Vector3(-4.5488615f, 0.02461195f, 1.1483378f), new Quaternion(0f, -0.6686626f, 0f, 0.7435661f));
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(-3.9473782f, 0.02069942f, 2.2295928f)) && base.gameObject.name == "EN_VLT_DirtC")
			{
				base.transform.position = new Vector3(-1.0223267f, base.transform.position.y, 1.955234f);
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(6.988696f, 0.02338815f, 2.6259081f)))
			{
				base.transform.position = new Vector3(4.307949f, base.transform.position.y, 1.5689228f);
			}
		}
		else if (base.gameObject.name.Contains("VLT_Wall") && HasParentContaining("H:289ed255-0a5b-4d0f-864d-8def6313c9eb") && IsPositionMatch(base.transform.position, new Vector3(-2.7602425f, -0.0250001f, 1.735559f)))
		{
			base.transform.position = new Vector3(-2.6373167f, base.transform.position.y, base.transform.position.z);
		}
		else if (base.gameObject.name.Contains("CV_Wall") && HasParentContaining("J:1ca46b23-782f-4fa2-867b-726ed184a5a1") && HasParentContaining("Wall 1") && IsPositionMatch(base.transform.position, new Vector3(-3.5008638f, -0.135f, -15.475908f)))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name == "EN_CST_Wall_K" && HasParentContaining("A:cff4a184-4d0c-4614-9ae2-6f9605000565") && IsPositionMatch(base.transform.position, new Vector3(0.2004213f, 8.220529E-07f, 11.855695f)))
		{
			base.transform.position = new Vector3(0.3199997f, 8.220529E-07f, 11.727938f);
		}
		else if (base.gameObject.name == "EN_CST_Wall_C" && HasParentContaining("A:cff4a184-4d0c-4614-9ae2-6f9605000565") && IsPositionMatch(base.transform.position, new Vector3(0.2004213f, 8.220529E-07f, 13.466738f)))
		{
			base.transform.position = new Vector3(0.3495326f, 8.220529E-07f, 14.888935f);
		}
		else if (base.gameObject.name == "EN_CST_Pillar_A" && HasParentContaining("A:cff4a184-4d0c-4614-9ae2-6f9605000565") && IsPositionMatch(base.transform.position, new Vector3(0.1540275f, -0.0192572f, 14.2821045f)))
		{
			base.transform.position = new Vector3(0.3031387f, -0.01925723f, 15.704302f);
		}
		else if (base.gameObject.name.Contains("RandomBPillar") && HasParentContaining("B:172933cc-1b40-472c-8460-9e52421de1f5") && IsPositionMatch(base.transform.position, new Vector3(-2.042626f, -0.06666479f, 4.927899f)))
		{
			base.transform.position = new Vector3(-1.5548608f, -0.06666479f, 6.8934813f);
		}
		if ((base.gameObject.name.Contains("EN_WFW_Door") && HasParentContaining("J:8a5ef3dd-4581-4fd3-85be-62403a03924d")) || (base.gameObject.name.Contains("EN_RF_Door") && HasParentContaining("E:87c2447b-42fc-41ad-9285-2439968eaf52")))
		{
			if (Application.isPlaying && !DontTouchMeAgain)
			{
				MeshRenderer[] componentsInChildren3 = base.gameObject.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
				for (int i = 0; i < componentsInChildren3.Length; i++)
				{
					Material material = componentsInChildren3[i].GetMaterialClonesSlow().FirstOrDefault((Material a) => a.name.Contains("RF_DoorsA") || a.name.Contains("WFW_Trim") || a.name.Contains("WFW_Flora"));
					if (!material)
					{
						break;
					}
					material.SetFloat("_UseEmissiveMap", 1f);
					if (material.GetTexture("_Emissive_Map") == null)
					{
						material.SetColor("_Emissive_Color", Color.black);
						material.SetColor("_Emissive_Boost", Color.black);
					}
					material.shader = Shader.Find("Amp_Basic_N_MRAO");
				}
				DontTouchMeAgain = true;
			}
		}
		else if ((base.gameObject.name.Contains("MetalObstacle_2hex") || base.gameObject.name.Contains("CV_Obstacle_2hex")) && !HasParentContaining("Bay"))
		{
			base.transform.localPosition = new Vector3(-0.88f, 0f, 0f);
		}
		else if (HasParentContaining("FH_07_01"))
		{
			if (base.gameObject.name.Contains("EN_CV_Pillar_A"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(5.010417f, -1.47f, 10.044376f)) || IsPositionMatch(base.transform.position, new Vector3(5.0420527f, -1.47f, 10.772356f)) || IsPositionMatch(base.transform.position, new Vector3(8.745939f, -1.47f, 10.803991f)) || IsPositionMatch(base.transform.position, new Vector3(8.714304f, -1.47f, 10.076012f)))
				{
					base.gameObject.SetActive(value: false);
				}
			}
			else if (base.gameObject.name.Contains("EN_CV_WallTiny_1x_A"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(9.062723f, -0.0050000027f, 10.173471f)) || IsPositionMatch(base.transform.position, new Vector3(4.6936336f, -0.0050000027f, 10.674896f)))
				{
					base.gameObject.SetActive(value: false);
				}
			}
			else if (base.gameObject.name.Contains("EN_CV_Hang_NestedStalagmite_E") && IsPositionMatch(base.transform.position, new Vector3(3.7764468f, 3.01f, 9.962794f)))
			{
				base.transform.position = new Vector3(3.4f, 3.01f, 9.962794f);
			}
		}
		else if (base.gameObject.name.Contains("T_CF_Clutter") && HasParentContaining("G:accd95fb-e5e4-4e0e-bbb9-7caf93ba89fb"))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(16.460192f, 0.02966791f, 6.9625025f)) || IsPositionMatch(base.transform.position, new Vector3(28.193918f, 0.02926053f, 7.283922f)) || IsPositionMatch(base.transform.position, new Vector3(28.444508f, 0.02940073f, 6.9089f)) || IsPositionMatch(base.transform.position, new Vector3(28.476904f, 0.02558422f, 6.9691987f)))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (!DontTouchMeAgain && base.gameObject.name.Contains("FH_08:45fa8d3e-e352-4cc0-8c0e-7b469fe1ca9e"))
		{
			Transform transform17 = base.transform.Find("Walls/Wall 1/Generated Content");
			if (transform17 != null)
			{
				foreach (Transform item6 in transform17)
				{
					if (item6.name.Contains("EN_VLT_Pillar_B") && !item6.name.Contains("_Moved"))
					{
						item6.position = new Vector3(29.733852f, 5.238057E-07f, 16.197262f);
						item6.name += "_Moved";
						break;
					}
				}
				foreach (Transform item7 in transform17)
				{
					if (item7.name.Contains("EN_VLT_Wall_Narrow_A") && !item7.name.Contains("_TurnedOff"))
					{
						item7.name += "_TurnedOff";
						item7.gameObject.SetActive(value: false);
					}
				}
				foreach (Transform item8 in transform17)
				{
					if (item8.name.Contains("EN_VLT_HangNarrow_A") && !item8.name.Contains("_TurnedOff"))
					{
						item8.name += "_TurnedOff";
						item8.gameObject.SetActive(value: false);
					}
				}
				foreach (Transform item9 in transform17)
				{
					if (item9.name.Contains("EN_VLT_Light_Lantern_C") && !item9.name.Contains("_TurnedOff"))
					{
						item9.name += "_TurnedOff";
						item9.gameObject.SetActive(value: false);
						break;
					}
				}
			}
		}
		else
		{
			if (!DontTouchMeAgain && base.gameObject.name.Contains("LN:2d1201ab-96bc-44f9-a9b6-de6077fe9761"))
			{
				Transform[] componentsInChildren = base.transform.Find("Walls/Wall 2/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform22 in componentsInChildren)
				{
					if (transform22.name.Contains("EN_RF_Wall_2Hex_B") && IsPositionMatch(transform22.position, new Vector3(19.474884f, 9.493765E-07f, 9.067659f)))
					{
						transform22.position = new Vector3(19.817495f, 9.493765E-07f, 8.786819f);
						break;
					}
				}
				componentsInChildren = base.transform.Find("Walls/Wall 4/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform23 in componentsInChildren)
				{
					if (transform23.name.Contains("EN_RF_Wall_2Hex_D") && IsPositionMatch(transform23.position, new Vector3(19.482777f, -0.06296945f, -3.460981f)))
					{
						transform23.name += "_TurnedOff";
						transform23.gameObject.SetActive(value: false);
					}
					else if (transform23.name.Contains("EN_RF_Wall_2Hex_E") && IsPositionMatch(transform23.position, new Vector3(14.972458f, -0.2329694f, -2.9209814f)))
					{
						transform23.name += "_TurnedOff";
						transform23.gameObject.SetActive(value: false);
					}
				}
				DontTouchMeAgain = true;
				return;
			}
			if (base.gameObject.name.Contains("FH_15_05:a555f3bf-d00a-4c4c-8a0c-ee62cc37ae9f"))
			{
				Transform transform24 = base.transform.Find("Walls");
				if (transform24 == null)
				{
					return;
				}
				{
					foreach (Transform item10 in transform24)
					{
						Transform transform25 = item10.Find("Generated Content");
						if (transform25 == null)
						{
							continue;
						}
						foreach (Transform item11 in transform25)
						{
							if (item11.name.Contains("PCG_DN_Wall_2x_A"))
							{
								if (IsPositionMatch(item11.position, new Vector3(-9.261627f, 9.492187E-07f, 7.193162f)))
								{
									item11.SetPositionAndRotation(new Vector3(-9.233126f, 0f, 7.2416844f), new Quaternion(0.000149505f, 0.8660254f, 7.4088695E-13f, 0.50000006f));
									item11.localScale = new Vector3(0.8f, 1f, 1f);
								}
								else if (IsPositionMatch(item11.position, new Vector3(-9.556778f, 9.492187E-07f, 7.2740703f)))
								{
									item11.SetPositionAndRotation(new Vector3(-9.651182f, 9.492187E-07f, 7.436738f), new Quaternion(0f, -0.8660254f, 0f, 0.5f));
									item11.localScale = new Vector3(0.7779899f, 1f, 1f);
								}
								else if (IsPositionMatch(item11.position, new Vector3(-9.552478f, 9.492187E-07f, 1.668396f)))
								{
									item11.SetPositionAndRotation(new Vector3(-9.650682f, 0f, 1.4991431f), new Quaternion(-7.408861E-13f, -0.5f, 0.00014950501f, 0.8660254f));
									item11.localScale = new Vector3(0.7850863f, 1f, 1f);
								}
								else if (IsPositionMatch(item11.position, new Vector3(-9.3673935f, 9.492187E-07f, 1.9250472f)))
								{
									item11.SetPositionAndRotation(new Vector3(-9.164125f, 9.492187E-07f, 1.5738187f), new Quaternion(0f, 0.5f, 0f, 0.8660254f));
									item11.localScale = new Vector3(0.71415f, 1f, 1f);
								}
							}
							else if (item11.name.Contains("PCG_DN_Pillar_B") && IsPositionMatch(item11.position, new Vector3(-9.400953f, 3.1175023E-07f, 7.0432463f)))
							{
								item11.SetPositionAndRotation(new Vector3(-9.536388f, 0f, 6.936418f), new Quaternion(-0.000118109405f, -0.95438695f, -3.084373E-05f, 0.29857314f));
							}
							else if (item11.name.Contains("PCG_DN_Pillar_C") && IsPositionMatch(item11.position, new Vector3(-9.400951f, 2.1319593E-07f, 1.8917682f)))
							{
								item11.SetPositionAndRotation(new Vector3(-9.442728f, 2.1319593E-07f, 1.8563726f), new Quaternion(0f, -0.30463687f, 0f, 0.9524686f));
							}
							else if (item11.name.Contains("EN_DN_Hang_Top_Stone_B") && IsPositionMatch(item11.position, new Vector3(-9.55583f, 3.3269813f, 7.2696505f)))
							{
								item11.position = new Vector3(-9.810682f, 3.3269813f, 7.7130003f);
							}
							else if (item11.name.Contains("EN_DN_Hang_Top_Window_A") && IsPositionMatch(item11.position, new Vector3(-9.258325f, 3.3269813f, 7.196105f)))
							{
								item11.position = new Vector3(-9.049068f, 3.33f, 7.7404823f);
							}
							else if (item11.name.Contains("EN_DN_Hang_Top_Stone_A") && IsPositionMatch(item11.position, new Vector3(-9.555832f, 3.3269813f, 1.6653647f)))
							{
								item11.position = new Vector3(-9.869049f, 3.327f, 1.122922f);
							}
							else if (item11.name.Contains("EN_DN_Hang_Top_ClosedWindow_A") && IsPositionMatch(item11.position, new Vector3(-9.368227f, 3.3269813f, 1.9292655f)))
							{
								if (!item11.name.Contains("TurnedOff"))
								{
									item11.name += "_TurnedOff";
								}
								item11.gameObject.SetActive(value: false);
							}
						}
					}
					return;
				}
			}
			if (base.gameObject.name.Contains("CV_Wall") && !base.gameObject.name.Contains("_Moved") && base.transform.position.z > 4f && base.transform.position.z < 22f && base.transform.position.x > 2f && base.transform.position.x < 7f && (HasParentContaining("FH_02_02:3c57d633-85fd-420c-bc68-f8c12df97cd5") || HasParentContaining("D:f102c2fc-7ba4-4015-ab6c-3b0ec8acf7cb") || HasParentContaining("L:c5d39593-2878-4abb-8c90-100d71cf7cfe")))
			{
				Vector3 position = base.transform.position;
				if (position.x < 4f)
				{
					position.x += 0.25f;
					base.gameObject.name += "_MovedSideOne";
					if (position.z >= 14f)
					{
						position.z -= 0.3f;
						base.gameObject.name += "_Still";
					}
				}
				else
				{
					base.transform.localScale = Vector3.one;
					position.x -= 0.15f;
					base.gameObject.name += "_MovedSideTwo";
				}
				base.transform.position = position;
			}
			else if (!DontTouchMeAgain && base.gameObject.name.Contains("FH_15:360d82ac-01fa-4ba9-b41e-f4d89cb38fb0"))
			{
				Transform[] componentsInChildren = base.transform.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform27 in componentsInChildren)
				{
					if (!transform27.name.StartsWith("TerrainVisualEffect"))
					{
						continue;
					}
					Transform transform28 = transform27.Find("Generated Content");
					if (transform28 == null)
					{
						break;
					}
					foreach (Transform item12 in transform28)
					{
						if (item12.name.Contains("FakeGate_Scenario072"))
						{
							item12.SetPositionAndRotation(new Vector3(30.951801f, 0f, 5.9566765f), new Quaternion(0f, 0.8660254f, 0f, 0.50000006f));
							break;
						}
					}
					break;
				}
				Vector2 vector5 = new Vector2(32.833954f, 5.94407f);
				Transform[] array = new Transform[3]
				{
					base.transform.Find("Walls/Wall 4/Generated Content"),
					base.transform.Find("Walls/Wall 3/Generated Content"),
					base.transform.Find("Walls/Wall 2/Generated Content")
				};
				Vector3 vector6 = new Vector3(32.86128f, -2.665f, 3.832435f);
				bool flag2 = false;
				componentsInChildren = array;
				foreach (Transform transform30 in componentsInChildren)
				{
					if (transform30 == null)
					{
						continue;
					}
					Transform[] componentsInChildren4 = transform30.GetComponentsInChildren<Transform>(includeInactive: true);
					foreach (Transform transform31 in componentsInChildren4)
					{
						if (transform31.name.Contains("Pillar") || transform31.name.Contains("Wall") || transform31.name.Contains("Hang"))
						{
							if ((new Vector2(transform31.position.x, transform31.position.z) - vector5).sqrMagnitude <= 4.6656003f)
							{
								transform31.gameObject.name += "_TurnedOff";
								transform31.gameObject.SetActive(value: false);
							}
						}
						else if (!flag2 && transform31.name.Contains("Stalacton_F") && Vector3.SqrMagnitude(transform31.position - vector6) < 0.0001f)
						{
							transform31.SetPositionAndRotation(new Vector3(32.324196f, -1.2200037f, 4.141576f), new Quaternion(0f, 0.7446977f, 0f, -0.667402f));
							flag2 = true;
						}
					}
				}
				DontTouchMeAgain = true;
			}
			else if (!DontTouchMeAgain && base.gameObject.name.Contains("A:c1ca1bf3-5113-4805-8559-b3125e94d5c3"))
			{
				foreach (Transform item13 in base.transform)
				{
					if (item13.name.Contains("Reflection Probe"))
					{
						ReflectionProbe component2 = item13.GetComponent<ReflectionProbe>();
						if (component2 != null)
						{
							component2.blendDistance = 0.5f;
							component2.size = new Vector3(12f, component2.size.y, 6f);
						}
						break;
					}
				}
				DontTouchMeAgain = true;
			}
			else if (!DontTouchMeAgain && base.gameObject.name.Contains("D:158d7b5d-5cc4-48bb-b4fc-e57c62690935"))
			{
				foreach (Transform item14 in base.transform)
				{
					if (item14.name.Contains("Reflection Probe"))
					{
						ReflectionProbe component3 = item14.GetComponent<ReflectionProbe>();
						if (component3 != null)
						{
							component3.blendDistance = 0.75f;
							component3.size = new Vector3(12f, component3.size.y, 12f);
						}
						break;
					}
				}
				DontTouchMeAgain = true;
			}
			else if (base.gameObject.name.Contains("D:711b5a93-7699-4013-b326-017a7bce71c8"))
			{
				Transform[] componentsInChildren = base.transform.Find("Walls/Wall 4/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform34 in componentsInChildren)
				{
					if (transform34.name.Contains("PCG_DN_Pillar_B") && IsPositionMatch(transform34.position, new Vector3(14.099967f, 2.767244E-07f, 13.402521f)))
					{
						if (!transform34.name.Contains("Scaled_Moved_NoTorchOverlap"))
						{
							transform34.name += "Scaled_Moved_NoTorchOverlap";
						}
						transform34.SetPositionAndRotation(new Vector3(14.240772f, 0.177f, 13.386774f), new Quaternion(-1.9887546E-05f, -0.8510013f, -8.399445E-05f, 0.52516365f));
						transform34.localScale = new Vector3(1f, 1f, 0.4391f);
						break;
					}
				}
				componentsInChildren = base.transform.Find("Walls/Wall 6/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform35 in componentsInChildren)
				{
					if (transform35.name.Contains("PCG_DN_LightTorch_B") && IsPositionMatch(transform35.position, new Vector3(5.5227222f, 1.8603201f, 13.064935f)))
					{
						if (!transform35.name.Contains("_Moved_NoTorchOverlap"))
						{
							transform35.name += "_Moved_NoTorchOverlap";
						}
						transform35.position = new Vector3(5.477529f, 1.8603201f, 12.799788f);
						break;
					}
				}
			}
			else if (base.gameObject.name.Contains("B:5683a43a-fd7a-42ac-8ecd-a98e3b3ca030"))
			{
				Transform[] componentsInChildren = base.transform.Find("Walls/Wall 4/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform36 in componentsInChildren)
				{
					if (transform36.name.Contains("PCG_DN_LightTorch_B") && IsPositionMatch(transform36.position, new Vector3(20.410276f, 1.8603201f, 10.594277f)))
					{
						if (!transform36.name.Contains("_Moved_NoTorchOverlap"))
						{
							transform36.name += "_Moved_NoTorchOverlap";
						}
						transform36.position = new Vector3(20.136593f, 1.8603201f, 10.628402f);
						break;
					}
				}
			}
			else if (base.gameObject.name.Contains("I:c9a1f438-383d-4465-9a79-4b237c49941b") && !DontTouchMeAgain)
			{
				DeclutterRooms("ClutterBake", 0.6f, 1.2f);
				DontTouchMeAgain = true;
				Transform[] componentsInChildren = base.transform.Find("Walls/Wall 1/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform37 in componentsInChildren)
				{
					if (transform37.name.Contains("PCG_DN_LightTorch_B") && IsPositionMatch(transform37.position, new Vector3(16.33846f, 2.0993972f, -2.6804714f)))
					{
						if (!transform37.name.Contains("_TurnedOff_NoTorchOverlap"))
						{
							transform37.name += "_TurnedOff_NoTorchOverlap";
						}
						transform37.gameObject.SetActive(value: false);
						break;
					}
				}
				componentsInChildren = base.transform.Find("Walls/Wall 3/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform38 in componentsInChildren)
				{
					if (!transform38.name.Contains("PCG_DN_LightTorch_B"))
					{
						continue;
					}
					if (IsPositionMatch(transform38.position, new Vector3(10.125113f, 2.0901093f, 0.8633146f)))
					{
						if (!transform38.name.Contains("_Moved_NoTorchOverlap"))
						{
							transform38.name += "_Moved_NoTorchOverlap";
						}
						transform38.position = new Vector3(10.158754f, 2.09f, 0.9209585f);
					}
					else if (IsPositionMatch(transform38.position, new Vector3(12.142449f, 2.0993974f, 4.3574457f)))
					{
						if (!transform38.name.Contains("_Moved_NoTorchOverlap"))
						{
							transform38.name += "_Moved_NoTorchOverlap";
						}
						transform38.position = new Vector3(12.191254f, 2.099f, 4.4413524f);
					}
				}
			}
			else if (!DontTouchMeAgain && (base.gameObject.name.Contains("B:e90dc38a-c25e-42db-b954-5e175c18d6e6") || base.gameObject.name.Contains("B:47dcc86d-f84b-437d-b046-195540cfd258")))
			{
				foreach (Transform item15 in base.transform)
				{
					if (item15.name.Contains("Reflection Probe"))
					{
						ReflectionProbe component4 = item15.GetComponent<ReflectionProbe>();
						if (component4 != null)
						{
							component4.blendDistance = 0.5f;
							component4.size = new Vector3(18f, component4.size.y, 8f);
						}
						break;
					}
				}
				DontTouchMeAgain = true;
			}
			else if (base.gameObject.name.Contains("L:b97f0332-0ca9-4463-968b-8f1b13fe63fc"))
			{
				Transform transform40 = base.transform.Find("Walls/Wall 4/Generated Content");
				if (transform40 != null)
				{
					Transform[] componentsInChildren = transform40.GetComponentsInChildren<Transform>(includeInactive: true);
					foreach (Transform transform41 in componentsInChildren)
					{
						if (transform41.name.Contains("DW_Pillar"))
						{
							transform41.localScale = Vector3.one;
							if (IsPositionMatch(targetPosition: new Vector3(16.382423f, 9.05744E-05f, 1.5701513f), position: transform41.position))
							{
								transform41.position = new Vector3(16.247368f, 9.05744E-05f, 1.4362264f);
							}
						}
						else if (!DontTouchMeAgain && transform41.name.Contains("DW_Wall"))
						{
							Vector3 localScale2 = transform41.localScale;
							transform41.localScale = new Vector3(localScale2.x, localScale2.y, 0.8f);
							DontTouchMeAgain = true;
						}
						else if (transform41.name.Contains("Shelf") && IsPositionMatch(transform41.position, new Vector3(15.564738f, -0.06190444f, 2.0170684f)))
						{
							transform41.position = new Vector3(13.789914f, -0.06190444f, 3.039791f);
						}
					}
				}
			}
			else if (base.gameObject.name.Contains("M:792a8bbe-e502-48c0-ab09-55fc1ca2a074"))
			{
				Transform transform42 = base.transform.Find("Walls/Wall 7/Generated Content");
				if (transform42 != null)
				{
					Transform[] componentsInChildren = transform42.GetComponentsInChildren<Transform>(includeInactive: true);
					foreach (Transform transform43 in componentsInChildren)
					{
						if (!transform43.name.Contains("Wall"))
						{
							continue;
						}
						if (transform43.position.x > 16f)
						{
							transform43.gameObject.SetActive(value: false);
							continue;
						}
						transform43.localScale = new Vector3(1f, transform43.localScale.y, 0.5f);
						if (transform43.name == "EN_DW_Wall_2Hex_ThinA")
						{
							transform43.position = new Vector3(17.390259f, 9.44932E-07f, 0.9957714f);
						}
						else
						{
							transform43.position = new Vector3(15.242517f, 9.44932E-07f, 2.2357717f);
						}
					}
				}
			}
			else if (((base.gameObject.name.Contains("shape") && HasParentContaining("G:76fb905c-e23b-490c-a49b-302c569d0721")) || HasParentContaining("FH_01_11_13:c669ba8f-2fd5-41ec-a950-03010a29eb44") || HasParentContaining("FH_01_11b:086a8b0e-c6e1-44ba-8c86-358753bb14f3")) && HasParentContaining("ThinDoor"))
			{
				base.gameObject.name += "_RenderersOff";
				SetAllMeshRenderersOf(base.gameObject, wantedState: false);
			}
		}
		if (HasParentContaining("FH_14_2:0ff1de12-ea3e-40a8-aa7e-cbc6b1d76bb6") && base.gameObject.name.Contains("Fshape") && IsPositionMatch(base.transform.position, new Vector3(4.298724f, -2.682209E-07f, 4.467432f)))
		{
			base.gameObject.name += "_TurnedOffOverHatch";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("Hshape") && HasParentContaining("FH_15_02x4:") && IsPositionMatch(base.transform.position, new Vector3(10.317261f, 0f, -2.9783378f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 0f, 0f, 1.0000001f)))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("PLN_Wall") && !base.gameObject.name.Contains("_Moved") && IsPositionMatch(base.transform.position, new Vector3(1.9094825f, 0.06349062f, -2.057425f)) && HasParentContaining("FH_16_07_13x2:cf546925-4f6a-4949-b7a9-52848d58fe3d"))
		{
			base.transform.position = new Vector3(1.9422574f, 0.06349062f, -2.9638085f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("PLN_Wall") && !base.gameObject.name.Contains("_TurnedOff") && IsPositionMatch(base.transform.position, new Vector3(1.5331011f, 0.06349062f, -2.063478f)) && HasParentContaining("FH_16_07_13x2:cf546925-4f6a-4949-b7a9-52848d58fe3d"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("PLN_Wall") && !base.gameObject.name.Contains("_TurnedOff") && IsPositionMatch(base.transform.position, new Vector3(14.776826f, 0.06349062f, -3.5972328f)) && HasParentContaining("FH_16_07_13x2:cf546925-4f6a-4949-b7a9-52848d58fe3d"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("Pillar") && !base.gameObject.name.Contains("_ScaleReset") && (HasParentContaining("Wall 29") || HasParentContaining("Wall 30")) && HasParentContaining("FH_16_07_13x2:cf546925-4f6a-4949-b7a9-52848d58fe3d"))
		{
			base.transform.localScale = Vector3.one;
			base.gameObject.name += "_ScaleReset";
		}
		else if (base.gameObject.name == "TST_MR_Floor_Hshape_A" && IsPositionMatch(base.transform.position, new Vector3(7.737945f, 0f, -1.4891689f)) && HasParentContaining("G:8840eb2b-69e5-423f-a2a4-f8e5d1e2488b"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (HasParentContaining("FH_02_04"))
		{
			if (base.gameObject.name.Contains("Lshape"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(13.756348f, -0.00999999f, 5.9566755f)) || IsPositionMatch(base.transform.position, new Vector3(8.597717f, -0.00999999f, 5.9566755f)) || IsPositionMatch(base.transform.position, new Vector3(16.335663f, -0.00999999f, 1.4891689f)) || IsPositionMatch(base.transform.position, new Vector3(13.756348f, -0.00999999f, -2.9783382f)) || IsPositionMatch(base.transform.position, new Vector3(8.597717f, -0.00999999f, -2.9783373f)) || IsPositionMatch(base.transform.position, new Vector3(7.737945f, -0.00999999f, 13.40252f)) || IsPositionMatch(base.transform.position, new Vector3(7.737946f, -0.00999999f, 13.40252f)))
				{
					base.gameObject.SetActive(value: false);
				}
			}
			else if (base.gameObject.name.Contains("Jshape") && (IsPositionMatch(base.transform.position, new Vector3(13.756348f, -0.00999999f, 5.9566755f)) || IsPositionMatch(base.transform.position, new Vector3(16.335663f, -0.00999999f, 1.4891691f)) || IsPositionMatch(base.transform.position, new Vector3(13.756348f, -0.00999999f, -2.9783373f)) || IsPositionMatch(base.transform.position, new Vector3(8.597717f, -0.00999999f, 5.9566755f)) || IsPositionMatch(base.transform.position, new Vector3(14.616119f, -0.00999999f, 13.40252f)) || IsPositionMatch(base.transform.position, new Vector3(14.616118f, -0.00999999f, 13.40252f))))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (base.gameObject.name.Contains("MR_Wall") && IsPositionMatch(base.transform.position, new Vector3(-0.7460022f, 8.220529E-07f, -8.639045f)) && HasParentContaining("G:71bb0d40-94e2-48ba-bc6f-183a9cb2166b"))
		{
			base.transform.position = new Vector3(-1.1253667f, 8.220529E-07f, -8.638625f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("MR_Wall") && IsPositionMatch(base.transform.position, new Vector3(-2.6930866f, 8.220529E-07f, -3.2743063f)) && HasParentContaining("G:cc9ac9bf-b41f-4420-98d7-143aa6bbc3b4"))
		{
			base.transform.position = new Vector3(-2.4036965f, 0f, -3.2743063f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("MR_Wall") && IsPositionMatch(base.transform.position, new Vector3(-14.174299f, 8.220529E-07f, -8.639045f)) && HasParentContaining("G:cc9ac9bf-b41f-4420-98d7-143aa6bbc3b4"))
		{
			base.transform.position = new Vector3(-14.5067215f, 8.220529E-07f, -8.638625f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("MR_Wall") && IsPositionMatch(base.transform.position, new Vector3(-16.702284f, 8.220529E-07f, -3.2743063f)) && HasParentContaining("G:987d015a-e118-4c0c-9ca4-b2fc5ef91e1f"))
		{
			base.transform.position = new Vector3(-16.404076f, 8.220529E-07f, -3.2743063f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("MR_Wall") && IsPositionMatch(base.transform.position, new Vector3(-29.70704f, 8.220529E-07f, -3.2743063f)) && HasParentContaining("G:d9491b16-8b03-4309-9f65-33dd15a6885a"))
		{
			base.transform.position = new Vector3(-29.421822f, 0.00012932967f, -3.2743049f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("MR_Wall") && IsPositionMatch(base.transform.position, new Vector3(-30.037704f, -0.009287178f, -8.639043f)) && HasParentContaining("G:d9491b16-8b03-4309-9f65-33dd15a6885a"))
		{
			base.transform.position = new Vector3(-29.740046f, 7.152557E-07f, -8.639047f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("Pillar") && IsPositionMatch(base.transform.position, new Vector3(-29.859343f, -0.009998032f, -3.2929478f)) && HasParentContaining("G:d9491b16-8b03-4309-9f65-33dd15a6885a"))
		{
			base.transform.position = new Vector3(-29.601433f, -0.009998032f, -3.2929478f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("Pillar") && IsPositionMatch(base.transform.position, new Vector3(-31.01917f, -0.009288149f, -8.638986f)) && HasParentContaining("G:d9491b16-8b03-4309-9f65-33dd15a6885a"))
		{
			base.transform.position = new Vector3(-30.733433f, -0.009288149f, -8.638626f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("MR_Wall") && IsPositionMatch(base.transform.position, new Vector3(-31.995544f, -0.009287178f, -8.639043f)) && HasParentContaining("G:d9491b16-8b03-4309-9f65-33dd15a6885a"))
		{
			base.transform.position = new Vector3(-31.611433f, -0.009287178f, -8.638626f);
			base.gameObject.name += "_Moved";
		}
		else if (base.gameObject.name.Contains("MR_Floor_Lshape") && IsPositionMatch(base.transform.position, new Vector3(-2.5793152f, -0.014998972f, -10.424181f)) && HasParentContaining("G:cc9ac9bf-b41f-4420-98d7-143aa6bbc3b4"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (HasParentContaining("FH_15_07"))
		{
			if (base.gameObject.name.Contains("Lshape"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(15.47589f, -0.00999999f, 5.9566765f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 0.50000006f, 0f, 0.8660254f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(15.469414f, -0.3233668f, 11.901427f), new Quaternion(-0.49999997f, -1.11072296E-07f, -0.86602545f, 6.079353E-08f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(24.073606f, -0.00999999f, 14.891689f)))
				{
					base.transform.position = new Vector3(16.336336f, -4.004687E-08f, 12.393721f);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(1.7195435f, -0.00999999f, 11.913351f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 1.0000001f, 0f, 0f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(15.913733f, -0.0039499355f, 12.158566f), new Quaternion(0f, 0.8660255f, 0f, -0.49999994f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(15.475891f, -0.00999999f, 11.913352f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 0.8660254f, 0f, 0.50000006f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(15.475899f, -0.321363f, 11.896947f), new Quaternion(-0.49999997f, -2.2203555E-07f, -0.86602545f, 1.4612397E-07f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(17.195436f, -0.00999999f, 2.9783378f)))
				{
					base.transform.position = new Vector3(15.053595f, -0.00395028f, 10.66856f);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(1.7195435f, -0.00999999f, 5.956675f)))
				{
					base.transform.position = new Vector3(15.469715f, -0.0020043254f, 10.908195f);
				}
			}
			else if (base.gameObject.name.Contains("Jshape"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(28.372467f, -0.014998972f, 7.4458437f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, -0.5f, 0f, 0.86602545f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(15.475645f, -7.5716525E-07f, 6.9662495f), new Quaternion(0f, -0.8660253f, 0f, -0.50000036f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(15.475891f, -0.00999999f, 5.956675f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, -0.49999997f, 0f, 0.8660254f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(15.490719f, -0.32136172f, 5.964738f), new Quaternion(5.066394E-07f, -3.6821262E-07f, 1f, 2.1063818E-07f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(-0.8597717f, -0.014998972f, 10.424182f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 0.8660254f, 0f, 0.50000006f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(16.33595f, -0.32136342f, 6.469553f), new Quaternion(-0.49999997f, 0f, 0.86602545f, 0f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(15.4692f, -0.0020047026f, 5.968642f)))
				{
					base.transform.position = new Vector3(15.469195f, -0.0020047026f, 5.968642f);
				}
			}
		}
		else if (base.gameObject.name.Contains("FH_15_07:ca5a3113-a36b-4740-8b8e-f7d18dfea652"))
		{
			Transform[] componentsInChildren = base.transform.Find("Walls/Wall 4/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform44 in componentsInChildren)
			{
				if (transform44.name.Contains("EN_VLT_Pillar_B") && IsPositionMatch(transform44.position, new Vector3(23.89554f, 5.194344E-07f, 3.2867804f)))
				{
					if (!transform44.name.Contains("_MovedUp_ForZFight"))
					{
						transform44.name += "_MovedUp_ForZFight";
					}
					transform44.position = new Vector3(23.89554f, 0.008f, 3.2867804f);
					break;
				}
			}
		}
		else if (base.gameObject.name.Contains("EN_CV_Pillar_A") && HasParentContaining("FH_15_13x2"))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(5.901824f, -1.4700003f, 0.34817314f)) || IsPositionMatch(base.transform.position, new Vector3(4.447077f, -1.4700003f, 0.37980843f)) || IsPositionMatch(base.transform.position, new Vector3(13.0448f, -1.4700003f, 0.37980843f)) || IsPositionMatch(base.transform.position, new Vector3(14.499546f, -1.4700003f, 0.34817314f)))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if ((base.gameObject.name.IndexOf("jshape", StringComparison.OrdinalIgnoreCase) >= 0 || base.gameObject.name.IndexOf("lshape", StringComparison.OrdinalIgnoreCase) >= 0) && !HasParentContaining("Bay") && (HasParentContaining("G:") || HasParentContaining("L:") || HasParentContaining("N:") || HasParentContaining("I:") || HasParentContaining("B:") || HasParentContaining("F:")))
		{
			float y = base.transform.localEulerAngles.y;
			if (Mathf.Approximately(y, 120f) || Mathf.Approximately(y, 240f))
			{
				base.gameObject.name += "_TurnedOff_ZFightTroubleMaker";
				base.gameObject.SetActive(value: false);
			}
		}
		else if (HasParentContaining("FH_07_16"))
		{
			if (base.gameObject.name.Contains("EN_CV_Pillar_A"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(2.3964634f, -1.4700003f, 5.490572f)) || IsPositionMatch(base.transform.position, new Vector3(4.416516f, -1.4700003f, 5.6035f)) || IsPositionMatch(base.transform.position, new Vector3(5.1579347f, -1.4700003f, 6.7001543f)) || IsPositionMatch(base.transform.position, new Vector3(11.359891f, -1.4700003f, 6.422781f)))
				{
					base.gameObject.SetActive(value: false);
				}
			}
			else if (base.gameObject.name.Contains("EN_CV_Pillar_B"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(9.281707f, -1.4700003f, 5.637063f)))
				{
					base.gameObject.SetActive(value: false);
				}
			}
			else if (base.gameObject.name.Contains("EN_CV_Pillar_C"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(5.1225696f, 0.050000004f, 4.829304f)))
				{
					base.gameObject.SetActive(value: false);
				}
			}
			else if (base.gameObject.name.Contains("EN_CV_Pillar_D") && (IsPositionMatch(base.transform.position, new Vector3(2.742846f, 0.010000025f, 6.5474796f)) || IsPositionMatch(base.transform.position, new Vector3(1.7684164f, 0.01000004f, 7.1318493f)) || IsPositionMatch(base.transform.position, new Vector3(4.135332f, 0.010000025f, 6.54748f)) || IsPositionMatch(base.transform.position, new Vector3(11.013509f, 0.010000025f, 5.3658733f)) || IsPositionMatch(base.transform.position, new Vector3(11.987938f, 0.01000004f, 4.7815046f)) || IsPositionMatch(base.transform.position, new Vector3(9.621024f, 0.010000025f, 6.54748f)) || IsPositionMatch(base.transform.position, new Vector3(8.646593f, 0.01000004f, 7.1318493f))))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if ((HasParentContaining("J:26ea19e6-b59f-45df-a52e-73565bce29ff") || HasParentContaining("FH_10_01x2:00fe366a-7858-4987-8ea9-8d7af0f77279")) && base.gameObject.name.Contains("ALG_Hang") && !DontTouchMeAgain && !HasParentContaining("Bay"))
		{
			if (base.gameObject.name.Contains("05"))
			{
				base.transform.localPosition += new Vector3(0f, 0f, -0.4f);
			}
			else
			{
				base.transform.localPosition += new Vector3(0f, 0f, -0.25f);
			}
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("G:dbcd4e3b-f836-4e1c-864b-8651c0f11c62"))
		{
			if (base.gameObject.name.Contains("04") || (base.gameObject.name.Contains("Torch") && HasParentContaining("Wall 2")))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("DN_Wall_1x") && HasParentContaining("Wall 4"))
			{
				base.transform.localScale = new Vector3(1.3f, base.transform.localScale.y, base.transform.localScale.z);
			}
		}
		else if (HasParentContaining("FH_10_01x2:00fe366a-7858-4987-8ea9-8d7af0f77279"))
		{
			if (base.gameObject.name.Contains("EN_ALG_AlgoxStatue_A"))
			{
				base.transform.SetPositionAndRotation(new Vector3(6.0184054f, 0.02000001f, -4.4675074f), new Quaternion(0f, 0.0065206885f, 0f, 0.99997884f));
			}
			else if (base.gameObject.name.Contains("EN_ALG_AlgoxStatue_B"))
			{
				base.transform.SetPositionAndRotation(new Vector3(6.8781776f, 0.02000001f, -5.9566774f), new Quaternion(0f, 0.0004941225f, 0f, MathF.PI * 113f / 355f));
			}
			else if (base.gameObject.name.Contains("EN_ALG_AlgoxStatue_Prison_A"))
			{
				base.transform.SetPositionAndRotation(new Vector3(7.737853f, -0.017f, -4.4673247f), new Quaternion(0f, 0.008623302f, 0f, 0.99996287f));
			}
		}
		if (!DontTouchMeAgain && (base.gameObject.name.Contains("E:77b5a342-823d-48e5-a129-512f3f715279") || base.gameObject.name.Contains("G:dbcd4e3b-f836-4e1c-864b-8651c0f11c62")))
		{
			DeclutterRooms("_scatter", 0.55f, 1.1f);
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("FenceWallObstacle"))
		{
			if (HasParentContaining("FH_01_11_13:c669ba8f-2fd5-41ec-a950-03010a29eb44") && IsPositionMatch(base.transform.position, new Vector3(16.320883f, 6.221235E-07f, 7.3690634f)))
			{
				base.transform.SetPositionAndRotation(new Vector3(16.269672f, 6.221235E-07f, 7.544846f), new Quaternion(0f, 0.8683037f, 0f, -0.496033f));
			}
			else if (HasParentContaining("FH_01_11b:086a8b0e-c6e1-44ba-8c86-358753bb14f3") && IsPositionMatch(base.transform.position, new Vector3(30.040962f, 6.221235E-07f, -16.321642f)))
			{
				base.transform.SetPositionAndRotation(new Vector3(30.179028f, 6.221235E-07f, -16.29686f), new Quaternion(0f, -0.5029222f, 0f, 0.8643318f));
			}
		}
		if (!DontTouchMeAgain && base.gameObject.name.Contains("J:4bd5a8c4-aa09-422c-9bbd-601aab85623e"))
		{
			DeclutterRooms("_Clutter", 0.4f, 0.8f);
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("FH_13_01x4:bea90e96-5fef-4a0f-bf83-a77b4f2f0e5f"))
		{
			if ((base.gameObject.name.Contains("MR_Floor_Pshape") && (IsPositionMatch(base.transform.position, new Vector3(4.2988586f, -0.000999987f, 10.424182f)) || (IsPositionMatch(base.transform.position, new Vector3(1.7195435f, -0.0009999871f, -5.9566755f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 0.8660254f, 0f, 0.50000006f))))) || (base.gameObject.name.Contains("Lshape") && (IsPositionMatch(base.transform.position, new Vector3(4.2988586f, 0f, 10.424181f)) || IsPositionMatch(base.transform.position, new Vector3(5.15863f, 0f, 14.89169f)) || HasParentContaining("Wall 31"))) || (base.gameObject.name.Contains("MR_Floor_Sshape") && IsPositionMatch(base.transform.position, new Vector3(4.2988586f, 0f, 10.424181f))) || (base.gameObject.name.Contains("Jshape") && IsPositionMatch(base.transform.position, new Vector3(1.7195435f, 0f, 14.891689f))))
			{
				base.gameObject.name += "_TurnedOff";
				base.gameObject.SetActive(value: false);
			}
			if (base.gameObject.name.Contains("MR_Floor_Sshape") && !base.gameObject.name.EndsWith("_DuplicateToFillAGap") && Application.isPlaying && !DontTouchMeAgain)
			{
				if (IsPositionMatch(base.transform.position, new Vector3(3.4390872f, 0f, -5.9566755f)))
				{
					Vector3 dstPos = new Vector3(1.6890872f, -0.5f, -5.9566755f);
					InstantiateDuplicate(dstPos, base.transform.rotation).name = base.gameObject.name + "_DuplicateToFillAGap_Lifted";
				}
				if (IsPositionMatch(base.transform.position, new Vector3(2.5793152f, 0f, 10.424182f)))
				{
					Vector3 dstPos2 = new Vector3(4.329315f, 0f, 10.424182f);
					InstantiateDuplicate(dstPos2, base.transform.rotation).name = base.gameObject.name + "_DuplicateToFillAGap";
				}
				if (IsPositionMatch(base.transform.position, new Vector3(1.7195435f, 0f, 14.89169f)))
				{
					base.transform.rotation = new Quaternion(0f, 1.1920929E-07f, 0f, MathF.PI * 113f / 355f);
					base.gameObject.name += "_Rotated";
				}
				if (HasParentContaining("Wall 5") || HasParentContaining("Wall 6") || HasParentContaining("Wall 31"))
				{
					base.transform.position = new Vector3(base.transform.position.x, -0.5f, base.transform.position.z);
				}
			}
			else if (base.gameObject.name.Contains("MR_Pillar_1x_A") && !base.gameObject.name.EndsWith("_DuplicateToFillAGap") && Application.isPlaying && !DontTouchMeAgain)
			{
				if (IsPositionMatch(base.transform.position, new Vector3(7.0781837f, 0f, -2.0391502f)))
				{
					Vector3 dstPos3 = new Vector3(7.0301776f, 0f, -4.9537144f);
					Quaternion dstRot = new Quaternion(0f, -0.7041956f, 0f, 0.7100061f);
					InstantiateDuplicate(dstPos3, dstRot).name = base.gameObject.name + "_DuplicateToFillAGap";
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(6.218769f, -1.4902147E-07f, 17.004292f)))
				{
					Vector3 dstPos4 = new Vector3(6.2184052f, -1.4902147E-07f, 15.077447f);
					Quaternion dstRot2 = new Quaternion(0f, -0.7071068f, 0f, 0.7071068f);
					InstantiateDuplicate(dstPos4, dstRot2).name = base.gameObject.name + "_DuplicateToFillAGap";
				}
				DontTouchMeAgain = true;
			}
			else if (base.gameObject.name.Contains("EN_MR_PRS_FenceWall_2x_"))
			{
				if (HasParentContaining("Wall 5") || HasParentContaining("Wall 6") || HasParentContaining("Wall 15") || HasParentContaining("Wall 16") || HasParentContaining("Wall 20") || HasParentContaining("Wall 21") || HasParentContaining("Wall 30") || HasParentContaining("Wall 31"))
				{
					base.gameObject.name += "_TurnedOff";
					base.gameObject.SetActive(value: false);
				}
				else if (HasParentContaining("Wall 29") && !DontTouchMeAgain)
				{
					if (IsPositionMatch(base.transform.position, new Vector3(4.8191404f, 9.49E-07f, -6.2526464f)))
					{
						base.transform.position = new Vector3(4.332102f, 9.49E-07f, -6.0330667f);
					}
					else if (IsPositionMatch(base.transform.position, new Vector3(2.6100054f, 9.49E-07f, -6.2526455f)))
					{
						base.transform.position = new Vector3(2.1362119f, 1.1876E-06f, -6.0330667f);
					}
					base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + -0.53999996f, base.transform.position.z);
					DontTouchMeAgain = true;
				}
				else if (HasParentContaining("Wall 22") && !DontTouchMeAgain)
				{
					base.transform.localPosition += new Vector3(0f, 0f, 0.45f);
					DontTouchMeAgain = true;
				}
				else if (HasParentContaining("Wall 14") && !DontTouchMeAgain)
				{
					base.transform.localPosition += new Vector3(0f, 0f, 0.29f);
					DontTouchMeAgain = true;
				}
				else if (HasParentContaining("Wall 7") && !DontTouchMeAgain)
				{
					base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + -0.53999996f, base.transform.position.z - 0.25f);
					base.transform.localScale = new Vector3(base.transform.localScale.x, 1f, 1f);
					DontTouchMeAgain = true;
				}
			}
			else if (base.gameObject.name.Contains("EN_MR_PRS_FencePillar_A"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(1.3732243f, 1.18E-08f, -6.156625f)) || IsPositionMatch(base.transform.position, new Vector3(4.4757752f, 4.5740062E-08f, 10.103425f)) || IsPositionMatch(base.transform.position, new Vector3(1.7983749f, 3.4709142E-08f, 14.586771f)))
				{
					base.gameObject.name += "_TurnedOff";
					base.gameObject.SetActive(value: false);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(5.605174f, 3.7219E-08f, 14.698031f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 0.14766079f, 0f, 0.9890381f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(5.5883694f, 3.7219E-08f, 15.209399f), new Quaternion(0f, -8.742278E-08f, 0f, -1f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(1.4154892f, -1.2292043E-08f, 15.181468f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(1.065764f, -1.2292043E-08f, 15.249081f), Quaternion.identity);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(-1.8735719f, 0.04f, 10.766546f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(-1.4734683f, 0.04f, 10.440574f), new Quaternion(0f, 1.0430812E-07f, 0f, -1f));
					base.transform.localScale = new Vector3(0.90571415f, 0.9151188f, 0.90571415f);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(2.1882977f, -1.7484556E-08f, 10.200424f)))
				{
					base.transform.position = new Vector3(0.6389937f, -1.7484556E-08f, 10.469891f);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(4.6451817f, 1.182592E-08f, 10.624132f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(4.676532f, 1.182592E-08f, 10.410573f), new Quaternion(0f, 1f, 0f, -4.371139E-08f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(4.645181f, -1.784106E-08f, -1.6891174f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 0.8660255f, 0f, 0.4999999f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(4.645181f, -1.784106E-08f, -1.4575586f), Quaternion.identity);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(4.4762273f, 4.548547E-08f, -1.1686907f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 0.2742633f, 0f, 0.9616547f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(0.55633116f, 4.548547E-08f, -1.3998766f), new Quaternion(0f, 0.2742633f, 0f, 0.9616547f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(6.1724324f, 0.04f, -6.299039f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(6.1721015f, 0.04f, -6.413067f), new Quaternion(0f, 0.7071065f, 0f, 0.7071071f));
					base.transform.localScale = new Vector3(1.1209602f, 1.1326f, 1.1209602f);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(1.5426304f, 0f, -5.6359158f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(1.0534115f, 0f, -6.0283833f), new Quaternion(0f, 1f, 0f, -4.371139E-08f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(3.8301077f, 0f, -5.732916f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(5.509412f, 0f, -5.914384f), Quaternion.identity);
				}
				if ((HasParentContaining("Wall 29") || HasParentContaining("Wall 31")) && !DontTouchMeAgain)
				{
					base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + -0.49f, base.transform.position.z - 0.08f);
					DontTouchMeAgain = true;
				}
				else if (HasParentContaining("Wall 7") && !DontTouchMeAgain)
				{
					base.transform.position = new Vector3(base.transform.position.x, base.transform.position.y + -0.49f, base.transform.position.z - 0.32f + 0.08f);
					DontTouchMeAgain = true;
				}
			}
			if (base.gameObject.name.Contains("MR_Pillar_1x_A"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(-1.7746224f, 5.0772556E-08f, -4.978655f)) || IsPositionMatch(base.transform.position, new Vector3(-1.7746224f, 3.945922E-08f, 13.913669f)))
				{
					base.gameObject.name += "_TurnedOff";
					base.gameObject.SetActive(value: false);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(-1.5655155f, 0.03000197f, 13.799591f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(-2.1695447f, 0.03000197f, 13.799744f), new Quaternion(0f, -1.0000001f, 0f, 0f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(-1.5655155f, 0.03142373f, 10.924582f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(-2.125544f, 0.03142373f, 10.919744f), Quaternion.identity);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(-1.0415511f, -0.01857623f, 2.0024264f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(-1.0397716f, -0.01857623f, -0.7134919f), new Quaternion(0f, 0.70710695f, 0f, 0.7071067f));
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(-1.0415516f, -0.00999803f, 6.932587f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(-1.0397725f, -0.00999803f, 9.467508f), new Quaternion(0f, 0.7071068f, 0f, 0.7071068f));
				}
			}
			else if (base.gameObject.name.Contains("MR_Wall_1x_A"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(-1.5870938f, 9.492187E-07f, -5.5941525f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(-1.7361779f, 9.492187E-07f, -5.517087f), new Quaternion(0f, 0.56130946f, 0f, 0.82760614f));
					base.transform.localScale = Vector3.one;
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(6.332015f, 8.220529E-07f, 14.445667f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(6.5032525f, 8.220529E-07f, 14.438691f), new Quaternion(0f, -0.8921166f, 0f, 0.45180547f));
				}
			}
			else if (base.gameObject.name.Contains("MR_Wall_2x_A"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(-1.3679566f, 9.492187E-07f, 14.908722f)))
				{
					base.transform.position = new Vector3(-1.7518811f, 9.492187E-07f, 14.518902f);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(-1.5701747f, 9.492187E-07f, 15.187658f)))
				{
					base.transform.position = new Vector3(-2.0435567f, 9.492187E-07f, 15.188081f);
				}
			}
			else if (base.gameObject.name == "EN_MR_Hang_PlatesTop_1_A" && HasParentContaining("Wall 12"))
			{
				base.gameObject.name += "_TurnedOff";
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("HangTop_C") && IsPositionMatch(base.transform.position, new Vector3(-1.573513f, 3.3269813f, 15.189047f)))
			{
				base.transform.position = new Vector3(-2.0555568f, 3.3269813f, 15.18908f);
			}
		}
		else if (HasParentContaining("E:a8b50c8d-ab34-4faa-bd51-2b9a54cd53ce") && base.gameObject.name.Contains("MR_Floor_Fshape") && !base.gameObject.name.EndsWith("_DuplicateToFillAGap") && Application.isPlaying && !DontTouchMeAgain)
		{
			if (IsPositionMatch(base.transform.position, new Vector3(12.896552f, -0.01912364f, 10.423975f)))
			{
				Vector3 dstPos5 = new Vector3(13.729207f, -0.028f, 11.8973055f);
				Quaternion dstRot3 = new Quaternion(0f, -0.8660254f, 0f, 0.49999997f);
				InstantiateDuplicate(dstPos5, dstRot3).name = base.gameObject.name + "_DuplicateToFillAGap";
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(11.177008f, -0.01912364f, 10.423975f)))
			{
				Vector3 dstPos6 = new Vector3(13.755839f, -0.001f, 8.935318f);
				Quaternion dstRot4 = new Quaternion(0f, -0.8660254f, 0f, 0.5f);
				InstantiateDuplicate(dstPos6, dstRot4).name = base.gameObject.name + "_DuplicateToFillAGap";
			}
			DontTouchMeAgain = true;
		}
		else if (!DontTouchMeAgain && base.gameObject.name.Contains("FH_RFS_05:1b1e60ba-a66e-4c5a-b8cd-f1af409067aa"))
		{
			Transform transform45 = base.transform.Find("Walls/Wall 13/Generated Content");
			if (transform45 != null)
			{
				foreach (Transform item16 in transform45)
				{
					if (item16.name.Contains("WallTiny"))
					{
						item16.name += "_TurnedOff";
						item16.gameObject.SetActive(value: false);
					}
				}
			}
			Transform transform47 = base.transform.Find("Walls/Wall 14/Generated Content");
			if (transform47 != null)
			{
				Transform transform48 = null;
				float num6 = float.PositiveInfinity;
				foreach (Transform item17 in transform47)
				{
					if (item17.name.Contains("MNT_Wall"))
					{
						float x = item17.localPosition.x;
						if (x < num6)
						{
							num6 = x;
							transform48 = item17;
						}
					}
				}
				if (transform48 != null)
				{
					if (!transform48.name.Contains("turned off"))
					{
						transform48.name += "_TurnedOff";
					}
					transform48.gameObject.SetActive(value: false);
				}
			}
			Transform transform50 = base.transform.Find("Generated Content/Full");
			if (transform50 != null)
			{
				foreach (Transform item18 in transform50)
				{
					if (item18.name.Contains("WFW_Hex_Fshape") && IsPositionMatch(item18.position, new Vector3(-8.597908f, -0.019123644f, 8.934933f)))
					{
						item18.name += "_TurnedOff";
						item18.gameObject.SetActive(value: false);
						break;
					}
				}
			}
			Transform transform52 = base.transform.Find("Walls/Wall 1/Generated Content");
			if (transform52 != null)
			{
				foreach (Transform item19 in transform52)
				{
					if (item19.name.Contains("EN_WFW_Pillar_C"))
					{
						item19.position = new Vector3(0.041238785f, 2.5024448E-07f, 3.0714293f);
						item19.name += "_Adjusted";
						break;
					}
				}
			}
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("J:997fc970-09a3-4c75-bc4e-67db15cdc8c6"))
		{
			if (HasParentContaining("Wall 7") && (base.gameObject.name.Contains("EN_RF_Pillar_A") || base.gameObject.name.Contains("EN_RF_Wall_2Hex_E") || base.gameObject.name.Contains("EN_RF_Wall_2Hex_A") || base.gameObject.name.Contains("EN_RF_Wall_2Hex_B") || base.gameObject.name.Contains("EN_RF_Bay_E")))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (HasParentContaining("M:66fbed46-4dc1-49d6-b816-3c73feee85b9"))
		{
			if (HasParentContaining("Wall 8") && (base.gameObject.name.Contains("Pillar") || base.gameObject.name.Contains("Wall") || base.gameObject.name.Contains("Bay")) && !base.gameObject.name.Contains("Wall_2Hex_C"))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("EN_RF_Wall_2Hex_F") && IsPositionMatch(base.transform.position, new Vector3(5.9330263f, -0.004998073f, 2.539497f)))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("EN_RF_Pillar_A") && (IsPositionMatch(base.transform.position, new Vector3(5.5729027f, -0.009997793f, 1.1745596f)) || IsPositionMatch(base.transform.position, new Vector3(5.5451503f, 0.0014222108f, 3.7318077f))))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("EN_RF_Wall_2Hex_C") && (IsPositionMatch(base.transform.position, new Vector3(6.279745f, -0.19000007f, 1.8518133f)) || IsPositionMatch(base.transform.position, new Vector3(4.295172f, -0.20928897f, 4.4368134f))))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (HasParentContaining("FH_08:faef83c1-f44f-4a90-b067-fe9fd7bec7e4"))
		{
			if (base.gameObject.name.Contains("EN_RF_Wall_2Hex_D") && IsPositionMatch(base.transform.position, new Vector3(10.528743f, -0.06796847f, 5.428413f)))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("EN_RF_Bay_E") && IsPositionMatch(base.transform.position, new Vector3(12.896584f, 0f, 6.453067f)))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("EN_RF_Wall_1Hex_A") && IsPositionMatch(base.transform.position, new Vector3(11.943769f, -0.009287126f, 5.2035885f)))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("EN_RF_Obstacle_1Hex_A") && IsPositionMatch(base.transform.position, new Vector3(12.210224f, -0.018575888f, 5.285821f)) && Approximately(base.transform.localScale, new Vector3(0.70832f, 0.70832f, 0.70832f)))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (HasParentContaining("Wall 3") && base.gameObject.name.Contains("_Wall_"))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (HasParentContaining("FH_01_11b:40dc00c2-38fa-427b-a54e-446a30306f5a") && IsPositionMatch(base.transform.position, new Vector3(13.616356f, -0.09736753f, 1.0377994f)) && !DontTouchMeAgain)
		{
			base.transform.position += new Vector3(0f, 0f, -3.3f);
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("FH_01_11b:40dc00c2-38fa-427b-a54e-446a30306f5a") || HasParentContaining("B:c4c398f5-8c03-4ea0-b9de-4e55ded78e53"))
		{
			if (base.gameObject.name.Contains("EN_CV_DoorThickFrame_A") || (HasParentContaining("Wall 7") && base.gameObject.name.Contains("EN_CV_WallThin_2x_A")))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (HasParentContaining("FH_03:d38604bc-e082-4f1d-a69b-8638e22ae126") && HasParentContaining("Wall 7"))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("ThinDoor") && (HasParentContaining("L:673ac203-c050-4165-b467-1fec28b6bd5b") || HasParentContaining("L:85c8cabd-a1ec-4046-80b4-ddf3f66ed4ca")) && !DontTouchMeAgain)
		{
			if (base.transform.position.z < 6f)
			{
				base.transform.position += base.transform.right * 0.5f;
			}
			base.transform.position -= base.transform.forward * 0.3f;
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("DoorFrame") && (HasParentContaining("L:673ac203-c050-4165-b467-1fec28b6bd5b") || HasParentContaining("L:85c8cabd-a1ec-4046-80b4-ddf3f66ed4ca")))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("MNT_Wall") && HasParentContaining("L:673ac203-c050-4165-b467-1fec28b6bd5b") && HasParentContaining("Wall 2"))
		{
			base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, 0.75f);
			if (base.transform.position.z < 0.5f)
			{
				base.transform.position += base.transform.forward * 0.1f;
			}
		}
		else if ((base.gameObject.name.Contains("Wall") || base.gameObject.name.Contains("Torch")) && HasParentContaining("L:85c8cabd-a1ec-4046-80b4-ddf3f66ed4ca") && HasParentContaining("Wall 4"))
		{
			if (HasParentContaining("Wall 4"))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (base.gameObject.name.Contains("Hshape") && HasParentContaining("ThinDoor") && !DontTouchMeAgain && (HasParentContaining("L:85c8cabd-a1ec-4046-80b4-ddf3f66ed4ca") || HasParentContaining("L:673ac203-c050-4165-b467-1fec28b6bd5b")))
		{
			base.transform.position -= base.transform.right * 0.02f;
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("FH_14:81d068b4-2040-43b4-a063-fd6fd8c77cda") && HasParentContaining("Wall 3") && base.gameObject.name.Contains("Wall") && !DontTouchMeAgain)
		{
			base.transform.position += base.transform.forward * 0.45f;
			if (base.transform.position.x > 2f)
			{
				base.transform.localPosition -= new Vector3(0.3f, 0f, 0f);
				base.transform.localScale *= 1.33f;
			}
			else if (base.transform.position.x < 0.7f)
			{
				base.transform.localPosition += new Vector3(0.5f, 0f, 0f);
			}
			if (base.gameObject.name == "EN_CV_WallThin_2x_A" && IsPositionMatch(base.transform.position, new Vector3(1.4235582f, -0.09736753f, 21.58102f)))
			{
				MeshRenderer[] componentsInChildren3 = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
				for (int i = 0; i < componentsInChildren3.Length; i++)
				{
					componentsInChildren3[i].shadowCastingMode = ShadowCastingMode.Off;
				}
			}
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("FH_14:81d068b4-2040-43b4-a063-fd6fd8c77cda") && HasParentContaining("Wall 3") && base.gameObject.name.Contains("Hang") && !DontTouchMeAgain)
		{
			base.transform.position += base.transform.forward * 0.55f;
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("FH_14:81d068b4-2040-43b4-a063-fd6fd8c77cda") && HasParentContaining("Wall 3") && base.gameObject.name.Contains("Torch") && !DontTouchMeAgain)
		{
			base.transform.position += base.transform.forward * 0.6f;
			base.transform.localScale = Vector3.one;
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("J:25585110-06a2-4830-9116-57f588f461fc") && HasParentContaining("Wall 1") && base.gameObject.name.Contains("Wall") && !DontTouchMeAgain)
		{
			base.transform.position += base.transform.forward * 0.05f;
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("FH_14_2"))
		{
			if (base.gameObject.name.Contains("Jshape") || base.gameObject.name.Contains("Lshape"))
			{
				base.gameObject.name += "_TurnedOff";
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("Hex") && base.gameObject.name.Contains("shape") && base.transform.position.y < 0f)
			{
				base.transform.position = new Vector3(base.transform.position.x, 0f, base.transform.position.z);
			}
			if (HasParentContaining("FH_14_2:0ff1de12-ea3e-40a8-aa7e-cbc6b1d76bb6") && Application.isPlaying && !DontTouchMeAgain)
			{
				if (base.gameObject.name.Contains("Fshape") && (IsPositionMatch(base.transform.position, new Vector3(0.8599076f, 0f, -7.4457684f)) || IsPositionMatch(base.transform.position, new Vector3(1.7196796f, 0f, -8.934937f)) || IsPositionMatch(base.transform.position, new Vector3(2.579452f, 0f, -10.424107f))))
				{
					base.gameObject.name += "_TurnedOff";
					base.gameObject.SetActive(value: false);
				}
				else if (base.gameObject.name.Contains("Hshape") && ((IsPositionMatch(base.transform.position, new Vector3(0f, 0f, -5.9566746f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, 0.5f, 0f, 0.86602545f))) || (IsPositionMatch(base.transform.position, new Vector3(8.597716f, 0f, -5.9566765f)) && IsRotationMatch(base.transform.rotation, new Quaternion(0f, -1.0000001f, 0f, 0f)))))
				{
					base.gameObject.name += "_TurnedOff";
					base.gameObject.SetActive(value: false);
				}
				else if (base.gameObject.name.Contains("Sshape"))
				{
					if (IsPositionMatch(base.transform.position, new Vector3(6.018401f, 0f, -10.424183f)))
					{
						if (IsRotationMatch(targetRotation: new Quaternion(0f, 0.8660254f, 0f, 0.5f), rotation: base.transform.rotation))
						{
							Vector3 position2 = new Vector3(0.8386946f, 0f, -7.443703f);
							Quaternion quaternion = new Quaternion(0f, 0.8660264f, 0f, -0.4999984f);
							base.transform.SetPositionAndRotation(position2, quaternion);
							Vector3 dstPos7 = new Vector3(1.7137012f, 0f, -8.959244f);
							InstantiateDuplicate(dstPos7, quaternion).name = base.gameObject.name + "_DuplicateToFillAGap";
						}
					}
					else if (IsPositionMatch(base.transform.position, new Vector3(4.298859f, 0f, -10.424182f)))
					{
						Vector3 dstPos8 = new Vector3(2.548859f, 0f, -10.424182f);
						InstantiateDuplicate(dstPos8, base.transform.rotation).name = base.gameObject.name + "_DuplicateToFillAGap";
					}
				}
				DontTouchMeAgain = true;
			}
			else
			{
				if (!HasParentContaining("FH_14_2:31ecd0e4-0cff-42f7-92cd-74a24d05b69d") || !Application.isPlaying || DontTouchMeAgain)
				{
					return;
				}
				if (base.gameObject.name.Contains("Sshape"))
				{
					if (IsPositionMatch(base.transform.position, new Vector3(7.7379446f, 0f, -1.4891686f)))
					{
						base.gameObject.name += "_TurnedOff";
						base.gameObject.SetActive(value: false);
					}
					else if (IsPositionMatch(base.transform.position, new Vector3(6.0184016f, 0f, -4.467506f)))
					{
						Vector3 dstPos9 = new Vector3(5.143401f, 0f, -5.9830513f);
						InstantiateDuplicate(dstPos9, base.transform.rotation).name = base.gameObject.name + "_DuplicateToFillAGap";
					}
				}
				else if (base.gameObject.name.Contains("Fshape"))
				{
					if (IsPositionMatch(base.transform.position, new Vector3(5.1586323f, 0f, -5.9565215f)))
					{
						base.gameObject.name += "_TurnedOff";
						base.gameObject.SetActive(value: false);
					}
				}
				else if (base.gameObject.name.Contains("Hshape") && IsPositionMatch(base.transform.position, new Vector3(1.7195435f, 0f, -5.9566755f)))
				{
					base.gameObject.name += "_TurnedOff";
					base.gameObject.SetActive(value: false);
				}
				DontTouchMeAgain = true;
			}
		}
		else if (HasParentContaining("LN:6b9679ab-bd9a-467e-b911-260972b0e42b") && IsPositionMatch(base.transform.position, new Vector3(22.354063f, -0.0017046928f, 8.935015f)) && base.gameObject.name.IndexOf("FShape", StringComparison.OrdinalIgnoreCase) >= 0)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (HasParentContaining("FH_01_04_08:") && base.gameObject.name.Contains("Lamp") && HasParentContaining("MainWall 1"))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (HasParentContaining("FH_11_15b") && (HasParentContaining("Wall 12") || HasParentContaining("Wall 13") || HasParentContaining("Wall 14") || HasParentContaining("Wall 15")))
		{
			if (base.gameObject.name.Contains("Wall"))
			{
				base.transform.localPosition = Vector3.zero;
			}
			else if (base.gameObject.name.Contains("Hang_Pipe"))
			{
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, 0f);
			}
			else if (base.gameObject.name.Contains("Hang"))
			{
				base.transform.localPosition = new Vector3(0f, base.transform.localPosition.y, 0f);
			}
		}
		else if (HasParentContaining("J:c7bbc60b-d4b6-4ad2-937c-1d6be839b1ab") && base.gameObject.name.Contains("Wall") && IsPositionWithinRange(base.transform.position, new Vector3(4.5830364f, -0.18999995f, 3.8457265f), 1f))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (HasParentContaining("L:ebf114ae-4d1f-42a8-894f-fb48d093d563") && base.gameObject.name.Contains("CVC_Wall") && (HasParentContaining("Wall 1") || HasParentContaining("Wall 3")) && base.transform.position.x > 17f && base.transform.position.x < 17.4f && base.transform.position.z > 19.4f && base.transform.position.z < 31.1f)
		{
			base.transform.position = new Vector3(17.06f, base.transform.position.y, base.transform.position.z);
		}
		else if (HasParentContaining("N:ca3bcea1-4b08-4589-bea0-c697c78b9a39"))
		{
			if (base.gameObject.name.Contains("DoorThickFrame"))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (HasParentContaining("Wall 3") && !DontTouchMeAgain)
			{
				base.transform.position -= new Vector3(0f, 0f, -0.4f);
				DontTouchMeAgain = true;
			}
		}
		else if (HasParentContaining("J:21511a68-b222-43c9-b708-321e7d4de22f") && HasParentContaining("Wall 6") && !DontTouchMeAgain)
		{
			base.transform.position -= new Vector3(0f, 0f, 0.4f);
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("EN_CV_Clutter") && HasParentContaining("FH_14:27d9fad3-a5df-4508-ac26-6b8fba7fd898") && (IsPositionMatch(base.transform.position, new Vector3(25.542242f, 0.08921429f, 12.859931f)) || IsPositionMatch(base.transform.position, new Vector3(26.45527f, 0.03209163f, 12.943436f))))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("EN_CV_RandomBPillar") && HasParentContaining("FH_14:27d9fad3-a5df-4508-ac26-6b8fba7fd898") && IsPositionMatch(base.transform.position, new Vector3(24.591476f, -0.06666479f, -0.9567804f)))
		{
			base.transform.position = new Vector3(23.26271f, -0.06666479f, -0.4799995f);
		}
		else if (base.gameObject.name.Contains("EN_CV_BPillar") && HasParentContaining("G:57717c59-b95f-44c7-bb3a-af2b4307d5e2") && IsPositionMatch(base.transform.position, new Vector3(19.26375f, -0.28000003f, 1.5145586f)))
		{
			base.transform.position = new Vector3(19.40475f, -0.28000003f, 1.6655586f);
		}
		else if (base.gameObject.name.Contains("EN_CV_Wall") && ((HasParentContaining("G:57717c59-b95f-44c7-bb3a-af2b4307d5e2") && (IsPositionMatch(base.transform.position, new Vector3(9.317495f, -0.09736753f, 2.5269678f)) || IsPositionMatch(base.transform.position, new Vector3(9.317495f, -0.09736753f, 6.4089174f)) || IsPositionMatch(base.transform.position, new Vector3(23.353851f, -0.09736753f, 2.5260966f)) || IsPositionMatch(base.transform.position, new Vector3(23.353851f, -0.09736753f, 6.408046f)))) || (HasParentContaining("M:24cbbc21-e69a-4ef3-8db4-d025b2202e34") && IsPositionMatch(base.transform.position, new Vector3(9.597494f, -0.09736753f, 2.60114f)))))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("EN_CV_Wall") && HasParentContaining("FH_14:27d9fad3-a5df-4508-ac26-6b8fba7fd898"))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(23.073853f, -0.09736753f, 6.814887f)))
			{
				Vector3 localScale3 = base.transform.localScale;
				base.transform.localScale = new Vector3(localScale3.x, localScale3.y, 0.6f);
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(23.073853f, -0.09736753f, 2.3321328f)))
			{
				base.transform.SetPositionAndRotation(new Vector3(23.21385f, -0.09736753f, 2.3289084f), base.transform.rotation);
				Vector3 localScale4 = base.transform.localScale;
				base.transform.localScale = new Vector3(localScale4.x, localScale4.y, 0.6f);
			}
		}
		else if (base.gameObject.name.Contains("LightTorch") && HasParentContaining("G:57717c59-b95f-44c7-bb3a-af2b4307d5e2") && IsPositionMatch(base.transform.position, new Vector3(22.619137f, 1.8603203f, 2.5265324f)))
		{
			base.transform.SetPositionAndRotation(new Vector3(22.53422f, 1.8603203f, 1.8670802f), new Quaternion(0f, -0.43242013f, 0f, 0.90167236f));
		}
		else if (HasParentContaining("N:ed3953dc-b4cd-43cc-9134-f5eab6e08d6d") || HasParentContaining("G:d0381507-474a-410e-a5bd-4638bb29e8cb") || HasParentContaining("H:589a1178-da60-4e39-aef6-a7beb83089ed"))
		{
			if ((HasParentContaining("Wall 1") || HasParentContaining("Wall 3") || HasParentContaining("Wall 5")) && base.transform.position.x < 3.6f && (base.transform.position.z < -13f || base.transform.position.z > -11f))
			{
				SwapGrandChildren("Default", "Iceberg");
			}
			if (base.gameObject.name.Contains("CV_Wall") && !base.gameObject.name.Contains("_Adjusted") && base.transform.position.x > 5f && base.transform.position.x < 7f && base.transform.position.z > -22f && base.transform.position.z < 0f)
			{
				Vector3 position3 = base.transform.position;
				position3.x -= 0.25f;
				base.transform.position = position3;
				base.gameObject.name += "_Adjusted";
			}
		}
		else if (base.gameObject.name == "Wall 3" && HasParentContaining("LN:edd31b61-de28-4aee-921d-8b0ebb00a929"))
		{
			base.gameObject.transform.position = new Vector3(-2.8902283f, 0f, 7.445846f);
		}
		else if (base.gameObject.name.Contains("MNT_ExitBay") && HasParentContaining("LN:edd31b61-de28-4aee-921d-8b0ebb00a929") && HasParentContaining("Wall 3"))
		{
			base.transform.SetPositionAndRotation(new Vector3(0.8597698f, 0f, 11.785846f), new Quaternion(0f, 0.7071067f, 0f, 0.7071068f));
		}
		else if (base.gameObject.name.Contains("DN_Door") && (HasParentContaining("M:0497e8ef-43fe-46b9-a4ca-64330caf5d88") || HasParentContaining("FH_02_01:0f2ee283-3c8e-4c8b-8766-a51039043847") || HasParentContaining("FH_02_01:0e5544bd-4697-41e2-ab38-684e1cb94c9e") || HasParentContaining("J:07220320-ec0b-46be-b1f7-75130697dd7a") || HasParentContaining("K:705259d9-dc88-4989-be02-3f6a53c135d4")))
		{
			Vector3 vector7 = new Vector3(4.2989006f, 0f, 1.4890199f);
			Vector3 vector8 = base.transform.position - vector7;
			vector8.y = 0f;
			if (vector8 != Vector3.zero)
			{
				Quaternion rotation3 = Quaternion.LookRotation(vector8);
				base.transform.rotation = rotation3;
			}
		}
		else if (base.gameObject.name.Contains("Hex_Lshape") && HasParentContaining("FH_02_01:0e5544bd-4697-41e2-ab38-684e1cb94c9e") && IsPositionMatch(base.transform.position, new Vector3(1.7195439f, -0.00999999f, -8.935013f)))
		{
			base.transform.SetPositionAndRotation(new Vector3(9.456789f, -0.0099999085f, -8.438603f), new Quaternion(0f, 0.86602545f, 0f, 0.5000001f));
		}
		else if (base.gameObject.name.Contains("ST_DMN_Obst") && HasParentContaining("M:e9eafc65-abb0-4166-8304-f97d6790e54d"))
		{
			Vector3 vector9 = new Vector3(11.177183f, 0f, 4.467468f) - base.transform.position;
			vector9.y = 0f;
			if (vector9 != Vector3.zero)
			{
				Quaternion rotation4 = Quaternion.LookRotation(vector9);
				base.transform.rotation = rotation4;
			}
			base.transform.position = new Vector3(base.transform.position.x, 0.05f, base.transform.position.z);
		}
		else if (HasParentContaining("FH_09_01:a005faf2-f05b-4add-ac31-438b9b600bd6"))
		{
			if (base.gameObject.name.Contains("Scenario15_Pillar") && !base.gameObject.name.EndsWith("_DuplicateToFillAGap") && Application.isPlaying && !DontTouchMeAgain)
			{
				InstantiateDuplicate(new Vector3(8.263664f, 3.7768565E-08f, 10.738792f), base.transform.rotation).name = base.gameObject.name + "_DuplicateToFillAGap";
				DontTouchMeAgain = true;
			}
			else if (base.gameObject.name.Contains("MR_Wall_1x_A") && IsPositionMatch(base.transform.position, new Vector3(17.627195f, 8.220529E-07f, 0.295969f)))
			{
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("MR_Wall_2x_Feature_J") && IsPositionMatch(base.transform.position, new Vector3(18.914978f, 0f, 0.49638987f)))
			{
				base.transform.position = new Vector3(18.44476f, base.transform.position.y, base.transform.position.z);
			}
			else if (base.gameObject.name.Contains("EN_VLT_Light_Lantern") && (HasParentContaining("Wall 0") || HasParentContaining("Wall 5")))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (HasParentContaining("FH_15_01x2"))
		{
			if (base.gameObject.name.Contains("Jshape") && Mathf.Approximately(base.transform.localEulerAngles.y, 120f))
			{
				base.gameObject.name += "_TurnedOffForBeingRedundant";
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("Lshape") && Mathf.Approximately(base.transform.localEulerAngles.y, 240f))
			{
				base.gameObject.name += "_TurnedOffForBeingRedundant";
				base.gameObject.SetActive(value: false);
			}
			else if ((HasParentContaining("Wall 1") || HasParentContaining("Elevator Wall 5")) && base.gameObject.name.Contains("Hang_") && base.transform.position.x >= 1f && base.transform.position.x <= 9f)
			{
				SetAllMeshRenderersOf(base.gameObject, wantedState: false);
				base.gameObject.name = base.gameObject.name + "_HangThatMustNotExist";
				base.gameObject.SetActive(value: false);
			}
			else if ((HasParentContaining("Elevator Wall 1") || HasParentContaining("Elevator Wall 5")) && (base.gameObject.name.Contains("Wall") || base.gameObject.name.Contains("Lamp")))
			{
				SetAllMeshRenderersOf(base.gameObject, wantedState: false);
				base.gameObject.name = base.gameObject.name + "_WallThatMustNotExist";
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("Pshape") && (IsPositionMatch(base.transform.position, new Vector3(5.1586304f, -0.0009999871f, 11.913351f)) || IsPositionMatch(base.transform.position, new Vector3(5.1586304f, -0.0009999871f, 0f))))
			{
				base.gameObject.name += "_GapTileTurnedOff";
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("Sshape") && !base.gameObject.name.EndsWith("_DuplicateToFillAGap") && Application.isPlaying && !DontTouchMeAgain)
			{
				if (IsPositionMatch(base.transform.position, new Vector3(3.439087f, 0f, 0f)))
				{
					InstantiateDuplicate(new Vector3(5.189087f, 0f, 0f), base.transform.rotation).name = base.gameObject.name + "_DuplicateToFillAGap";
					DontTouchMeAgain = true;
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(6.878174f, 0f, 11.913351f)))
				{
					InstantiateDuplicate(new Vector3(5.128174f, 0f, 11.913351f), base.transform.rotation).name = base.gameObject.name + "_DuplicateToFillAGap";
					DontTouchMeAgain = true;
				}
			}
		}
		else if (base.gameObject.name.Contains("FH_15_01x2"))
		{
			Transform transform54 = base.transform.Find("Generated Content/Full");
			if (!(transform54 != null))
			{
				return;
			}
			Transform[] componentsInChildren = transform54.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform55 in componentsInChildren)
			{
				if (transform55.name.Contains("SmallMetalDebris"))
				{
					Vector3 position4 = transform55.position;
					bool num7 = position4.x >= 1f && position4.x <= 9f;
					bool flag3 = (position4.z >= 10f && position4.z <= 12f) || (position4.z >= -1f && position4.z <= 1f);
					if (num7 & flag3)
					{
						transform55.name += "_DebrisRemoved";
						transform55.gameObject.SetActive(value: false);
					}
				}
			}
		}
		else if (HasParentContaining("N:0b9258c6-b40c-439e-b0bf-fdc9045199f6") && base.gameObject.name.Contains("EN_RF_Wall_2Hex_A") && Mathf.Abs(base.transform.position.x) < 14f && base.transform.position.z < -2.5f)
		{
			base.transform.Rotate(0f, -60f, 0f, Space.World);
			base.transform.position = new Vector3(12.9f, base.transform.position.y, base.transform.position.z);
		}
		else if (base.gameObject.name.Contains("RF_Door") && (HasParentContaining("FH_03_01:cd3e3e41-4861-4360-bd17-3fca1ec902f3") || HasParentContaining("G:61aaa7f5-229f-4522-a92d-1bcfa81d4cd0") || HasParentContaining("N:14f416b5-5ffc-458b-b8a6-f2e2b48ec659")))
		{
			if (Vector3.SqrMagnitude(base.transform.position - new Vector3(3.5489883f, 0.49077606f, -17.85978f)) <= 3.24f)
			{
				base.transform.position = new Vector3(3.5489883f, 0.49077606f, -17.85978f);
				base.transform.rotation = new Quaternion(0f, -1f, 0f, -3.5762784E-07f);
			}
			else if (Vector3.SqrMagnitude(base.transform.position - new Vector3(-6.768278f, 0.49077606f, -17.85978f)) <= 3.24f)
			{
				base.transform.position = new Vector3(-6.768278f, 0.49077606f, -17.85978f);
				base.transform.rotation = new Quaternion(0f, 0f, 0f, 1f);
			}
			else if (Vector3.SqrMagnitude(base.transform.position - new Vector3(1.8294439f, 0.49077606f, -11.903103f)) <= 3.24f)
			{
				base.transform.position = new Vector3(1.8294439f, 0.49077606f, -11.903103f);
				base.transform.rotation = new Quaternion(0f, -1f, 0f, 5.8114523E-07f);
			}
		}
		else if (HasParentContaining("G:61aaa7f5-229f-4522-a92d-1bcfa81d4cd0"))
		{
			if (HasParentContaining("Wall 3"))
			{
				if (base.gameObject.name.Contains("EN_RF_Wall_2Hex_B"))
				{
					base.gameObject.SetActive(value: false);
				}
				else if (base.gameObject.name.Contains("EN_RF_Wall_2Hex_C"))
				{
					base.transform.position = new Vector3(-1.4595444f, base.transform.position.y, base.transform.position.z);
				}
			}
		}
		else if (((HasParentContaining("FH_13_02:8c5e41bd-cd26-4d22-af53-80a1fcb5c086") && HasParentContaining("Sails_Wall")) || (HasParentContaining("N:889a5fa7-c6ab-4b51-999b-6c6a558b9c22") && (HasParentContaining("Wall 4") || HasParentContaining("Wall 2"))) || (HasParentContaining("L:ee43df9d-20a7-461e-af88-995c01ac2361") && HasParentContaining("Wall 2"))) && base.gameObject.name.Contains("Wall") && base.transform.position.z > 2.5f && base.transform.position.z < 6.5f)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (HasParentContaining("J:510e13c7-8a72-4eac-a240-52733afecdce") && HasParentContaining("Wall 2") && base.gameObject.name.Contains("EN_CV_WallThin_2x_B") && base.transform.position.z < -9.5f)
		{
			base.transform.localScale = new Vector3(0.6f, base.transform.localScale.y, base.transform.localScale.z);
			base.transform.Rotate(0f, 26f, 0f, Space.World);
		}
		else if (HasParentContaining("D:3507c74d-4bc9-4ff7-a89d-e0fcd4d7d0b2") && base.gameObject.name.Contains("EN_CV_ThickDoor_A"))
		{
			base.transform.Rotate(0f, -180f, 0f, Space.World);
		}
		else if (HasParentContaining("FH_07x5:77b40f10-fbc3-4470-8d39-de2282cf4202") && base.gameObject.name.Contains("SShape") && base.transform.position.z < -2.5f)
		{
			base.gameObject.SetActive(value: false);
		}
		else if (HasParentContaining("B:5d9226e7-6901-4ea8-8385-2aeaec0231d1") && HasParentContaining("Wall 3") && base.gameObject.name.Contains("Wall") && !DontTouchMeAgain)
		{
			base.transform.position += new Vector3(0f, 0f, -0.53f);
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("G:aeef6c6a-67e9-4f60-90b1-00976f5ef779") && HasParentContaining("Wall 3") && base.gameObject.name.Contains("Wall") && !DontTouchMeAgain && base.transform.position.x > 15f)
		{
			base.transform.position += new Vector3(0f, 0f, 0.53f);
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("B:5d9226e7-6901-4ea8-8385-2aeaec0231d1") && base.gameObject.name.Contains("Terrain_Difficult") && !DontTouchMeAgain)
		{
			base.transform.Rotate(0f, 30f, 0f, Space.World);
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("FH_15_11x2b:457624a1-fb7b-4cda-996f-5bb376d4cc45"))
		{
			if (base.gameObject.name.Contains("PortalTerrain"))
			{
				if (base.transform.position.x > 0f)
				{
					base.transform.SetPositionAndRotation(new Vector3(13.756355f, 0f, 2.9783382f), new Quaternion(0f, 1f, 0f, -4.371139E-08f));
				}
				else
				{
					base.transform.SetPositionAndRotation(new Vector3(-10.317266f, 0f, 2.9783382f), Quaternion.identity);
				}
			}
			else if (base.gameObject.name.Contains("PineTree") && base.transform.position.x < 0f && base.transform.position.z > 2f && base.transform.position.z < 5f)
			{
				base.gameObject.SetActive(value: false);
			}
			else if (base.gameObject.name.Contains("PineTree") && !DontTouchMeAgain && base.transform.position.x < 0f && base.transform.position.z > 1f && base.transform.position.z < 2f)
			{
				base.transform.position -= new Vector3(0f, 0f, 0.7f);
				DontTouchMeAgain = true;
			}
		}
		else if (HasParentContaining("FH_15_10b:0753466d-036c-4713-ae36-d85d12aff91f"))
		{
			if (base.gameObject.name.Contains("PortalTerrain"))
			{
				ToggleChildren("Scenario 67", wantedState: false);
				ToggleChildren("Scenario 68", wantedState: true);
				ToggleGrandChildren("Scenario 68", wantedState: true);
			}
			else if (base.gameObject.name.Contains("Portal_Demonic") && !DontTouchMeAgain)
			{
				base.transform.position = new Vector3(4.219743f, 0.094111204f, -5.1775074f);
				base.transform.localRotation = Quaternion.identity;
				DontTouchMeAgain = true;
			}
			else if (base.gameObject.name.Contains("Broken"))
			{
				if (IsPositionMatch(base.transform.position, new Vector3(-0.4849782f, 0f, 6.900881f)) || IsPositionMatch(base.transform.position, new Vector3(-0.2076664f, 0f, 7.069352f)) || IsPositionMatch(base.transform.position, new Vector3(8.69911f, 0f, 7.45831f)))
				{
					base.gameObject.SetActive(value: false);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(9.51257f, 0f, -0.5111487f)))
				{
					base.transform.position = new Vector3(9.987492f, base.transform.position.y, -0.241662f);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(9.506876f, 0f, 6.481577f)))
				{
					base.transform.position = new Vector3(9.958109f, base.transform.position.y, 6.464259f);
				}
				else if (IsPositionMatch(base.transform.position, new Vector3(8.805388f, 0f, 7.069351f)))
				{
					base.transform.SetPositionAndRotation(new Vector3(9.055742f, 0f, 6.970557f), new Quaternion(0f, -0.9659258f, 0f, 0.25881946f));
				}
				else if (base.transform.GetComponentsInParent<Transform>(includeInactive: true).Any((Transform p) => p.name == "Wall_1"))
				{
					base.transform.localScale = new Vector3(base.transform.localScale.x, base.transform.localScale.y, 1f);
				}
				else if (HasParentContaining("Wall_4"))
				{
					if (base.transform.position.x > 1.8f)
					{
						base.gameObject.SetActive(value: false);
					}
				}
				else if (HasParentContaining("Wall_3") && base.transform.position.x < 6f)
				{
					base.gameObject.SetActive(value: false);
				}
			}
			else if (base.gameObject.name.Contains("Pillar") && IsPositionMatch(base.transform.position, new Vector3(-0.1793213f, 0f, 7.8387046f)))
			{
				base.transform.position = new Vector3(-0.7854686f, base.transform.position.y, 7.7648973f);
				base.transform.localScale = Vector3.one;
			}
		}
		else if (HasParentContaining("FH_11_03_05:f2afa723-100f-40f0-bfc0-76738c76c9a0") && (HasParentContaining("WallSmall 5") || HasParentContaining("WallSmall 1") || HasParentContaining("WallBig 8") || HasParentContaining("WallBig 10")) && base.gameObject.name.Contains("Wall") && !base.gameObject.name.Contains("DoNotRenameMe"))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("Lshape") && HasParentContaining("FH_12_12b:"))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(7.737945f, -0.00999999f, 7.4458447f)))
			{
				base.transform.rotation = new Quaternion(0f, 1f, 0f, 3.5762787E-07f);
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(0.8597722f, -0.00999999f, 7.4458447f)))
			{
				base.transform.rotation = new Quaternion(0f, 0.8660252f, 0f, 0.50000024f);
			}
		}
		else if (base.gameObject.name.Contains("Jshape") && HasParentContaining("FH_12_12b:"))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(7.737945f, -0.00999999f, 7.445844f)))
			{
				base.transform.rotation = new Quaternion(0f, -6.258488E-07f, 0f, 1f);
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(0.8597717f, -0.00999999f, 7.4458456f)))
			{
				base.transform.rotation = new Quaternion(0f, 0.50000024f, 0f, 0.8660252f);
			}
		}
		else if (base.gameObject.name.Contains("MR_Pillar") && HasParentContaining("FH_12_12b:"))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(MathF.E * 23f / 51f, 3.7591587E-08f, 7.4458456f)))
			{
				base.transform.position = new Vector3(1.2643785f, 3.7591587E-08f, 7.437866f);
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(7.3718305f, 3.7591587E-08f, 7.4458456f)))
			{
				base.transform.position = new Vector3(7.353895f, 3.7591587E-08f, 7.4454246f);
			}
		}
		else if (base.gameObject.name.Contains("CV_Clutter") && (HasParentContaining("N:90bb60f9-a3f2-41dd-8c86-bef136b68cf2") || HasParentContaining("J:f2012e66-b3a7-4eea-98ee-ad91f8dbf895")))
		{
			base.gameObject.name += "_Moved_Scenario136";
			base.transform.position = new Vector3(base.transform.position.x, -0.1f, base.transform.position.z);
		}
		else if (base.gameObject.name == "EN_DoorFrame_A" && HasParentContaining("N:90bb60f9-a3f2-41dd-8c86-bef136b68cf2"))
		{
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform56 in componentsInChildren)
			{
				if (transform56.name.Contains("EN_DoorFrame_A_Left"))
				{
					transform56.gameObject.name += "_Moved_Scenario136";
					transform56.position = new Vector3(7.815157f, 0.023329973f, 4.140177f);
					break;
				}
			}
		}
		else if (Application.isPlaying && base.gameObject.name.Contains("shape", StringComparison.OrdinalIgnoreCase) && (HasParentContaining("N:90bb60f9-a3f2-41dd-8c86-bef136b68cf2") || HasParentContaining("J:f2012e66-b3a7-4eea-98ee-ad91f8dbf895")) && HasParentContaining("Door"))
		{
			base.gameObject.SetActive(value: false);
			base.gameObject.name += "_TurnedOffDoorHex_Scenario 136";
		}
		else if (Application.isPlaying && base.gameObject.name.Contains("N:90bb60f9-a3f2-41dd-8c86-bef136b68cf2") && !DontTouchMeAgain)
		{
			Transform transform57 = base.transform.Find("Generated Content/Full");
			if (transform57 != null)
			{
				Transform[] componentsInChildren = transform57.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform58 in componentsInChildren)
				{
					if (transform58.name.Contains("073_Water") && !transform58.name.Contains("_Handled") && IsPositionMatch(transform58.position, new Vector3(-1.7195435f, -0.0017045736f, 2.9783378f)))
					{
						transform58.name += "_Handled";
						InstantiateDuplicateFromRoom(dstPos: new Vector3(7.738681f, -0.001704542f, 4.467561f), dstRot: new Quaternion(0f, 1f, 0f, 0f), source: transform58.gameObject).name = transform58.gameObject.name + "_Duplicate";
						break;
					}
				}
			}
			DontTouchMeAgain = true;
		}
		else if (Application.isPlaying && base.gameObject.name.Contains("CV_Hex") && !base.gameObject.name.Contains("_Handled") && (HasParentContaining("J:f2012e66-b3a7-4eea-98ee-ad91f8dbf895") || HasParentContaining("I:e9df9d63-3d19-447e-b69e-00f4b6f05e55")))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(11.1769905f, -5.066395E-07f, 1.4893188f)))
			{
				if (base.gameObject.name.Contains("EN_CV_Hex_Fshape_A"))
				{
					base.gameObject.name += "_Handled";
					InstantiateDuplicate(new Vector3(21.42699f, -0.027f, 7.4893193f), new Quaternion(0f, -0.5000002f, 0f, 0.8660252f)).name = base.gameObject.name + "_Duplicate";
				}
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(20.634521f, 0f, 11.91335f)))
			{
				if (base.gameObject.name.Contains("EN_CV_Hex_Hshape_A"))
				{
					base.gameObject.name += "_Handled";
					InstantiateDuplicate(base.transform.position, new Quaternion(0f, 0.50000167f, 0f, -0.8660246f)).name = base.gameObject.name + "_RotatedDuplicate";
				}
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(22.354065f, 0f, 8.935015f)))
			{
				if (base.gameObject.name.Contains("EN_CV_Hex_Sshape_A"))
				{
					base.gameObject.name += "_Handled";
					InstantiateDuplicate(base.transform.position, new Quaternion(0f, 0.5000028f, 0f, -0.86602384f)).name = base.gameObject.name + "_RotatedDuplicate";
				}
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(22.354067f, -0.0009999871f, 8.935014f)) && base.gameObject.name.Contains("EN_CV_Hex_Pshape_A"))
			{
				base.gameObject.name += "_TurnedOff";
				base.gameObject.SetActive(value: false);
			}
		}
		else if (base.gameObject.name.Contains("SHP_Hex_Jshape") && IsPositionMatch(base.transform.position, new Vector3(2.5793161f, -0.00999999f, 13.40252f)) && HasParentContaining("FH_14:a1767713-07f0-4773-9314-ebfedc6da957"))
		{
			base.gameObject.name += "_TurnedOff";
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name == "EN_CV_Hex_Pshape_A" && HasParentContaining("J:d5fa88c6-cc79-4eab-9940-1df481075c68"))
		{
			if (IsPositionMatch(base.transform.position, new Vector3(0.8597715f, -0.0009999871f, 10.424182f)))
			{
				base.gameObject.name += "_Adjusted";
				base.transform.SetPositionAndRotation(new Vector3(0.8597715f, -0.0009999871f, 10.424182f), new Quaternion(0f, -0.86602545f, 0f, 0.5f));
			}
			else if (IsPositionMatch(base.transform.position, new Vector3(2.5793152f, -0.0009999871f, 13.40252f)))
			{
				base.gameObject.name += "_Adjusted";
				base.transform.SetPositionAndRotation(new Vector3(2.5793152f, -0.0009999871f, 13.40252f), new Quaternion(0f, 0f, 0f, 1f));
			}
		}
		else if (base.gameObject.name == "FH_14:a1767713-07f0-4773-9314-ebfedc6da957")
		{
			Transform[] componentsInChildren = base.transform.Find("Walls/Wall 1/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform59 in componentsInChildren)
			{
				if (transform59.name == "EN_SHP_DeckPillar_1x_C" && IsPositionMatch(transform59.position, new Vector3(2.0631566f, 5.270536E-08f, 13.402522f)))
				{
					if (!transform59.name.Contains("_Moved_Scenario137"))
					{
						transform59.name += "_Moved_Scenario137";
					}
					transform59.position = new Vector3(2.0835023f, 0f, 12.934133f);
					break;
				}
			}
			componentsInChildren = base.transform.Find("Walls/Wall 6/Generated Content").GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform60 in componentsInChildren)
			{
				if (transform60.name == "EN_SHP_DeckPillar_1x_C" && IsPositionMatch(transform60.position, new Vector3(0.60169315f, 5.270536E-08f, 10.871192f)))
				{
					if (!transform60.name.Contains("_Moved_Scenario137"))
					{
						transform60.name += "_Moved_Scenario137";
					}
					transform60.position = new Vector3(0.9442744f, 5.270536E-08f, 11.050379f);
					break;
				}
			}
		}
		else if (base.gameObject.name.Contains("CV_Wall") && IsPositionMatch(base.transform.position, new Vector3(0.19457293f, -0.09736753f, 10.267795f)) && HasParentContaining("J:d5fa88c6-cc79-4eab-9940-1df481075c68"))
		{
			base.gameObject.name += "_Moved";
			base.transform.position = new Vector3(0.19457293f, -0.09736753f, 10.147795f);
		}
		else if (base.gameObject.name == "EN_CV_ThickDoor_A" && (HasParentContaining("K:4429d572-f2fc-40a8-9895-c6c60849fdc4") || HasParentContaining("D:a144d131-df0b-4a80-8555-b0b5d2c946da")))
		{
			base.transform.localScale = new Vector3(0.6f, 0.8f, 0.6f);
		}
		else if (base.gameObject.name.Contains("CV_Pillar") && IsPositionMatch(base.transform.position, new Vector3(-11.23512f, 0.010000048f, -10.323586f)) && HasParentContaining("K:4429d572-f2fc-40a8-9895-c6c60849fdc4"))
		{
			base.gameObject.name += "_Moved_Rotated";
			base.transform.SetPositionAndRotation(new Vector3(-11.095638f, 0.010000048f, -10.485197f), new Quaternion(0f, -0.40160227f, 0f, 0.91581416f));
		}
		else if (base.gameObject.name == "EN_CV_Wall_2x_B" && IsPositionMatch(base.transform.position, new Vector3(-10.169887f, -0.16000001f, -9.156967f)) && HasParentContaining("K:4429d572-f2fc-40a8-9895-c6c60849fdc4"))
		{
			base.gameObject.name += "_Moved";
			base.transform.position = new Vector3(-10.296627f, -0.16000001f, -9.377268f);
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform61 in componentsInChildren)
			{
				if (transform61.name.Contains("Stalagmite"))
				{
					transform61.name += "_TurnedOff";
					transform61.gameObject.SetActive(value: false);
				}
			}
		}
		else if (base.gameObject.name == "EN_CV_Hang_NestedStalagmite_E" && IsPositionMatch(base.transform.position, new Vector3(-10.775729f, 3f, -8.806316f)) && HasParentContaining("K:4429d572-f2fc-40a8-9895-c6c60849fdc4"))
		{
			base.gameObject.name += "_Moved";
			base.transform.position = new Vector3(-10.775729f, 2.868f, -8.806316f);
		}
		else if (HasParentContaining("I:15f1ad43-b015-4cc9-9327-7e7b80e4b4a1") && (base.gameObject.name.Contains("PCG_DN_Hex_Jshape_A") || base.gameObject.name.Contains("PCG_DN_Hex_Lshape_A")) && (HasParentContaining("Wall 1") || HasParentContaining("Wall 3")))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (HasParentContaining("FH_07_04:09bcde7f-d7f2-4755-b4b5-1239bac52d50"))
		{
			if (base.gameObject.name.Contains("PCG_DN_Wall_1x_A") && HasParentContaining("Wall 10"))
			{
				base.gameObject.SetActive(value: false);
			}
			else if ((base.gameObject.name.Contains("PCG_DN_Hex_Lshape_A") || base.gameObject.name.Contains("PCG_MR_Floor_Eshape_A")) && (HasParentContaining("Wall 9") || HasParentContaining("Wall 7")))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (!DontTouchMeAgain && base.gameObject.name.Contains("EN_TIF_Wall_2x_C") && HasParentContaining("LL:535ea116-1555-4c20-b5f6-187d9dceae82"))
		{
			Quaternion quaternion2 = Quaternion.Euler(0f, 0.6f, 0f);
			base.transform.rotation = base.transform.rotation * quaternion2;
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("EN_SHD_DeckWall_2x_C") && HasParentContaining("FH_14:cf8a9dbb-2e2c-4c48-8d2e-dde5be58d922"))
		{
			if (HasParentContaining("Wall 6"))
			{
				base.transform.localPosition = new Vector3(1.66f, -0.3365093f, -1.5f);
				base.transform.localScale = new Vector3(0.8316928f, 1f, 1f);
			}
			else if (HasParentContaining("Wall 5"))
			{
				base.transform.localPosition = new Vector3(-1.682f, -0.3365093f, -3.549f);
				base.transform.localScale = new Vector3(0.8349111f, 1f, 1f);
			}
		}
	}

	[ContextMenu("Adjust Lighting")]
	public void AdjustLighting()
	{
		if (!DontTouchMeAgain && base.gameObject.name.Contains("MNT_Wall") && base.transform.position.z > 9f && HasParentContaining("LN:edd31b61-de28-4aee-921d-8b0ebb00a929") && HasParentContaining("Wall 3") && !HasParentContaining("ExitBay"))
		{
			base.gameObject.transform.position -= new Vector3(0f, 0f, 3f);
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name == "A:17f1fc57-76e0-4281-bc00-ede62f30ae2a" || base.gameObject.name == "A:9ace8bed-b07c-4134-b503-6fa8e2ecf760" || base.gameObject.name == "A:d698de22-632f-4251-b769-9d2f620dc66f" || base.gameObject.name == "A:8e63a418-3be4-4524-afde-3ae57547180c" || base.gameObject.name == "B:d91230d1-c087-4642-b823-8e2a2da39957" || base.gameObject.name == "BB:cdc062df-e4f8-4c0c-8e07-b65ee981d449")
		{
			foreach (Transform item3 in base.transform.Find("Walls"))
			{
				foreach (Transform item4 in item3.Find("Generated Content"))
				{
					if (item4.name.Contains("MNT_Pillar") && item4.localScale.x < 0.6f && item4.localScale.z < 0.6f)
					{
						if (Near(item4.position, new Vector3(10.317261f, -0.33925724f, -6.204871f)))
						{
							item4.SetPositionAndRotation(new Vector3(10.678267f, -0.33925724f, -5.967871f), new Quaternion(0f, 0.99709064f, 0f, 0.076225445f));
							item4.localScale = Vector3.one;
						}
						else if (Near(item4.position, new Vector3(10.042491f, -0.33925724f, -5.708481f)))
						{
							item4.SetPositionAndRotation(new Vector3(9.904406f, -0.33925724f, -5.927482f), new Quaternion(0f, -0.8168917f, 0f, 0.5767912f));
							item4.localScale = new Vector3(1.1f, 1f, 1.1f);
						}
						else if (Near(item4.position, new Vector3(7.152947f, -0.33925724f, 5.708482f)))
						{
							item4.SetPositionAndRotation(new Vector3(7.2789497f, -0.33925724f, 6.0209823f), new Quaternion(0f, -0.7621189f, 0f, 0.64743716f));
							item4.localScale = Vector3.one;
						}
						else if (Near(item4.position, new Vector3(6.878174f, -0.33925724f, 6.2048717f)))
						{
							item4.SetPositionAndRotation(new Vector3(6.704178f, -0.33925724f, 6.1818714f), new Quaternion(0f, 0f, 0f, 1f));
							item4.localScale = Vector3.one;
						}
						else if (Near(item4.position, new Vector3(6.603404f, -0.33925724f, 5.708482f)))
						{
							item4.SetPositionAndRotation(new Vector3(6.357911f, -0.33925724f, 6.0328064f), new Quaternion(0f, 0.9826288f, 0f, 0.185582f));
							item4.localScale = Vector3.one;
						}
					}
				}
			}
			foreach (Transform item5 in transform)
			{
				if (!item5.name.Contains("Door : ("))
				{
					continue;
				}
				DoorAudio[] componentsInChildren = item5.GetComponentsInChildren<DoorAudio>(includeInactive: true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].OpenAudioEvent = string.Empty;
				}
				Stack<Transform> stack = new Stack<Transform>();
				foreach (Transform item6 in item5)
				{
					stack.Push(item6);
				}
				while (stack.Count > 0)
				{
					Transform transform3 = stack.Pop();
					if (transform3.name.Contains("_DoorFrame_", StringComparison.InvariantCultureIgnoreCase) || transform3.name.Contains("_Doorway", StringComparison.InvariantCultureIgnoreCase))
					{
						transform3.name += "_TurnedOff";
						transform3.gameObject.SetActive(value: false);
						continue;
					}
					foreach (Transform item7 in transform3)
					{
						stack.Push(item7);
					}
				}
			}
			if (base.gameObject.name.Contains("BB:"))
			{
				Transform transform4 = transform.Find("Walls/Wall 6/Generated Content");
				if (transform4 != null)
				{
					foreach (Transform item8 in transform4)
					{
						if (item8.name.Contains("EN_MNT_Hex_Hshape_A") && !item8.name.Contains("_Moved") && Near(item8.position, new Vector3(0f, 0f, 5.9566755f)))
						{
							item8.position = new Vector3(0f, 0.038f, 5.9566755f);
							item8.name += "_Moved";
							break;
						}
					}
				}
			}
		}
		else if (base.gameObject.name.Contains("PressurePlate") && HasParentContaining("J:d865523d-f2e1-4077-aeed-d192841f2729"))
		{
			ToggleChildren("PointLightDoNotRename", wantedState: false);
		}
		else if (base.gameObject.name.Contains("MetalObstacle") && (HasParentContaining("D:94d43154-ffaf-4acd-9208-bd526bd99a08") || HasParentContaining("K:f7c10db0-4ae9-4203-aece-f25db96a61a2")))
		{
			ToggleChildren("PropLightWarmDoNotRename", wantedState: false);
		}
		else if (base.gameObject.name.Contains("EN_RF_Teleport") && transform.position.x > -3f && transform.position.x < 9f)
		{
			MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].SetLayer(RenderingLayer.EVERYTHING);
			}
		}
		else if (base.gameObject.name.Contains("Point Light") && HasParentContaining("EN_RF_TeleportBay") && transform.position.x > -3f && transform.position.x < 9f)
		{
			Light component = base.gameObject.GetComponent<Light>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
		else if (HasParentContaining("M:f0f7f3ea-7bf1-45f9-bec6-de86d1e69b5e") && (HasParentContaining("Wall 2") || HasParentContaining("Wall 3") || HasParentContaining("Wall 4")) && base.gameObject.name.Contains("Wall") && transform.position.z >= 12.9f && transform.position.z <= 19.7f)
		{
			MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].SetLayer(RenderingLayer.EVERYTHING);
			}
		}
		else if (HasParentContaining("N:b81dac2e-82ec-4a2f-9d51-f853ac89dec3") && HasParentContaining("Wall 2") && base.gameObject.name.Contains("Wall") && transform.position.z >= 12.9f && transform.position.z <= 19.7f)
		{
			MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].SetLayer(RenderingLayer.EVERYTHING);
			}
		}
		else if (HasParentContaining("B:688be3f2-3d32-483a-9e01-41f27fe370e7") && HasParentContaining("Wall 1") && (base.gameObject.name.Contains("Wall_1Hex_A") || base.gameObject.name.Contains("Wall_2Hex_F") || base.gameObject.name.Contains("Wall_2Hex_G")))
		{
			MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].SetLayer(RenderingLayer.EVERYTHING);
			}
		}
		else if (HasParentContaining("M:aa9dea61-cbd3-4e42-92fa-48adab1fc48c") && (HasParentContaining("Wall 6") || HasParentContaining("Wall 8")) && base.gameObject.name.Contains("Wall") && transform.position.z >= 1.7f && transform.position.z <= 10.1f)
		{
			MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].SetLayer(RenderingLayer.EVERYTHING);
			}
		}
		else if (base.gameObject.name.Contains("FH_13_01x4:bea90e96-5fef-4a0f-bf83-a77b4f2f0e5f"))
		{
			Transform transform6 = transform.Find("Walls/Wall 16_NoSlice_NoFade/Generated Content");
			if (transform6 != null)
			{
				Transform[] componentsInChildren3 = transform6.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform7 in componentsInChildren3)
				{
					if (transform7.name.Contains("Clutter"))
					{
						transform7.gameObject.name += "_TurnedOff";
						transform7.gameObject.SetActive(value: false);
					}
				}
			}
			Transform transform8 = transform.Find("Walls/Wall 5_NoSlice_NoFade/Generated Content");
			if (transform8 != null)
			{
				Transform[] componentsInChildren3 = transform8.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform9 in componentsInChildren3)
				{
					if (transform9.name.Contains("Clutter"))
					{
						transform9.gameObject.name += "_TurnedOff";
						transform9.gameObject.SetActive(value: false);
					}
				}
			}
			Transform transform10 = transform.Find("Walls/Wall 31_NoSlice_NoFade");
			if (transform10 != null)
			{
				Transform[] componentsInChildren3 = transform10.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform11 in componentsInChildren3)
				{
					if (transform11.name.Contains("Scatter"))
					{
						transform11.gameObject.name += "_TurnedOff";
						transform11.gameObject.SetActive(value: false);
					}
				}
			}
		}
		else if (HasParentContaining("N:ed3953dc-b4cd-43cc-9134-f5eab6e08d6d") || HasParentContaining("G:d0381507-474a-410e-a5bd-4638bb29e8cb") || HasParentContaining("H:589a1178-da60-4e39-aef6-a7beb83089ed"))
		{
			if (base.gameObject.name.Contains("Difficult"))
			{
				transform.rotation = Quaternion.identity;
			}
			else if (HasParentContaining("Wall 1") || HasParentContaining("Wall 3") || HasParentContaining("Wall 5"))
			{
				ToggleGrandChildren("Default", wantedState: true);
			}
		}
		else if (base.gameObject.name.Contains("FH_09_01:a005faf2-f05b-4add-ac31-438b9b600bd6") || base.gameObject.name.Contains("B:4a7857b5-2731-4519-a641-a90f3460ebcb"))
		{
			if (Application.isPlaying && !DontTouchMeAgain)
			{
				MaterialModification.MaterialCache sharedCache = new MaterialModification.MaterialCache();
				HashSet<int> hashSet = new HashSet<int>();
				Transform transform12 = transform.Find("Generated Content/Full");
				if (transform12 != null)
				{
					Transform[] componentsInChildren3 = transform12.GetComponentsInChildren<Transform>(includeInactive: true);
					foreach (Transform transform13 in componentsInChildren3)
					{
						if (!transform13.name.Contains("Hex_Fshape"))
						{
							continue;
						}
						MeshRenderer[] componentsInChildren2 = transform13.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
						for (int k = 0; k < componentsInChildren2.Length; k++)
						{
							Material[] materialClonesSlow = componentsInChildren2[k].GetMaterialClonesSlow(sharedCache);
							foreach (Material material in materialClonesSlow)
							{
								if (!material)
								{
									continue;
								}
								int instanceID = material.GetInstanceID();
								if (hashSet.Add(instanceID))
								{
									if (material.HasProperty("_LocalDimAmount"))
									{
										material.SetFloat("_LocalDimAmount", 0.8f);
									}
									if (material.HasProperty("_GlobalHeightDimming") && material.GetFloat("_GlobalHeightDimming") > 0.5f)
									{
										material.SetFloat("_GlobalHeightDimming", 0f);
										material.DisableKeyword("_GLOBALHEIGHTDIMMING_ON");
									}
								}
							}
						}
					}
				}
				Transform transform14 = transform.Find("Walls");
				if (transform14 != null)
				{
					foreach (Transform item9 in transform14)
					{
						if (!item9.name.StartsWith("Wall "))
						{
							continue;
						}
						Transform transform16 = item9.Find("Generated Content");
						if (transform16 == null)
						{
							continue;
						}
						Transform[] componentsInChildren3 = transform16.GetComponentsInChildren<Transform>(includeInactive: true);
						foreach (Transform transform17 in componentsInChildren3)
						{
							if ((!transform17.name.Contains("MR_Wall") && !transform17.name.Contains("MR_Floor") && !transform17.name.Contains("MR_Pillar") && !transform17.name.Contains("Bay")) || transform17.name.Contains("MR_Pillar_1x_G"))
							{
								continue;
							}
							MeshRenderer[] componentsInChildren2 = transform17.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
							for (int k = 0; k < componentsInChildren2.Length; k++)
							{
								Material[] materialClonesSlow = componentsInChildren2[k].GetMaterialClonesSlow(sharedCache);
								foreach (Material material2 in materialClonesSlow)
								{
									if (!material2)
									{
										continue;
									}
									int instanceID2 = material2.GetInstanceID();
									if (hashSet.Add(instanceID2))
									{
										if (material2.HasProperty("_LocalDimAmount"))
										{
											material2.SetFloat("_LocalDimAmount", 0.8f);
										}
										if (material2.HasProperty("_GlobalHeightDimming") && material2.GetFloat("_GlobalHeightDimming") > 0.5f)
										{
											material2.SetFloat("_GlobalHeightDimming", 0f);
											material2.DisableKeyword("_GLOBALHEIGHTDIMMING_ON");
										}
									}
								}
							}
						}
					}
				}
				DontTouchMeAgain = true;
			}
		}
		else if (base.gameObject.name.Contains("M:e9eafc65-abb0-4166-8304-f97d6790e54d") && Application.isPlaying && !DontTouchMeAgain)
		{
			MaterialModification.MaterialCache sharedCache2 = new MaterialModification.MaterialCache();
			HashSet<int> hashSet2 = new HashSet<int>();
			Transform transform18 = transform.Find("Generated Content/Full");
			if (transform18 != null)
			{
				Transform[] componentsInChildren3 = transform18.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform19 in componentsInChildren3)
				{
					if (!transform19.name.Contains("Hex_Fshape"))
					{
						continue;
					}
					MeshRenderer[] componentsInChildren2 = transform19.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
					for (int k = 0; k < componentsInChildren2.Length; k++)
					{
						Material[] materialClonesSlow = componentsInChildren2[k].GetMaterialClonesSlow(sharedCache2);
						foreach (Material material3 in materialClonesSlow)
						{
							if ((bool)material3)
							{
								int instanceID3 = material3.GetInstanceID();
								if (hashSet2.Add(instanceID3) && material3.HasProperty("_Tint"))
								{
									Color value = new Color(0.62f, 0.62f, 0.62f, material3.GetColor("_Tint").a);
									material3.SetColor("_Tint", value);
								}
							}
						}
					}
				}
			}
			Transform transform20 = transform.Find("Walls");
			if (transform20 != null)
			{
				foreach (Transform item10 in transform20)
				{
					if (!item10.name.StartsWith("Wall "))
					{
						continue;
					}
					Transform transform22 = item10.Find("Generated Content");
					if (transform22 == null)
					{
						continue;
					}
					Transform[] componentsInChildren3 = transform22.GetComponentsInChildren<Transform>(includeInactive: true);
					foreach (Transform transform23 in componentsInChildren3)
					{
						if (!transform23.name.Contains("MR_Floor"))
						{
							continue;
						}
						MeshRenderer[] componentsInChildren2 = transform23.GetComponentsInChildren<MeshRenderer>(includeInactive: true);
						for (int k = 0; k < componentsInChildren2.Length; k++)
						{
							Material[] materialClonesSlow = componentsInChildren2[k].GetMaterialClonesSlow(sharedCache2);
							foreach (Material material4 in materialClonesSlow)
							{
								if ((bool)material4)
								{
									int instanceID4 = material4.GetInstanceID();
									if (hashSet2.Add(instanceID4) && material4.HasProperty("_Tint"))
									{
										Color value2 = new Color(0.62f, 0.62f, 0.62f, material4.GetColor("_Tint").a);
										material4.SetColor("_Tint", value2);
									}
								}
							}
						}
					}
				}
			}
			DontTouchMeAgain = true;
		}
		if (!DontTouchMeAgain && base.gameObject.name.Contains("FH_15_Wide:babfa3f6-c334-4f6c-9ac2-d56bd8325e30"))
		{
			Transform transform24 = transform.Find("Walls/Wall 3/Generated Content/Full");
			if (transform24 != null)
			{
				Transform[] componentsInChildren3 = transform24.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform25 in componentsInChildren3)
				{
					if (transform25.name.Contains("MNT_Hex") && IsPositionMatch(transform25.position, new Vector3(8.597825f, -5.66E-07f, 2.9784486f)))
					{
						if (Application.isPlaying)
						{
							transform25.gameObject.name += "_TurnedOff";
							transform25.gameObject.SetActive(value: false);
						}
						break;
					}
				}
			}
			DontTouchMeAgain = true;
		}
		else if (!DontTouchMeAgain && base.gameObject.name.Contains("F:2ee9d42e-4abb-405c-953e-473bc81d3212"))
		{
			Transform transform26 = transform.Find("Walls/Wall 2/Generated Content");
			if (transform26 != null)
			{
				Transform[] componentsInChildren3 = transform26.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform27 in componentsInChildren3)
				{
					if (transform27.name.Contains("PCG_DN_Wall_1x_A") && IsPositionMatch(transform27.position, new Vector3(-10.224754f, 9.44932E-07f, 21.902393f)))
					{
						transform27.position = new Vector3(-10.403723f, 9.44932E-07f, 21.855267f);
						transform27.localScale = new Vector3(1.5f, 1f, 1f);
						if (!transform27.name.Contains("_Adjusted"))
						{
							transform27.name += "_Adjusted";
						}
						break;
					}
				}
			}
			Transform transform28 = transform.Find("Walls/Wall 3/Generated Content");
			if (transform28 != null)
			{
				Transform[] componentsInChildren3 = transform28.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform29 in componentsInChildren3)
				{
					if (transform29.name.Contains("PCG_DN_Wall_2x_A") && IsPositionMatch(transform29.position, new Vector3(-10.228426f, 9.417823E-07f, 23.388647f)))
					{
						transform29.position = new Vector3(-10.220773f, 0.000152576f, 23.400734f);
						if (!transform29.name.Contains("_Moved"))
						{
							transform29.name += "_Moved";
						}
						break;
					}
				}
			}
			foreach (Transform item11 in transform)
			{
				if (!item11.name.Contains("ThinDoor"))
				{
					continue;
				}
				Transform[] componentsInChildren3 = item11.GetComponentsInChildren<Transform>(includeInactive: true);
				foreach (Transform transform31 in componentsInChildren3)
				{
					if (!(transform31.name == "PCG_DN_DoorFrame_A") || !IsPositionMatch(transform31.position, new Vector3(-8.597721f, 0.013955593f, 20.848368f)))
					{
						continue;
					}
					Transform[] componentsInChildren4 = transform31.GetComponentsInChildren<Transform>(includeInactive: true);
					foreach (Transform transform32 in componentsInChildren4)
					{
						if (transform32.name.Contains("p_fire_torch") && IsPositionMatch(transform32.position, new Vector3(-9.88349f, 2.0879555f, 20.90135f)))
						{
							transform32.gameObject.SetActive(value: false);
							if (!transform32.name.Contains("_TurnedOff"))
							{
								transform32.name += "_TurnedOff";
							}
						}
						else if (transform32.name.Contains("Point Light") && IsPositionMatch(transform32.position, new Vector3(-9.994419f, 2.2589555f, 20.725216f)))
						{
							transform32.gameObject.SetActive(value: false);
							if (!transform32.name.Contains("_TurnedOff"))
							{
								transform32.name += "_TurnedOff";
							}
						}
					}
					transform31.position = new Vector3(-8.658343f, 0.013955593f, 20.883368f);
					if (!transform31.name.Contains("_Moved"))
					{
						transform31.name += "_Moved";
					}
					break;
				}
			}
			DontTouchMeAgain = true;
		}
		else if (HasParentContaining("FH_01_11c:2109a0f0-8102-4759-bcc2-d47ffbda716e") && base.gameObject.name.Contains("WFW_DoorFrame") && IsPositionMatch(transform.position, new Vector3(7.67063f, -0.059999943f, 4.390906f)))
		{
			Transform[] componentsInChildren3 = GetComponentsInChildren<Transform>();
			foreach (Transform transform33 in componentsInChildren3)
			{
				if (transform33.name.Trim().ToLower().Contains("right"))
				{
					Renderer[] componentsInChildren5 = transform33.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
					for (int k = 0; k < componentsInChildren5.Length; k++)
					{
						componentsInChildren5[k].enabled = false;
					}
				}
			}
		}
		else if (HasParentContaining("J:57a9d34d-28f9-4bdf-8508-7dcef56f1195") && base.gameObject.name.Contains("WFW_DoorFrame"))
		{
			Transform[] componentsInChildren3 = GetComponentsInChildren<Transform>();
			foreach (Transform transform34 in componentsInChildren3)
			{
				if (transform34.name.Trim().ToLower().Contains("left"))
				{
					Renderer[] componentsInChildren5 = transform34.gameObject.GetComponentsInChildren<Renderer>(includeInactive: true);
					for (int k = 0; k < componentsInChildren5.Length; k++)
					{
						componentsInChildren5[k].enabled = false;
					}
				}
			}
		}
		else if (HasParentContaining("FH_04_06:a80bf37b-abb5-4938-bcd3-5c6466ffd108") && base.gameObject.name.Contains("RF_Light") && !DontTouchMeAgain && transform.position.z > -3f)
		{
			transform.position += new Vector3(0f, 1.9f, 0f);
			DontTouchMeAgain = true;
		}
		else if ((HasParentContaining("N:29aec0b2-98da-4461-a726-c7103c22226d") || HasParentContaining("A:bf9d47ef-a33d-4a80-9e18-66e970872170") || HasParentContaining("A:2fada872-6bef-47a5-bd75-4fc40d596e37") || HasParentContaining("L:94345f57-8128-4451-a5d1-602b119bc3d3")) && base.gameObject.name.Contains("PCG_STE_ABD_Lamp"))
		{
			base.gameObject.SetActive(value: false);
		}
		else if (base.gameObject.name.Contains("MR_Pillar") && Application.isPlaying && !DontTouchMeAgain && (HasParentContaining("B:0f32f5a7-02bc-439b-895a-04603611e4c4") || HasParentContaining("G:316f6d48-faaa-4744-bb75-0112751f60a4") || HasParentContaining("AJ:08d024e5-aa5e-4b42-840f-e1794cb4c25f")))
		{
			GameObject gameObject = null;
			if (IsPositionMatch(transform.position, new Vector3(30.990465f, -0.018576229f, -13.71713f)))
			{
				gameObject = InstantiateDuplicate(transform.position, transform.rotation);
				gameObject.transform.position = new Vector3(29.990465f, -0.018576229f, -13.717475f);
			}
			else if (IsPositionMatch(transform.position, new Vector3(31.114887f, -0.009998032f, -20.533756f)))
			{
				gameObject = InstantiateDuplicate(transform.position, transform.rotation);
				gameObject.transform.position = new Vector3(30.16112f, -0.016f, -20.531979f);
			}
			else if (IsPositionMatch(transform.position, new Vector3(6.878606f, -0.009998032f, -8.306539f)))
			{
				gameObject = InstantiateDuplicate(transform.position, transform.rotation);
				gameObject.transform.position = new Vector3(5.458606f, -0.009998032f, -5.847027f);
			}
			else if (IsPositionMatch(transform.position, new Vector3(12.210327f, -2.1697852E-08f, -6.84927f)))
			{
				gameObject = InstantiateDuplicate(transform.position, transform.rotation);
				gameObject.transform.position = new Vector3(10.048111f, -2.1697852E-08f, -3.1085377f);
			}
			else if (IsPositionMatch(transform.position, new Vector3(28.686943f, -0.018576229f, -19.044592f)))
			{
				gameObject = InstantiateDuplicate(transform.position, transform.rotation);
				gameObject.transform.position = new Vector3(30.073053f, -0.02f, -19.042812f);
			}
			if (gameObject != null)
			{
				gameObject.name = base.gameObject.name + "_DuplicateToFillAGap";
			}
			DontTouchMeAgain = true;
		}
		else if (base.gameObject.name.Contains("_Lamp") && (IsPositionMatch(transform.position, new Vector3(30.216331f, 2.095896f, -14.436626f)) || IsPositionMatch(transform.position, new Vector3(36.4098f, 2.345531f, -13.8356085f)) || IsPositionMatch(transform.position, new Vector3(37.081932f, 2.0824494f, -19.864155f)) || IsPositionMatch(transform.position, new Vector3(30.760168f, 2.0958958f, -20.427675f))))
		{
			if (HasParentContaining("B:0f32f5a7-02bc-439b-895a-04603611e4c4"))
			{
				base.gameObject.name += "_TurnedOff";
				base.gameObject.SetActive(value: false);
			}
		}
		else if (base.gameObject.name.Contains("_Lamp") && !base.gameObject.name.EndsWith("_Duplicate") && Application.isPlaying && !DontTouchMeAgain && (IsPositionMatch(transform.position, new Vector3(23.288567f, 2.062775f, -18.929619f)) || IsPositionMatch(transform.position, new Vector3(5.9097805f, 2.345531f, -5.36516f))))
		{
			GameObject gameObject2 = null;
			if (HasParentContaining("AJ:08d024e5-aa5e-4b42-840f-e1794cb4c25f") && IsPositionMatch(transform.position, new Vector3(23.288567f, 2.062775f, -18.929619f)))
			{
				gameObject2 = InstantiateDuplicate(new Vector3(18.057749f, 2.061498f, -15.580894f), new Quaternion(-6.1035153E-05f, 0.49989384f, 0.00010571611f, 0.8660866f));
			}
			else if (HasParentContaining("J:4bd5a8c4-aa09-422c-9bbd-601aab85623e") && IsPositionMatch(transform.position, new Vector3(5.9097805f, 2.345531f, -5.36516f)))
			{
				gameObject2 = InstantiateDuplicate(new Vector3(9.373882f, 2.345531f, -3.3651605f), transform.rotation);
			}
			if (gameObject2 != null)
			{
				gameObject2.name = base.gameObject.name + "_Duplicate";
			}
			DontTouchMeAgain = true;
		}
		static bool Near(Vector3 a, Vector3 b, float tol = 0.02f)
		{
			return Vector3.SqrMagnitude(a - b) <= tol * tol;
		}
	}

	public static void OnStartPostProcessMapTile(ProceduralMapTile mapTile)
	{
		ScenarioScriptFrontend.OnMapTileGenBeforeLights(mapTile);
		if (mapTile.name.Contains("FH_04_06:a80bf37b-abb5-4938-bcd3-5c6466ffd108"))
		{
			foreach (Transform item in mapTile.transform.Find("Walls"))
			{
				if (!item.name.Contains("_NoSlice_NoFade"))
				{
					item.name += "_NoSlice_NoFade";
				}
				item.GetOrAdd<EnableCameraFade>();
			}
			return;
		}
		if (mapTile.name.Contains("FH_15_10b"))
		{
			foreach (Transform item2 in mapTile.transform.Find("Walls"))
			{
				if (!item2.name.Contains("_NoSlice_NoFade"))
				{
					item2.name += "_NoSlice_NoFade";
				}
				item2.GetOrAdd<EnableCameraFade>();
			}
			return;
		}
		if (mapTile.name.Contains("FH_15_01x2"))
		{
			foreach (Transform item3 in mapTile.transform.Find("Walls"))
			{
				if (!item3.name.Contains("_NoSlice_NoFade"))
				{
					item3.name += "_NoSlice_NoFade";
				}
				item3.GetOrAdd<EnableCameraFade>();
			}
			return;
		}
		if (mapTile.name.Contains("LN:edd31b61-de28-4aee-921d-8b0ebb00a929"))
		{
			foreach (Transform item4 in mapTile.transform.Find("Walls"))
			{
				if (item4.name == "Wall 3")
				{
					item4.transform.position = new Vector3(-2.894785f, 0f, 12.085848f);
				}
			}
			return;
		}
		if (!mapTile.name.Contains("FH_13_01x4:bea90e96-5fef-4a0f-bf83-a77b4f2f0e5f"))
		{
			return;
		}
		foreach (Transform item5 in mapTile.transform.Find("Walls"))
		{
			if (!(item5.name == "Wall 1") && !(item5.name == "Wall 2") && !(item5.name == "Wall 18") && !(item5.name == "Wall 34"))
			{
				if (!item5.name.Contains("_NoSlice_NoFade"))
				{
					item5.name += "_NoSlice_NoFade";
				}
				item5.GetOrAdd<EnableCameraFade>();
			}
		}
	}

	private GameObject InstantiateDuplicate(Vector3 dstPos, Quaternion dstRot)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(base.gameObject, dstPos, dstRot, transform.parent);
		SceneUtil.SmartDestroy(gameObject.GetComponent<PropSpecialCaseAdjuster2000>());
		MaterialModification[] componentsInChildren = gameObject.GetComponentsInChildren<MaterialModification>();
		if (componentsInChildren.Length == 1)
		{
			MaterialModification componentInChildren = base.gameObject.GetComponentInChildren<MaterialModification>();
			if (componentInChildren != null)
			{
				componentsInChildren[0].OnDuplicate(componentInChildren);
			}
		}
		return gameObject;
	}

	private GameObject InstantiateDuplicateFromRoom(GameObject source, Vector3 dstPos, Quaternion dstRot, Transform parentOverride = null)
	{
		Transform parent = ((parentOverride != null) ? parentOverride : source.transform.parent);
		GameObject gameObject = UnityEngine.Object.Instantiate(source, dstPos, dstRot, parent);
		SceneUtil.SmartDestroy(gameObject.GetComponent<PropSpecialCaseAdjuster2000>());
		MaterialModification[] componentsInChildren = gameObject.GetComponentsInChildren<MaterialModification>();
		if (componentsInChildren.Length == 1)
		{
			MaterialModification componentInChildren = source.GetComponentInChildren<MaterialModification>();
			if (componentInChildren != null)
			{
				componentsInChildren[0].OnDuplicate(componentInChildren);
			}
		}
		return gameObject;
	}

	private void SetAllMeshRenderersOf(GameObject obj, bool wantedState)
	{
		Transform[] componentsInChildren = obj.GetComponentsInChildren<Transform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			MeshRenderer component = componentsInChildren[i].GetComponent<MeshRenderer>();
			if (component != null)
			{
				component.enabled = wantedState;
			}
		}
	}

	private void ToggleChildren(string childName, bool wantedState)
	{
		Transform transform = base.transform.Find(childName);
		if (transform != null && transform.gameObject.activeSelf != wantedState)
		{
			transform.gameObject.SetActive(wantedState);
		}
	}

	private void ToggleGrandChildren(string childName, bool wantedState)
	{
		Transform transform = base.transform.Find(childName);
		if (!(transform != null))
		{
			return;
		}
		foreach (Transform item in transform)
		{
			item.gameObject.SetActive(wantedState);
		}
	}

	private void SwapGrandChildren(string swapper, string swappee)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Transform transform = base.transform.Find(swapper);
		Transform transform2 = base.transform.Find(swappee);
		if (transform == null || transform2 == null)
		{
			return;
		}
		List<Transform> list = new List<Transform>();
		foreach (Transform item3 in transform)
		{
			list.Add(item3);
		}
		List<Transform> list2 = new List<Transform>();
		foreach (Transform item4 in transform2)
		{
			list2.Add(item4);
		}
		foreach (Transform item5 in list)
		{
			item5.SetParent(null, worldPositionStays: true);
		}
		foreach (Transform item6 in list2)
		{
			item6.SetParent(null, worldPositionStays: true);
		}
		foreach (Transform item7 in list2)
		{
			item7.SetParent(transform, worldPositionStays: true);
		}
		foreach (Transform item8 in list)
		{
			item8.SetParent(transform2, worldPositionStays: true);
		}
	}

	private bool HasParentContaining(string keyword)
	{
		Transform parent = transform.parent;
		while (parent != null)
		{
			if (parent.name.Contains(keyword))
			{
				return true;
			}
			parent = parent.parent;
		}
		return false;
	}

	private Transform GetParentContaining(string namePart)
	{
		Transform parent = transform.parent;
		while (parent != null)
		{
			if (parent.name.Contains(namePart))
			{
				return parent;
			}
			parent = parent.parent;
		}
		return null;
	}

	private bool IsPositionMatch(Vector3 position, Vector3 targetPosition)
	{
		return Approximately(position, targetPosition);
	}

	private bool IsRotationMatch(Quaternion rotation, Quaternion targetRotation)
	{
		return Approximately(rotation, targetRotation);
	}

	private bool IsPositionWithinRange(Vector3 position, Vector3 targetPosition, float range)
	{
		if (Mathf.Abs(position.x - targetPosition.x) <= range && Mathf.Abs(position.y - targetPosition.y) <= range)
		{
			return Mathf.Abs(position.z - targetPosition.z) <= range;
		}
		return false;
	}

	private bool Approximately(Vector3 a, Vector3 b)
	{
		if (Mathf.Abs(a.x - b.x) < 0.1f && Mathf.Abs(a.y - b.y) < 0.1f)
		{
			return Mathf.Abs(a.z - b.z) < 0.1f;
		}
		return false;
	}

	private bool Approximately(Quaternion a, Quaternion b)
	{
		if (Mathf.Abs(a.x - b.x) < 0.1f && Mathf.Abs(a.y - b.y) < 0.1f && Mathf.Abs(a.z - b.z) < 0.1f)
		{
			return Mathf.Abs(a.w - b.w) < 0.1f;
		}
		return false;
	}

	private void DeclutterRooms(string clutterName, float closeDist, float sameNameDist)
	{
		List<Transform> list = new List<Transform>();
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(base.transform);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			if (transform.name == "Generated Content")
			{
				foreach (Transform item2 in transform)
				{
					if (item2.gameObject.name.IndexOf(clutterName, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						list.Add(item2);
					}
				}
				Transform transform3 = null;
				foreach (Transform item3 in transform)
				{
					if (item3.name == "Full")
					{
						transform3 = item3;
						break;
					}
				}
				if (!(transform3 != null))
				{
					continue;
				}
				foreach (Transform item4 in transform3)
				{
					if (item4.gameObject.name.IndexOf(clutterName, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						list.Add(item4);
					}
				}
				continue;
			}
			foreach (Transform item5 in transform)
			{
				stack.Push(item5);
			}
		}
		list.Sort((Transform a, Transform b) =>
		{
			int num10 = string.CompareOrdinal(a.gameObject.name, b.gameObject.name);
			if (num10 != 0)
			{
				return num10;
			}
			int num11 = a.position.x.CompareTo(b.position.x);
			return (num11 != 0) ? num11 : a.position.z.CompareTo(b.position.z);
		});
		for (int num = 0; num < list.Count; num++)
		{
			Transform transform6 = list[num];
			if ((bool)transform6 && transform6.gameObject.activeSelf)
			{
				Vector3 position = transform6.position;
				position.y = 0.025f;
				transform6.SetPositionAndRotation(position, transform6.rotation);
			}
		}
		float num2 = closeDist * closeDist;
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			Transform transform7 = list[num3];
			if (!transform7 || !transform7.gameObject.activeSelf)
			{
				continue;
			}
			for (int num4 = num3 + 1; num4 < list.Count; num4++)
			{
				Transform transform8 = list[num4];
				if ((bool)transform8 && transform8.gameObject.activeSelf && PlanarDistSqr(transform7.position, transform8.position) <= num2)
				{
					transform8.gameObject.name += "_TurnedOffForBeingTooClose";
					transform8.gameObject.SetActive(value: false);
				}
			}
		}
		list.RemoveAll((Transform t) => !t || !t.gameObject.activeSelf);
		float num5 = sameNameDist * sameNameDist;
		for (int num6 = 0; num6 < list.Count; num6++)
		{
			Transform transform9 = list[num6];
			if (!transform9 || !transform9.gameObject.activeSelf)
			{
				continue;
			}
			string text = transform9.gameObject.name;
			for (int num7 = num6 + 1; num7 < list.Count; num7++)
			{
				Transform transform10 = list[num7];
				if ((bool)transform10 && transform10.gameObject.activeSelf && PlanarDistSqr(transform9.position, transform10.position) <= num5 && transform10.gameObject.name == text)
				{
					transform10.gameObject.name += "_TurnedOffForBeingRedundant";
					transform10.gameObject.SetActive(value: false);
				}
			}
		}
		list.RemoveAll((Transform t) => !t || !t.gameObject.activeSelf);
		for (int num8 = 0; num8 < list.Count; num8++)
		{
			Transform transform11 = list[num8];
			if (!transform11 || !transform11.gameObject.activeSelf)
			{
				continue;
			}
			Renderer renderer = Rndr(transform11);
			if (!renderer)
			{
				continue;
			}
			Bounds bounds = renderer.bounds;
			for (int num9 = num8 + 1; num9 < list.Count; num9++)
			{
				Transform transform12 = list[num9];
				if (!transform12 || !transform12.gameObject.activeSelf || PlanarDistSqr(transform11.position, transform12.position) > num5)
				{
					continue;
				}
				Renderer renderer2 = Rndr(transform12);
				if (!renderer2)
				{
					continue;
				}
				Bounds bounds2 = renderer2.bounds;
				if (AABB_Overlap_XZ(bounds, bounds2))
				{
					transform11.SetPositionAndRotation(new Vector3(transform11.position.x, 0.025f, transform11.position.z), Quaternion.LookRotation(Vector3.forward));
					transform12.SetPositionAndRotation(new Vector3(transform12.position.x, 0.025f, transform12.position.z), Quaternion.LookRotation(Vector3.back));
					transform11.gameObject.name += "_Rotated";
					transform12.gameObject.name += "_Rotated";
					renderer = Rndr(transform11);
					if (renderer != null)
					{
						bounds = renderer.bounds;
					}
				}
			}
		}
	}

	private static Renderer Rndr(Transform t)
	{
		if (!t)
		{
			return null;
		}
		return t.GetComponent<Renderer>();
	}

	private static float PlanarDistSqr(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return num * num + num2 * num2;
	}

	private static bool AABB_Overlap_XZ(Bounds a, Bounds b)
	{
		bool flag = Mathf.Abs(a.center.x - b.center.x) <= a.extents.x + b.extents.x;
		bool flag2 = Mathf.Abs(a.center.z - b.center.z) <= a.extents.z + b.extents.z;
		return flag & flag2;
	}
}
