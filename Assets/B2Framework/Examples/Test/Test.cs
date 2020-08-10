using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug= B2Framework.Debug;
using B2Framework;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(string.Format("[Assets]{0}", "???????"));
        // Debug.Log("[Assets]{0}", "???????");
        Debug.Log(string.Format("<color=#ff0000>{0}</color>", "hello world"));
        Debug.Log(string.Format("<color=yellow>{0}</color>", "welcome to unity"));
        Debug.Log(System.Environment.CurrentDirectory);
        Debug.Log(Application.dataPath);
        Debug.Log(Application.platform.ToString());
        // Debug.Log(UnityEditor.EditorUserBuildSettings.activeBuildTarget.ToString());
        // Debug.Log(UnityEditor.AssetDatabase.LoadAssetAtPath)
        Debug.Log(Application.streamingAssetsPath);
        Debug.Log(Application.persistentDataPath);

        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Launch");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
