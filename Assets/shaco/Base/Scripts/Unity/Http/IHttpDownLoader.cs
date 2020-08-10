using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public abstract class IHttpDownLoader : shaco.Base.IGameInstance
    {
        /// <summary>
        /// 开始下载
        /// <param name="url">地址</param>
        /// <param name="callbackProgress">下载进度回调</param>
        /// <param name="callbackEnd">下载完毕回调</param>
        /// </summary>
        abstract public IHttpDownLoader Start(string url, System.Action<float> callbackProgress, System.Action<byte[], string> callbackEnd);

        /// <summary>
        /// 额外添加结束回调方法
        /// </summary>
        abstract public void AddEndCallBack(System.Action<byte[], string> callbackEnd);

        /// <summary>
        /// 强制中断下载
        /// </summary>
        abstract public void Stop();
    }
}