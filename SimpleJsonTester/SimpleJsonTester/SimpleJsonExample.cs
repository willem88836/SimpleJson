using SimpleJsonLibrary;
using System;

public class MyJsonObject
{
	public int MyInteger;
	public string MyString;
	// This field is ignored.
	[JsonIgnore] public bool MyBoolean;
	public MyJsonObject MyNestedReference;
}


public class SimpleJsonExample
{
	public void DoJsonExample()
	{
		MyJsonObject jsonObject = new MyJsonObject()
		{
			MyInteger = 15,
			MyString = "I am some string!",
			MyBoolean = true,
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
			MyInteger = 24,
			MyString = "Hello World!",
			MyBoolean = false
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
