using B2Framework;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Log.Debug("[Assets]{0}", "???????");
        // Debug.Log("[Assets]{0}", "???????");
        Log.Debug("<color=#ff0000>{0}</color>", "hello world");
        Log.Debug("<color=yellow>{0}</color>", "welcome to unity");
        Log.Debug(System.Environment.CurrentDirectory);
        Log.Debug(Application.dataPath);
        Log.Debug(Application.platform.ToString());
        // Debug.Log(UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString());
        // Debug.Log(UnityEditor.AssetDatabase.LoadAssetAtPath)
        Log.Debug(Application.streamingAssetsPath);
        Log.Debug(Application.persistentDataPath);

        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Launch");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
