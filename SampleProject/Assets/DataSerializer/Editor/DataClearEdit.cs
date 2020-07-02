using UnityEditor;

public class DataClearEdit {

	[MenuItem("Tools/DataSerializer/Clear All Data")]
	public static void DataClear() {
		DataSerializer.ClearAllData();
	}
}
