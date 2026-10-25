using System;
using System.Collections.Generic;
using System.Linq;
using FHGUI.Scenario.Services;
using ScenarioRuleLibrary;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FHGUI.Common;

public class UIGeneralInfoModule : MonoBehaviour
{
	public delegate void InfoModuleEventHandler(IGeneralInfoData currentData);

	private record GenInfoHoverContext
	{
		public HexViewService.HexInfoParams HexInfo;

		public IGeneralInfoProvider UIObject;

		public IGeneralInfoProvider MapObject;

		public CClientTile Tile;

		public bool IsEmpty
		{
			get
			{
				if (HexInfo.IsEmpty && UIObject == null && MapObject == null)
				{
					return Tile == null;
				}
				return false;
			}
		}

		public IEnumerable<IGeneralInfoData> GetData()
		{
			if (UIObject != null)
			{
				yield return UIObject.GetInfoData();
				yield break;
			}
			if (HexInfo.ActorOnTileWithIniative != null)
			{
				yield return UIGeneralInfoUtils.GetActor(HexInfo.ActorOnTileWithIniative);
			}
			if (HexInfo.Props == null)
			{
				yield break;
			}
			foreach (CObjectProp prop in HexInfo.Props)
			{
				UnityGameEditorObject propEditorObject = UnityGameEditorObject.s_all.FirstOrDefault((UnityGameEditorObject o) => o.PropObject == prop);
				yield return UIGeneralInfoUtils.GetPropData(prop, propEditorObject);
			}
			if (HexInfo.Props.Count != 0 || HexInfo.ActorOnTileWithIniative != null || MapObject == null)
			{
				yield break;
			}
			if (MapObject.GetInfoData() is UIGeneralInfoMonster.MonsterGeneralInfoData monsterGeneralInfoData)
			{
				if ((bool)monsterGeneralInfoData.Illustration)
				{
					yield return MapObject.GetInfoData();
				}
			}
			else
			{
				yield return MapObject.GetInfoData();
			}
		}
	}

	public struct GeneralInfoListDTO
	{
		public Action OnPrev;

		public Action OnNext;

		public IGeneralInfoData CurrentData;
	}

	[SerializeField]
	private AudioMapViewModuleProfile _audioProfile;

	[Header("Screens")]
	[SerializeField]
	private UIGeneralInfoItem _itemInfo;

	[SerializeField]
	private UIGeneralInfoCard _cardInfo;

	[SerializeField]
	private UIGeneralInfoCharacter _characterInfo;

	[SerializeField]
	private UIGeneralInfoMonster _monsterInfo;

	[SerializeField]
	private UIGeneralInfoSummon _summonInfo;

	[SerializeField]
	private UIGeneralInfoMonsterCard _monsterCardInfo;

	[SerializeField]
	private UIGeneralInfoProp _propInfo;

	[SerializeField]
	private UIGeneralInfoTerrain _terrainInfo;

	[SerializeField]
	private UIGeneralInfoSpawner _spawner;

	[Header("Other")]
	[SerializeField]
	private GraphicRaycaster _graphicRaycaster;

	[SerializeField]
	private UIActorEventsChannel _actorEventsChannel;

	[SerializeField]
	private Image _blocker;

	private IUIGeneralInfo[] _modules;

	private IUIGeneralInfo _currentModule;

	private Camera _camera;

	private LayerMask _targetLayer;

	private List<RaycastResult> _raycastResults = new List<RaycastResult>();

	private RaycastHit[] _raycastHits = new RaycastHit[10];

	private readonly List<IGeneralInfoData> _datas = new List<IGeneralInfoData>();

	private int _currentDataIndex;

	private GenInfoHoverContext _context;

	private AudioMapViewModuleProfile AudioProfile
	{
		get
		{
			if (!(_audioProfile != null))
			{
				if (!(UIInfoTools.Instance != null))
				{
					return null;
				}
				return UIInfoTools.Instance.defaultAudioMapViewModuleProfile;
			}
			return _audioProfile;
		}
	}

	public IUIGeneralInfo CurrentModule => _currentModule;

	private GenInfoHoverContext Context
	{
		get
		{
			if (_context == null)
			{
				BuildContext();
			}
			return _context;
		}
	}

	public CClientTile CurrentTile => Context.Tile;

	public HexViewService.HexInfoParams HexInfo => Context.HexInfo;

	public IGeneralInfoProvider CurrentUIObject => Context.UIObject;

	public IGeneralInfoProvider CurrentMapObject => Context.MapObject;

	public event InfoModuleEventHandler OnShow;

	private void OnEnable()
	{
		if (AudioProfile != null && !SceneController.Instance.IsLoading)
		{
			AudioControllerUtils.PlaySound(AudioProfile.OnShow);
		}
	}

	private void Awake()
	{
		_targetLayer = 1 << LayerMask.NameToLayer("Hovering");
		_camera = Camera.main;
		_blocker.enabled = false;
		Canvas component = GetComponent<Canvas>();
		if (component != null)
		{
			component.sortingLayerName = "Default";
			component.sortingOrder = 1010;
		}
		_modules = GetComponentsInChildren<IUIGeneralInfo>(includeInactive: true);
		IUIGeneralInfo[] modules = _modules;
		foreach (IUIGeneralInfo module in modules)
		{
			module.Hide();
			IUIGeneralInfo iUIGeneralInfo = module;
			iUIGeneralInfo.OnHide = (Action)Delegate.Combine(iUIGeneralInfo.OnHide, (Action)(() =>
			{
				OnInfoHide(module);
			}));
		}
	}

	private void OnDisable()
	{
		if (AudioProfile != null && !SceneController.Instance.IsLoading)
		{
			AudioControllerUtils.PlaySound(AudioProfile.OnHide);
		}
		_currentModule?.Hide();
		_context = null;
		_datas.Clear();
		_currentDataIndex = 0;
	}

	private void OnInfoHide(IUIGeneralInfo module)
	{
		_blocker.enabled = false;
		if (CameraController.s_CameraController != null)
		{
			CameraController.s_CameraController.DisableCameraInput(disableInput: false, this);
		}
		if (_currentModule == module)
		{
			_currentModule = null;
		}
	}

	private bool Show(object data, out IUIGeneralInfo info)
	{
		info = null;
		if (!(data is UIGeneralInfoItem.UIGeneralInfoItemData data2))
		{
			if (!(data is UIGeneralInfoCard.UIGeneralInfoCardData data3))
			{
				if (!(data is UIGeneralInfoCharacter.PlayerCharacterGeneralInfoData data4))
				{
					if (!(data is UIGeneralInfoMonster.MonsterGeneralInfoData data5))
					{
						if (!(data is UIGeneralInfoSummon.SummonGeneralInfoData data6))
						{
							if (!(data is UIGeneralInfoProp.PropGeneralInfoData data7))
							{
								if (!(data is UIGeneralInfoMonsterCard.MonsterCardGeneralInfoData data8))
								{
									if (!(data is UIGeneralInfoTerrain.TerrainGeneralInfoData data9))
									{
										if (data is UIGeneralInfoSpawner.SpawnerGeneralInfoData data10)
										{
											info = _spawner;
											return _spawner.Show(data10);
										}
										return false;
									}
									info = _terrainInfo;
									return _terrainInfo.Show(data9);
								}
								info = _monsterCardInfo;
								return _monsterCardInfo.Show(data8);
							}
							info = _propInfo;
							return _propInfo.Show(data7);
						}
						info = _summonInfo;
						return _summonInfo.Show(data6);
					}
					info = _monsterInfo;
					return _monsterInfo.Show(data5);
				}
				info = _characterInfo;
				return _characterInfo.Show(data4);
			}
			info = _cardInfo;
			return _cardInfo.Show(data3);
		}
		info = _itemInfo;
		return _itemInfo.Show(data2);
	}

	public void CloseInfo()
	{
		_currentModule?.Hide();
	}

	private bool CanShowGeneralInfo()
	{
		if (SaveData.Instance != null)
		{
			return SaveData.Instance.Global.GameMode != EGameMode.LevelEditor;
		}
		return false;
	}

	public bool TryShowGeneralInfoUnderCursor()
	{
		if (!Singleton<InputFHManager>.IsInitialized)
		{
			return false;
		}
		if (InputFHManager.IsTextFieldFocused)
		{
			return false;
		}
		if (!CanShowGeneralInfo())
		{
			return false;
		}
		if (_context == null)
		{
			BuildContext();
		}
		IGeneralInfoData generalInfoData = null;
		if (_context.UIObject == null)
		{
			_ = _context.MapObject;
		}
		if (_context.IsEmpty)
		{
			CloseInfo();
			return false;
		}
		try
		{
			OnShow_Internal(_context.GetData());
			return true;
		}
		catch (Exception ex)
		{
			_currentModule?.Hide();
			Log.Warning($"General Info UI for {generalInfoData?.GetType()} failed with {ex.Message} {ex.StackTrace}");
			return false;
		}
	}

	private void BuildContext()
	{
		RaycastUI(out var infoObject);
		Raycast3D(out var infoObject2);
		ScenarioScriptFrontend.GetTileUnderCursor(out var tile);
		HexViewService.HexInfoParams hexInfo = ((tile != null) ? HexViewService.GenerateHexInfoParams(tile.m_Tile) : default(HexViewService.HexInfoParams));
		_context = new GenInfoHoverContext
		{
			HexInfo = hexInfo,
			Tile = tile,
			MapObject = infoObject2,
			UIObject = infoObject
		};
	}

	private void LateUpdate()
	{
		_context = null;
	}

	private void OnInfoShow(IGeneralInfoData data)
	{
		_blocker.enabled = true;
		if (CameraController.s_CameraController != null)
		{
			CameraController.s_CameraController.DisableCameraInput(disableInput: true, this);
		}
		OnShow?.Invoke(data);
	}

	private void OnShow_Internal(IEnumerable<IGeneralInfoData> datas)
	{
		_datas.Clear();
		_datas.AddRange(datas);
		_currentDataIndex = 0;
		if (_datas.Count > 0)
		{
			OnShow_Internal(_datas[_currentDataIndex]);
		}
	}

	private void OnShow_Internal(IGeneralInfoData data)
	{
		_currentModule?.Hide();
		if (Show(data, out _currentModule))
		{
			OnInfoShow(data);
			GeneralInfoListDTO listParams = new GeneralInfoListDTO
			{
				OnPrev = ShowPrevInfo,
				OnNext = ShowNextInfo,
				CurrentData = data
			};
			_currentModule.Navigator.Init(listParams, _datas);
		}
	}

	private void ShowPrevInfo()
	{
		ChangeCurrentDataIndexAndShow(-1);
	}

	private void ShowNextInfo()
	{
		ChangeCurrentDataIndexAndShow(1);
	}

	private void ChangeCurrentDataIndexAndShow(int indexStep)
	{
		_currentDataIndex = Mathf.Clamp(_currentDataIndex + indexStep, 0, _datas.Count - 1);
		OnShow_Internal(_datas[_currentDataIndex]);
	}

	public void RequestGeneralInfo(IGeneralInfoData data)
	{
		OnShow_Internal(data);
	}

	public void RequestGeneralInfo(IEnumerable<IGeneralInfoData> datas)
	{
		OnShow_Internal(datas);
	}

	private bool RaycastUI(out IGeneralInfoProvider infoObject)
	{
		EventSystem current = EventSystem.current;
		infoObject = null;
		if (!current.IsPointerOverGameObject())
		{
			return false;
		}
		current.RaycastAll(new PointerEventData(current)
		{
			position = InputFHManager.CursorPosition
		}, _raycastResults);
		if (_raycastResults.Count == 0)
		{
			return false;
		}
		GameObject gameObject = _raycastResults[0].gameObject;
		infoObject = gameObject.GetComponentInParent<IGeneralInfoProvider>();
		if (infoObject == null)
		{
			return false;
		}
		return true;
	}

	private bool Raycast3D(out IGeneralInfoProvider infoObject)
	{
		infoObject = null;
		if (EventSystem.current.IsPointerOverGameObject())
		{
			return false;
		}
		if (_camera == null || !_camera.gameObject.activeSelf)
		{
			_camera = Camera.main;
		}
		if (_camera == null)
		{
			return false;
		}
		Ray ray = _camera.ScreenPointToRay(InputFHManager.CursorPosition);
		Array.Clear(_raycastHits, 0, _raycastHits.Length);
		Physics.RaycastNonAlloc(ray, _raycastHits, 1000f, _targetLayer);
		if (_raycastHits.Length == 0)
		{
			return false;
		}
		RaycastHit[] raycastHits = _raycastHits;
		for (int i = 0; i < raycastHits.Length; i++)
		{
			RaycastHit raycastHit = raycastHits[i];
			if (!(raycastHit.transform == null))
			{
				infoObject = raycastHit.transform.GetComponentInParent<IGeneralInfoProvider>();
				if (infoObject != null)
				{
					return true;
				}
			}
		}
		return false;
	}
}
