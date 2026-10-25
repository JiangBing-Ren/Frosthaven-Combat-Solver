using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using FHRL;
using Libs.Threading;
using MapRuleLibrary.Client;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SharedLibrary.Client;

namespace FH.YML;

public class RulebaseFile : IDisposable
{
	public struct Header
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
		public string Check;

		public uint Entries;

		public const string CheckValue = "02392fe1";

		public const int Size = 16;
	}

	public struct Entry
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string FileName;

		public YMLLoading.EYMLParserType ParserType;

		public bool IsBinaryData;

		public uint DataSectionOffset;

		public uint FileSize;

		public const int Size = 140;
	}

	private class BatchLoadEntries
	{
		public List<Entry> Entries = new List<Entry>();

		public YMLLoading.EYMLParserType ParserType;

		public bool IsBinary => Entries.FirstOrDefault().IsBinaryData;
	}

	private class CompileEntry
	{
		public Entry Entry;

		public byte[] BinaryData;
	}

	private FileStream _fs;

	private Header _header;

	private List<Entry> _entries = new List<Entry>();

	public RulebaseFile(string filePath)
	{
		_fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		_header = ReadStruct<Header>(_fs);
		if (_header.Check != "02392fe1")
		{
			throw new Exception("File '" + filePath + "' is not a valid Rulebase file");
		}
		for (int i = 0; i < _header.Entries; i++)
		{
			_entries.Add(ReadStruct<Entry>(_fs));
		}
	}

	public void Dispose()
	{
		_fs.Close();
		_fs.Dispose();
	}

	public bool ProcessFiles()
	{
		bool result = true;
		if (_entries.Count == 0)
		{
			return true;
		}
		long position = _fs.Position;
		byte[] array = new byte[_fs.Length - position];
		_fs.Read(array);
		List<BatchLoadEntries> list = new List<BatchLoadEntries>();
		BatchLoadEntries batchLoadEntries = new BatchLoadEntries
		{
			ParserType = _entries[0].ParserType
		};
		for (int i = 0; i < _entries.Count; i++)
		{
			if (_entries[i].ParserType == batchLoadEntries.ParserType)
			{
				batchLoadEntries.Entries.Add(_entries[i]);
				continue;
			}
			list.Add(batchLoadEntries);
			batchLoadEntries = new BatchLoadEntries();
			batchLoadEntries.Entries.Add(_entries[i]);
			batchLoadEntries.ParserType = _entries[i].ParserType;
		}
		if (batchLoadEntries.Entries.Any())
		{
			list.Add(batchLoadEntries);
		}
		new List<MemoryStream>();
		foreach (BatchLoadEntries item in list)
		{
			if (!LoadBatch(item, array))
			{
				result = false;
			}
		}
		return result;
	}

	private bool LoadBinary(BatchLoadEntries batch, byte[] data, int index)
	{
		Entry entry = batch.Entries[index];
		using MemoryStream stream = new MemoryStream(data, (int)entry.DataSectionOffset, (int)entry.FileSize);
		if (batch.ParserType == YMLLoading.EYMLParserType.CustomLevel)
		{
			return YMLLoading.ProcessCustomLevel(stream, entry.FileName);
		}
		if (entry.IsBinaryData)
		{
			YMLLoading.AddYMLDataFromType(BinarySerializerHelper.Deserialize(stream), entry.FileName);
			return true;
		}
		return YMLLoading.ProcessYMLFile(stream, entry.FileName);
	}

	private static bool LoadText(BatchLoadEntries batch, byte[] data, int index)
	{
		Entry entry = batch.Entries[index];
		using (MemoryStream stream = new MemoryStream(data, (int)entry.DataSectionOffset, (int)entry.FileSize))
		{
			if (entry.ParserType == YMLLoading.EYMLParserType.CustomLevel)
			{
				if (!YMLLoading.ProcessCustomLevel(stream, entry.FileName))
				{
					return false;
				}
			}
			else if (!YMLLoading.ProcessYMLFile(stream, entry.FileName))
			{
				return false;
			}
		}
		return true;
	}

	private bool LoadBatch(BatchLoadEntries batch, byte[] data)
	{
		DateTime now = DateTime.Now;
		bool loadedSuccessfully = true;
		if (DataLoadingFlags.HandleAbilitiesOnMainThread && (batch.ParserType == YMLLoading.EYMLParserType.HeroSummon || batch.ParserType == YMLLoading.EYMLParserType.AbilityCard || batch.ParserType == YMLLoading.EYMLParserType.ItemCard || batch.ParserType == YMLLoading.EYMLParserType.AttackModifier || batch.ParserType == YMLLoading.EYMLParserType.Perk || batch.ParserType == YMLLoading.EYMLParserType.EnemyCard || batch.ParserType == YMLLoading.EYMLParserType.ScenarioAbility))
		{
			if (batch.IsBinary)
			{
				for (int i = 0; i < batch.Entries.Count; i++)
				{
					loadedSuccessfully = loadedSuccessfully && LoadBinary(batch, data, i);
				}
			}
			else
			{
				for (int j = 0; j < batch.Entries.Count; j++)
				{
					loadedSuccessfully = loadedSuccessfully && LoadText(batch, data, j);
				}
			}
		}
		else if (batch.IsBinary)
		{
			JobSystem.ExecuteFastJobs(batch.Entries.Count, (int index) =>
			{
				loadedSuccessfully = loadedSuccessfully && LoadBinary(batch, data, index);
			});
		}
		else
		{
			JobSystem.ExecuteFastJobs(batch.Entries.Count, (int index) =>
			{
				loadedSuccessfully = loadedSuccessfully && LoadText(batch, data, index);
			});
		}
		if (loadedSuccessfully)
		{
			ScenarioRuleClient.SRLYML.UploadNewData();
			MapRuleLibraryClient.MRLYML.UploadNewData();
		}
		Log.YMLInfo($"Loading {batch.Entries.Count} {batch.ParserType} entries took {(DateTime.Now - now).TotalSeconds} seconds");
		return loadedSuccessfully;
	}

	private static byte[] StructToByteArray<TStruct>(TStruct structure) where TStruct : struct
	{
		int num = Marshal.SizeOf(structure);
		byte[] array = new byte[num];
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		Marshal.StructureToPtr(structure, intPtr, fDeleteOld: true);
		Marshal.Copy(intPtr, array, 0, num);
		return array;
	}

	private static T ReadStruct<T>(FileStream stream, int size = 0) where T : struct
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (size == 0)
		{
			size = Marshal.SizeOf(typeof(T));
		}
		byte[] array = new byte[size];
		if (stream.Read(array, 0, size) != size)
		{
			throw new InvalidOperationException($"Failed to read {size} bytes from the stream");
		}
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		try
		{
			return Marshal.PtrToStructure<T>(gCHandle.AddrOfPinnedObject());
		}
		finally
		{
			gCHandle.Free();
		}
	}

	private static List<CompileEntry> CompileText(List<string> files, string rulesetPath)
	{
		List<CompileEntry> list = new List<CompileEntry>();
		foreach (string file in files)
		{
			CompileEntry compileEntry = new CompileEntry
			{
				Entry = new Entry
				{
					FileName = file.Substring(rulesetPath.Length + 1).Replace("\\", "/")
				}
			};
			if (Path.GetExtension(file) == ".yml")
			{
				string parserLine;
				using (StreamReader streamReader = new StreamReader(file))
				{
					parserLine = streamReader.ReadLine();
				}
				compileEntry.Entry.ParserType = YMLLoading.GetParserType(parserLine, file);
				if (compileEntry.Entry.ParserType == YMLLoading.EYMLParserType.None)
				{
					throw new Exception("Failed to compile ruleset at: " + rulesetPath + "\nThe Parser type for file " + file + " is not valid");
				}
				compileEntry.BinaryData = File.ReadAllBytes(file);
				list.Add(compileEntry);
			}
			else if (Path.GetExtension(file) == ".lvldat")
			{
				compileEntry.Entry.ParserType = YMLLoading.EYMLParserType.CustomLevel;
				compileEntry.BinaryData = File.ReadAllBytes(file);
				list.Add(compileEntry);
			}
		}
		return list;
	}

	private static List<CompileEntry> ComplieBinary(List<string> files, string rulesetPath)
	{
		DataLoadingFlags.IsCompilingRulebase = true;
		SharedClient.Initialise(string.Empty, YMLLoading.RulebaseFolder);
		ScenarioRuleClient.Initialise(reinitialise: true);
		MapRuleLibraryClient.Instance.Initialise(reinitialise: true);
		List<(YMLLoading.EYMLParserType, string)> list = new List<(YMLLoading.EYMLParserType, string)>();
		foreach (string file in files)
		{
			if (Path.GetExtension(file) == ".yml")
			{
				string parserLine;
				using (StreamReader streamReader = new StreamReader(file))
				{
					parserLine = streamReader.ReadLine();
				}
				list.Add((YMLLoading.GetParserType(parserLine, file), file));
			}
			else if (Path.GetExtension(file) == ".lvldat")
			{
				list.Add((YMLLoading.EYMLParserType.CustomLevel, file));
			}
		}
		list = list.OrderBy(((YMLLoading.EYMLParserType, string) tuple) => tuple.Item1).ToList();
		foreach (var item in list)
		{
			using MemoryStream stream = new MemoryStream(File.ReadAllBytes(item.Item2));
			if (Path.GetExtension(item.Item2) == ".yml")
			{
				YMLLoading.ProcessYMLFile(stream, item.Item2);
			}
			else if (Path.GetExtension(item.Item2) == ".lvldat")
			{
				YMLLoading.ProcessCustomLevel(stream, item.Item2);
			}
		}
		List<CompileEntry> list2 = new List<CompileEntry>();
		foreach (YMLLoading.EYMLParserType l in list.Select(((YMLLoading.EYMLParserType, string) s) => s.Item1).Distinct())
		{
			Type yMLDataType = YMLLoading.GetYMLDataType(l);
			if (yMLDataType != null && (typeof(IDef).IsAssignableFrom(yMLDataType) || typeof(BinarySerializableYMLData).IsAssignableFrom(yMLDataType)))
			{
				foreach (object item2 in YMLLoading.GetYMLDataFromType(l) ?? throw new Exception($"Unknown data for type: {l}"))
				{
					string empty = string.Empty;
					if (item2 is BinarySerializableYMLData binarySerializableYMLData)
					{
						empty = binarySerializableYMLData.FileName;
					}
					else
					{
						empty = ((!(item2 is IDef def)) ? yMLDataType.Name : $"{yMLDataType.Name} : {def.ID}");
					}
					list2.Add(new CompileEntry
					{
						Entry = new Entry
						{
							ParserType = l,
							FileName = empty,
							IsBinaryData = true
						},
						BinaryData = BinarySerializerHelper.Serialize(item2)
					});
				}
				continue;
			}
			foreach (var item3 in list.Where(((YMLLoading.EYMLParserType, string) s) => s.Item1 == l))
			{
				list2.Add(new CompileEntry
				{
					Entry = new Entry
					{
						ParserType = item3.Item1,
						FileName = item3.Item2.Substring(rulesetPath.Length + 1)
					},
					BinaryData = File.ReadAllBytes(item3.Item2)
				});
			}
		}
		ScenarioRuleClient.Uninitalize();
		MapRuleLibraryClient.Instance.Uninitalize();
		DataLoadingFlags.IsCompilingRulebase = false;
		return list2;
	}

	public static void Compile(string rulesetPath, string outPath, List<string> includeList = null, bool binary = false)
	{
		List<string> list = new List<string>();
		string[] fileSystemEntries = Directory.GetFileSystemEntries(rulesetPath, "*.yml", SearchOption.AllDirectories);
		foreach (string file in fileSystemEntries)
		{
			if (includeList == null || includeList.Any((string a) => file.Replace("\\", "/").Contains(a)))
			{
				list.Add(file);
			}
		}
		fileSystemEntries = Directory.GetFiles(rulesetPath, "*.lvldat", SearchOption.AllDirectories);
		foreach (string file2 in fileSystemEntries)
		{
			if (includeList == null || includeList.Any((string a) => file2.Replace("\\", "/").Contains(a)))
			{
				list.Add(file2);
			}
		}
		List<CompileEntry> list2 = (from t in binary ? ComplieBinary(list, rulesetPath) : CompileText(list, rulesetPath)
			orderby t.Entry.ParserType, t.Entry.FileName
			select t).ToList();
		Header structure = new Header
		{
			Check = "02392fe1",
			Entries = (uint)list2.Count
		};
		using FileStream fileStream = new FileStream(outPath, FileMode.Create, FileAccess.Write, FileShare.None);
		byte[] array = StructToByteArray(structure);
		fileStream.Write(array);
		uint num = 0u;
		foreach (CompileEntry item in list2)
		{
			Entry entry = item.Entry;
			entry.FileSize = (uint)item.BinaryData.Length;
			entry.DataSectionOffset = num;
			num += entry.FileSize;
			byte[] array2 = StructToByteArray(entry);
			fileStream.Write(array2);
		}
		foreach (CompileEntry item2 in list2)
		{
			byte[] binaryData = item2.BinaryData;
			fileStream.Write(binaryData);
		}
	}
}
