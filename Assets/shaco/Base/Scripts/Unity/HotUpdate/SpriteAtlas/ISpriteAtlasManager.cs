using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class SpriteAtlasDefine
    {
        public const string EXTENSION_SPRITE_ATTLAS = ".spriteatlas";
    }

    /// <summary>
    /// 图集管理器
    /// 详细配置内容在ISpriteAtlasSeting
    /// </summary>
    public interface ISpriteAtlasManager : shaco.Base.IGameInstance
    {
        /// <summary>
        /// 开始检查图集是否被请求同步加载
        /// </summary>
        void StartCheckLoadAtlas();

        /// <summary>
        /// 等图集同步加载完毕
        /// </summary>
        void EndCheckLoadAtlas(System.Type type);

        /// <summary>
        /// 开始检查图集是否被请求异步加载
        /// </summary>
        void StartCheckLoadAtlasAysnc();

        /// <summary>
        /// 等图集异步加载完毕
        /// </summary>
        void EndCheckLoadAtlasAsync(System.Type type, System.Action callbackEnd);
    }
}