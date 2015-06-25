namespace JSONNETExtensions
{
	using UnityEngine;
	using System.Collections.Generic;
	using System.Collections;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using System.Runtime.Serialization;

	public static class JSONNETExtender
	{
		public static bool ContainsKey(this JObject p_this, string p_key)
		{
			foreach(KeyValuePair<string, JToken> pair in p_this)
			{
				if(pair.Key == p_key)
					return true;
			}

			return false;
		}

		public static void Populate<T>(this JToken value, T target) where T : class
		{
			using (var sr = value.CreateReader())
			{
				JsonSerializer.Create(null).Populate(sr, target);
				//JsonSerializer.CreateDefault().Populate(sr, target); // Uses the system default JsonSerializerSettings
			}
		}
	}
}
