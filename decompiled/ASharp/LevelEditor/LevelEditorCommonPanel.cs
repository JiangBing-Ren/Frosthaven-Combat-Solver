using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ScenarioRuleLibrary;
using SharedLibrary;
using UnityEngine;
using UnityEngine.UI;

namespace LevelEditor;

public class LevelEditorCommonPanel : MonoBehaviour
{
	public static bool LevelEditorCommonPanel_kvar;

	[InspectorName("UI Sprite")]
	public Sprite UISprite;

	private Vector2 _mousePosition;

	private bool _leftButtonDown;

	private bool _leftButtonUp;

	private Canvas _oldCanvas;

	private int _comboOpen;

	private CObjectChest.EType _chestType;

	private string _chestGUID;

	private Vector3 _chestPos;

	private int _chestWbId;

	private WrapBox _chestThisBox;

	private WrapBox _chestRemainBox;

	private int _chestComboWbId;

	private WrapBox _chestComboThisBox;

	private WrapBox _chestComboRemainBox;

	private float _chestComboScroll;

	private Dictionary<int, CObjectChest.EType> _chestPlaceholder = new Dictionary<int, CObjectChest.EType>();

	private byte[] _lootTableFallback = CLootDeck.RandomScenarioLootTable(new SharedLibrary.Random());

	private byte[] _lootTable
	{
		get
		{
			if (ScenarioManager.CurrentScenarioState == null || ScenarioManager.CurrentScenarioState.LootTable == null)
			{
				return _lootTableFallback;
			}
			return ScenarioManager.CurrentScenarioState.LootTable;
		}
	}

	private bool Button(WrapBox wbox, string text, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string caller = null)
	{
		return DebugMenuWBUI.ButtonRes(wbox, text, null, lineNumber, caller) == QUI.WidgetResult.Pressed;
	}

	private WrapBox Panel_ui(WrapBox wbox)
	{
		if (!LevelEditorCommonPanel_kvar)
		{
			if (Button(wbox.TopCenter(100f, 20f), "Loot", 37, "Panel_ui"))
			{
				LevelEditorCommonPanel_kvar = true;
			}
			if ((bool)_oldCanvas)
			{
				_oldCanvas.enabled = true;
			}
			return wbox;
		}
		if ((bool)_oldCanvas)
		{
			_oldCanvas.enabled = false;
		}
		wbox = wbox.TopLeft(wbox.W, 24 * LootCard.AllTypes.Length);
		WBUI.Texture(wbox, null, new Color(0.7f, 0.7f, 0.7f), 0, 52, "Panel_ui");
		ScenarioState currentScenarioState = ScenarioManager.CurrentScenarioState;
		int num = 1;
		num = ((currentScenarioState == null) ? (num + 2) : (num + currentScenarioState.ChestProps.Count));
		float num2 = Mathf.Min(300f, wbox.W / (float)num);
		float num3 = (wbox.W - (float)num * num2) / 2f;
		if (Button(wbox.BottomCenter(100f, 20f, -20f), "Hide", 66, "Panel_ui"))
		{
			LevelEditorCommonPanel_kvar = false;
		}
		LootTable_ui(wbox.TopLeft(num2, wbox.H, num3 + 0f * num2));
		if (currentScenarioState != null)
		{
			for (int i = 0; i < currentScenarioState.ChestProps.Count; i++)
			{
				_ = currentScenarioState.ChestProps[i];
				Chest_ui(i, wbox.TopLeft(num2, wbox.H, num3 + (float)(i + 1) * num2));
			}
		}
		else
		{
			for (int j = 0; j < 2; j++)
			{
				Chest_ui(j, wbox.TopLeft(num2, wbox.H, num3 + (float)(j + 1) * num2, 0f, j));
			}
		}
		return wbox;
	}

	private void Update()
	{
		if (!QUI.canvas)
		{
			QUI.canvas = new GameObject("QUI Root").AddComponent<Canvas>();
			QUI.canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			_oldCanvas = GetComponentInParent<Canvas>();
			if (!_oldCanvas || !_oldCanvas.GetComponent<LevelEditorUI>())
			{
				Canvas[] array = Object.FindObjectsOfType<Canvas>();
				foreach (Canvas canvas in array)
				{
					if ((bool)canvas.GetComponent<LevelEditorUI>())
					{
						_oldCanvas = canvas;
						break;
					}
				}
			}
			if ((bool)_oldCanvas)
			{
				QUI.canvas.transform.SetParent(_oldCanvas.transform.parent, worldPositionStays: false);
			}
		}
		GetComponentInParent<LevelEditorUI>()?.gameObject.SetActive(value: true);
		DebugMenuWBUI.Begin();
		WrapBox wbox = new WrapBox
		{
			w = Screen.width,
			h = Screen.height
		};
		Panel_ui(wbox);
		DebugMenuWBUI.End();
	}

	private void Start()
	{
		if ((bool)QUI.canvas)
		{
			Object.Destroy(QUI.canvas.gameObject);
		}
	}

	public void OnDestroy()
	{
		if ((bool)QUI.canvas)
		{
			Object.Destroy(QUI.canvas.gameObject);
		}
	}

	private WrapBox ChestEntry(float H)
	{
		_chestThisBox = _chestRemainBox.TopCenter(_chestRemainBox.W, H, 0f, _chestWbId++);
		_chestRemainBox = _chestRemainBox.BottomCenter(_chestRemainBox.W, _chestRemainBox.H - H);
		return _chestThisBox;
	}

	private WrapBox ChestComboEntry(float H)
	{
		_chestComboThisBox = _chestComboRemainBox.TopCenter(_chestComboRemainBox.W, H, 0f, _chestComboWbId++);
		_chestComboRemainBox = _chestComboRemainBox.BottomCenter(_chestComboRemainBox.W, _chestComboRemainBox.H - H);
		return _chestComboThisBox;
	}

	private void ChestTitle_ui()
	{
		DebugMenuWBUI.Text("Chest Setup", ChestEntry(30f), 0, TextAnchor.MiddleCenter, null, 0, 48, "ChestTitle_ui");
	}

	private void ChestGUID_ui()
	{
		WrapBox wbox = ChestEntry(24f);
		QUI.WidgetResult widgetResult = DebugMenuWBUI.ButtonRes(wbox, _chestGUID, null, 53, "ChestGUID_ui");
		if (widgetResult != QUI.WidgetResult.Idle)
		{
			DebugMenuWBUI.TooltipTopRight(wbox, "Click on the GUID\nto look at the chest");
		}
		if (widgetResult == QUI.WidgetResult.Pressed && (bool)CameraController.s_CameraController)
		{
			LevelEditorController.s_Instance.ShowLocationIndicator(_chestPos);
			CameraController.s_CameraController.m_TargetFocalPoint = _chestPos;
		}
	}

	private void ChestComboButton_ui(WrapBox wbox)
	{
		if (Button(wbox, _chestType.ToString(), 66, "ChestComboButton_ui"))
		{
			_comboOpen = _chestWbId;
		}
		DebugMenuWBUI.Text("▼  ", wbox, DebugMenuWBUI.ButtTextSize(wbox), TextAnchor.MiddleRight, null, 0, 69, "ChestComboButton_ui");
	}

	private void ChestComboListEntry_ui(CObjectChest.EType type, float H)
	{
		float num = H * (float)CObjectChest.AllTypes.Length;
		WrapBox wrapBox = ChestComboEntry(H);
		wrapBox = wrapBox.TopLeft(wrapBox.W, wrapBox.H, 0f, (0f - num) * _chestComboScroll);
		QUI.WidgetResult widgetResult = WBUI.ClickRect(wrapBox, 0, 76, "ChestComboListEntry_ui");
		if (widgetResult != QUI.WidgetResult.Idle)
		{
			WBUI.Texture(wrapBox, null, new Color(0f, 0f, 0f, 0.2f), 0, 78, "ChestComboListEntry_ui");
			if (widgetResult == QUI.WidgetResult.Pressed)
			{
				_chestType = type;
				_comboOpen = 0;
			}
		}
		if (type == _chestType)
		{
			WBUI.Texture(wrapBox, null, new Color(0f, 0f, 0f, 0.2f), 0, 85, "ChestComboListEntry_ui");
		}
		DebugMenuWBUI.Text(type.ToString(), wrapBox, DebugMenuWBUI.ButtTextSize(wrapBox), TextAnchor.MiddleCenter, null, 0, 87, "ChestComboListEntry_ui");
	}

	private void ChestComboListBgr_ui(WrapBox wbox)
	{
		WBUI.Sprite(wbox, UISprite, null, scissor: false, 1f, Image.Type.Sliced, 0, 91, "ChestComboListBgr_ui");
	}

	private void ChestComboList_ui(WrapBox wbox)
	{
		float h = wbox.H;
		float num = h * (float)CObjectChest.AllTypes.Length;
		float num2 = Mathf.Min(num, 1000f);
		wbox = wbox.TopLeft(wbox.W, num2);
		ChestComboListBgr_ui(wbox);
		WBUI.EnableScissor(wbox.Center(wbox.W - 4f, wbox.H - 4f), 0, 101, "ChestComboList_ui");
		_chestComboWbId = 0;
		_chestComboRemainBox = wbox;
		foreach (CObjectChest.EType allType in CObjectChest.AllTypes)
		{
			ChestComboListEntry_ui(allType, h);
		}
		WBUI.DisableScissor(0, 107, "ChestComboList_ui");
		WrapBox wbox2 = wbox.TopRight(20f, wbox.H);
		if (Scrollbar(wbox2, ref _chestComboScroll, h, num, num2, out var wboxHandle))
		{
			WBUI.Sprite(wboxHandle, UISprite, new Color(0.9f, 0.9f, 0.9f), scissor: false, 1f, Image.Type.Sliced, 0, 112, "ChestComboList_ui");
		}
	}

	private void ChestCombo_ui()
	{
		WrapBox wrapBox = ChestEntry(24f);
		WrapBox wbox = wrapBox.TopLeft(30f, wrapBox.H);
		DebugMenuWBUI.Text("id: ", wbox, 0, TextAnchor.MiddleRight, null, 0, 120, "ChestCombo_ui");
		wrapBox = wrapBox.TopRight(wrapBox.W - wbox.W, wrapBox.H);
		if (_comboOpen == _chestWbId)
		{
			ChestComboList_ui(wrapBox);
		}
		else
		{
			ChestComboButton_ui(wrapBox);
		}
	}

	private WrapBox Chest_ui(int chest, WrapBox wbox)
	{
		if (!_chestPlaceholder.ContainsKey(chest))
		{
			_chestPlaceholder[chest] = CObjectChest.EType.None;
		}
		CObjectChest cObjectChest = (CObjectChest)(ScenarioManager.CurrentScenarioState?.ChestProps[chest]);
		_chestType = cObjectChest?.Type ?? _chestPlaceholder[chest];
		_chestGUID = ((cObjectChest != null) ? cObjectChest.PropGuid : "Chest GUID goes here");
		if (cObjectChest != null && (bool)CameraController.s_CameraController)
		{
			TileIndex arrayIndex = cObjectChest.ArrayIndex;
			CClientTile cClientTile = ClientScenarioManager.s_ClientScenarioManager.ClientTileArray[arrayIndex.X, arrayIndex.Y];
			_chestPos = cClientTile.m_GameObject.transform.position;
		}
		_chestWbId = (wbox.id = chest);
		_chestRemainBox = (wbox = wbox.Center(wbox.W * 0.95f, wbox.H * 0.95f));
		ChestTitle_ui();
		ChestGUID_ui();
		ChestCombo_ui();
		if (cObjectChest != null)
		{
			cObjectChest.Type = _chestType;
		}
		_chestPlaceholder[chest] = _chestType;
		return wbox;
	}

	private float ClampScroll(float val, float handleSize)
	{
		return Mathf.Clamp(val, 0f, 1f - handleSize);
	}

	private WrapBox ScrollHandle(WrapBox wbox, ref float scroll, float handleSize)
	{
		scroll = ClampScroll(scroll, handleSize);
		float y = wbox.y + wbox.h * scroll;
		float x = wbox.x;
		WrapBox result = wbox.TopLeft(wbox.W, wbox.H * handleSize, 0f, wbox.H * scroll);
		QUI.Drag(ref x, ref y, result.w, result.h, wbox.id, 172, "ScrollHandle");
		scroll = ClampScroll((y - wbox.y) / wbox.h, handleSize);
		return result;
	}

	private bool Scrollbar(WrapBox wbox, ref float scroll, float itemHeight, float allItemsHeight, float containerHeight, out WrapBox wboxHandle)
	{
		if (containerHeight >= allItemsHeight)
		{
			wboxHandle = wbox;
			return false;
		}
		float handleSize = containerHeight / allItemsHeight;
		wboxHandle = ScrollHandle(wbox, ref scroll, handleSize);
		return true;
	}

	private bool LootTableResourceButtonPlus_ui(WrapBox wbox)
	{
		wbox = wbox.TopLeft(wbox.H, wbox.H, wbox.W / 2f + wbox.H * 0.25f + wbox.H);
		return Button(wbox, "+", 27, "LootTableResourceButtonPlus_ui");
	}

	private bool LootTableResourceButtonMinus_ui(WrapBox wbox)
	{
		wbox = wbox.TopLeft(wbox.H, wbox.H, wbox.W / 2f + wbox.H * 0.25f);
		return Button(wbox, "-", 32, "LootTableResourceButtonMinus_ui");
	}

	private WrapBox LootTableResourceString_ui(WrapBox wbox, LootCard.EType t, bool fade)
	{
		wbox = wbox.TopLeft(wbox.W / 2f, wbox.H);
		DebugMenuWBUI.Text($"{t} {_lootTable[(int)t]}", wbox, 0, TextAnchor.MiddleRight, fade ? new Color(0f, 0f, 0f, 0.5f) : Color.black, 0, 37, "LootTableResourceString_ui");
		return wbox;
	}

	private WrapBox LootTableEntry_ui(WrapBox wbox, LootCard.EType t, int i, bool zero)
	{
		float num = Mathf.Min(24f, wbox.H / (float)(LootCard.AllTypes.Length - 1));
		wbox = wbox.TopLeft(wbox.W, num, 0f, (float)i * num, i);
		if (WBUI.CursorInRect(wbox))
		{
			WrapBox wrapBox = wbox.Center(300f, wbox.H);
			QUI.Texture(wrapBox.x, wrapBox.y, wrapBox.w, wrapBox.h, null, new Color(0f, 0f, 0f, 0.2f), scissor: false, 0, 51, "LootTableEntry_ui");
		}
		LootTableResourceString_ui(wbox, t, zero);
		if (!zero && LootTableResourceButtonMinus_ui(wbox))
		{
			_lootTable[(int)t]--;
		}
		if (LootTableResourceButtonPlus_ui(wbox))
		{
			_lootTable[(int)t] = (byte)Mathf.Min(_lootTable[(int)t] + 1, CLootDeck.numCardsPerType[(int)t]);
		}
		return wbox;
	}

	private WrapBox LootTableList_ui(WrapBox wbox)
	{
		byte[] array = (byte[])_lootTable.Clone();
		int num = 0;
		LootCard.EType[] allTypes = LootCard.AllTypes;
		foreach (LootCard.EType eType in allTypes)
		{
			if (eType != LootCard.EType.None && array[(int)eType] > 0)
			{
				LootTableEntry_ui(wbox, eType, num, zero: false);
				num++;
			}
		}
		allTypes = LootCard.AllTypes;
		foreach (LootCard.EType eType2 in allTypes)
		{
			if (eType2 != LootCard.EType.None && array[(int)eType2] == 0)
			{
				LootTableEntry_ui(wbox, eType2, num, zero: true);
				num++;
			}
		}
		return wbox;
	}

	private WrapBox LootTableTitle_ui(WrapBox wbox)
	{
		wbox = wbox.TopCenter(wbox.W, 30f);
		DebugMenuWBUI.Text("Loot Table", wbox, 0, TextAnchor.MiddleCenter, null, 0, 94, "LootTableTitle_ui");
		return wbox;
	}

	private WrapBox LootTable_ui(WrapBox wbox)
	{
		wbox = wbox.Center(wbox.W * 0.95f, wbox.H * 0.95f);
		WrapBox wrapBox = LootTableTitle_ui(wbox);
		WrapBox wbox2 = wbox.BottomCenter(wbox.W, wbox.H - wrapBox.H);
		LootTableList_ui(wbox2);
		return wbox;
	}
}
