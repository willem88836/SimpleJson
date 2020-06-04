using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SimpleJsonLibrary
{
	// TODO: Serializing Arrays does not work. 
	// TODO: Serializing Arrays with multiple dimensions does not work. 
	// TODO: Arrays with no content doesn't work.
	// TODO: Nested strings do not work.
	// TODO: Commentary...
	/// <summary>
	///		JsonUtility provides simple Json serialization and deserialization functionalities.
	/// </summary>
	public static class JsonUtility
	{
		private class Json
		{
			protected const string NULL = "null";
			protected const char OBJPREFIX = '{';
			protected const char OBJSUFFIX = '}';
			protected const char ARRAYPREFIX = '[';
			protected const char ARRAYSUFFIX = ']';
			protected const char OBJDEFINITION = ':';
			protected const char OBJSEPARATOR = ',';
			protected const char QUOTATIONMARK = '\"';
			protected const char OBJREFERENCE = '$';
		}

		private class JsonSerializer : Json
		{
			private StringBuilder jsonBuilder;
			private List<int> objectHashes;
			private bool enableHashing;


			public string ToJson(object element, Type elementType, bool enableHashing)
			{
				this.jsonBuilder = new StringBuilder();
				this.enableHashing = enableHashing;
				if (this.enableHashing)
				{
					objectHashes = new List<int>();
				}

				SerializeObject(element, elementType);
				string json = jsonBuilder.ToString();
				return json;
			}

			private void SerializeObject(object element, Type elementType)
			{
				if (element == null)
				{
					jsonBuilder.Append(NULL);
					return;
				}

				int hash = element.GetHashCode();

				if (enableHashing && objectHashes.Contains(hash))
				{
					int index = objectHashes.IndexOf(hash);
					jsonBuilder.Append(OBJREFERENCE);
					jsonBuilder.Append(index);
					jsonBuilder.Append(OBJREFERENCE);
				}
				else
				{
					if (enableHashing)
					{
						int index = objectHashes.Count;
						objectHashes.Add(hash);
					}

					jsonBuilder.Append(OBJPREFIX);

					FieldInfo[] fields = elementType.GetFields().Where(info => 
						info.GetCustomAttribute(typeof(JsonIgnore)) == null 
							&& !info.IsLiteral
							&& !info.IsInitOnly).ToArray();

					PropertyInfo[] properties = elementType.GetProperties().Where(info => 
						info.GetCustomAttribute(typeof(JsonIgnore)) == null 
							&& info.CanWrite 
							&& info.CanRead).ToArray();

					MemberInfo[] members = new MemberInfo[fields.Length + properties.Length];
					fields.CopyTo(members, 0);
					properties.CopyTo(members, fields.Length);

					SerializeMembers(element, members);

					jsonBuilder.Append(OBJSUFFIX);
				}
			}

			/// <summary>
			///		Serializes all members of an object. 
			/// </summary>
			/// <typeparam name="T">Type of the serialized element</typeparam>
			/// <param name="element">The serialized element</param>
			/// <param name="members">members of serialized element</param>
			private void SerializeMembers<T>(T element, MemberInfo[] members)
			{
				for (int i = 0; i < members.Length; i++)
				{
					MemberInfo member = members[i];

					jsonBuilder.Append(QUOTATIONMARK);
					jsonBuilder.Append(member.Name);
					jsonBuilder.Append(QUOTATIONMARK);
					jsonBuilder.Append(OBJDEFINITION);

					// Grabs the member's value and type 
					// based on whether its a property
					// or a field.
					object value = null;
					Type valueType = null; 

					if (member.MemberType == MemberTypes.Field)
					{
						FieldInfo field = member as FieldInfo;
						value = field.GetValue(element);
						valueType = field.FieldType;
					}
					else
					{
						PropertyInfo field = member as PropertyInfo;
						value = field.GetValue(element);
						valueType = field.PropertyType;
					}

					// Determines serialization method 
					// based on the object type.
					if (valueType.IsArray)
					{
						SerializeArray(value);
					}
					else if (valueType.IsPrimitive)
					{
						SerializePrimitive(value);
					}
					else
					{
						SerializeObject(value, valueType);
					}

					// if the last object is not reached,
					// a separator is added. 
					if (i < members.Length - 1)
					{
						jsonBuilder.Append(OBJSEPARATOR);
					}
				}
			}

			private void SerializeArray(object arrayElement)
			{
				Type arraySubtype = arrayElement.GetType().GetElementType();
				jsonBuilder.Append(ARRAYPREFIX);
				Array array = (Array)arrayElement;
				if (arraySubtype.IsPrimitive)
				{
					for (int i = 0; i < array.Length; i++)
					{
						object element = array.GetValue(i);
						SerializePrimitive(element);
						if (i < array.Length - 1)
						{
							jsonBuilder.Append(OBJSEPARATOR);
						}
					}
				}
				else if (arraySubtype.IsArray)
				{
					for (int i = 0; i < array.Length; i++)
					{
						object element = array.GetValue(i);
						SerializeArray(element);
						if (i < array.Length - 1)
						{
							jsonBuilder.Append(OBJSEPARATOR);
						}
					}
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						object element = array.GetValue(i);
						SerializeObject(element, arraySubtype);
						if (i < array.Length - 1)
						{
							jsonBuilder.Append(OBJSEPARATOR);
						}
					}
				}
				jsonBuilder.Append(ARRAYSUFFIX);
			}

			private void SerializePrimitive(object element)
			{
				if (element.GetType() == typeof(string))
				{
					jsonBuilder.Append(QUOTATIONMARK);
					jsonBuilder.Append(element.ToString());
					jsonBuilder.Append(QUOTATIONMARK);
				}
				else
				{
					jsonBuilder.Append(element.ToString());
				}
			}
		}

		private class JsonDeserializer : Json
		{
			private List<object> chronologicalObjects = new List<object>();


			public object DeserializeObject(string json, ref int i, Type elementType)
			{
				string nullValue = json.Substring(i, 4);
				if (nullValue == NULL)
				{
					i += 3;
					return null;
				}

				object element = elementType == typeof(string)
					? ""
					: Activator.CreateInstance(elementType);

				chronologicalObjects.Add(element);

				StringBuilder nameBuilder = new StringBuilder();
				StringBuilder valueBuilder = new StringBuilder();

				if (json[i] != OBJPREFIX)
				{
					return null;
				}
				else
				{
					i++;
				}

				for (char jsonChar; i < json.Length; i++)
				{
					jsonChar = json[i];

					if (jsonChar == OBJSUFFIX)
					{
						return element;
					}

					if (jsonChar != OBJDEFINITION)
					{
						nameBuilder.Append(jsonChar);
					}
					else
					{
						i++;
						string memberName = nameBuilder.ToString();

						if (memberName.StartsWith("\""))
						{
							memberName = memberName.Substring(1, memberName.Length - 2);
						}

						FieldInfo fieldInfo = elementType.GetField(memberName);
						PropertyInfo propertyInfo = elementType.GetProperty(memberName);
						Type objectType;
						
						Action<object> setValue;
						if (fieldInfo == null)
						{
							setValue = (object value) => { propertyInfo.SetValue(element, value); };
							objectType = propertyInfo.PropertyType;
						}
						else
						{
							setValue = (object value) => { fieldInfo.SetValue(element, value); };
							objectType = fieldInfo.FieldType;
						}

						setValue += delegate
						{
							nameBuilder.Clear();
							valueBuilder.Clear();
						};

						object memberValue = null;

						if (objectType.IsPrimitive)
						{
							memberValue = DeserializePrimitive(json, ref i, objectType);
						}
						else if (objectType.IsArray)
						{
							memberValue = DeserializeArray(json, ref i, objectType.GetElementType());
						}
						else
						{
							jsonChar = json[i];
							if (jsonChar == OBJREFERENCE)
							{
								memberValue = DeserializeReference(json, ref i);
							}
							else
							{
								memberValue = DeserializeObject(json, ref i, objectType);
							}
							i++;
						}
						setValue.Invoke(memberValue);


						jsonChar = json[i];
						if (jsonChar == OBJSUFFIX)
						{
							return element;
						}
					}
				}

				return element;
			}

			private object DeserializePrimitive(string json, ref int i, Type primitiveType)
			{
				StringBuilder valueBuilder = new StringBuilder();
				bool isString = false; 
				for (char jsonChar; i < json.Length; i++)
				{
					jsonChar = json[i];
					if (jsonChar == QUOTATIONMARK)
					{
						isString = !isString;
					}

					
					if (!isString && (jsonChar == OBJSEPARATOR || jsonChar == OBJSUFFIX || jsonChar == ARRAYSUFFIX))
					{
						break;
					}

					valueBuilder.Append(jsonChar);
				}

				string value = valueBuilder.ToString();

				if (value == NULL)
				{
					return null;
				}

				if (primitiveType == typeof(string))
				{
					value = value.Substring(1, value.Length - 2);
				}

				return Convert.ChangeType(value, primitiveType);
			}

			private Array DeserializeArray(string json, ref int i, Type arraySubType)
			{
				List<object> arrayElements = new List<object>();

				if (arraySubType.IsPrimitive)
				{
					for (char jsonChar; i < json.Length; i++)
					{
						jsonChar = json[i];

						if (jsonChar == ARRAYSUFFIX)
						{
							break;
						}
						else if (jsonChar == ARRAYPREFIX)
						{
							// When the prefix is followed by a suffix, the array is empty.
							int j = i + 1;
							if (j < json.Length && json[j] == ARRAYSUFFIX)
							{
								break;
							}
						}

						// if the code reaches this point, json[i] is always the 
						// prefix or a comma. Therefore, i is incremented. 
						i++;

						object element = DeserializePrimitive(json, ref i, arraySubType);
						arrayElements.Add(element);

						// i is decremented so the prior if-statement can see 
						// whether it's a special character.
						i--;
					}

					// Is incremented so json[i] no longer points at the array-suffix.
					i++;
				}
				else if (arraySubType.IsArray)
				{
					for (char jsonChar; i < json.Length; i++)
					{
						jsonChar = json[i];
						if (jsonChar == ARRAYPREFIX)
						{
							i++;
							object element = DeserializeArray(json, ref i, arraySubType.GetElementType());
							arrayElements.Add(element);
							i++;
							jsonChar = json[i];
						}

						if (jsonChar == ARRAYSUFFIX)
						{
							break;
						}

						i--;
					}
				}
				else
				{
					for (char jsonChar; i < json.Length; i++)
					{
						jsonChar = json[i];
						if (jsonChar == OBJPREFIX)
						{
							object element = DeserializeObject(json, ref i, arraySubType);
							arrayElements.Add(element);
						}
						else if (jsonChar == OBJREFERENCE)
						{
							object element = DeserializeReference(json, ref i);
							arrayElements.Add(element);
						}

						jsonChar = json[i];
						if (jsonChar == ARRAYSUFFIX)
						{
							break;
						}
					}
				}

				// All elements of the list are copied to Array.
				Array array = Array.CreateInstance(arraySubType, arrayElements.Count);
				for (int j = 0; j < arrayElements.Count; j++)
				{
					object element = arrayElements[j];
					array.SetValue(element, j);
				}

				return array;
			}

			private object DeserializeReference(string json, ref int i)
			{
				if (json[i] == OBJREFERENCE)
				{
					i++;
				}

				StringBuilder referenceBuilder = new StringBuilder();

				for (char jsonChar; i < json.Length; i++)
				{
					jsonChar = json[i];

					if (jsonChar == OBJREFERENCE)
					{
						break;
					}
					else
					{ 
						referenceBuilder.Append(jsonChar);
					}
				}

				int j = (int)Convert.ChangeType(referenceBuilder.ToString(), typeof(int));
				return chronologicalObjects[j];
			}
		}


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
		public static T FromJson<T>(string json) where  T : new()
		{
			JsonDeserializer jsonDeserializer = new JsonDeserializer();
			int i = 0;
			return (T)jsonDeserializer.DeserializeObject(json, ref i, typeof(T));
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
			int i = 0;
			return jsonDeserializer.DeserializeObject(json, ref i, elementType);
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
