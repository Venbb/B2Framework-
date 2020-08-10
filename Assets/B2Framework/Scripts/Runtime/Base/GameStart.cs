using UnityEngine;

namespace B2Framework
{
    public class GameStart : MonoBehaviour
    {
        void Start()
        {
            The.LuaMgr.Initialize().StartGame();
            //就先放这了。。
            The.sensitiveWordsFilter.AddNewChecker(Utility.Assets.GetAssetPath("Localization/SensitiveWords_Chinese.txt"));
        }
    }
}