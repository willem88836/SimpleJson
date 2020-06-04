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

				Console.WriteLine("Hello World!");

				JsonTestObject jsonTestObject = new JsonTestObject()
				{
					index = 3
				};




				JsonTestObject myObject = new JsonTestObject()
				{
					index = 0,
					myJsonObject = new JsonTestObject()
					{
						index = 1,
						myJsonObject = new JsonTestObject() 
						{
							index = 2
						},
						myJsonObject2 = jsonTestObject
					},
					myJsonObject2 = jsonTestObject
				};
				
				string json1 = JsonUtility.ToJson(myObject, typeof(JsonTestObject), false);

				JsonTestObject deserializedJson = JsonUtility.FromJson<JsonTestObject>(json1);
				string json2 = JsonUtility.ToJson(deserializedJson, typeof(JsonTestObject), false);
				Console.WriteLine(json1.Replace(",", ",\n").Replace("{", "{\n") + "\n\n");
				Console.WriteLine(json2.Replace(",", ",\n").Replace("{", "{\n"));

				bool equal = areEqual(json1, json2);

				Console.WriteLine("original = deserialized: " + equal);
			}
			catch(Exception ex) {
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







	public class JsonTestObject
	{
		public int index = -1;
		//public bool Boolvalue = true;
		//public byte Bytevalue = 126;
		//public sbyte Sbytevalue = 66;
		//public char Charvalue = 'd';
		//public decimal Decimalvalue = (decimal)123.82654654;
		//public double Doublevalue = (double)321.8520963;
		//public float Floatvalue = (float)987.321654987;
		//public int Intvalue = -5533;
		//public uint Uintvalue = 445626;
		//public long Longvalue = -987654;
		//public ulong Ulongvalue = 98756462432665;
		//public object Objectvalue = 654654;
		//public short Shortvalue = -7789;
		//public ushort Ushortvalue = 987;
		//public string Stringvalue = "I'm a string string! string string, woah string";

		[JsonIgnore] public bool Boolvalue2 = false;
		[JsonIgnore] public byte Bytevalue2 = 45;
		[JsonIgnore] public sbyte Sbytevalue2 = 89;
		[JsonIgnore] public char Charvalue2 = ',';
		[JsonIgnore] public decimal Decimalvalue2 = (decimal)987;
		[JsonIgnore] public double Doublevalue2 = (double)987654;
		[JsonIgnore] public float Floatvalue2 = (float)652.5555;
		[JsonIgnore] public int Intvalue2 = -852;
		[JsonIgnore] public uint Uintvalue2 = 654654;
		[JsonIgnore] public long Longvalue2 = 987654;
		[JsonIgnore] public ulong Ulongvalue2 = 3333936;
		[JsonIgnore] public object Objectvalue2 = 987654128;
		[JsonIgnore] public short Shortvalue2 = 21;
		[JsonIgnore] public ushort Ushortvalue2 = 1212;
		[JsonIgnore] public string Stringvalue2 = "A SECOND STRING?! ERMGH!";

		public JsonTestObject myJsonObject;
		public JsonTestObject myJsonObject2;
	}
}
