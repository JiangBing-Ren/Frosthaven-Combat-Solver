using UnityEngine;
using UnityEngine.EventSystems;

internal class FullCardHandHighlighter : MonoBehaviour
{
	[SerializeField]
	private float _highlightScale = 1.1f;

	private GameObject _lastSelectedGameObject;

	private void OnEnable()
	{
		SetHighlightScale(_highlightScale);
	}

	private void OnDisable()
	{
		SetHighlightScale(1f);
	}

	private void SetHighlightScale(float highlightScale)
	{
		FullAbilityCard[] componentsInChildren = GetComponentsInChildren<FullAbilityCard>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].HighlightScale = highlightScale;
		}
	}

	private void Update()
	{
		if (!Singleton<InputFHManager>.Instance.UsingGamepad)
		{
			_lastSelectedGameObject = null;
		}
		else if (!(_lastSelectedGameObject == EventSystem.current.currentSelectedGameObject))
		{
			TryExecuteEvent(_lastSelectedGameObject, ExecuteEvents.pointerExitHandler);
			_lastSelectedGameObject = EventSystem.current.currentSelectedGameObject;
			TryExecuteEvent(_lastSelectedGameObject, ExecuteEvents.pointerEnterHandler);
		}
	}

	private static void TryExecuteEvent<T>(GameObject target, ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
	{
		if (target != null && target.TryGetComponent<AbilityCardUI>(out var component))
		{
			ExecuteEvents.Execute(component.fullAbilityCard.gameObject, new PointerEventData(EventSystem.current), functor);
		}
	}
}
