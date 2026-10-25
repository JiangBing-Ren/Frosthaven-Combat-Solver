using System.Collections;

namespace FH.Platforms;

public interface INewPlatformStorage
{
	bool IsReady { get; }

	IEnumerator InitCrt();

	string GetPersistentDataPath();

	string GetSaveDataPath();

	bool LoadFromFile(string filename, out object data);

	bool SaveToFile(string filename, object data);
}
