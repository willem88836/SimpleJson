﻿using System;
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

			JsonTestObject myObject = new JsonTestObject()
			{
				myJsonObject = new JsonTestObject()
				{
					myJsonObject = new JsonTestObject()
				},
				myJsonObject2 = new JsonTestObject()
			};



			string json = JsonUtility.ToJson(myObject);
			Console.WriteLine(json);

			//JsonTestObject deserializedJson = JsonUtility.FromJson<JsonTestObject>(json);
			//string json2 = JsonUtility.ToJson(deserializedJson);
			//Console.WriteLine(json2);

			//Console.WriteLine("original = deserialized: " + json == json2);
			}
			catch(Exception ex) {
				Console.WriteLine(ex.Message + "\n" + ex.StackTrace);
			}
		}
	}







	public class JsonTestObject
	{
		public bool Boolvalue = true;
		public byte Bytevalue = 126;
		public sbyte Sbytevalue = 66;
		public char Charvalue = 'd';
		public decimal Decimalvalue = (decimal)123.82654654;
		public double Doublevalue = (double)321.8520963;
		public float Floatvalue = (float)987.321654987;
		public int Intvalue = -5533;
		public uint Uintvalue = 445626;
		public long Longvalue = -987654;
		public ulong Ulongvalue = 98756462432665;
		public object Objectvalue = 654654;
		public short Shortvalue = -7789;
		public ushort Ushortvalue = 987;
		public string Stringvalue = "I'm a string string! string string, woah string";

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
