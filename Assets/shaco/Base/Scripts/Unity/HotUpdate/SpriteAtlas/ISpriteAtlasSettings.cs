using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    [System.Serializable]
    public class SpriteAtlasSettingsInfo
    {
        public string atlasName;
        public string atlasFolder;
    }

    /// <summary>
    /// 图集配置类
    /// </summary>
    public interface ISpriteAtlasSettings : shaco.Base.IGameInstance
    {
        /// <summary>
        /// 清空图集配置数据
        /// </summary>
        void Clear();

        /// <summary>
        /// 添加图集配置数据
        /// </summary>
        void AddAtlasInfo(SpriteAtlasSettingsInfo info);

        /// <summary>
        /// 图集名字是否已经存在
        /// <param name="atlasName">图集名字</param>
        /// </summary>
        bool ContainsAtlasName(string atlasName);

        /// <summary>
        /// 图集路径是否已经存在
        /// <param name="atlasPath">图集名字</param>
        /// </summary>
        bool ContainsAtlasPath(string atlasPath);

        /// <summary>
        /// 根据图集名字获取对应的相对路径
        /// 根据名字下划线'_'进行划分并拼接路径，例如图集名字为a_b.spriteatlas，那么全路径为a/a_b.spriteatlas
        /// </summary>
        string GetAtlasPath(string atlasName);

        /// <summary>
        /// 根据图集名字获取对应的多版本路径
        /// </summary>
        string GetMultiVersionControlRelativePath(string atlasName);

        /// <summary>
        /// 根据图集名字获取对应的文件夹相对路径
        /// 根据名字下划线'_'进行划分并拼接路径，例如图集名字为a_b.spriteatlas，那么全路径为a/a_b.spriteatlas
        /// </summary>
        string GetAtlasFolderPath(string atlasName);
    }
}