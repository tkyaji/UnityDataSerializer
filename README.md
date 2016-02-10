# UnityDataSerializer

This Unity package save the serialized C# objects to file.
Serialized data can be encrypted and compressed.

## Example
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public class Sample {

	public static void execute() {
		// enable encrypt data (option)
		DataSerializer.EnableEncryption("abcdefghijklmnopqrstuvwxyz012345", "abcdefghijklmnop");

		// set data
		DataSerializer.SetData ("data", new Data (10, "test"));
		DataSerializer.SetData ("int", 999);
		// save data to file
		DataSerializer.Apply ();

		// get data
		Data data = DataSerializer.GetData<Data> ("data");
		int i = DataSerializer.GetData<int> ("int", 0);

		Debug.Log (data);
		Debug.Log (i);
	}

	[Serializable]
	private class Data {
		public Data(int i, string s) {
			intVal = i;
			strVal = s;
		}
		override public string ToString() {
			return string.Format ("i:{0}, s:{1}", intVal, strVal);
		}
		private int intVal;
		private string strVal;
	}
}
```
