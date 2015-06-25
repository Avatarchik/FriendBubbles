using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Reflection;
using JSONNETExtensions;

/**
 * Because JSON.Net has sooooooo many different classes that do sooooooo many different things, I made this helper
 * class. 
 * Now, kish!
 * 
 * @author Yaniv Peer
 */
public class JsonTools
{
	public class SerializeGroup : System.Attribute
	{
		public enum Type{IDENTIFIER, DYNAMIC, STATIC, ORIENTATION};

		public Type groupType;

		public SerializeGroup(Type p_groupType)
		{
			this.groupType = p_groupType;
		}
	}

	public static JToken ConvertObjectToJToken(object p_object, SerializeGroup.Type p_group)
	{
		return ConvertObjectToJToken(p_object, new SerializeGroup.Type[]{p_group});
	}

	public static JToken ConvertObjectsToJToken(object[] p_objects, SerializeGroup.Type[] p_groups)
	{
		JArray jArray = new JArray();
		foreach(object p_object in p_objects)
			jArray.Add(ConvertObjectToJToken(p_object, p_groups));

		return jArray;
	}

	public static JToken ConvertObjectToJToken(object p_object, SerializeGroup.Type[] p_groups)
	{
		JObject jToken = (JObject)ConvertObjectToJToken(p_object);

		foreach(PropertyInfo property in p_object.GetType().GetProperties(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
		{
			foreach(object attribute in property.GetCustomAttributes(true))
			{
				if(attribute.GetType().IsAssignableFrom(typeof(SerializeGroup)))
				{
					SerializeGroup group = (SerializeGroup)attribute;
					bool hasAttribute = false;
					foreach(SerializeGroup.Type groupType in p_groups)
					{
						if(group.groupType == groupType)
							hasAttribute = true;
					}
					
					// Remove the property?
					if(!hasAttribute)
						jToken.Remove(property.Name);
				}
			}
		}

		foreach(FieldInfo field in p_object.GetType().GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance))
		{
			foreach(object attribute in field.GetCustomAttributes(true))
			{
				if(attribute.GetType().IsAssignableFrom(typeof(SerializeGroup)))
				{
					SerializeGroup group = (SerializeGroup)attribute;
					bool hasAttribute = false;
					foreach(SerializeGroup.Type groupType in p_groups)
					{
						if(group.groupType == groupType)
							hasAttribute = true;
					}
					
					// Remove the property?
					if(!hasAttribute)
						jToken.Remove(field.Name);
				}
			}
		}

		return (JToken)jToken;
	}

	public static string ConvertObjectToString(object p_object, SerializeGroup.Type[] p_groups)
	{
		//return JsonConvert.SerializeObject(p_object);
		return JsonTools.ConvertObjectToJToken(p_object, p_groups).ToString();
	}

	public static string ConvertObjectToString(object p_object)
	{
		return JsonConvert.SerializeObject(p_object);
	}

	public static JToken ConvertObjectToJToken(object p_object)
	{
		return JToken.FromObject(p_object);
		//return JToken.Parse(JsonTools.ConvertObjectToString(p_object));
	}

	public static object ConvertStringToNewObject(string p_string)
	{
		return JsonConvert.DeserializeObject(p_string);
	}

	public static JToken ConvertStringToJToken(string p_string)
	{
		return JToken.Parse(p_string);
	}

	public static void PopulateStringToExistingObject(string p_string, object p_object)
	{
		JsonConvert.PopulateObject(p_string, p_object);
	}

	public static void PopulateJObjectToExistingObject(JToken p_jToken, object p_object)
	{
		if(p_jToken != null)
			p_jToken.Populate(p_object);
	}

	public static void PopulateJObjectToExistingObject(JObject p_jObject, object p_object)
	{
		if(p_jObject != null)
		{
			p_jObject.Populate(p_object);
			//PopulateStringToExistingObject(p_jobject.ToString(), p_object);
		}
	}
}
