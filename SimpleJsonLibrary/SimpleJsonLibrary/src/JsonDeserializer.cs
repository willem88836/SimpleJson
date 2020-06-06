using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace SimpleJsonLibrary
{
	/// <summary>
	///		Class that deserializes Json strings to objects. 
	/// </summary>
	internal class JsonDeserializer : Json
	{
		private List<object> chronologicalObjects;
		private string json;
		private int index;


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
		internal object FromJson(string json, Type elementType)
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

					// strings are handled as primitives during deserialization, 
					// as this is far easier to handle than when they're an object. 
					// Note: during serialization they ARE handled as objects. 
					// NOT as primitives. 
					if (objectType.IsPrimitive || objectType == typeof(string))
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
			// inString is used to determine whether the current character
			// is inside of a string. If it is, it cannot break the loop. 
			bool inString = false;
			for (char jsonChar; index < json.Length; index++)
			{
				jsonChar = json[index];
				
				if (jsonChar == QUOTATIONMARK)
				{
					inString = !inString;
				}

				if (!inString && (jsonChar == OBJECTSEPARATOR || jsonChar == OBJECTSUFFIX || jsonChar == ARRAYSUFFIX))
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
							element = DeserializeObject(arraySubType);
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
}
