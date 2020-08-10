using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shacoEditor
{
    public sealed class OpenSceneAssetAttribute : shaco.Base.OrderCallBackAttribute
    {
        public OpenSceneAssetAttribute() { }
        public OpenSceneAssetAttribute(int callbackOrder) : base(callbackOrder) { }

        /// <summary>
        /// 回调方法和参数例子，当编辑器中打开场景的时候被调用
        /// </summary>
        // static public void OpenSceneAssetCallBack(UnityEngine.Object openedSceneAsset)
        // {

        // }
    }
}