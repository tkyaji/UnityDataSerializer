using UnityEditor;

public class DataClearEdit {

	[MenuItem("DataSerializer/Clear All Data")]
	public static void DataClear() {
		DataSerializer.ClearAllData();
	}
}
