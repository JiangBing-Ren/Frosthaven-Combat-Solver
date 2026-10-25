using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Chronos;
using UnityEngine;

namespace MEC;

public class Timing : MonoBehaviour
{
	private struct ProcessIndex : IEquatable<ProcessIndex>
	{
		public Segment seg;

		public int i;

		public bool Equals(ProcessIndex other)
		{
			if (seg == other.seg)
			{
				return i == other.i;
			}
			return false;
		}

		public override bool Equals(object other)
		{
			if (other is ProcessIndex)
			{
				return Equals((ProcessIndex)other);
			}
			return false;
		}

		public static bool operator ==(ProcessIndex a, ProcessIndex b)
		{
			if (a.seg == b.seg)
			{
				return a.i == b.i;
			}
			return false;
		}

		public static bool operator !=(ProcessIndex a, ProcessIndex b)
		{
			if (a.seg == b.seg)
			{
				return a.i != b.i;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return (int)(seg - 2) * 715827882 + i;
		}
	}

	[Tooltip("How quickly the SlowUpdate segment ticks.")]
	public float TimeBetweenSlowUpdateCalls = 1f / 7f;

	[Tooltip("How much data should be sent to the profiler window when it's open.")]
	public DebugInfoType ProfilerDebugAmount;

	[Tooltip("A count of the number of Update coroutines that are currently running.")]
	[Space(12f)]
	public int UpdateCoroutines;

	[Tooltip("A count of the number of FixedUpdate coroutines that are currently running.")]
	public int FixedUpdateCoroutines;

	[Tooltip("A count of the number of LateUpdate coroutines that are currently running.")]
	public int LateUpdateCoroutines;

	[Tooltip("A count of the number of SlowUpdate coroutines that are currently running.")]
	public int SlowUpdateCoroutines;

	[NonSerialized]
	public float localTime;

	[NonSerialized]
	public float deltaTime;

	public static Func<IEnumerator<float>, CoroutineHandle, IEnumerator<float>> ReplacementFunction;

	public const float WaitForOneFrame = float.NegativeInfinity;

	private CoroutineHandle _currentCoroutine;

	private static object _tmpRef;

	private static bool _tmpBool;

	private static CoroutineHandle _tmpHandle;

	private int _currentUpdateFrame;

	private int _currentLateUpdateFrame;

	private int _currentSlowUpdateFrame;

	private int _nextUpdateProcessSlot;

	private int _nextLateUpdateProcessSlot;

	private int _nextFixedUpdateProcessSlot;

	private int _nextSlowUpdateProcessSlot;

	private int _lastUpdateProcessSlot;

	private int _lastLateUpdateProcessSlot;

	private int _lastFixedUpdateProcessSlot;

	private int _lastSlowUpdateProcessSlot;

	private float _lastUpdateTime;

	private float _lastLateUpdateTime;

	private float _lastFixedUpdateTime;

	private float _lastSlowUpdateTime;

	private float _lastSlowUpdateDeltaTime;

	private ushort _framesSinceUpdate;

	private ushort _expansions = 1;

	[SerializeField]
	[HideInInspector]
	private byte _instanceID;

	private readonly Dictionary<CoroutineHandle, HashSet<CoroutineHandle>> _waitingTriggers = new Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>();

	private readonly HashSet<CoroutineHandle> _allWaiting = new HashSet<CoroutineHandle>();

	private readonly Dictionary<CoroutineHandle, ProcessIndex> _handleToIndex = new Dictionary<CoroutineHandle, ProcessIndex>();

	private readonly Dictionary<ProcessIndex, CoroutineHandle> _indexToHandle = new Dictionary<ProcessIndex, CoroutineHandle>();

	private readonly Dictionary<CoroutineHandle, string> _processTags = new Dictionary<CoroutineHandle, string>();

	private readonly Dictionary<string, HashSet<CoroutineHandle>> _taggedProcesses = new Dictionary<string, HashSet<CoroutineHandle>>();

	private IEnumerator<float>[] UpdateProcesses = new IEnumerator<float>[256];

	private IEnumerator<float>[] LateUpdateProcesses = new IEnumerator<float>[8];

	private IEnumerator<float>[] FixedUpdateProcesses = new IEnumerator<float>[64];

	private IEnumerator<float>[] SlowUpdateProcesses = new IEnumerator<float>[64];

	private bool[] UpdatePaused = new bool[256];

	private bool[] LateUpdatePaused = new bool[8];

	private bool[] FixedUpdatePaused = new bool[64];

	private bool[] SlowUpdatePaused = new bool[64];

	private bool[] UpdateHeld = new bool[256];

	private bool[] LateUpdateHeld = new bool[8];

	private bool[] FixedUpdateHeld = new bool[64];

	private bool[] SlowUpdateHeld = new bool[64];

	private const ushort FramesUntilMaintenance = 64;

	private const int ProcessArrayChunkSize = 64;

	private const int InitialBufferSizeLarge = 256;

	private const int InitialBufferSizeMedium = 64;

	private const int InitialBufferSizeSmall = 8;

	private static Timing[] ActiveInstances = new Timing[16];

	private static Timing _instance;

	public static float LocalTime => Instance.localTime;

	public static float DeltaTime => Instance.deltaTime;

	public static Thread MainThread { get; private set; }

	public static CoroutineHandle CurrentCoroutine
	{
		get
		{
			for (int i = 1; i < 16; i++)
			{
				if (ActiveInstances[i] != null && ActiveInstances[i]._currentCoroutine.IsValid)
				{
					return ActiveInstances[i]._currentCoroutine;
				}
			}
			return default;
		}
	}

	public CoroutineHandle currentCoroutine => _currentCoroutine;

	public static Timing Instance
	{
		get
		{
			if (_instance == null || !_instance.gameObject)
			{
				GameObject gameObject = GameObject.Find("Timing Controller");
				if (gameObject == null)
				{
					gameObject = new GameObject
					{
						name = "Timing Controller"
					};
					UnityEngine.Object.DontDestroyOnLoad(gameObject);
				}
				_instance = gameObject.GetComponent<Timing>() ?? gameObject.AddComponent<Timing>();
				_instance.InitializeInstanceID();
			}
			return _instance;
		}
		set
		{
			_instance = value;
		}
	}

	public static event Action OnPreExecute;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		else
		{
			deltaTime = _instance.deltaTime;
		}
		if (MainThread == null)
		{
			MainThread = Thread.CurrentThread;
		}
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
	}

	private void OnEnable()
	{
		InitializeInstanceID();
	}

	private void OnDisable()
	{
		if (_instanceID < ActiveInstances.Length)
		{
			ActiveInstances[_instanceID] = null;
		}
	}

	private void InitializeInstanceID()
	{
		if (!(ActiveInstances[_instanceID] == null))
		{
			return;
		}
		if (_instanceID == 0)
		{
			_instanceID++;
		}
		while (_instanceID <= 16)
		{
			if (_instanceID == 16)
			{
				UnityEngine.Object.Destroy(gameObject);
				throw new OverflowException("You are only allowed 15 different contexts for MEC to run inside at one time.");
			}
			if (ActiveInstances[_instanceID] == null)
			{
				ActiveInstances[_instanceID] = this;
				break;
			}
			_instanceID++;
		}
	}

	private void Update()
	{
		if (OnPreExecute != null)
		{
			OnPreExecute();
		}
		if (_lastSlowUpdateTime + TimeBetweenSlowUpdateCalls < Time.realtimeSinceStartup && _nextSlowUpdateProcessSlot > 0)
		{
			ProcessIndex key = new ProcessIndex
			{
				seg = Segment.SlowUpdate
			};
			if (UpdateTimeValues(key.seg))
			{
				_lastSlowUpdateProcessSlot = _nextSlowUpdateProcessSlot;
			}
			key.i = 0;
			while (key.i < _lastSlowUpdateProcessSlot)
			{
				try
				{
					if (!SlowUpdatePaused[key.i] && !SlowUpdateHeld[key.i] && SlowUpdateProcesses[key.i] != null && !(localTime < SlowUpdateProcesses[key.i].Current))
					{
						_currentCoroutine = _indexToHandle[key];
						if (ProfilerDebugAmount != DebugInfoType.None)
						{
							_indexToHandle.ContainsKey(key);
						}
						if (!SlowUpdateProcesses[key.i].MoveNext())
						{
							if (_indexToHandle.ContainsKey(key))
							{
								KillCoroutinesOnInstance(_indexToHandle[key]);
							}
						}
						else if (SlowUpdateProcesses[key.i] != null && float.IsNaN(SlowUpdateProcesses[key.i].Current))
						{
							if (ReplacementFunction != null)
							{
								SlowUpdateProcesses[key.i] = ReplacementFunction(SlowUpdateProcesses[key.i], _indexToHandle[key]);
								ReplacementFunction = null;
							}
							key.i--;
						}
						_ = ProfilerDebugAmount;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
				key.i++;
			}
		}
		if (_nextUpdateProcessSlot > 0)
		{
			ProcessIndex key2 = new ProcessIndex
			{
				seg = Segment.Update
			};
			if (UpdateTimeValues(key2.seg))
			{
				_lastUpdateProcessSlot = _nextUpdateProcessSlot;
			}
			key2.i = 0;
			while (key2.i < _lastUpdateProcessSlot)
			{
				try
				{
					if (!UpdatePaused[key2.i] && !UpdateHeld[key2.i] && UpdateProcesses[key2.i] != null && !(localTime < UpdateProcesses[key2.i].Current))
					{
						_currentCoroutine = _indexToHandle[key2];
						if (ProfilerDebugAmount != DebugInfoType.None)
						{
							_indexToHandle.ContainsKey(key2);
						}
						if (!UpdateProcesses[key2.i].MoveNext())
						{
							if (_indexToHandle.ContainsKey(key2))
							{
								KillCoroutinesOnInstance(_indexToHandle[key2]);
							}
						}
						else if (UpdateProcesses[key2.i] != null && float.IsNaN(UpdateProcesses[key2.i].Current))
						{
							if (ReplacementFunction != null)
							{
								UpdateProcesses[key2.i] = ReplacementFunction(UpdateProcesses[key2.i], _indexToHandle[key2]);
								ReplacementFunction = null;
							}
							key2.i--;
						}
						_ = ProfilerDebugAmount;
					}
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
				}
				key2.i++;
			}
		}
		_currentCoroutine = default;
		if (++_framesSinceUpdate > 64)
		{
			_framesSinceUpdate = 0;
			_ = ProfilerDebugAmount;
			RemoveUnused();
			_ = ProfilerDebugAmount;
		}
	}

	private void FixedUpdate()
	{
		if (OnPreExecute != null)
		{
			OnPreExecute();
		}
		if (_nextFixedUpdateProcessSlot <= 0)
		{
			return;
		}
		ProcessIndex key = new ProcessIndex
		{
			seg = Segment.FixedUpdate
		};
		if (UpdateTimeValues(key.seg))
		{
			_lastFixedUpdateProcessSlot = _nextFixedUpdateProcessSlot;
		}
		key.i = 0;
		while (key.i < _lastFixedUpdateProcessSlot)
		{
			try
			{
				if (!FixedUpdatePaused[key.i] && !FixedUpdateHeld[key.i] && FixedUpdateProcesses[key.i] != null && !(localTime < FixedUpdateProcesses[key.i].Current))
				{
					_currentCoroutine = _indexToHandle[key];
					if (ProfilerDebugAmount != DebugInfoType.None)
					{
						_indexToHandle.ContainsKey(key);
					}
					if (!FixedUpdateProcesses[key.i].MoveNext())
					{
						if (_indexToHandle.ContainsKey(key))
						{
							KillCoroutinesOnInstance(_indexToHandle[key]);
						}
					}
					else if (FixedUpdateProcesses[key.i] != null && float.IsNaN(FixedUpdateProcesses[key.i].Current))
					{
						if (ReplacementFunction != null)
						{
							FixedUpdateProcesses[key.i] = ReplacementFunction(FixedUpdateProcesses[key.i], _indexToHandle[key]);
							ReplacementFunction = null;
						}
						key.i--;
					}
					_ = ProfilerDebugAmount;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			key.i++;
		}
		_currentCoroutine = default;
	}

	private void LateUpdate()
	{
		if (OnPreExecute != null)
		{
			OnPreExecute();
		}
		if (_nextLateUpdateProcessSlot <= 0)
		{
			return;
		}
		ProcessIndex key = new ProcessIndex
		{
			seg = Segment.LateUpdate
		};
		if (UpdateTimeValues(key.seg))
		{
			_lastLateUpdateProcessSlot = _nextLateUpdateProcessSlot;
		}
		key.i = 0;
		while (key.i < _lastLateUpdateProcessSlot)
		{
			try
			{
				if (!LateUpdatePaused[key.i] && !LateUpdateHeld[key.i] && LateUpdateProcesses[key.i] != null && !(localTime < LateUpdateProcesses[key.i].Current))
				{
					_currentCoroutine = _indexToHandle[key];
					if (ProfilerDebugAmount != DebugInfoType.None)
					{
						_indexToHandle.ContainsKey(key);
					}
					if (!LateUpdateProcesses[key.i].MoveNext())
					{
						if (_indexToHandle.ContainsKey(key))
						{
							KillCoroutinesOnInstance(_indexToHandle[key]);
						}
					}
					else if (LateUpdateProcesses[key.i] != null && float.IsNaN(LateUpdateProcesses[key.i].Current))
					{
						if (ReplacementFunction != null)
						{
							LateUpdateProcesses[key.i] = ReplacementFunction(LateUpdateProcesses[key.i], _indexToHandle[key]);
							ReplacementFunction = null;
						}
						key.i--;
					}
					_ = ProfilerDebugAmount;
				}
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			key.i++;
		}
		_currentCoroutine = default;
	}

	private void RemoveUnused()
	{
		Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>.Enumerator enumerator = _waitingTriggers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.Count == 0)
			{
				_waitingTriggers.Remove(enumerator.Current.Key);
				enumerator = _waitingTriggers.GetEnumerator();
			}
			else if (_handleToIndex.ContainsKey(enumerator.Current.Key) && CoindexIsNull(_handleToIndex[enumerator.Current.Key]))
			{
				CloseWaitingProcess(enumerator.Current.Key);
				enumerator = _waitingTriggers.GetEnumerator();
			}
		}
		ProcessIndex key = default;
		ProcessIndex processIndex = default;
		key.seg = (processIndex.seg = Segment.Update);
		key.i = (processIndex.i = 0);
		while (key.i < _nextUpdateProcessSlot)
		{
			if (UpdateProcesses[key.i] != null)
			{
				if (key.i != processIndex.i)
				{
					UpdateProcesses[processIndex.i] = UpdateProcesses[key.i];
					UpdatePaused[processIndex.i] = UpdatePaused[key.i];
					UpdateHeld[processIndex.i] = UpdateHeld[key.i];
					if (_indexToHandle.ContainsKey(processIndex))
					{
						RemoveTag(_indexToHandle[processIndex]);
						_handleToIndex.Remove(_indexToHandle[processIndex]);
						_indexToHandle.Remove(processIndex);
					}
					_handleToIndex[_indexToHandle[key]] = processIndex;
					_indexToHandle.Add(processIndex, _indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				processIndex.i++;
			}
			key.i++;
		}
		key.i = processIndex.i;
		while (key.i < _nextUpdateProcessSlot)
		{
			UpdateProcesses[key.i] = null;
			UpdatePaused[key.i] = false;
			UpdateHeld[key.i] = false;
			if (_indexToHandle.ContainsKey(key))
			{
				RemoveTag(_indexToHandle[key]);
				_handleToIndex.Remove(_indexToHandle[key]);
				_indexToHandle.Remove(key);
			}
			key.i++;
		}
		_lastUpdateProcessSlot -= _nextUpdateProcessSlot - processIndex.i;
		UpdateCoroutines = (_nextUpdateProcessSlot = processIndex.i);
		key.seg = (processIndex.seg = Segment.FixedUpdate);
		key.i = (processIndex.i = 0);
		while (key.i < _nextFixedUpdateProcessSlot)
		{
			if (FixedUpdateProcesses[key.i] != null)
			{
				if (key.i != processIndex.i)
				{
					FixedUpdateProcesses[processIndex.i] = FixedUpdateProcesses[key.i];
					FixedUpdatePaused[processIndex.i] = FixedUpdatePaused[key.i];
					FixedUpdateHeld[processIndex.i] = FixedUpdateHeld[key.i];
					if (_indexToHandle.ContainsKey(processIndex))
					{
						RemoveTag(_indexToHandle[processIndex]);
						_handleToIndex.Remove(_indexToHandle[processIndex]);
						_indexToHandle.Remove(processIndex);
					}
					_handleToIndex[_indexToHandle[key]] = processIndex;
					_indexToHandle.Add(processIndex, _indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				processIndex.i++;
			}
			key.i++;
		}
		key.i = processIndex.i;
		while (key.i < _nextFixedUpdateProcessSlot)
		{
			FixedUpdateProcesses[key.i] = null;
			FixedUpdatePaused[key.i] = false;
			FixedUpdateHeld[key.i] = false;
			if (_indexToHandle.ContainsKey(key))
			{
				RemoveTag(_indexToHandle[key]);
				_handleToIndex.Remove(_indexToHandle[key]);
				_indexToHandle.Remove(key);
			}
			key.i++;
		}
		_lastFixedUpdateProcessSlot -= _nextFixedUpdateProcessSlot - processIndex.i;
		FixedUpdateCoroutines = (_nextFixedUpdateProcessSlot = processIndex.i);
		key.seg = (processIndex.seg = Segment.LateUpdate);
		key.i = (processIndex.i = 0);
		while (key.i < _nextLateUpdateProcessSlot)
		{
			if (LateUpdateProcesses[key.i] != null)
			{
				if (key.i != processIndex.i)
				{
					LateUpdateProcesses[processIndex.i] = LateUpdateProcesses[key.i];
					LateUpdatePaused[processIndex.i] = LateUpdatePaused[key.i];
					LateUpdateHeld[processIndex.i] = LateUpdateHeld[key.i];
					if (_indexToHandle.ContainsKey(processIndex))
					{
						RemoveTag(_indexToHandle[processIndex]);
						_handleToIndex.Remove(_indexToHandle[processIndex]);
						_indexToHandle.Remove(processIndex);
					}
					_handleToIndex[_indexToHandle[key]] = processIndex;
					_indexToHandle.Add(processIndex, _indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				processIndex.i++;
			}
			key.i++;
		}
		key.i = processIndex.i;
		while (key.i < _nextLateUpdateProcessSlot)
		{
			LateUpdateProcesses[key.i] = null;
			LateUpdatePaused[key.i] = false;
			LateUpdateHeld[key.i] = false;
			if (_indexToHandle.ContainsKey(key))
			{
				RemoveTag(_indexToHandle[key]);
				_handleToIndex.Remove(_indexToHandle[key]);
				_indexToHandle.Remove(key);
			}
			key.i++;
		}
		_lastLateUpdateProcessSlot -= _nextLateUpdateProcessSlot - processIndex.i;
		LateUpdateCoroutines = (_nextLateUpdateProcessSlot = processIndex.i);
		key.seg = (processIndex.seg = Segment.SlowUpdate);
		key.i = (processIndex.i = 0);
		while (key.i < _nextSlowUpdateProcessSlot)
		{
			if (SlowUpdateProcesses[key.i] != null)
			{
				if (key.i != processIndex.i)
				{
					SlowUpdateProcesses[processIndex.i] = SlowUpdateProcesses[key.i];
					SlowUpdatePaused[processIndex.i] = SlowUpdatePaused[key.i];
					SlowUpdateHeld[processIndex.i] = SlowUpdateHeld[key.i];
					if (_indexToHandle.ContainsKey(processIndex))
					{
						RemoveTag(_indexToHandle[processIndex]);
						_handleToIndex.Remove(_indexToHandle[processIndex]);
						_indexToHandle.Remove(processIndex);
					}
					_handleToIndex[_indexToHandle[key]] = processIndex;
					_indexToHandle.Add(processIndex, _indexToHandle[key]);
					_indexToHandle.Remove(key);
				}
				processIndex.i++;
			}
			key.i++;
		}
		key.i = processIndex.i;
		while (key.i < _nextSlowUpdateProcessSlot)
		{
			SlowUpdateProcesses[key.i] = null;
			SlowUpdatePaused[key.i] = false;
			SlowUpdateHeld[key.i] = false;
			if (_indexToHandle.ContainsKey(key))
			{
				RemoveTag(_indexToHandle[key]);
				_handleToIndex.Remove(_indexToHandle[key]);
				_indexToHandle.Remove(key);
			}
			key.i++;
		}
		_lastSlowUpdateProcessSlot -= _nextSlowUpdateProcessSlot - processIndex.i;
		SlowUpdateCoroutines = (_nextSlowUpdateProcessSlot = processIndex.i);
	}

	public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine)
	{
		if (coroutine != null)
		{
			return Instance.RunCoroutineInternal(coroutine, Segment.Update, null, new CoroutineHandle(Instance._instanceID), prewarm: true);
		}
		return default;
	}

	public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, string tag)
	{
		if (coroutine != null)
		{
			return Instance.RunCoroutineInternal(coroutine, Segment.Update, tag, new CoroutineHandle(Instance._instanceID), prewarm: true);
		}
		return default;
	}

	public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment)
	{
		if (coroutine != null)
		{
			return Instance.RunCoroutineInternal(coroutine, segment, null, new CoroutineHandle(Instance._instanceID), prewarm: true);
		}
		return default;
	}

	public static CoroutineHandle RunCoroutine(IEnumerator<float> coroutine, Segment segment, string tag)
	{
		if (coroutine != null)
		{
			return Instance.RunCoroutineInternal(coroutine, segment, tag, new CoroutineHandle(Instance._instanceID), prewarm: true);
		}
		return default;
	}

	public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine)
	{
		if (coroutine != null)
		{
			return RunCoroutineInternal(coroutine, Segment.Update, null, new CoroutineHandle(_instanceID), prewarm: true);
		}
		return default;
	}

	public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, string tag)
	{
		if (coroutine != null)
		{
			return RunCoroutineInternal(coroutine, Segment.Update, tag, new CoroutineHandle(_instanceID), prewarm: true);
		}
		return default;
	}

	public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment)
	{
		if (coroutine != null)
		{
			return RunCoroutineInternal(coroutine, segment, null, new CoroutineHandle(_instanceID), prewarm: true);
		}
		return default;
	}

	public CoroutineHandle RunCoroutineOnInstance(IEnumerator<float> coroutine, Segment segment, string tag)
	{
		if (coroutine != null)
		{
			return RunCoroutineInternal(coroutine, segment, tag, new CoroutineHandle(_instanceID), prewarm: true);
		}
		return default;
	}

	private CoroutineHandle RunCoroutineInternal(IEnumerator<float> coroutine, Segment segment, string tag, CoroutineHandle handle, bool prewarm)
	{
		ProcessIndex processIndex = new ProcessIndex
		{
			seg = segment
		};
		if (_handleToIndex.ContainsKey(handle))
		{
			_indexToHandle.Remove(_handleToIndex[handle]);
			_handleToIndex.Remove(handle);
		}
		float num = localTime;
		float num2 = deltaTime;
		CoroutineHandle coroutineHandle = _currentCoroutine;
		_currentCoroutine = handle;
		switch (segment)
		{
		case Segment.Update:
			if (_nextUpdateProcessSlot >= UpdateProcesses.Length)
			{
				IEnumerator<float>[] updateProcesses = UpdateProcesses;
				bool[] updatePaused = UpdatePaused;
				bool[] updateHeld = UpdateHeld;
				UpdateProcesses = new IEnumerator<float>[UpdateProcesses.Length + 64 * _expansions++];
				UpdatePaused = new bool[UpdateProcesses.Length];
				UpdateHeld = new bool[UpdateProcesses.Length];
				for (int k = 0; k < updateProcesses.Length; k++)
				{
					UpdateProcesses[k] = updateProcesses[k];
					UpdatePaused[k] = updatePaused[k];
					UpdateHeld[k] = updateHeld[k];
				}
			}
			if (UpdateTimeValues(processIndex.seg))
			{
				_lastUpdateProcessSlot = _nextUpdateProcessSlot;
			}
			processIndex.i = _nextUpdateProcessSlot++;
			UpdateProcesses[processIndex.i] = coroutine;
			if (tag != null)
			{
				AddTag(tag, handle);
			}
			_indexToHandle.Add(processIndex, handle);
			_handleToIndex.Add(handle, processIndex);
			while (prewarm)
			{
				if (!UpdateProcesses[processIndex.i].MoveNext())
				{
					if (_indexToHandle.ContainsKey(processIndex))
					{
						KillCoroutinesOnInstance(_indexToHandle[processIndex]);
					}
					prewarm = false;
				}
				else if (UpdateProcesses[processIndex.i] != null && float.IsNaN(UpdateProcesses[processIndex.i].Current))
				{
					if (ReplacementFunction != null)
					{
						UpdateProcesses[processIndex.i] = ReplacementFunction(UpdateProcesses[processIndex.i], _indexToHandle[processIndex]);
						ReplacementFunction = null;
					}
					prewarm = !UpdatePaused[processIndex.i] || !UpdateHeld[processIndex.i];
				}
				else
				{
					prewarm = false;
				}
			}
			break;
		case Segment.FixedUpdate:
			if (_nextFixedUpdateProcessSlot >= FixedUpdateProcesses.Length)
			{
				IEnumerator<float>[] fixedUpdateProcesses = FixedUpdateProcesses;
				bool[] fixedUpdatePaused = FixedUpdatePaused;
				bool[] fixedUpdateHeld = FixedUpdateHeld;
				FixedUpdateProcesses = new IEnumerator<float>[FixedUpdateProcesses.Length + 64 * _expansions++];
				FixedUpdatePaused = new bool[FixedUpdateProcesses.Length];
				FixedUpdateHeld = new bool[FixedUpdateProcesses.Length];
				for (int l = 0; l < fixedUpdateProcesses.Length; l++)
				{
					FixedUpdateProcesses[l] = fixedUpdateProcesses[l];
					FixedUpdatePaused[l] = fixedUpdatePaused[l];
					FixedUpdateHeld[l] = fixedUpdateHeld[l];
				}
			}
			if (UpdateTimeValues(processIndex.seg))
			{
				_lastFixedUpdateProcessSlot = _nextFixedUpdateProcessSlot;
			}
			processIndex.i = _nextFixedUpdateProcessSlot++;
			FixedUpdateProcesses[processIndex.i] = coroutine;
			if (tag != null)
			{
				AddTag(tag, handle);
			}
			_indexToHandle.Add(processIndex, handle);
			_handleToIndex.Add(handle, processIndex);
			while (prewarm)
			{
				if (!FixedUpdateProcesses[processIndex.i].MoveNext())
				{
					if (_indexToHandle.ContainsKey(processIndex))
					{
						KillCoroutinesOnInstance(_indexToHandle[processIndex]);
					}
					prewarm = false;
				}
				else if (FixedUpdateProcesses[processIndex.i] != null && float.IsNaN(FixedUpdateProcesses[processIndex.i].Current))
				{
					if (ReplacementFunction != null)
					{
						FixedUpdateProcesses[processIndex.i] = ReplacementFunction(FixedUpdateProcesses[processIndex.i], _indexToHandle[processIndex]);
						ReplacementFunction = null;
					}
					prewarm = !FixedUpdatePaused[processIndex.i] || !FixedUpdateHeld[processIndex.i];
				}
				else
				{
					prewarm = false;
				}
			}
			break;
		case Segment.LateUpdate:
			if (_nextLateUpdateProcessSlot >= LateUpdateProcesses.Length)
			{
				IEnumerator<float>[] lateUpdateProcesses = LateUpdateProcesses;
				bool[] lateUpdatePaused = LateUpdatePaused;
				bool[] lateUpdateHeld = LateUpdateHeld;
				LateUpdateProcesses = new IEnumerator<float>[LateUpdateProcesses.Length + 64 * _expansions++];
				LateUpdatePaused = new bool[LateUpdateProcesses.Length];
				LateUpdateHeld = new bool[LateUpdateProcesses.Length];
				for (int j = 0; j < lateUpdateProcesses.Length; j++)
				{
					LateUpdateProcesses[j] = lateUpdateProcesses[j];
					LateUpdatePaused[j] = lateUpdatePaused[j];
					LateUpdateHeld[j] = lateUpdateHeld[j];
				}
			}
			if (UpdateTimeValues(processIndex.seg))
			{
				_lastLateUpdateProcessSlot = _nextLateUpdateProcessSlot;
			}
			processIndex.i = _nextLateUpdateProcessSlot++;
			LateUpdateProcesses[processIndex.i] = coroutine;
			if (tag != null)
			{
				AddTag(tag, handle);
			}
			_indexToHandle.Add(processIndex, handle);
			_handleToIndex.Add(handle, processIndex);
			while (prewarm)
			{
				if (!LateUpdateProcesses[processIndex.i].MoveNext())
				{
					if (_indexToHandle.ContainsKey(processIndex))
					{
						KillCoroutinesOnInstance(_indexToHandle[processIndex]);
					}
					prewarm = false;
				}
				else if (LateUpdateProcesses[processIndex.i] != null && float.IsNaN(LateUpdateProcesses[processIndex.i].Current))
				{
					if (ReplacementFunction != null)
					{
						LateUpdateProcesses[processIndex.i] = ReplacementFunction(LateUpdateProcesses[processIndex.i], _indexToHandle[processIndex]);
						ReplacementFunction = null;
					}
					prewarm = !LateUpdatePaused[processIndex.i] || !LateUpdateHeld[processIndex.i];
				}
				else
				{
					prewarm = false;
				}
			}
			break;
		case Segment.SlowUpdate:
			if (_nextSlowUpdateProcessSlot >= SlowUpdateProcesses.Length)
			{
				IEnumerator<float>[] slowUpdateProcesses = SlowUpdateProcesses;
				bool[] slowUpdatePaused = SlowUpdatePaused;
				bool[] slowUpdateHeld = SlowUpdateHeld;
				SlowUpdateProcesses = new IEnumerator<float>[SlowUpdateProcesses.Length + 64 * _expansions++];
				SlowUpdatePaused = new bool[SlowUpdateProcesses.Length];
				SlowUpdateHeld = new bool[SlowUpdateProcesses.Length];
				for (int i = 0; i < slowUpdateProcesses.Length; i++)
				{
					SlowUpdateProcesses[i] = slowUpdateProcesses[i];
					SlowUpdatePaused[i] = slowUpdatePaused[i];
					SlowUpdateHeld[i] = slowUpdateHeld[i];
				}
			}
			if (UpdateTimeValues(processIndex.seg))
			{
				_lastSlowUpdateProcessSlot = _nextSlowUpdateProcessSlot;
			}
			processIndex.i = _nextSlowUpdateProcessSlot++;
			SlowUpdateProcesses[processIndex.i] = coroutine;
			if (tag != null)
			{
				AddTag(tag, handle);
			}
			_indexToHandle.Add(processIndex, handle);
			_handleToIndex.Add(handle, processIndex);
			while (prewarm)
			{
				if (!SlowUpdateProcesses[processIndex.i].MoveNext())
				{
					if (_indexToHandle.ContainsKey(processIndex))
					{
						KillCoroutinesOnInstance(_indexToHandle[processIndex]);
					}
					prewarm = false;
				}
				else if (SlowUpdateProcesses[processIndex.i] != null && float.IsNaN(SlowUpdateProcesses[processIndex.i].Current))
				{
					if (ReplacementFunction != null)
					{
						SlowUpdateProcesses[processIndex.i] = ReplacementFunction(SlowUpdateProcesses[processIndex.i], _indexToHandle[processIndex]);
						ReplacementFunction = null;
					}
					prewarm = !SlowUpdatePaused[processIndex.i] || !SlowUpdateHeld[processIndex.i];
				}
				else
				{
					prewarm = false;
				}
			}
			break;
		default:
			handle = default;
			break;
		}
		localTime = num;
		deltaTime = num2;
		_currentCoroutine = coroutineHandle;
		return handle;
	}

	public static int KillCoroutines()
	{
		if (!(_instance == null))
		{
			return _instance.KillCoroutinesOnInstance();
		}
		return 0;
	}

	public int KillCoroutinesOnInstance()
	{
		int result = _nextUpdateProcessSlot + _nextLateUpdateProcessSlot + _nextFixedUpdateProcessSlot + _nextSlowUpdateProcessSlot;
		UpdateProcesses = new IEnumerator<float>[256];
		UpdatePaused = new bool[256];
		UpdateHeld = new bool[256];
		UpdateCoroutines = 0;
		_nextUpdateProcessSlot = 0;
		LateUpdateProcesses = new IEnumerator<float>[8];
		LateUpdatePaused = new bool[8];
		LateUpdateHeld = new bool[8];
		LateUpdateCoroutines = 0;
		_nextLateUpdateProcessSlot = 0;
		FixedUpdateProcesses = new IEnumerator<float>[64];
		FixedUpdatePaused = new bool[64];
		FixedUpdateHeld = new bool[64];
		FixedUpdateCoroutines = 0;
		_nextFixedUpdateProcessSlot = 0;
		SlowUpdateProcesses = new IEnumerator<float>[64];
		SlowUpdatePaused = new bool[64];
		SlowUpdateHeld = new bool[64];
		SlowUpdateCoroutines = 0;
		_nextSlowUpdateProcessSlot = 0;
		_processTags.Clear();
		_taggedProcesses.Clear();
		_handleToIndex.Clear();
		_indexToHandle.Clear();
		_waitingTriggers.Clear();
		_expansions = (ushort)(_expansions / 2 + 1);
		return result;
	}

	public static int KillCoroutines(CoroutineHandle handle)
	{
		if (!(ActiveInstances[handle.Key] != null))
		{
			return 0;
		}
		return GetInstance(handle.Key).KillCoroutinesOnInstance(handle);
	}

	public int KillCoroutinesOnInstance(CoroutineHandle handle)
	{
		bool flag = false;
		if (_handleToIndex.ContainsKey(handle))
		{
			if (_waitingTriggers.ContainsKey(handle))
			{
				CloseWaitingProcess(handle);
			}
			flag = CoindexExtract(_handleToIndex[handle]) != null;
			RemoveTag(handle);
		}
		if (!flag)
		{
			return 0;
		}
		return 1;
	}

	public static int KillCoroutines(string tag)
	{
		if (!(_instance == null))
		{
			return _instance.KillCoroutinesOnInstance(tag);
		}
		return 0;
	}

	public int KillCoroutinesOnInstance(string tag)
	{
		if (tag == null)
		{
			return 0;
		}
		int num = 0;
		while (_taggedProcesses.ContainsKey(tag))
		{
			HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
			enumerator.MoveNext();
			if (Nullify(_handleToIndex[enumerator.Current]))
			{
				if (_waitingTriggers.ContainsKey(enumerator.Current))
				{
					CloseWaitingProcess(enumerator.Current);
				}
				num++;
			}
			RemoveTag(enumerator.Current);
			if (_handleToIndex.ContainsKey(enumerator.Current))
			{
				_indexToHandle.Remove(_handleToIndex[enumerator.Current]);
				_handleToIndex.Remove(enumerator.Current);
			}
		}
		return num;
	}

	public static int PauseCoroutines()
	{
		if (!(_instance == null))
		{
			return _instance.PauseCoroutinesOnInstance();
		}
		return 0;
	}

	public int PauseCoroutinesOnInstance()
	{
		int num = 0;
		for (int i = 0; i < _nextUpdateProcessSlot; i++)
		{
			if (!UpdatePaused[i] && UpdateProcesses[i] != null)
			{
				num++;
				UpdatePaused[i] = true;
				if (UpdateProcesses[i].Current > GetSegmentTime(Segment.Update))
				{
					UpdateProcesses[i] = _InjectDelay(UpdateProcesses[i], UpdateProcesses[i].Current - GetSegmentTime(Segment.Update));
				}
			}
		}
		for (int i = 0; i < _nextLateUpdateProcessSlot; i++)
		{
			if (!LateUpdatePaused[i] && LateUpdateProcesses[i] != null)
			{
				num++;
				LateUpdatePaused[i] = true;
				if (LateUpdateProcesses[i].Current > GetSegmentTime(Segment.LateUpdate))
				{
					LateUpdateProcesses[i] = _InjectDelay(LateUpdateProcesses[i], LateUpdateProcesses[i].Current - GetSegmentTime(Segment.LateUpdate));
				}
			}
		}
		for (int i = 0; i < _nextFixedUpdateProcessSlot; i++)
		{
			if (!FixedUpdatePaused[i] && FixedUpdateProcesses[i] != null)
			{
				num++;
				FixedUpdatePaused[i] = true;
				if (FixedUpdateProcesses[i].Current > GetSegmentTime(Segment.FixedUpdate))
				{
					FixedUpdateProcesses[i] = _InjectDelay(FixedUpdateProcesses[i], FixedUpdateProcesses[i].Current - GetSegmentTime(Segment.FixedUpdate));
				}
			}
		}
		for (int i = 0; i < _nextSlowUpdateProcessSlot; i++)
		{
			if (!SlowUpdatePaused[i] && SlowUpdateProcesses[i] != null)
			{
				num++;
				SlowUpdatePaused[i] = true;
				if (SlowUpdateProcesses[i].Current > GetSegmentTime(Segment.SlowUpdate))
				{
					SlowUpdateProcesses[i] = _InjectDelay(SlowUpdateProcesses[i], SlowUpdateProcesses[i].Current - GetSegmentTime(Segment.SlowUpdate));
				}
			}
		}
		return num;
	}

	public static int PauseCoroutines(CoroutineHandle handle)
	{
		if (!(ActiveInstances[handle.Key] != null))
		{
			return 0;
		}
		return GetInstance(handle.Key).PauseCoroutinesOnInstance(handle);
	}

	public int PauseCoroutinesOnInstance(CoroutineHandle handle)
	{
		if (!_handleToIndex.ContainsKey(handle) || CoindexIsNull(_handleToIndex[handle]) || SetPause(_handleToIndex[handle], newPausedState: true))
		{
			return 0;
		}
		return 1;
	}

	public static int PauseCoroutines(string tag)
	{
		if (!(_instance == null))
		{
			return _instance.PauseCoroutinesOnInstance(tag);
		}
		return 0;
	}

	public int PauseCoroutinesOnInstance(string tag)
	{
		if (tag == null || !_taggedProcesses.ContainsKey(tag))
		{
			return 0;
		}
		int num = 0;
		HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!CoindexIsNull(_handleToIndex[enumerator.Current]) && !SetPause(_handleToIndex[enumerator.Current], newPausedState: true))
			{
				num++;
			}
		}
		return num;
	}

	public static int ResumeCoroutines()
	{
		if (!(_instance == null))
		{
			return _instance.ResumeCoroutinesOnInstance();
		}
		return 0;
	}

	public int ResumeCoroutinesOnInstance()
	{
		int num = 0;
		ProcessIndex processIndex = default;
		processIndex.i = 0;
		processIndex.seg = Segment.Update;
		while (processIndex.i < _nextUpdateProcessSlot)
		{
			if (UpdatePaused[processIndex.i] && UpdateProcesses[processIndex.i] != null)
			{
				UpdatePaused[processIndex.i] = false;
				num++;
			}
			processIndex.i++;
		}
		processIndex.i = 0;
		processIndex.seg = Segment.LateUpdate;
		while (processIndex.i < _nextLateUpdateProcessSlot)
		{
			if (LateUpdatePaused[processIndex.i] && LateUpdateProcesses[processIndex.i] != null)
			{
				LateUpdatePaused[processIndex.i] = false;
				num++;
			}
			processIndex.i++;
		}
		processIndex.i = 0;
		processIndex.seg = Segment.FixedUpdate;
		while (processIndex.i < _nextFixedUpdateProcessSlot)
		{
			if (FixedUpdatePaused[processIndex.i] && FixedUpdateProcesses[processIndex.i] != null)
			{
				FixedUpdatePaused[processIndex.i] = false;
				num++;
			}
			processIndex.i++;
		}
		processIndex.i = 0;
		processIndex.seg = Segment.SlowUpdate;
		while (processIndex.i < _nextSlowUpdateProcessSlot)
		{
			if (SlowUpdatePaused[processIndex.i] && SlowUpdateProcesses[processIndex.i] != null)
			{
				SlowUpdatePaused[processIndex.i] = false;
				num++;
			}
			processIndex.i++;
		}
		return num;
	}

	public static int ResumeCoroutines(CoroutineHandle handle)
	{
		if (!(ActiveInstances[handle.Key] != null))
		{
			return 0;
		}
		return GetInstance(handle.Key).ResumeCoroutinesOnInstance(handle);
	}

	public int ResumeCoroutinesOnInstance(CoroutineHandle handle)
	{
		if (!_handleToIndex.ContainsKey(handle) || CoindexIsNull(_handleToIndex[handle]) || !SetPause(_handleToIndex[handle], newPausedState: false))
		{
			return 0;
		}
		return 1;
	}

	public static int ResumeCoroutines(string tag)
	{
		if (!(_instance == null))
		{
			return _instance.ResumeCoroutinesOnInstance(tag);
		}
		return 0;
	}

	public int ResumeCoroutinesOnInstance(string tag)
	{
		if (tag == null || !_taggedProcesses.ContainsKey(tag))
		{
			return 0;
		}
		int num = 0;
		HashSet<CoroutineHandle>.Enumerator enumerator = _taggedProcesses[tag].GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (!CoindexIsNull(_handleToIndex[enumerator.Current]) && SetPause(_handleToIndex[enumerator.Current], newPausedState: false))
			{
				num++;
			}
		}
		return num;
	}

	private bool UpdateTimeValues(Segment segment)
	{
		switch (segment)
		{
		case Segment.Update:
			if (_currentUpdateFrame != Time.frameCount)
			{
				deltaTime = Timekeeper.SafeDeltaTime();
				_lastUpdateTime += deltaTime;
				localTime = _lastUpdateTime;
				_currentUpdateFrame = Time.frameCount;
				return true;
			}
			deltaTime = Timekeeper.SafeDeltaTime();
			localTime = _lastUpdateTime;
			return false;
		case Segment.LateUpdate:
			if (_currentLateUpdateFrame != Time.frameCount)
			{
				deltaTime = Timekeeper.SafeDeltaTime();
				_lastLateUpdateTime += deltaTime;
				localTime = _lastLateUpdateTime;
				_currentLateUpdateFrame = Time.frameCount;
				return true;
			}
			deltaTime = Timekeeper.SafeDeltaTime();
			localTime = _lastLateUpdateTime;
			return false;
		case Segment.FixedUpdate:
			deltaTime = Timekeeper.SafeFixedDeltaTime();
			localTime = Time.fixedTime;
			if (_lastFixedUpdateTime + 0.0001f < Time.fixedTime)
			{
				_lastFixedUpdateTime = Time.fixedTime;
				return true;
			}
			return false;
		case Segment.SlowUpdate:
			if (_currentSlowUpdateFrame != Time.frameCount)
			{
				deltaTime = (_lastSlowUpdateDeltaTime = Time.realtimeSinceStartup - _lastSlowUpdateTime);
				localTime = (_lastSlowUpdateTime = Time.realtimeSinceStartup);
				_currentSlowUpdateFrame = Time.frameCount;
				return true;
			}
			deltaTime = _lastSlowUpdateDeltaTime;
			localTime = _lastSlowUpdateTime;
			return false;
		default:
			return true;
		}
	}

	private float GetSegmentTime(Segment segment)
	{
		switch (segment)
		{
		case Segment.Update:
			if (_currentUpdateFrame == Time.frameCount)
			{
				return _lastUpdateTime;
			}
			return _lastUpdateTime + Timekeeper.SafeDeltaTime();
		case Segment.LateUpdate:
			if (_currentUpdateFrame == Time.frameCount)
			{
				return _lastLateUpdateTime;
			}
			return _lastLateUpdateTime + Timekeeper.SafeDeltaTime();
		case Segment.FixedUpdate:
			return Time.fixedTime;
		case Segment.SlowUpdate:
			return Time.realtimeSinceStartup;
		default:
			return 0f;
		}
	}

	public static Timing GetInstance(byte ID)
	{
		if (ID >= 16)
		{
			return null;
		}
		return ActiveInstances[ID];
	}

	private void AddTag(string tag, CoroutineHandle coindex)
	{
		_processTags.Add(coindex, tag);
		if (_taggedProcesses.ContainsKey(tag))
		{
			_taggedProcesses[tag].Add(coindex);
			return;
		}
		_taggedProcesses.Add(tag, new HashSet<CoroutineHandle> { coindex });
	}

	private void RemoveTag(CoroutineHandle coindex)
	{
		if (_processTags.ContainsKey(coindex))
		{
			if (_taggedProcesses[_processTags[coindex]].Count > 1)
			{
				_taggedProcesses[_processTags[coindex]].Remove(coindex);
			}
			else
			{
				_taggedProcesses.Remove(_processTags[coindex]);
			}
			_processTags.Remove(coindex);
		}
	}

	private bool Nullify(ProcessIndex coindex)
	{
		switch (coindex.seg)
		{
		case Segment.Update:
		{
			bool result4 = UpdateProcesses[coindex.i] != null;
			UpdateProcesses[coindex.i] = null;
			return result4;
		}
		case Segment.FixedUpdate:
		{
			bool result3 = FixedUpdateProcesses[coindex.i] != null;
			FixedUpdateProcesses[coindex.i] = null;
			return result3;
		}
		case Segment.LateUpdate:
		{
			bool result2 = LateUpdateProcesses[coindex.i] != null;
			LateUpdateProcesses[coindex.i] = null;
			return result2;
		}
		case Segment.SlowUpdate:
		{
			bool result = SlowUpdateProcesses[coindex.i] != null;
			SlowUpdateProcesses[coindex.i] = null;
			return result;
		}
		default:
			return false;
		}
	}

	private IEnumerator<float> CoindexExtract(ProcessIndex coindex)
	{
		switch (coindex.seg)
		{
		case Segment.Update:
		{
			IEnumerator<float> result4 = UpdateProcesses[coindex.i];
			UpdateProcesses[coindex.i] = null;
			return result4;
		}
		case Segment.FixedUpdate:
		{
			IEnumerator<float> result3 = FixedUpdateProcesses[coindex.i];
			FixedUpdateProcesses[coindex.i] = null;
			return result3;
		}
		case Segment.LateUpdate:
		{
			IEnumerator<float> result2 = LateUpdateProcesses[coindex.i];
			LateUpdateProcesses[coindex.i] = null;
			return result2;
		}
		case Segment.SlowUpdate:
		{
			IEnumerator<float> result = SlowUpdateProcesses[coindex.i];
			SlowUpdateProcesses[coindex.i] = null;
			return result;
		}
		default:
			return null;
		}
	}

	private IEnumerator<float> CoindexPeek(ProcessIndex coindex)
	{
		return coindex.seg switch
		{
			Segment.Update => UpdateProcesses[coindex.i], 
			Segment.FixedUpdate => FixedUpdateProcesses[coindex.i], 
			Segment.LateUpdate => LateUpdateProcesses[coindex.i], 
			Segment.SlowUpdate => SlowUpdateProcesses[coindex.i], 
			_ => null, 
		};
	}

	private bool CoindexIsNull(ProcessIndex coindex)
	{
		return coindex.seg switch
		{
			Segment.Update => UpdateProcesses[coindex.i] == null, 
			Segment.FixedUpdate => FixedUpdateProcesses[coindex.i] == null, 
			Segment.LateUpdate => LateUpdateProcesses[coindex.i] == null, 
			Segment.SlowUpdate => SlowUpdateProcesses[coindex.i] == null, 
			_ => true, 
		};
	}

	private bool SetPause(ProcessIndex coindex, bool newPausedState)
	{
		if (CoindexPeek(coindex) == null)
		{
			return false;
		}
		switch (coindex.seg)
		{
		case Segment.Update:
		{
			bool result3 = UpdatePaused[coindex.i];
			UpdatePaused[coindex.i] = newPausedState;
			if (newPausedState && UpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
			{
				UpdateProcesses[coindex.i] = _InjectDelay(UpdateProcesses[coindex.i], UpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
			}
			return result3;
		}
		case Segment.FixedUpdate:
		{
			bool result2 = FixedUpdatePaused[coindex.i];
			FixedUpdatePaused[coindex.i] = newPausedState;
			if (newPausedState && FixedUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
			{
				FixedUpdateProcesses[coindex.i] = _InjectDelay(FixedUpdateProcesses[coindex.i], FixedUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
			}
			return result2;
		}
		case Segment.LateUpdate:
		{
			bool result4 = LateUpdatePaused[coindex.i];
			LateUpdatePaused[coindex.i] = newPausedState;
			if (newPausedState && LateUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
			{
				LateUpdateProcesses[coindex.i] = _InjectDelay(LateUpdateProcesses[coindex.i], LateUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
			}
			return result4;
		}
		case Segment.SlowUpdate:
		{
			bool result = SlowUpdatePaused[coindex.i];
			SlowUpdatePaused[coindex.i] = newPausedState;
			if (newPausedState && SlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
			{
				SlowUpdateProcesses[coindex.i] = _InjectDelay(SlowUpdateProcesses[coindex.i], SlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
			}
			return result;
		}
		default:
			return false;
		}
	}

	private bool SetHeld(ProcessIndex coindex, bool newHeldState)
	{
		if (CoindexPeek(coindex) == null)
		{
			return false;
		}
		switch (coindex.seg)
		{
		case Segment.Update:
		{
			bool result3 = UpdateHeld[coindex.i];
			UpdateHeld[coindex.i] = newHeldState;
			if (newHeldState && UpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
			{
				UpdateProcesses[coindex.i] = _InjectDelay(UpdateProcesses[coindex.i], UpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
			}
			return result3;
		}
		case Segment.FixedUpdate:
		{
			bool result2 = FixedUpdateHeld[coindex.i];
			FixedUpdateHeld[coindex.i] = newHeldState;
			if (newHeldState && FixedUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
			{
				FixedUpdateProcesses[coindex.i] = _InjectDelay(FixedUpdateProcesses[coindex.i], FixedUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
			}
			return result2;
		}
		case Segment.LateUpdate:
		{
			bool result4 = LateUpdateHeld[coindex.i];
			LateUpdateHeld[coindex.i] = newHeldState;
			if (newHeldState && LateUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
			{
				LateUpdateProcesses[coindex.i] = _InjectDelay(LateUpdateProcesses[coindex.i], LateUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
			}
			return result4;
		}
		case Segment.SlowUpdate:
		{
			bool result = SlowUpdateHeld[coindex.i];
			SlowUpdateHeld[coindex.i] = newHeldState;
			if (newHeldState && SlowUpdateProcesses[coindex.i].Current > GetSegmentTime(coindex.seg))
			{
				SlowUpdateProcesses[coindex.i] = _InjectDelay(SlowUpdateProcesses[coindex.i], SlowUpdateProcesses[coindex.i].Current - GetSegmentTime(coindex.seg));
			}
			return result;
		}
		default:
			return false;
		}
	}

	private IEnumerator<float> _InjectDelay(IEnumerator<float> proc, float delayTime)
	{
		yield return WaitForSecondsOnInstance(delayTime);
		_tmpRef = proc;
		ReplacementFunction = ReturnTmpRefForRepFunc;
		yield return float.NaN;
	}

	private bool CoindexIsPaused(ProcessIndex coindex)
	{
		return coindex.seg switch
		{
			Segment.Update => UpdatePaused[coindex.i], 
			Segment.FixedUpdate => FixedUpdatePaused[coindex.i], 
			Segment.LateUpdate => LateUpdatePaused[coindex.i], 
			Segment.SlowUpdate => SlowUpdatePaused[coindex.i], 
			_ => false, 
		};
	}

	private bool CoindexIsHeld(ProcessIndex coindex)
	{
		return coindex.seg switch
		{
			Segment.Update => UpdateHeld[coindex.i], 
			Segment.FixedUpdate => FixedUpdateHeld[coindex.i], 
			Segment.LateUpdate => LateUpdateHeld[coindex.i], 
			Segment.SlowUpdate => SlowUpdateHeld[coindex.i], 
			_ => false, 
		};
	}

	private void CoindexReplace(ProcessIndex coindex, IEnumerator<float> replacement)
	{
		switch (coindex.seg)
		{
		case Segment.Update:
			UpdateProcesses[coindex.i] = replacement;
			break;
		case Segment.FixedUpdate:
			FixedUpdateProcesses[coindex.i] = replacement;
			break;
		case Segment.LateUpdate:
			LateUpdateProcesses[coindex.i] = replacement;
			break;
		case Segment.SlowUpdate:
			SlowUpdateProcesses[coindex.i] = replacement;
			break;
		}
	}

	public static float WaitForSeconds(float waitTime)
	{
		if (float.IsNaN(waitTime))
		{
			waitTime = 0f;
		}
		return LocalTime + waitTime;
	}

	public float WaitForSecondsOnInstance(float waitTime)
	{
		if (float.IsNaN(waitTime))
		{
			waitTime = 0f;
		}
		return localTime + waitTime;
	}

	public static float WaitUntilDone(CoroutineHandle otherCoroutine)
	{
		return WaitUntilDone(otherCoroutine, warnOnIssue: true);
	}

	public static float WaitUntilDone(CoroutineHandle otherCoroutine, bool warnOnIssue)
	{
		Timing instance = GetInstance(otherCoroutine.Key);
		if (instance != null && instance._handleToIndex.ContainsKey(otherCoroutine))
		{
			if (instance.CoindexIsNull(instance._handleToIndex[otherCoroutine]))
			{
				return 0f;
			}
			if (!instance._waitingTriggers.ContainsKey(otherCoroutine))
			{
				instance.CoindexReplace(instance._handleToIndex[otherCoroutine], instance._StartWhenDone(otherCoroutine, instance.CoindexPeek(instance._handleToIndex[otherCoroutine])));
				instance._waitingTriggers.Add(otherCoroutine, new HashSet<CoroutineHandle>());
			}
			if (instance._currentCoroutine == otherCoroutine)
			{
				return float.NegativeInfinity;
			}
			if (!instance._currentCoroutine.IsValid)
			{
				return float.NegativeInfinity;
			}
			instance._waitingTriggers[otherCoroutine].Add(instance._currentCoroutine);
			if (!instance._allWaiting.Contains(instance._currentCoroutine))
			{
				instance._allWaiting.Add(instance._currentCoroutine);
			}
			instance.SetHeld(instance._handleToIndex[instance._currentCoroutine], newHeldState: true);
			instance.SwapToLast(otherCoroutine, instance._currentCoroutine);
			return float.NaN;
		}
		return float.NegativeInfinity;
	}

	private IEnumerator<float> _StartWhenDone(CoroutineHandle handle, IEnumerator<float> proc)
	{
		if (!_waitingTriggers.ContainsKey(handle))
		{
			yield break;
		}
		try
		{
			if (proc.Current > localTime)
			{
				yield return proc.Current;
			}
			while (proc.MoveNext())
			{
				yield return proc.Current;
			}
		}
		finally
		{
			CloseWaitingProcess(handle);
		}
	}

	private void SwapToLast(CoroutineHandle firstHandle, CoroutineHandle lastHandle)
	{
		if (firstHandle.Key == lastHandle.Key)
		{
			ProcessIndex processIndex = _handleToIndex[firstHandle];
			ProcessIndex processIndex2 = _handleToIndex[lastHandle];
			if (processIndex.seg == processIndex2.seg && processIndex.i >= processIndex2.i)
			{
				IEnumerator<float> replacement = CoindexPeek(processIndex);
				CoindexReplace(processIndex, CoindexPeek(processIndex2));
				CoindexReplace(processIndex2, replacement);
				_indexToHandle[processIndex] = lastHandle;
				_indexToHandle[processIndex2] = firstHandle;
				_handleToIndex[firstHandle] = processIndex2;
				_handleToIndex[lastHandle] = processIndex;
				bool newPausedState = SetPause(processIndex, CoindexIsPaused(processIndex2));
				SetPause(processIndex2, newPausedState);
				newPausedState = SetHeld(processIndex, CoindexIsHeld(processIndex2));
				SetHeld(processIndex2, newPausedState);
			}
		}
	}

	private void CloseWaitingProcess(CoroutineHandle handle)
	{
		if (!_waitingTriggers.ContainsKey(handle))
		{
			return;
		}
		HashSet<CoroutineHandle>.Enumerator enumerator = _waitingTriggers[handle].GetEnumerator();
		_waitingTriggers.Remove(handle);
		while (enumerator.MoveNext())
		{
			if (_handleToIndex.ContainsKey(enumerator.Current) && !HandleIsInWaitingList(enumerator.Current))
			{
				SetHeld(_handleToIndex[enumerator.Current], newHeldState: false);
				_allWaiting.Remove(enumerator.Current);
			}
		}
	}

	private bool HandleIsInWaitingList(CoroutineHandle handle)
	{
		Dictionary<CoroutineHandle, HashSet<CoroutineHandle>>.Enumerator enumerator = _waitingTriggers.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.Contains(handle))
			{
				return true;
			}
		}
		return false;
	}

	private static IEnumerator<float> ReturnTmpRefForRepFunc(IEnumerator<float> coptr, CoroutineHandle handle)
	{
		return _tmpRef as IEnumerator<float>;
	}

	public static float WaitUntilDone(AsyncOperation operation)
	{
		if (operation == null || operation.isDone)
		{
			return float.NaN;
		}
		CoroutineHandle key = CurrentCoroutine;
		Timing instance = GetInstance(CurrentCoroutine.Key);
		if (instance == null)
		{
			return float.NaN;
		}
		_tmpRef = _StartWhenDone(operation, instance.CoindexPeek(instance._handleToIndex[key]));
		ReplacementFunction = ReturnTmpRefForRepFunc;
		return float.NaN;
	}

	private static IEnumerator<float> _StartWhenDone(AsyncOperation operation, IEnumerator<float> pausedProc)
	{
		while (!operation.isDone)
		{
			yield return float.NegativeInfinity;
		}
		_tmpRef = pausedProc;
		ReplacementFunction = ReturnTmpRefForRepFunc;
		yield return float.NaN;
	}

	public static float WaitUntilDone(CustomYieldInstruction operation)
	{
		if (operation == null || !operation.keepWaiting)
		{
			return float.NaN;
		}
		CoroutineHandle key = CurrentCoroutine;
		Timing instance = GetInstance(CurrentCoroutine.Key);
		if (instance == null)
		{
			return float.NaN;
		}
		_tmpRef = _StartWhenDone(operation, instance.CoindexPeek(instance._handleToIndex[key]));
		ReplacementFunction = ReturnTmpRefForRepFunc;
		return float.NaN;
	}

	private static IEnumerator<float> _StartWhenDone(CustomYieldInstruction operation, IEnumerator<float> pausedProc)
	{
		while (operation.keepWaiting)
		{
			yield return float.NegativeInfinity;
		}
		_tmpRef = pausedProc;
		ReplacementFunction = ReturnTmpRefForRepFunc;
		yield return float.NaN;
	}

	public bool LockCoroutine(CoroutineHandle coroutine, CoroutineHandle key)
	{
		if (coroutine.Key != _instanceID || key == default(CoroutineHandle) || key.Key != 0)
		{
			return false;
		}
		if (!_waitingTriggers.ContainsKey(key))
		{
			_waitingTriggers.Add(key, new HashSet<CoroutineHandle> { coroutine });
		}
		else
		{
			_waitingTriggers[key].Add(coroutine);
		}
		SetHeld(_handleToIndex[coroutine], newHeldState: true);
		return true;
	}

	public bool UnlockCoroutine(CoroutineHandle coroutine, CoroutineHandle key)
	{
		if (coroutine.Key != _instanceID || key == default(CoroutineHandle) || !_handleToIndex.ContainsKey(coroutine) || !_waitingTriggers.ContainsKey(key))
		{
			return false;
		}
		_waitingTriggers[key].Remove(coroutine);
		SetHeld(_handleToIndex[coroutine], HandleIsInWaitingList(coroutine));
		return true;
	}

	public static CoroutineHandle CallDelayed(float delay, Action action)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._DelayedCall(delay, action, null));
		}
		return default;
	}

	public CoroutineHandle CallDelayedOnInstance(float delay, Action action)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_DelayedCall(delay, action, null));
		}
		return default;
	}

	public static CoroutineHandle CallDelayed(float delay, Action action, GameObject cancelWith)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._DelayedCall(delay, action, cancelWith));
		}
		return default;
	}

	public CoroutineHandle CallDelayedOnInstance(float delay, Action action, GameObject cancelWith)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_DelayedCall(delay, action, cancelWith));
		}
		return default;
	}

	private IEnumerator<float> _DelayedCall(float delay, Action action, GameObject cancelWith)
	{
		yield return WaitForSecondsOnInstance(delay);
		if ((object)cancelWith == null || cancelWith != null)
		{
			action();
		}
	}

	public static CoroutineHandle CallPeriodically(float timeframe, float period, Action action, Action onDone = null)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._CallContinuously(timeframe, period, action, onDone), Segment.Update);
		}
		return default;
	}

	public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, Action action, Action onDone = null)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_CallContinuously(timeframe, period, action, onDone), Segment.Update);
		}
		return default;
	}

	public static CoroutineHandle CallPeriodically(float timeframe, float period, Action action, Segment segment, Action onDone = null)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._CallContinuously(timeframe, period, action, onDone), segment);
		}
		return default;
	}

	public CoroutineHandle CallPeriodicallyOnInstance(float timeframe, float period, Action action, Segment segment, Action onDone = null)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_CallContinuously(timeframe, period, action, onDone), segment);
		}
		return default;
	}

	public static CoroutineHandle CallContinuously(float timeframe, Action action, Action onDone = null)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._CallContinuously(timeframe, 0f, action, onDone), Segment.Update);
		}
		return default;
	}

	public CoroutineHandle CallContinuouslyOnInstance(float timeframe, Action action, Action onDone = null)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_CallContinuously(timeframe, 0f, action, onDone), Segment.Update);
		}
		return default;
	}

	public static CoroutineHandle CallContinuously(float timeframe, Action action, Segment timing, Action onDone = null)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._CallContinuously(timeframe, 0f, action, onDone), timing);
		}
		return default;
	}

	public CoroutineHandle CallContinuouslyOnInstance(float timeframe, Action action, Segment timing, Action onDone = null)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_CallContinuously(timeframe, 0f, action, onDone), timing);
		}
		return default;
	}

	private IEnumerator<float> _CallContinuously(float timeframe, float period, Action action, Action onDone)
	{
		double startTime = localTime;
		while ((double)localTime <= startTime + (double)timeframe)
		{
			yield return WaitForSecondsOnInstance(period);
			action();
		}
		onDone?.Invoke();
	}

	public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period, Action<T> action, Action<T> onDone = null)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._CallContinuously(reference, timeframe, period, action, onDone), Segment.Update);
		}
		return default;
	}

	public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period, Action<T> action, Action<T> onDone = null)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_CallContinuously(reference, timeframe, period, action, onDone), Segment.Update);
		}
		return default;
	}

	public static CoroutineHandle CallPeriodically<T>(T reference, float timeframe, float period, Action<T> action, Segment timing, Action<T> onDone = null)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._CallContinuously(reference, timeframe, period, action, onDone), timing);
		}
		return default;
	}

	public CoroutineHandle CallPeriodicallyOnInstance<T>(T reference, float timeframe, float period, Action<T> action, Segment timing, Action<T> onDone = null)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_CallContinuously(reference, timeframe, period, action, onDone), timing);
		}
		return default;
	}

	public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, Action<T> action, Action<T> onDone = null)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._CallContinuously(reference, timeframe, 0f, action, onDone), Segment.Update);
		}
		return default;
	}

	public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, Action<T> action, Action<T> onDone = null)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_CallContinuously(reference, timeframe, 0f, action, onDone), Segment.Update);
		}
		return default;
	}

	public static CoroutineHandle CallContinuously<T>(T reference, float timeframe, Action<T> action, Segment timing, Action<T> onDone = null)
	{
		if (action != null)
		{
			return RunCoroutine(Instance._CallContinuously(reference, timeframe, 0f, action, onDone), timing);
		}
		return default;
	}

	public CoroutineHandle CallContinuouslyOnInstance<T>(T reference, float timeframe, Action<T> action, Segment timing, Action<T> onDone = null)
	{
		if (action != null)
		{
			return RunCoroutineOnInstance(_CallContinuously(reference, timeframe, 0f, action, onDone), timing);
		}
		return default;
	}

	private IEnumerator<float> _CallContinuously<T>(T reference, float timeframe, float period, Action<T> action, Action<T> onDone = null)
	{
		double startTime = localTime;
		while ((double)localTime <= startTime + (double)timeframe)
		{
			yield return WaitForSecondsOnInstance(period);
			action(reference);
		}
		onDone?.Invoke(reference);
	}

	[Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
	public new Coroutine StartCoroutine(IEnumerator routine)
	{
		return null;
	}

	[Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
	public new Coroutine StartCoroutine(string methodName, object value)
	{
		return null;
	}

	[Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
	public new Coroutine StartCoroutine(string methodName)
	{
		return null;
	}

	[Obsolete("Unity coroutine function, use RunCoroutine instead.", true)]
	public new Coroutine StartCoroutine_Auto(IEnumerator routine)
	{
		return null;
	}

	[Obsolete("Unity coroutine function, use KillCoroutines instead.", true)]
	public new void StopCoroutine(string methodName)
	{
	}

	[Obsolete("Unity coroutine function, use KillCoroutines instead.", true)]
	public new void StopCoroutine(IEnumerator routine)
	{
	}

	[Obsolete("Unity coroutine function, use KillCoroutines instead.", true)]
	public new void StopCoroutine(Coroutine routine)
	{
	}

	[Obsolete("Unity coroutine function, use KillCoroutines instead.", true)]
	public new void StopAllCoroutines()
	{
	}

	[Obsolete("Use your own GameObject for this.", true)]
	public new static void Destroy(UnityEngine.Object obj)
	{
	}

	[Obsolete("Use your own GameObject for this.", true)]
	public new static void Destroy(UnityEngine.Object obj, float f)
	{
	}

	[Obsolete("Use your own GameObject for this.", true)]
	public new static void DestroyObject(UnityEngine.Object obj)
	{
	}

	[Obsolete("Use your own GameObject for this.", true)]
	public new static void DestroyObject(UnityEngine.Object obj, float f)
	{
	}

	[Obsolete("Use your own GameObject for this.", true)]
	public new static void DestroyImmediate(UnityEngine.Object obj)
	{
	}

	[Obsolete("Use your own GameObject for this.", true)]
	public new static void DestroyImmediate(UnityEngine.Object obj, bool b)
	{
	}

	[Obsolete("Use your own GameObject for this.", true)]
	public new static void Instantiate(UnityEngine.Object obj)
	{
	}

	[Obsolete("Use your own GameObject for this.", true)]
	public new static void Instantiate(UnityEngine.Object original, Vector3 position, Quaternion rotation)
	{
	}

	[Obsolete("Use your own GameObject for this.", true)]
	public new static void Instantiate<T>(T original) where T : UnityEngine.Object
	{
	}

	[Obsolete("Just.. no.", true)]
	public new static T FindObjectOfType<T>() where T : UnityEngine.Object
	{
		return null;
	}

	[Obsolete("Just.. no.", true)]
	public new static UnityEngine.Object FindObjectOfType(Type t)
	{
		return null;
	}

	[Obsolete("Just.. no.", true)]
	public new static T[] FindObjectsOfType<T>() where T : UnityEngine.Object
	{
		return null;
	}

	[Obsolete("Just.. no.", true)]
	public new static UnityEngine.Object[] FindObjectsOfType(Type t)
	{
		return null;
	}

	[Obsolete("Just.. no.", true)]
	public new static void print(object message)
	{
	}
}
