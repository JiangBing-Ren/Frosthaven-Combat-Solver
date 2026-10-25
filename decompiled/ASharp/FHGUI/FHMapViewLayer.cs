using System.Collections.Generic;
using System.Linq;

namespace FHGUI;

public class FHMapViewLayer
{
	public enum LayerType
	{
		None = 0,
		Base = 1,
		Modal = 4,
		Tutorial = 6
	}

	private struct StateStackOperation
	{
		public enum EType
		{
			None,
			Push,
			Pop,
			Replace
		}

		public FHMapViewState State;

		public EType Type;
	}

	private readonly Stack<FHMapViewState> _stateStack = new Stack<FHMapViewState>();

	private readonly Queue<StateStackOperation> _stateStackOperationQueue = new Queue<StateStackOperation>();

	private readonly List<FHMapViewState> _lastUpdatePoppedStates = new List<FHMapViewState>();

	public readonly LayerType Type;

	private const string ScreenChangeAudioItem = "PlaySound_UIScreenChange";

	public FHMapViewState State
	{
		get
		{
			if (!_stateStack.TryPeek(out var result))
			{
				return null;
			}
			return result;
		}
	}

	public bool HasState => _stateStack.Any();

	public IReadOnlyList<FHMapViewState> LastUpdatePoppedStates => _lastUpdatePoppedStates;

	public FHMapViewLayer(LayerType type)
	{
		Type = type;
	}

	public IEnumerable<IUIViewModule> GetStateModules()
	{
		FHMapViewState state = State;
		if (state == null)
		{
			yield break;
		}
		foreach (IUIViewModule item in state.UseModules().SelectMany(GatherModuleDependencies))
		{
			yield return item;
		}
	}

	private IEnumerable<IUIViewModule> GatherModuleDependencies(IUIViewModule module)
	{
		yield return module;
		if (!(module is FHMapViewModule fHMapViewModule))
		{
			yield break;
		}
		foreach (IUIViewModule item in fHMapViewModule.GetDependencies().SelectMany(GatherModuleDependencies))
		{
			yield return item;
		}
	}

	public IEnumerable<FHMapViewState> GetStates()
	{
		return _stateStack;
	}

	public void PushState(FHMapViewState state, bool avoidDuplicates = false)
	{
		if (!avoidDuplicates || !_stateStackOperationQueue.Any((StateStackOperation op) => op.State.GetType() == state.GetType() && op.Type == StateStackOperation.EType.Push))
		{
			_stateStackOperationQueue.Enqueue(new StateStackOperation
			{
				State = state,
				Type = StateStackOperation.EType.Push
			});
		}
	}

	public void PopState()
	{
		FHMapViewState curState = State;
		if (!_stateStackOperationQueue.Any((StateStackOperation op) => op.State == curState && op.Type == StateStackOperation.EType.Pop))
		{
			_stateStackOperationQueue.Enqueue(new StateStackOperation
			{
				Type = StateStackOperation.EType.Pop,
				State = curState
			});
		}
	}

	public void SetState(FHMapViewState state)
	{
		_stateStackOperationQueue.Clear();
		_stateStackOperationQueue.Enqueue(new StateStackOperation
		{
			State = state,
			Type = StateStackOperation.EType.Push
		});
	}

	public void ReplaceState(FHMapViewState state)
	{
		_stateStackOperationQueue.Enqueue(new StateStackOperation
		{
			State = state,
			Type = StateStackOperation.EType.Replace
		});
	}

	public bool TryUpdate(out FHMapViewState oldState)
	{
		if (!_stateStackOperationQueue.Any())
		{
			oldState = null;
			_lastUpdatePoppedStates.Clear();
			return false;
		}
		oldState = State;
		Queue<StateStackOperation> queue = new Queue<StateStackOperation>(_stateStackOperationQueue);
		_stateStackOperationQueue.Clear();
		bool flag = false;
		StateStackOperation result;
		while (queue.TryDequeue(out result))
		{
			switch (result.Type)
			{
			case StateStackOperation.EType.Pop:
			{
				_stateStack.TryPop(out var result3);
				_lastUpdatePoppedStates.Add(result3);
				break;
			}
			case StateStackOperation.EType.Replace:
			{
				_stateStack.TryPop(out var result2);
				_stateStack.Push(result.State);
				_lastUpdatePoppedStates.Add(result2);
				if (result2?.GetType() != result.State.GetType())
				{
					flag = true;
				}
				break;
			}
			case StateStackOperation.EType.Push:
				_stateStack.Push(result.State);
				break;
			default:
				Log.GUIWarning($"StateStackOperation has Invalid Type {result.Type}");
				break;
			}
		}
		if (flag)
		{
			AudioControllerUtils.PlaySound("PlaySound_UIScreenChange");
		}
		return true;
	}

	public bool TryRefresh()
	{
		if (State == null)
		{
			return false;
		}
		State.OnShow();
		return true;
	}

	public void Clear()
	{
		if (_stateStack.Any())
		{
			_lastUpdatePoppedStates.AddRange(_stateStack);
		}
		_stateStack.Clear();
	}
}
