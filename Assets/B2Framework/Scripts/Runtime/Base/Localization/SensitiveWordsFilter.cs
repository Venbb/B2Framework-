using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace B2Framework
{
    public class SensitiveWordsFilter : MonoSingleton<SensitiveWordsFilter>
    {
        public List<SensitiveWordsChecker> sensitiveWordsCheckerList = new List<SensitiveWordsChecker>();
        public SensitiveWordsFilter()
        {
            //TODO: 多语言：是只过滤当前国家的语言，还是同时过滤所有的语言？
        }

        public void AddNewChecker(string path)
        {
            SensitiveWordsChecker swc = new SensitiveWordsChecker();
            swc.Init(path);
            sensitiveWordsCheckerList.Add(swc);
        }

        public string Check(string s)
        {
            for(int i = 0; i < sensitiveWordsCheckerList.Count; ++i)
            {
                s = sensitiveWordsCheckerList[i].CheckAndReplace(s);
            }
            return s;
        }

        public override void Dispose()
        {
            sensitiveWordsCheckerList.Clear();
        }
    }

}