using SimpleJsonLibrary;
using System;

public class MyJsonObject
{
	public int myInteger;
	public string myString;
	// This field is ignored.
	[JsonIgnore] public bool myBoolean;
	public MyJsonObject MyNestedReference;
}


public class SimpleJsonExample
{
	public void DoJsonExample()
	{
		MyJsonObject jsonObject = new MyJsonObject()
		{
			myInteger = 15,
			myString = "I am some string!",
			myBoolean = true,
			MyNestedReference = new MyJsonObject()
		};

		// With just one line of code it is converted to json!
		string json = JsonUtility.ToJson(jsonObject);
		
		Console.WriteLine(json);

		// And with another line of code it's converted back!
		MyJsonObject deserializedJson = JsonUtility.FromJson<MyJsonObject>(json);
	}

	public void DoJsonExampleHashed()
	{
		MyJsonObject jsonObject = new MyJsonObject()
		{
			myInteger = 24,
			myString = "Hello World!",
			myBoolean = false
		};

		// The object now references itself!
		jsonObject.MyNestedReference = jsonObject;

		// Set the enableHashing parameter to true, 
		// so the object can reference itself.
		string json = JsonUtility.ToJson(jsonObject, true);

		Console.WriteLine(json);

		// And with the same simple line it is converted back!
		MyJsonObject deserializedJson = JsonUtility.FromJson<MyJsonObject>(json);
	}
}
