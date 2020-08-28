using UnityEngine;

namespace B2Framework
{
    public class GameStart : MonoBehaviour
    {
        void Start()
        {
            var init = LuaManager.Instance.Initialize() as LuaManager;
            init.StartGame();
            //就先放这了。。
            SensitiveWordsFilter.Instance.AddNewChecker(GameUtility.Assets.GetAssetPath("Localization/SensitiveWords_Chinese.txt"));
        }
    }
}