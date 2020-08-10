using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class HotUpdateExportWindow : EditorWindow
    {
		/// <summary>
		/// 当有文件删除的时候，刷新配置并重新写入文件
		/// </summary>
		public void UpdateVersionControlWhenDelete(string pathRoot, shaco.HotUpdateDefine.SerializeVersionControl versionControl, string[] deleteAssetbundleNames)
		{
            int deletedCount = 0;
			for (int i = 0; i < deleteAssetbundleNames.Length; ++i)
			{
                //删除配置文件中assetbundle信息
                var deleteInfo = DeleteAssetBundleInConfig(versionControl, deleteAssetbundleNames[i]);
				
                //删除assetbundle和manifest文件
				if (null != deleteInfo)
				{
                    DeleteAssetBudnleAndManifest(pathRoot, deleteInfo);
                    ++deletedCount;
                }
            }

            //没有文件被删除，则不继续后续的计算操作
            if (deletedCount == 0)
            {
                return;
            }

            var platform = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(pathRoot);
            if (shaco.HotUpdateDefine.Platform.None == platform)
                platform = shaco.HotUpdateHelper.GetAssetBundleAutoPlatform();

            //重新计算main md5并写入文件
            var mainMD5 = ComputerAndWriteMainMD5(pathRoot, versionControl);

            //重新计算总文件大小
            ComputerAllDataSize(pathRoot, versionControl);

            //重新计算引用关系
            var manifestInfo = CollectionManifestDepencies(pathRoot, versionControl, platform);

            //重写配置文件
            ExportVersionControl(pathRoot, mainMD5, manifestInfo, platform, versionControl);
        }

        /// <summary>
        /// 删除assetbundle和manifest文件
        /// <param name="assetbundleName">assetbundle名字</param>
        /// </summary>
        private void DeleteAssetBudnleAndManifest(string pathRoot, shaco.HotUpdateDefine.ExportAssetBundle deleteInfo)
		{
			var fullAssetBundlePath = GetAssetBundleFullPathWithFileNameStatus(pathRoot, deleteInfo, GetFileNameStatus(pathRoot, deleteInfo));

            if (shaco.Base.FileHelper.ExistsFile(fullAssetBundlePath))
            {
                shaco.Base.FileHelper.DeleteByUserPath(fullAssetBundlePath);
                shaco.HotUpdateHelper.DeleteManifest(fullAssetBundlePath);

                //如果为空白文件夹则自动删除
                var folderPath = shaco.Base.FileHelper.GetFolderNameByPath(fullAssetBundlePath);

                if (shaco.Base.FileHelper.DeleteEmptyFolder(folderPath, ".DS_Store"))
                {

                }
            }
		}

        /// <summary>
        /// 删除配置信息中的assetbndle
        /// <param name="versionControl">版本配置信息</param>
        /// <param name="assetbundleName">assetbundle名字</param>
		/// <return>被删除的资源信息</return>
        /// </summary>
        private shaco.HotUpdateDefine.ExportAssetBundle DeleteAssetBundleInConfig(shaco.HotUpdateDefine.SerializeVersionControl versionControl, string assetbundleName)
		{
			shaco.HotUpdateDefine.ExportAssetBundle retValue = null;
			var assetbundleKey = shaco.HotUpdateHelper.AssetBundlePathToKey(assetbundleName);
            int indexFind = versionControl.ListAssetBundles.FindIndex(value => value.AssetBundleName == assetbundleKey);
			if (indexFind >= 0)
			{
                retValue = versionControl.ListAssetBundles[indexFind];
				versionControl.ListAssetBundles.RemoveAt(indexFind);
			}
			return retValue;
		}
	}
}