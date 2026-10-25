using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using I2.Loc;
using Libs.Build;
using Libs.Utils.Profiler;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MF
{
	public struct SBBox2D
	{
		public float minx;

		public float maxx;

		public float miny;

		public float maxy;
	}

	public static bool EnableYieldIfOverBudget = true;

	public static void SetVersion(TextMeshProUGUI version)
	{
		version.text = string.Format(LocalizationManager.GetTranslation("GUI_GAME_VERSION_WIP"), RuntimeBuildInfo.UserVersion);
		Log.Info(version.text);
	}

	public static Vector3 SnapVector3ToHexes(Vector3 position)
	{
		Vector3 vector = position;
		vector.z = SnapFloat(UnityGameEditorRuntime.s_TileSize.z, vector.z);
		if ((Mathf.Abs(GetTileIntegerSnapSpace(vector).z) & 1) == 1)
		{
			vector.x = SnapFloat(UnityGameEditorRuntime.s_TileSize.x, vector.x - UnityGameEditorRuntime.s_TileSize.x * 0.5f) + UnityGameEditorRuntime.s_TileSize.x * 0.5f;
		}
		else
		{
			vector.x = SnapFloat(UnityGameEditorRuntime.s_TileSize.x, vector.x);
		}
		vector.y = 0f;
		return vector;
	}

	public static Vector3Int GetTileIntegerSnapSpace(Vector3 snappedPosition)
	{
		int num = (int)(Mathf.Abs(snappedPosition.z) / UnityGameEditorRuntime.s_TileSize.z + 0.25f) * (int)Mathf.Sign(snappedPosition.z);
		return new Vector3Int(((num & 1) == 1) ? ((int)(Mathf.Abs(snappedPosition.x - UnityGameEditorRuntime.s_TileSize.x * 0.5f) / UnityGameEditorRuntime.s_TileSize.x + 0.25f) * (int)Mathf.Sign(snappedPosition.x)) : ((int)(Mathf.Abs(snappedPosition.x) / UnityGameEditorRuntime.s_TileSize.x + 0.25f) * (int)Mathf.Sign(snappedPosition.x)), 0, num);
	}

	public static void SerialiseOutVector3(BinaryWriter writer, Vector3 vector)
	{
		writer.Write(vector.x);
		writer.Write(vector.y);
		writer.Write(vector.z);
	}

	public static void SerialiseInVector3(BinaryReader reader, out Vector3 vector)
	{
		vector.x = reader.ReadSingle();
		vector.y = reader.ReadSingle();
		vector.z = reader.ReadSingle();
	}

	public static bool GameObjectAnimatorInTransition(GameObject gameObject)
	{
		Animator gameObjectAnimator = GetGameObjectAnimator(gameObject);
		if (gameObjectAnimator != null)
		{
			return gameObjectAnimator.IsInTransition(0);
		}
		return false;
	}

	public static bool GameObjectAnimatorPlay(GameObject gameObject, string state)
	{
		return AnimatorPlay(GetGameObjectAnimator(gameObject), state);
	}

	public static bool AnimatorPlay(Animator animator, string state)
	{
		if (animator != null && animator.runtimeAnimatorController != null && animator.HasState(0, Animator.StringToHash(state)))
		{
			List<string> list = new List<string>();
			list.Add(state);
			for (int i = 1; i < 3; i++)
			{
				string text = state + "-" + i;
				if (animator.HasState(0, Animator.StringToHash(text)))
				{
					list.Add(text);
				}
			}
			animator.enabled = true;
			string stateName = list[UnityEngine.Random.Range(0, list.Count)];
			animator.Play(stateName);
			return true;
		}
		return false;
	}

	public static void GameObjectAnimatorRemoveEvents(GameObject gameObject, string eventName)
	{
		Animator gameObjectAnimator = GetGameObjectAnimator(gameObject);
		if (!(gameObjectAnimator != null) || !(gameObjectAnimator.runtimeAnimatorController != null))
		{
			return;
		}
		AnimationClip[] animationClips = gameObjectAnimator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			List<AnimationEvent> list = new List<AnimationEvent>();
			AnimationEvent[] events = animationClip.events;
			foreach (AnimationEvent animationEvent in events)
			{
				if (animationEvent.functionName.GetHashCode() != eventName.GetHashCode())
				{
					list.Add(animationEvent);
				}
			}
			animationClip.events = list.ToArray();
		}
	}

	public static void GameObjectAnimatorInvalidateEvents(GameObject gameObject, string eventName)
	{
		Animator gameObjectAnimator = GetGameObjectAnimator(gameObject);
		if (!(gameObjectAnimator != null) || !(gameObjectAnimator.runtimeAnimatorController != null))
		{
			return;
		}
		List<AnimationEvent> list = new List<AnimationEvent>();
		AnimationClip[] animationClips = gameObjectAnimator.runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			AnimationEvent[] events = animationClip.events;
			foreach (AnimationEvent animationEvent in events)
			{
				if (animationEvent.functionName.GetHashCode() == eventName.GetHashCode())
				{
					animationEvent.intParameter = -1;
					list.Add(animationEvent);
				}
			}
			animationClip.events = list.ToArray();
		}
	}

	public static Animator GetGameObjectAnimator(GameObject gameObject)
	{
		Animator[] componentsInChildren = gameObject.GetComponentsInChildren<Animator>();
		foreach (Animator animator in componentsInChildren)
		{
			if (animator.runtimeAnimatorController != null)
			{
				return animator;
			}
		}
		return null;
	}

	public static RuntimeAnimatorController GetGameObjectAnimatorController(GameObject gameObject)
	{
		Animator animator = gameObject?.GetComponentInChildren<Animator>();
		if (animator != null)
		{
			return animator.runtimeAnimatorController;
		}
		return null;
	}

	public static bool HasValidAnimatorAndController(GameObject gameObject)
	{
		Animator componentInChildren = gameObject.GetComponentInChildren<Animator>();
		if (componentInChildren != null)
		{
			return componentInChildren.runtimeAnimatorController != null;
		}
		return false;
	}

	public static bool AnimatorControllerHasParameter(string paramName, Animator animator)
	{
		AnimatorControllerParameter[] parameters = animator.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == paramName)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnimatorControllerIsCurrentState(Animator animator, string state)
	{
		return animator.GetCurrentAnimatorStateInfo(0).IsName(state);
	}

	public static bool GameObjectAnimatorControllerIsCurrentState(GameObject gameObject, string state)
	{
		Animator gameObjectAnimator = GetGameObjectAnimator(gameObject);
		if (gameObjectAnimator != null && gameObjectAnimator.HasState(0, Animator.StringToHash(state)) && gameObjectAnimator.runtimeAnimatorController != null)
		{
			return gameObjectAnimator.GetCurrentAnimatorStateInfo(0).IsName(state);
		}
		return false;
	}

	public static bool GameObjectAnimatorControllerIsCurrentState(GameObject gameObject, int stateFullPathHash)
	{
		Animator gameObjectAnimator = GetGameObjectAnimator(gameObject);
		if (gameObjectAnimator != null && gameObjectAnimator.runtimeAnimatorController != null)
		{
			return gameObjectAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == stateFullPathHash;
		}
		return false;
	}

	public static bool AnimatorHasParam(Animator animator, string param)
	{
		AnimatorControllerParameter[] parameters = animator.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == param)
			{
				return true;
			}
		}
		return false;
	}

	public static bool GameObjectAnimatorControllerHasState(GameObject gameObject, string state)
	{
		Animator gameObjectAnimator = GetGameObjectAnimator(gameObject);
		if (gameObjectAnimator != null)
		{
			return gameObjectAnimator.HasState(0, Animator.StringToHash(state));
		}
		return false;
	}

	public static List<T> GameObjectAnimatorStateBehaviours<T>(GameObject gameObject) where T : StateMachineBehaviour
	{
		Animator gameObjectAnimator = GetGameObjectAnimator(gameObject);
		if (gameObjectAnimator != null && gameObjectAnimator.runtimeAnimatorController != null)
		{
			return gameObjectAnimator.GetBehaviours<T>().ToList();
		}
		return new List<T>();
	}

	public static float SnapFloat(float snapsize, float val)
	{
		return snapsize * (float)(int)(Mathf.Abs(val) / snapsize + 0.5f) * Mathf.Sign(val);
	}

	public static void WriteNextChunk(BinaryWriter writer, string chunk)
	{
		uint value = ((uint)chunk[3] << 24) | ((uint)chunk[2] << 16) | ((uint)chunk[1] << 8) | chunk[0];
		writer.Write(value);
	}

	public static bool FindNextChunk(BinaryReader reader, string chunk)
	{
		uint num = ((uint)chunk[3] << 24) | ((uint)chunk[2] << 16) | ((uint)chunk[1] << 8) | chunk[0];
		while (reader.ReadUInt32() != num)
		{
		}
		return true;
	}

	public static bool IsNextChunk(BinaryReader reader, string chunk)
	{
		if (reader.BaseStream.Position > reader.BaseStream.Length - 4)
		{
			return false;
		}
		uint num = ((uint)chunk[3] << 24) | ((uint)chunk[2] << 16) | ((uint)chunk[1] << 8) | chunk[0];
		if (reader.ReadUInt32() == num)
		{
			return true;
		}
		reader.BaseStream.Seek(-4L, SeekOrigin.Current);
		return false;
	}

	public static Hash128 CreateUniquePersistantID(GameObject go)
	{
		return Hash128.Parse(SceneManager.GetActiveScene().name + "#" + $"{go.transform.position.x:0.00} {go.transform.position.y:0.00} {go.transform.position.z:0.00}" + "#" + go.transform.rotation.eulerAngles.ToString());
	}

	public static float Get2DAngleFromCenter(Vector2 position, Vector2 center, out float length)
	{
		Vector2 rhs = position - center;
		length = rhs.magnitude;
		rhs *= 1f / length;
		float f = Vector2.Dot(Vector2.up, rhs);
		f = Mathf.Acos(f) * 57.29578f;
		return (rhs.x < 0f) ? (360f - f) : f;
	}

	public static float Round2DP(float f)
	{
		return (float)Mathf.RoundToInt(f * 100f) / 100f;
	}

	public static bool Approx(float a, float b, float threshold)
	{
		return ((a < b) ? (b - a) : (a - b)) <= threshold;
	}

	public static GameObject FindChildWithName(GameObject go, string name)
	{
		foreach (Transform item in go.transform)
		{
			GameObject gameObject = item.gameObject;
			if (gameObject.name == name)
			{
				return gameObject;
			}
			GameObject result;
			if ((bool)(result = FindChildWithName(gameObject, name)))
			{
				return result;
			}
		}
		return null;
	}

	public static void SetLayerRecursively(GameObject go, LayerMask mask, Type exclude = null)
	{
		go.layer = mask;
		foreach (Transform item in go.transform)
		{
			if (exclude == null || item.GetComponent(exclude) == null)
			{
				SetLayerRecursively(item.gameObject, mask, exclude);
			}
		}
	}

	public static Bounds GetColliderBounds(GameObject go)
	{
		Bounds result = default;
		Collider[] componentsInChildren = go.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (result.extents == Vector3.zero)
			{
				result.center = collider.transform.position;
			}
			result.Encapsulate(collider.bounds);
		}
		return result;
	}

	public static Bounds GetColliderAndMeshBounds(GameObject go)
	{
		Bounds result = default;
		Collider[] componentsInChildren = go.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (result.extents == Vector3.zero)
			{
				result.center = collider.transform.position;
			}
			result.Encapsulate(collider.bounds);
		}
		MeshRenderer[] componentsInChildren2 = go.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren2)
		{
			if (result.extents == Vector3.zero)
			{
				result.center = meshRenderer.transform.position;
			}
			result.Encapsulate(meshRenderer.bounds);
		}
		return result;
	}

	public static Bounds GetMeshBounds(GameObject go)
	{
		Bounds result = default;
		MeshRenderer[] componentsInChildren = go.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer meshRenderer in componentsInChildren)
		{
			if (result.extents == Vector3.zero)
			{
				result.center = meshRenderer.transform.position;
			}
			result.Encapsulate(meshRenderer.bounds);
		}
		return result;
	}

	public static void EnableRenderers(GameObject go, bool enable)
	{
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = enable;
		}
	}

	public static GameObject FindGameObjectRootAtMousePosition(out GameObject actual_go)
	{
		actual_go = null;
		Vector2 cursorPosition = InputFHManager.CursorPosition;
		if ((Singleton<InputFHManager>.InstanceFast == null || !Singleton<InputFHManager>.InstanceFast.UsingGamepad) && (cursorPosition.x < 0f || cursorPosition.y < 0f || cursorPosition.x > (float)Screen.width || cursorPosition.y > (float)Screen.height))
		{
			return null;
		}
		Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
		RaycastHit hitInfo = default;
		if (Physics.Raycast(ray, out hitInfo, 1000f, -1))
		{
			actual_go = hitInfo.collider.gameObject;
			return hitInfo.collider.transform.root.gameObject;
		}
		return null;
	}

	public static CInteractable FindInteractableAtMousePosition(bool ignoreinteractableviaguiflag, LayerMask gameSelectionRaycastLayer)
	{
		if (Camera.main == null)
		{
			return null;
		}
		Vector2 cursorPosition = InputFHManager.CursorPosition;
		if ((Singleton<InputFHManager>.InstanceFast == null || !Singleton<InputFHManager>.InstanceFast.UsingGamepad) && (cursorPosition.x < 0f || cursorPosition.y < 0f || cursorPosition.x > (float)Screen.width || cursorPosition.y > (float)Screen.height))
		{
			return null;
		}
		CInteractable result = null;
		Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
		RaycastHit hitInfo = default;
		if (Physics.Raycast(ray, out hitInfo, 1000f, gameSelectionRaycastLayer))
		{
			result = hitInfo.transform.gameObject.GetComponentInParent<CInteractable>();
		}
		return result;
	}

	public static Vector3 FindGroundPointAtMousePosition(Plane groundPlane)
	{
		if (Camera.main == null)
		{
			return Vector3.zero;
		}
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0f;
		if (groundPlane.Raycast(ray, out enter))
		{
			return ray.GetPoint(enter);
		}
		return Vector3.zero;
	}

	public static CInteractable FindNearestInteractableToPosition(bool ignoreinteractableviaguiflag, LayerMask gameSelectionRaycastLayer, Vector3 position)
	{
		CInteractable result = null;
		Ray ray = Camera.main.ScreenPointToRay(position);
		RaycastHit hitInfo = default;
		if (Physics.Raycast(ray, out hitInfo, 1000f, gameSelectionRaycastLayer))
		{
			result = hitInfo.collider.transform.root.gameObject.GetComponent<CInteractable>();
		}
		return result;
	}

	public static string RenameToInstanceIDAndGetGameObjectPath(GameObject obj)
	{
		obj.name = obj.GetInstanceID().ToString();
		string text = "/" + obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;
			text = "/" + obj.name + text;
		}
		return text;
	}

	public static Transform GetChildTransform(Transform parent, int index)
	{
		if (index >= parent.childCount)
		{
			return null;
		}
		return parent.GetChild(index);
	}

	public static async Task YieldIfOverBudget(string awaitScopeName = null, float maxElapsedSinceFrameStart = 0.05f)
	{
		if (!EnableYieldIfOverBudget)
		{
			return;
		}
		double unscaledTimeAsDouble = Time.unscaledTimeAsDouble;
		if ((float)(Time.realtimeSinceStartupAsDouble - unscaledTimeAsDouble) < maxElapsedSinceFrameStart)
		{
			return;
		}
		using (new MiniProfilerScope(awaitScopeName ?? "await in YieldIfOverBudget"))
		{
			await Task.Yield();
		}
	}
}
