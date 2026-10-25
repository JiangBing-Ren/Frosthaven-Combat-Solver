namespace System.Runtime.Serialization;

public static class SerializationInfoExtensions
{
	public static bool Exists(this SerializationInfo info, string name)
	{
		foreach (SerializationEntry item in info)
		{
			if (name == item.Name)
			{
				return true;
			}
		}
		return false;
	}
}
