using Newtonsoft.Json.Linq;

namespace Base.Utils.Web;

public static class WebExtensionMethods
{
	public static T GetValue<T>(this JObject obj, string name)
	{
		return obj.GetValue(name).Value<T>();
	}
}
