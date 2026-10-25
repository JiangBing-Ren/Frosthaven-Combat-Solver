using System.Collections.Generic;
using LitJson;

namespace BestHTTP.SocketIO.JsonEncoders;

public sealed class LitJsonEncoder : IJsonEncoder
{
	public List<object> Decode(string json)
	{
		return JsonMapper.ToObject<List<object>>(new JsonReader(json));
	}

	public string Encode(List<object> obj)
	{
		JsonWriter jsonWriter = new JsonWriter();
		JsonMapper.ToJson(obj, jsonWriter);
		return jsonWriter.ToString();
	}
}
