using System;
using System.Collections;
using DG.Tweening;
using FHGUI.DTO;
using FHGUI.Elements;
using UnityEngine;

public class FHPartyTravel : MonoBehaviour
{
	public struct TravelParams
	{
		public string sourceId;

		public string destinationId;

		public int partySize;

		public float startAt;

		public float endAt;

		public Action OnTravelComplete;

		public OutpostEventDTO Event;
	}

	[Header("Components")]
	[SerializeField]
	private FHEventMarker _eventMarker;

	[SerializeField]
	private FHPartyMarker _partyMarker;

	[SerializeField]
	private FHPartyTravelPath _partyTravelPath;

	[SerializeField]
	private FHPOIMarker _pointOfInterestMarker;

	[SerializeField]
	private GameObject _outpost;

	[Header("Settings")]
	[SerializeField]
	private AnimationCurve _travelEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	private float _unlockScenarioFocusDuration = 3f;

	[SerializeField]
	private float _travelDuration = 2f;

	[SerializeField]
	private Vector3 _pointOffset;

	[SerializeField]
	private Sprite _pathStartPointIcon;

	private FHWorldMapScene _map;

	private Tweener travelTween;

	private bool _initialized;

	public bool IsTravelling
	{
		get
		{
			if (travelTween != null)
			{
				return travelTween.IsActive();
			}
			return false;
		}
	}

	public void InitCache()
	{
		if (!_initialized)
		{
			_initialized = true;
			_map = GetComponent<FHWorldMapScene>();
			if (_partyMarker != null)
			{
				_partyMarker.Hide(instant: true);
			}
			if (_pointOfInterestMarker != null)
			{
				_pointOfInterestMarker.Hide();
			}
		}
	}

	public void Init(TravelParams @params)
	{
		InitCache();
		Vector3 position = GetPosition(@params.sourceId);
		Vector3 position2 = GetPosition(@params.destinationId);
		Vector3 position3 = Vector3.Lerp(position, position2, @params.startAt);
		Vector3 endValue = Vector3.Lerp(position, position2, @params.endAt);
		_map.CameraRig.LockOntoTarget(_partyMarker.transform, 1f);
		_partyMarker.Init(@params.partySize);
		_partyMarker.transform.position = position3;
		if (string.IsNullOrEmpty(@params.sourceId))
		{
			_pointOfInterestMarker.Init(_pathStartPointIcon);
			_pointOfInterestMarker.Show(position);
		}
		travelTween = _partyMarker.transform.DOMove(endValue, _travelDuration).SetEase(_travelEasing).OnComplete(() =>
		{
			travelTween = null;
			_partyMarker.Hide();
			if (@params.Event == null)
			{
				@params.OnTravelComplete?.Invoke();
			}
			else
			{
				ShowEventEncounter(@params.Event, _partyMarker.transform.position, @params.OnTravelComplete);
			}
		});
		_partyTravelPath.Init(new Vector3[2] { position, position2 });
		if (!string.IsNullOrEmpty(@params.destinationId))
		{
			_map.GetMarker(@params.destinationId)?.Focus(FHScenarioMarker.HighlightType.Normal);
		}
	}

	public void Clear()
	{
		_partyMarker.Hide();
		_eventMarker.Hide();
		_partyTravelPath.Hide();
		_pointOfInterestMarker.Hide();
	}

	private void ShowEventEncounter(OutpostEventDTO eventDef, Vector3 position, Action onComplete)
	{
		_eventMarker.Init(eventDef);
		_eventMarker.Show(position, onComplete);
		AudioControllerUtils.PlaySound(UIInfoTools.Instance?.encounterAudioItem);
	}

	public void StopPartyTravel()
	{
		_partyMarker.Hide();
		if (travelTween != null)
		{
			travelTween.Kill();
		}
		travelTween = null;
	}

	public void ShowUnlockScenarios(int[] scenarios, Action OnTravelComplete)
	{
		InitCache();
		if (scenarios.Length != 0)
		{
			StartCoroutine(UnlockScenariosEffect(scenarios, OnTravelComplete));
		}
	}

	private IEnumerator UnlockScenariosEffect(int[] scenarios, Action OnDoneCallback)
	{
		for (int i = 0; i < scenarios.Length; i++)
		{
			_map.FocusOnScenario(scenarios[i]);
			yield return new WaitForSeconds(_unlockScenarioFocusDuration);
		}
		OnDoneCallback?.Invoke();
		StopAllCoroutines();
	}

	public Vector3 GetPosition(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return _outpost.transform.position;
		}
		FHScenarioMarker marker = _map.GetMarker(id);
		if (marker != null)
		{
			return marker.transform.position + _pointOffset;
		}
		FHPOI pointOfInterest = _map.GetPointOfInterest(id);
		if (pointOfInterest != null)
		{
			return pointOfInterest.transform.position + _pointOffset;
		}
		return Vector3.zero;
	}
}
