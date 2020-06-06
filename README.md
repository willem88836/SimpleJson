# SimpleJson
SimpleJson is a simple-to-use Json library for .NET C#.
It focusses on the essential elements of a Json library, without all the complexities other libraries often offer. 

### Features
  * (De)serialization of one-layer objects (objects that only contain primitives).
  * (De)serialization of multi-layer objects (objects that contain other objects).
  * (De)serialization of objects with multi- and single-level arrays (containing any type of object).
  * Attribute to ignore specific fields/properties of your objects. 
  * (De)serialization of self-referencing objects (or objects that refer to the same object multiple times).
  
### Getting Started
The easiest way to get started is to download the SimpleJsonLibary.dll file from [here](https://github.com/willem88836/SimpleJson/releases). 
Import this file into your project, and you're good to go!

### Quick Example
``` cs
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
```
