using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace shacoEditor
{
    /// <summary>
    /// 编辑器图集设置接口
    /// </summary>
    public interface ISpriteAtlasSettings
    {
        /// <summary>
        /// 是否可以被添加到普通图集中
        /// </summary>
        bool CanBuildInAtlas(Texture texture);

        /// <summary>
        /// 是否可以被添加到公用图集中
        /// </summary>
        bool CanBuildInGlobalAtlas(Texture texture, System.Collections.Generic.ICollection<string> referenceTargets);

        /// <summary>
        /// 是否可以被添加到文件夹共享图集中
        /// </summary>
        bool CanBuildInSubSharedAtlas(Texture texture, System.Collections.Generic.ICollection<string> referenceTargets);

        /// <summary>
        /// 保存配置
        /// </summary>
        void SaveSettings(shaco.Base.IDataSave datasave);

        /// <summary>
        /// 加载配置
        /// </summary>
        void LoadSettings(shaco.Base.IDataSave datasave);

        /// <summary>
        /// 编辑器界面绘制
        /// </summary>
        void OnGUI();
    }
}