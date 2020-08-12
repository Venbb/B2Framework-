using UnityEngine;

namespace B2Framework.Unity
{
    public class GameStart : MonoBehaviour
    {
        void Start()
        {
            The.LuaMgr.Initialize().StartGame();
            //就先放这了。。
            The.sensitiveWordsFilter.AddNewChecker(GameUtility.Assets.GetAssetPath("Localization/SensitiveWords_Chinese.txt"));
        }
    }
}