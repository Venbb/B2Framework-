using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public abstract class IHttpUpLoader : shaco.Base.IGameInstance
    {
        /// <summary>
        /// 开始上传
        /// <param name="url">地址</param>
        /// <param name="header">信息头，可以为空</param> 
        /// <param name="body">信息体，可以为空</param>
        /// <param name="callbackProgress">上传进度回调</param>
        /// <param name="callbackEnd">上传完毕回调</param>
        /// </summary>
        abstract public IHttpUpLoader Start(string url, shaco.Base.HttpComponent[] header, shaco.Base.HttpComponent[] body, System.Action<float> callbackProgress, System.Action<byte[], string> callbackEnd);

        /// <summary>
        /// 开始上传
        /// <param name="url">地址</param>
        /// <param name="form">表单</param> 
        /// <param name="callbackProgress">上传进度回调</param>
        /// <param name="callbackEnd">上传完毕回调</param>
        /// </summary>
        abstract public IHttpUpLoader StartWithForm(string url, shaco.Base.HttpComponent[] form, System.Action<float> callbackProgress, System.Action<byte[], string> callbackEnd);

        /// <summary>
        /// 额外添加结束回调方法
        /// </summary>
        abstract public void AddEndCallBack(System.Action<byte[], string> callbackEnd);

        /// <summary>
        /// 强制中断上传
        /// </summary>
        abstract public void Stop();
    }
}