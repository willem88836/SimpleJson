using System;
using System.Threading;
using SimpleJsonLibrary;

namespace SimpleJsonTester
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				JsonTestObject myObject = new JsonTestObject();




				string json1 = JsonUtility.ToJson(myObject, typeof(JsonTestObject), false);
				Console.WriteLine(json1/*.Replace(",", ",\n").Replace("{", "{\n")*/ + "\n\n");

				JsonTestObject deserializedJson = JsonUtility.FromJson<JsonTestObject>(json1);
				string json2 = JsonUtility.ToJson(deserializedJson, typeof(JsonTestObject), false);
				Console.WriteLine(json2/*.Replace(",", ",\n").Replace("{", "{\n")*/ + "\n\n");

				bool equal = areEqual(json1, json2);

				Console.WriteLine("original = deserialized: " + equal);
			}
			catch (Exception ex) {
				Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
			}
		}

		private static bool areEqual(string json1, string json2)
		{
			bool equal = true;
			for (int i = 0; i < json1.Length && i < json2.Length; i++)
			{
				char c1 = json1[i];
				char c2 = json2[i];
				if (c1 != c2)
				{
					equal = false;
					Console.WriteLine("I: " + i.ToString() + " - " + c1.ToString() + " - " + c2.ToString());
				}
			}
			return equal;
		}
	}




	public class JsonArrayObject
	{
		public int index = 0;
		public JsonArrayObject(int index)
		{
			this.index = index;
		}

		public JsonArrayObject()
		{

		}
	}



	public class JsonTestObject
	{
		public int[] intArrayA = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
		public int[] intArrayB = new int[0];
		public int[] intArrayC = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };

		public int[][] intArrayArrayA = new int[][]
		{
			new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 },
			new int[] { 0, 9, 8, 7, 6, 5, 4, 3, 2, 1 },
			new int[] { 2, 4, 6, 8, 0, 1, 3, 5, 7, 9 },
			new int[] { 9, 7, 5, 3, 1, 2, 4, 6, 8, 0 }
		};

		public int[][] intArrayArrayB = new int[0][];

		//public string[] stringArrayA = new string[] { "1", "2", "3", "4", "5" };
		//public string[] stringArrayB = new string[0];

		//public JsonArrayObject[] jsonArrayA = new JsonArrayObject[]
		//{
		//	new JsonArrayObject(0),
		//	new JsonArrayObject(1),
		//	new JsonArrayObject(2),
		//	new JsonArrayObject(3),
		//	new JsonArrayObject(4),
		//};

		//public JsonArrayObject[] jsonArrayB = new JsonArrayObject[0];
	}
}
