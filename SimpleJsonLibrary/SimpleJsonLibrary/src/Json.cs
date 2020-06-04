using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SimpleJsonLibrary
{
	// TODO: Nested strings do not work.
	/// <summary>
	///		JsonUtility provides simple Json serialization and deserialization functionalities.
	/// </summary>
	public static class JsonUtility
	{
		private class Json
		{
			protected const string NULL = "null";
			protected const char OBJECTPREFIX = '{';
			protected const char OBJECTSUFFIX = '}';
			protected const char ARRAYPREFIX = '[';
			protected const char ARRAYSUFFIX = ']';
			protected const char OBJECTDEFINITION = ':';
			protected const char OBJECTSEPARATOR = ',';
			protected const char QUOTATIONMARK = '\"';
			protected const char OBJECTREFERENCE = '$';
		}


		/// <summary>
		///		Class that serializes objects to Json strings.
		/// </summary>
		private class JsonSerializer : Json
		{
			private StringBuilder jsonBuilder;
			private List<int> objectHashes;
			private bool enableHashing;


			/// <summary>
			///		Serializes one object to a json string. 
			/// </summary>
			/// <param name="element">
			///		The element that is serialized.
			///	</param>
			/// <param name="elementType">
			///		The type of the element that is serialized.
			/// </param>
			/// <param name="enableHashing">
			///		If true, the objects' hashcode is used to determine nested references. 
			///		And duplicates are referred to by a number.
			///		<br></br>
			///		If set to false, and nested objects to refer to earlier objects, 
			///		you will get a StackOverflow Exception.
			/// </param>
			/// <returns>The object's Json string.</returns>
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

			/// <summary>
			///		Serializes one object to json, and appends this value to the jsonBuilder. 
			/// </summary>
			/// <param name="element">
			///		Element that is serialized.
			///	</param>
			/// <param name="elementType">
			///		Type of the element taht is serialized.
			///	</param>
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
					jsonBuilder.Append(OBJECTREFERENCE);
					jsonBuilder.Append(index);
					jsonBuilder.Append(OBJECTREFERENCE);
				}
				else
				{
					if (enableHashing)
					{
						int index = objectHashes.Count;
						objectHashes.Add(hash);
					}

					jsonBuilder.Append(OBJECTPREFIX);

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

					jsonBuilder.Append(OBJECTSUFFIX);
				}
			}

			/// <summary>
			///		Serializes all members of one object. appends strings to jsonBuilder.
			/// </summary>
			/// <param name="element">
			///		The serialized element.
			///	</param>
			/// <param name="members">
			///		Members of serialized element.
			///	</param>
			private void SerializeMembers<T>(T element, MemberInfo[] members)
			{
				for (int i = 0; i < members.Length; i++)
				{
					MemberInfo member = members[i];

					jsonBuilder.Append(QUOTATIONMARK);
					jsonBuilder.Append(member.Name);
					jsonBuilder.Append(QUOTATIONMARK);
					jsonBuilder.Append(OBJECTDEFINITION);

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
					else if (IsPrimitive(valueType))
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
						jsonBuilder.Append(OBJECTSEPARATOR);
					}
				}
			}

			/// <summary>
			///		Serializes an array object that contains any type of elements.
			///		Appends strings to jsonBuilder.
			/// </summary>
			/// <param name="arrayElement">
			///		Array that is serialized.
			///	</param>
			private void SerializeArray(object arrayElement)
			{
				Type arraySubtype = arrayElement.GetType().GetElementType();
				jsonBuilder.Append(ARRAYPREFIX);
				Array array = (Array)arrayElement;
				if (IsPrimitive(arraySubtype))
				{
					for (int i = 0; i < array.Length; i++)
					{
						object element = array.GetValue(i);
						SerializePrimitive(element);
						if (i < array.Length - 1)
						{
							jsonBuilder.Append(OBJECTSEPARATOR);
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
							jsonBuilder.Append(OBJECTSEPARATOR);
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
							jsonBuilder.Append(OBJECTSEPARATOR);
						}
					}
				}
				jsonBuilder.Append(ARRAYSUFFIX);
			}

			/// <summary>
			///		Serializes a primitive element to Json. Appends Json string to jsonBuilder.
			/// </summary>
			/// <param name="element">
			///		The Serialized element.
			/// </param>
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


		/// <summary>
		///		Class that deserializes Json strings to objects. 
		/// </summary>
		private class JsonDeserializer : Json
		{
			private List<object> chronologicalObjects;
			private string json;
			private int index = 0;


			/// <summary>
			///		Deserializes one object from Json. 
			/// </summary>
			/// <param name="json">
			///		The Json string that is deserialized.
			///	</param>
			/// <param name="elementType">
			///		Type of the element that is deserialized.
			///	</param>
			/// <returns></returns>
			public object FromJson(string json, Type elementType)
			{
				this.chronologicalObjects = new List<object>();
				this.json = json;
				this.index = 0;
				return DeserializeObject(elementType);
			}

			/// <summary>
			///		Deserializes one object from Json. 
			///		Uses local index to determine where in the Json string it is.
			/// </summary>
			/// <param name="elementType">
			///		Type of the element that is deserialized.
			///	</param>
			/// <returns>
			///		Deserialized object. 
			/// </returns>
			private object DeserializeObject(Type elementType)
			{
				string nullValue = json.Substring(index, 4);
				if (nullValue == NULL)
				{
					index += 3;
					return null;
				}

				object element = elementType == typeof(string)
					? ""
					: Activator.CreateInstance(elementType);

				chronologicalObjects.Add(element);

				StringBuilder nameBuilder = new StringBuilder();
				StringBuilder valueBuilder = new StringBuilder();

				if (json[index] != OBJECTPREFIX)
				{
					return null;
				}
				else
				{
					index++;
				}

				for (char jsonChar; index < json.Length; index++)
				{
					jsonChar = json[index];

					if (jsonChar == OBJECTSUFFIX)
					{
						return element;
					}

					if (jsonChar != OBJECTDEFINITION)
					{
						nameBuilder.Append(jsonChar);
					}
					else
					{
						index++;
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

						if (IsPrimitive(objectType))
						{
							memberValue = DeserializePrimitive(objectType);
						}
						else if (objectType.IsArray)
						{
							memberValue = DeserializeArray(objectType.GetElementType());
						}
						else
						{
							if (json[index] == OBJECTREFERENCE)
							{
								memberValue = DeserializeReference();
							}
							else
							{
								memberValue = DeserializeObject(objectType);
							}
							index++;
						}
						setValue.Invoke(memberValue);


						jsonChar = json[index];
						if (jsonChar == OBJECTSUFFIX)
						{
							return element;
						}
					}
				}

				return element;
			}

			/// <summary>
			///		Deserializes primitive from Json. 
			///		Uses local index to determine where in the Json string it is. 
			/// </summary>
			/// <param name="primitiveType">
			///		Type of the object that is deserialized.
			/// </param>
			/// <returns>
			///		Deserialized object.
			/// </returns>
			private object DeserializePrimitive(Type primitiveType)
			{
				StringBuilder valueBuilder = new StringBuilder();
				bool isString = false; 
				for (char jsonChar; index < json.Length; index++)
				{
					jsonChar = json[index];
					if (jsonChar == QUOTATIONMARK)
					{
						isString = !isString;
					}

					
					if (!isString && (jsonChar == OBJECTSEPARATOR || jsonChar == OBJECTSUFFIX || jsonChar == ARRAYSUFFIX))
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

			/// <summary>
			///		Deserializes array from Json. 
			///		Uses local index to determine where in the Json string it is. 
			/// </summary>
			/// <param name="arraySubType">
			///		The subtype of the array that is deserialized.
			/// </param>
			/// <returns>
			///		Deserialized array.
			/// </returns>
			private Array DeserializeArray(Type arraySubType)
			{
				List<object> arrayElements = new List<object>();

				// When the prefix is followed by a suffix, the array is empty.
				if (json[index + 1] != ARRAYSUFFIX)
				{
					for (char jsonChar; index < json.Length; index++)
					{
						jsonChar = json[index];

						// if json[i] is a suffix, the array is finished. 
						if (jsonChar == ARRAYSUFFIX || json[index + 1] == OBJECTSUFFIX)
						{
							break;
						}

						// i is incremented so it points to the element
						// after the comma or array's own prefix.
						index++;

						object element = null;

						// Based on the current character, 
						// a deserialization methodis chosen. 
						switch (json[index])
						{
							case OBJECTPREFIX:
								element= DeserializeObject(arraySubType);
								// TODO: Is incremented, as DeserializeObject ends on 
								// the last character, not the next.
								index++;
								break;
							case OBJECTREFERENCE:
								element = DeserializeReference();
								// TODO: Is incremented, as DeserializeReference ends on 
								// the last character, not the next.
								index++;
								break;
							case ARRAYPREFIX:
								element = DeserializeArray(arraySubType.GetElementType());
								break;
							default:
								element = DeserializePrimitive(arraySubType);
								break;
						}

						arrayElements.Add(element);

						// i is decremented so the prior, in-loop if-statement can see 
						// whether it's a special character.
						index--;
					}
				}
				else
				{
					// i is incremented, so it points to the array-suffix.
					index++;
				}


				// Is incremented so json[i] no longer points at the array-suffix.
				index++;

				// All elements of the list are copied to Array.
				Array array = Array.CreateInstance(arraySubType, arrayElements.Count);
				for (int j = 0; j < arrayElements.Count; j++)
				{
					object element = arrayElements[j];
					array.SetValue(element, j);
				}

				return array;
			}

			/// <summary>
			///		Deserializes a reference from Json.
			///		Uses local index to determine where in the Json string it is. 
			///		Uses chronologicalObjects to determine what object is being referenced.
			/// </summary>
			/// <returns>
			///		The referenced object. 
			/// </returns>
			private object DeserializeReference()
			{
				if (json[index] == OBJECTREFERENCE)
				{
					index++;
				}

				StringBuilder referenceBuilder = new StringBuilder();

				for (char jsonChar; index < json.Length; index++)
				{
					jsonChar = json[index];

					if (jsonChar == OBJECTREFERENCE)
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
		///		Returns true if the type is primitive or a string.
		/// </summary>
		/// <param name="type">
		///		tested type.
		///	</param>
		/// <returns></returns>
		private static bool IsPrimitive(Type type)
		{
			return type.IsPrimitive || type == typeof(string);
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
