using System.Collections.Generic;
using System.Linq;
using FH.ApparanceBaking;
using FH.LevelComponents.ProcGen;
using Libs.Utils;
using SETUtil;
using UnityEngine;
using UnityEngine.Serialization;

public class PropParent : MonoBehaviour, IContentUpdateMonitor
{
	[SerializeField]
	[FormerlySerializedAs("PlacedInScenario")]
	private bool _placedInScenario = true;

	private readonly HashSet<Renderer> _renderers = new HashSet<Renderer>();

	private bool _visible = true;

	private bool _visibilityReady;

	private readonly List<GameObject> _placeholders = new List<GameObject>();

	public bool PlacedInScenario => _placedInScenario;

	public int PlaceholderCount => _placeholders.Count;

	public void Init(bool placedBeforehand)
	{
		_placedInScenario = placedBeforehand;
	}

	public void SetPropLighting(bool visible, Ambience ambience)
	{
		if (!(ambience == null))
		{
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SetLayer(ambience.LayerFromVisibility(visible));
			}
			Light[] componentsInChildren2 = GetComponentsInChildren<Light>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].SetLayer(ambience.LayerFromVisibility(visible));
			}
			ambience.SetReflectionsTo(transform);
		}
	}

	private void PrepareVisibilityControl()
	{
		if ((bool)this && TryGetComponent<ApparanceEntity>(out var component))
		{
			component.Frozen = true;
		}
		bool flag = PlaceholderCount > 0;
		Transform transform = base.transform.FindTransformInChildren("Generated Content");
		if ((object)transform == null)
		{
			transform = base.transform;
		}
		if (!flag || !_placeholders.Contains(transform.gameObject))
		{
			Renderer[] components = transform.GetComponents<Renderer>();
			foreach (Renderer renderer in components)
			{
				if (renderer.enabled)
				{
					_renderers.Add(renderer);
				}
			}
		}
		foreach (Transform item in transform)
		{
			if (flag && _placeholders.Contains(item.gameObject))
			{
				continue;
			}
			Renderer[] components = item.GetComponentsInChildren<Renderer>(includeInactive: true);
			foreach (Renderer renderer2 in components)
			{
				if (renderer2.enabled)
				{
					_renderers.Add(renderer2);
				}
			}
		}
		_visibilityReady = true;
	}

	public GameObject AddPlaceholder(GameObject placeholderReference, Vector3 position, Quaternion rotation)
	{
		if (placeholderReference == null)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(placeholderReference, position, rotation, transform.parent);
		gameObject.name = $"Placeholder {_placeholders.Count} of {base.gameObject.name}";
		gameObject.SetActive(!_visible);
		_placeholders.Add(gameObject);
		return gameObject;
	}

	public void RemoveAllPlaceholders()
	{
		foreach (GameObject placeholder in _placeholders)
		{
			SceneUtil.SmartDestroy(placeholder);
		}
		_placeholders.Clear();
	}

	public IEnumerable<Renderer> GetPlaceholderRenderers()
	{
		return _placeholders.Where((GameObject x) => x != null).SelectMany((GameObject x) => x.GetComponentsInChildren<Renderer>(includeInactive: false));
	}

	public void SetPropVisibility(bool visible, Ambience ambience)
	{
		if (!_visibilityReady)
		{
			PrepareVisibilityControl();
		}
		else
		{
			_renderers.RemoveWhere((Renderer r) => r == null);
		}
		foreach (Renderer renderer in _renderers)
		{
			renderer.enabled = visible;
		}
		_visible = visible;
		_placeholders.Where((GameObject x) => x != null).ForEach((GameObject x) =>
		{
			x.SetActive(!visible);
		});
		SetPropLighting(visible, ambience);
	}

	public void SetPropHoverable(bool hoverable)
	{
		if (TryGetComponent<Collider>(out var component))
		{
			component.enabled = hoverable;
		}
	}

	public void NotifyContentPlacementComplete()
	{
		EnvironmentLoader.ProceduralContentCreated(this);
		PrepareVisibilityControl();
	}

	public void NotifyContentPlacementStarted()
	{
	}

	public void NotifyContentRemovalStarted()
	{
	}

	public void NotifyContentRemovalComplete()
	{
	}
}
