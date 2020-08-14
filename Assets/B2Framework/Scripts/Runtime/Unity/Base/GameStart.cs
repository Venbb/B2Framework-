using UnityEngine;

namespace B2Framework.Unity
{
    public class GameStart : MonoBehaviour
    {
        void Start()
        {
            LuaManager.Instance.Initialize().StartGame();
            //就先放这了。。
            SensitiveWordsFilter.Instance.AddNewChecker(GameUtility.Assets.GetAssetPath("Localization/SensitiveWords_Chinese.txt"));
        }
    }
}