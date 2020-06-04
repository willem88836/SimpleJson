using System;

namespace SimpleJsonLibrary
{
	// TODO: Nested strings do not work.
	/// <summary>
	///		JsonUtility provides simple Json serialization and deserialization functionalities.
	/// </summary>
	public static class JsonUtility
	{
		/// <summary>
		///		Converts Json string into a usable object. 
		/// </summary>
		/// <typeparam name="T">
		///		The type of the string that is deserialized.
		///	</typeparam>
		/// <param name="json">
		///		The string that is deserialized.
		///	</param>
		/// <returns>
		///		The deserialized Json object.
		///	</returns>
		public static T FromJson<T>(string json) where T : new()
		{
			JsonDeserializer jsonDeserializer = new JsonDeserializer();
			return (T)jsonDeserializer.FromJson(json, typeof(T));
		}

		/// <summary>
		///		Converts Json string into a usable object. 
		/// </summary>
		/// <param name="json">
		///		The string that is deserialized.
		///	</param>
		/// <param name="elementType">
		///		The type of the string that is deserialized.
		///	</param>
		/// <returns>
		///		The deserialized Json object.
		///	</returns>
		public static object FromJson(string json, Type elementType)
		{
			JsonDeserializer jsonDeserializer = new JsonDeserializer();
			return jsonDeserializer.FromJson(json, elementType);
		}

		/// <summary>
		///		Creates Json code from the provided element.
		/// </summary>
		/// <typeparam name="T">
		///		The type of the object that is serialized.
		///	</typeparam>
		/// <param name="element">
		///		The object that is serialized.
		/// </param>
		/// <param name="enableHashing">
		///		If true, the objects' hashcode is used to determine nested references. 
		///		And duplicates are referred to by a number.
		///		<br></br>
		///		If set to false, and nested objects to refer to earlier objects, 
		///		you will get a StackOverflow Exception.
		///	</param>
		/// <returns>
		///		Json string of the serialized object.
		///	</returns>
		public static string ToJson<T>(T element, bool enableHashing = false)
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			return jsonSerializer.ToJson(element, typeof(T), enableHashing);
		}

		/// <summary>
		///		Creates Json code from the provided element.
		/// </summary>
		/// <param name="element">
		///		The object that is serialized.
		///	</param>
		/// <param name="elementType">
		///		The type of the object that is serialized.
		///	</param>
		/// <param name="enableHashing">
		///		If true, the objects' hashcode is used to determine nested references. 
		///		And duplicates are referred to by a number.
		///		<br></br>
		///		If set to false, and nested objects to refer to earlier objects, 
		///		you will get a StackOverflow Exception.
		///	</param>
		/// <returns>
		///		Json string of the serialized object.
		///	</returns>
		public static string ToJson(object element, Type elementType, bool enableHashing = false)
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			return jsonSerializer.ToJson(element, elementType, enableHashing);
		}
	}
}
