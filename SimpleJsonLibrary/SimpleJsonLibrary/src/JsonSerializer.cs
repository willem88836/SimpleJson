using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SimpleJsonLibrary
{
	/// <summary>
	///		Class that serializes objects to Json strings.
	/// </summary>
	internal class JsonSerializer : Json
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
		internal string ToJson(object element, Type elementType, bool enableHashing)
		{
			this.jsonBuilder = new StringBuilder();
			this.enableHashing = enableHashing;
			if (this.enableHashing)
			{
				objectHashes = new List<int>();
			}

			if (elementType.IsArray)
			{
				SerializeArray(element);
			}
			else if (elementType.IsPrimitive)
			{
				SerializePrimitive(element);
			}
			else
			{
				SerializeObject(element, elementType);
			}

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

			// String is the only object with exception to the rule.
			if (elementType == typeof(string))
			{
				jsonBuilder.Append(QUOTATIONMARK);
				jsonBuilder.Append(element.ToString());
				jsonBuilder.Append(QUOTATIONMARK);
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
			if (arraySubtype.IsPrimitive)
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
			jsonBuilder.Append(element.ToString());
		}
	}
}
