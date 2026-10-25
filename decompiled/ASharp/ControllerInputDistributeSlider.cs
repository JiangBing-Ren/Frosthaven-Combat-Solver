using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerInputDistributeSlider : MonoBehaviour, IMoveHandler, IEventSystemHandler
{
	[SerializeField]
	private Selectable addPointButton;

	[SerializeField]
	private GUIAnimator addPointGlow;

	[SerializeField]
	private Selectable removePointButton;

	[SerializeField]
	private GUIAnimator removePointGlow;

	public void OnMove(AxisEventData eventData)
	{
		switch (eventData.moveDir)
		{
		case MoveDirection.Right:
			if (addPointButton != null && addPointButton.IsInteractable())
			{
				ExecuteEvents.Execute(addPointButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
				addPointGlow.Play();
			}
			break;
		case MoveDirection.Left:
			if (removePointButton != null && removePointButton.IsInteractable())
			{
				ExecuteEvents.Execute(removePointButton.gameObject, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);
				removePointGlow.Play();
			}
			break;
		}
	}

	private void OnDisable()
	{
		addPointGlow.Stop();
		removePointGlow.Stop();
	}
}
