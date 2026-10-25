using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using MapRuleLibrary.MapState;
using ScenarioRuleLibrary;

[Serializable]
public class ClientIndependantValues : ISerializable
{
	[Serializable]
	public class CIVKeyValuePair : ISerializable
	{
		public string Key { get; private set; }

		public bool Value { get; private set; }

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Key", Key);
			info.AddValue("Value", Value);
		}

		public CIVKeyValuePair(SerializationInfo info, StreamingContext context)
		{
			foreach (SerializationEntry item in info)
			{
				try
				{
					string name = item.Name;
					if (!(name == "Key"))
					{
						if (name == "Value")
						{
							Value = info.GetBoolean("Value");
						}
					}
					else
					{
						Key = info.GetString("Key");
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Exception while trying to deserialize CIVKeyValuePair entry " + item.Name);
					throw ex;
				}
			}
		}

		public CIVKeyValuePair(string key, bool value)
		{
			Key = key;
			Value = value;
		}
	}

	public List<CIVKeyValuePair> CIVItemIsNewDictionary { get; private set; }

	public List<CIVKeyValuePair> CIVQuestIsNewDictionary { get; private set; }

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("CIVItemIsNewDictionary", CIVItemIsNewDictionary);
		info.AddValue("CIVQuestIsNewDictionary", CIVQuestIsNewDictionary);
	}

	public ClientIndependantValues(SerializationInfo info, StreamingContext context)
	{
		foreach (SerializationEntry item in info)
		{
			try
			{
				string name = item.Name;
				if (!(name == "CIVItemIsNewDictionary"))
				{
					if (name == "CIVQuestIsNewDictionary")
					{
						CIVQuestIsNewDictionary = (List<CIVKeyValuePair>)info.GetValue("CIVQuestIsNewDictionary", typeof(List<CIVKeyValuePair>));
					}
				}
				else
				{
					CIVItemIsNewDictionary = (List<CIVKeyValuePair>)info.GetValue("CIVItemIsNewDictionary", typeof(List<CIVKeyValuePair>));
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception while trying to deserialize ClientIndependantValues entry " + item.Name);
				throw ex;
			}
		}
		if (CIVItemIsNewDictionary == null)
		{
			CIVItemIsNewDictionary = new List<CIVKeyValuePair>();
		}
		if (CIVQuestIsNewDictionary == null)
		{
			CIVQuestIsNewDictionary = new List<CIVKeyValuePair>();
		}
	}

	public ClientIndependantValues()
	{
		CIVItemIsNewDictionary = new List<CIVKeyValuePair>();
		CIVQuestIsNewDictionary = new List<CIVKeyValuePair>();
	}

	public void UpdateItemIsNewDictionary(List<CItem> items)
	{
		try
		{
			CIVItemIsNewDictionary.Clear();
			foreach (CItem item in items)
			{
				if (item != null)
				{
					if (CIVItemIsNewDictionary.Exists((CIVKeyValuePair e) => e.Key == item.ItemGuid))
					{
						Log.Error("Duplicate item in UpdateItemIsNewDictionary.  Item: " + item.Name);
					}
					else
					{
						CIVItemIsNewDictionary.Add(new CIVKeyValuePair(item.ItemGuid, item.IsNew));
					}
				}
			}
		}
		catch (Exception exception)
		{
			Log.Exception(exception, "Exception at Update Item Is New Dictionary");
		}
	}

	public void UpdateQuestIsNewDictionary(List<CQuestState> questStates)
	{
		CIVQuestIsNewDictionary.Clear();
		foreach (CQuestState questState in questStates)
		{
			CIVQuestIsNewDictionary.Add(new CIVKeyValuePair(questState.ID, questState.IsNew));
		}
	}
}
