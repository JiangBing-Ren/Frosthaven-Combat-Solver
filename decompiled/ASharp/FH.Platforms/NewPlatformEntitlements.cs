using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Libs.GameConsole;
using UnityEngine;

namespace FH.Platforms;

public abstract class NewPlatformEntitlements : IDisposable
{
	private EntitlementsDatabase _database;

	private HashSet<EntitlementDefinition> _compatibleEntitlements = new HashSet<EntitlementDefinition>();

	private HashSet<EntitlementDefinition> _activeEntitlements = new HashSet<EntitlementDefinition>();

	public Action<string> OnEntitlementActivated;

	public Action<string> OnEntitlementDeactivated;

	private string _platformID;

	internal virtual IEnumerator Init(NewPlatform platform)
	{
		_platformID = platform.ID;
		if ((object)_database == null)
		{
			_database = EntitlementsDatabase.LoadResource();
		}
		_compatibleEntitlements = _database.GetCompatibleEntitlements(_platformID);
		UpdateActivePlatformEntitlements();
		yield break;
	}

	protected void UpdateActivePlatformEntitlements()
	{
		HashSet<EntitlementDefinition> hashSet = new HashSet<EntitlementDefinition>(_activeEntitlements);
		_activeEntitlements.Clear();
		foreach (EntitlementDefinition compatibleEntitlement in _compatibleEntitlements)
		{
			bool flag = hashSet.Contains(compatibleEntitlement);
			if (IsEntitlementActiveOnPlatform(compatibleEntitlement))
			{
				_activeEntitlements.Add(compatibleEntitlement);
				if (!flag)
				{
					OnEntitlementActivated?.Invoke(compatibleEntitlement.ID);
				}
			}
			else if (flag)
			{
				OnEntitlementDeactivated?.Invoke(compatibleEntitlement.ID);
			}
		}
	}

	protected abstract bool IsEntitlementActiveOnPlatform(EntitlementDefinition entitlement);

	protected abstract void OpenPlatformStore(EntitlementDefinition entitlement);

	public bool IsEntitlementActive(string entitlementID)
	{
		EntitlementDefinition entitlement = _database.GetEntitlement(entitlementID);
		return _activeEntitlements.Contains(entitlement);
	}

	public EntitlementDefinition GetEntitlement(string entitlementID)
	{
		return _database.GetEntitlement(entitlementID);
	}

	public IEnumerable<EntitlementDefinition> GetCompatibleEntitlements()
	{
		return _database.GetCompatibleEntitlements(_platformID);
	}

	public IEnumerable<EntitlementDefinition> GetActiveEntitlements()
	{
		return new HashSet<EntitlementDefinition>(_activeEntitlements);
	}

	public IEnumerable<EntitlementDefinition> GetAllEntitlements()
	{
		return _database.GetAllEntitlements();
	}

	public void OpenStore(string entitlementID)
	{
		EntitlementDefinition entitlement = _database.GetEntitlement(entitlementID);
		OpenPlatformStore(entitlement);
	}

	public void DebugForceActivateEntitlement(string entitlementID)
	{
		EntitlementDefinition entitlement = _database.GetEntitlement(entitlementID);
		_compatibleEntitlements.Add(entitlement);
		_activeEntitlements.Add(entitlement);
		OnEntitlementActivated?.Invoke(entitlementID);
	}

	public void DebugForceDeactivateEntitlement(string entitlementID)
	{
		EntitlementDefinition entitlement = _database.GetEntitlement(entitlementID);
		_activeEntitlements.Remove(entitlement);
		OnEntitlementDeactivated?.Invoke(entitlementID);
	}

	[ConsoleCommand(Command = "entitlements_print_all")]
	public static async void ConsolePrintAllEntitlements(IConsole console)
	{
		IEnumerable<string> source = (await PlatformLayer.Instance.NewPlatformLayer.GetAllEntitlements()).Select((EntitlementDefinition x) => x.ID);
		console.WriteLine("[NewPlatform] All Entitlements: " + source.ToStringPretty("\n"));
	}

	[ConsoleCommand(Command = "entitlements_print_compatible")]
	public static async void ConsolePrintCompatibleEntitlements(IConsole console)
	{
		IEnumerable<string> source = (await PlatformLayer.Instance.NewPlatformLayer.GetCompatibleEntitlements()).Select((EntitlementDefinition x) => x.ID);
		console.WriteLine("[NewPlatform] Compatible Entitlements: " + source.ToStringPretty("\n"));
	}

	[ConsoleCommand(Command = "entitlements_print_active")]
	public static async void ConsolePrintActiveEntitlements(IConsole console)
	{
		IEnumerable<string> source = (await PlatformLayer.Instance.NewPlatformLayer.GetActiveEntitlements()).Select((EntitlementDefinition x) => x.ID);
		console.WriteLine("[NewPlatform] Active Entitlements: " + source.ToStringPretty("\n"));
	}

	[ConsoleCommand(Command = "open_store")]
	public static async void ConsoleOpenStore(IConsole console, string entitlementId)
	{
		await PlatformLayer.Instance.NewPlatformLayer.OpenStore(entitlementId);
	}

	[ConsoleCommand(Command = "entitlements_force_activate")]
	public static void ConsoleForceActivateEntitlement(IConsole console, string entitlementId)
	{
		PlatformLayer.Instance.NewPlatformLayer.CurrentPlatform.Entitlements.DebugForceActivateEntitlement(entitlementId);
		console.WriteLine("[NewPlatform] Forced activation of entitlement: " + entitlementId);
	}

	[ConsoleCommand(Command = "entitlements_force_deactivate")]
	public static void ConsoleForceDeactivateEntitlement(IConsole console, string entitlementId)
	{
		PlatformLayer.Instance.NewPlatformLayer.CurrentPlatform.Entitlements.DebugForceDeactivateEntitlement(entitlementId);
		console.WriteLine("[NewPlatform] Forced deactivation of entitlement: " + entitlementId);
	}

	public virtual void Dispose()
	{
		Resources.UnloadAsset(_database);
	}
}
