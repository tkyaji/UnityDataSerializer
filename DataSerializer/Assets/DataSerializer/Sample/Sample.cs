using UnityEngine;
using System;
using System.Collections;

public class Sample : MonoBehaviour {

	void Start () {
		// enable encrypt data (option)
		DataSerializer.EnableEncryption("abcdefghijklmnopqrstuvwxyz012345", "abcdefghijklmnop");

		// enable compression data (option)
		DataSerializer.EnableCompression ();

		// preload data
		DataSerializer.PreLoad ("data_val", "int_val");
	}

	void OnApplicationPause(bool pauseStatus) {
		if (pauseStatus) {
			// save data to file
			DataSerializer.Apply ();
		}
	}

	void OnApplicationQuit() {
		// save data to file
		DataSerializer.Apply ();
	}


	public void SetData() {
		// set data
		DataSerializer.SetData ("data_val", new Data (10, "test"));
		DataSerializer.SetData ("int_val", 999);
	}

	public void GetData() {
		// get data
		Data data = DataSerializer.GetData<Data> ("data_val");
		int i = DataSerializer.GetData<int> ("int_val", 1);

		Debug.Log (data);
		Debug.Log (i);
	}

	public void RemoveData() {
		// remove data
		DataSerializer.RemoveData ("data_val");
		DataSerializer.RemoveData ("int_val");
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
