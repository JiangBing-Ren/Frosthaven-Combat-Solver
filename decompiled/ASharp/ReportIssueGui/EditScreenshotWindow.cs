using Libs.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ReportIssueGui;

public class EditScreenshotWindow : MonoBehaviour
{
	public int DrawLineWidth = 2;

	[Bind]
	private RawImage _screenshotRawImage;

	public Texture2D OriginalScreenshotTexture;

	private Texture2D _screenshotTexture;

	private Vector2 _imageRectToTextureRectRatio;

	private Vector2 _imageRectDimensions;

	private Vector2? _prevDragTexel;

	public bool Visible
	{
		get
		{
			return gameObject.activeSelf;
		}
		set
		{
			if (value != Visible)
			{
				gameObject.SetActive(value);
			}
		}
	}

	public Texture2D ScreenshotTexture
	{
		get
		{
			return _screenshotTexture;
		}
		set
		{
			_screenshotTexture = value;
			_screenshotRawImage.texture = _screenshotTexture;
			Canvas componentInParent = GetComponentInParent<Canvas>();
			RectTransform rectTransform = _screenshotRawImage.rectTransform;
			_imageRectDimensions = new Vector2(rectTransform.rect.width * componentInParent.scaleFactor, rectTransform.rect.height * componentInParent.scaleFactor);
			_imageRectToTextureRectRatio = new Vector2((float)_screenshotTexture.width / _imageRectDimensions.x, (float)_screenshotTexture.height / _imageRectDimensions.y);
		}
	}

	private void Awake()
	{
		Binder.BindFields(this);
		EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
		trigger.AddCallback(EventTriggerType.BeginDrag, OnBeginDrag);
		trigger.AddCallback(EventTriggerType.Drag, OnDrag);
		trigger.AddCallback(EventTriggerType.PointerClick, OnClick);
		trigger.AddCallback(EventTriggerType.PointerDown, OnButtonDown);
		trigger.AddCallback(EventTriggerType.Scroll, OnScroll);
	}

	private void OnScroll(BaseEventData ev)
	{
		Debug.Log(((PointerEventData)ev).scrollDelta);
	}

	private void OnButtonDown(BaseEventData ev)
	{
		if (((PointerEventData)ev).button == PointerEventData.InputButton.Right)
		{
			Graphics.CopyTexture(OriginalScreenshotTexture, _screenshotTexture);
		}
	}

	private void OnClick(BaseEventData ev)
	{
		PointerEventData pointerEventData = (PointerEventData)ev;
		if (pointerEventData.button == PointerEventData.InputButton.Left && pointerEventData.clickCount == 2)
		{
			Visible = false;
		}
	}

	private void OnBeginDrag(BaseEventData ev)
	{
		((PointerEventData)ev).eligibleForClick = false;
		_prevDragTexel = null;
	}

	private void OnEndDrag(BaseEventData ev)
	{
		Debug.Log("OnEndDrag " + ev);
	}

	private void OnDrag(BaseEventData ev)
	{
		PointerEventData pointerEventData = (PointerEventData)ev;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(_screenshotRawImage.rectTransform, pointerEventData.position, null, out var localPoint);
		localPoint += _imageRectDimensions / 2f;
		Vector2 value = new Vector2(_imageRectToTextureRectRatio.x * localPoint.x, _imageRectToTextureRectRatio.y * localPoint.y);
		if (!_prevDragTexel.HasValue)
		{
			_prevDragTexel = value;
		}
		DrawLine(_screenshotTexture, (int)_prevDragTexel.Value.x, (int)_prevDragTexel.Value.y, (int)value.x, (int)value.y, DrawLineWidth, Color.red);
		_screenshotTexture.Apply();
		_prevDragTexel = value;
	}

	private void DrawSpot(Texture2D tex, int x, int y, int lineWidth, Color color)
	{
		int i = x - lineWidth / 2;
		for (int num = x + lineWidth / 2; i < num; i++)
		{
			int j = y - lineWidth / 2;
			for (int num2 = y + lineWidth / 2; j < num2; j++)
			{
				if (i >= 0 && i < tex.width && j >= 0 && j < tex.height)
				{
					tex.SetPixel(i, j, color);
				}
			}
		}
	}

	private void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, int lineWidth, Color col)
	{
		int num = y1 - y0;
		int num2 = x1 - x0;
		int num3;
		if (num < 0)
		{
			num = -num;
			num3 = -1;
		}
		else
		{
			num3 = 1;
		}
		int num4;
		if (num2 < 0)
		{
			num2 = -num2;
			num4 = -1;
		}
		else
		{
			num4 = 1;
		}
		num <<= 1;
		num2 <<= 1;
		DrawSpot(texture, x0, y0, lineWidth, col);
		float num5;
		if (num2 > num)
		{
			num5 = num - (num2 >> 1);
			while (Mathf.Abs(x0 - x1) > 1)
			{
				if (num5 >= 0f)
				{
					y0 += num3;
					num5 -= (float)num2;
				}
				x0 += num4;
				num5 += (float)num;
				DrawSpot(texture, x0, y0, lineWidth, col);
			}
			return;
		}
		num5 = num2 - (num >> 1);
		while (Mathf.Abs(y0 - y1) > 1)
		{
			if (num5 >= 0f)
			{
				x0 += num4;
				num5 -= (float)num;
			}
			y0 += num3;
			num5 += (float)num2;
			DrawSpot(texture, x0, y0, lineWidth, col);
		}
	}
}
