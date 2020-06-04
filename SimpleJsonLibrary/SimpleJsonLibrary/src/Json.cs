using System;

namespace SimpleJsonLibrary
{
	internal class Json
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


		/// <summary>
		///		Returns true if the type is primitive or a string.
		/// </summary>
		/// <param name="type">
		///		tested type.
		///	</param>
		/// <returns></returns>
		protected bool IsPrimitive(Type type)
		{
			return type.IsPrimitive || type == typeof(string);
		}
	}
}
