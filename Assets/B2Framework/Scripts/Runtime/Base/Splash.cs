using UnityEngine;
using UnityEngine.SceneManagement;

namespace B2Framework
{
    public class Splash : MonoBehaviour
    {
        public int sceneIndex;
        public void Load()
        {
            SceneManager.LoadSceneAsync(sceneIndex);
        }
    }
}