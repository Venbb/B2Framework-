using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class HotUpdateManifestInfo
    {
		public class ManifestInfo
		{
			public Dictionary<string, string[]> dependenies = new Dictionary<string, string[]>();
		}

        private ManifestInfo _manifestInfo = new ManifestInfo();

		public bool LoadFromPath(string path)
		{
			try
			{
				if (null == _manifestInfo)
                    _manifestInfo = new ManifestInfo();
				else
                    _manifestInfo.dependenies.Clear();
			
				bool isVersion2_1OrUpper = false;

                using (var fileStream = System.IO.File.Open(path, System.IO.FileMode.OpenOrCreate))
				{
                    //检查版本标记头
                    var versionCheckBytes = new byte[HotUpdateDefine.DEPEND_TAG_MAX_LENGTH];
                    fileStream.Read(versionCheckBytes, 0, versionCheckBytes.Length);
                    isVersion2_1OrUpper = HotUpdateManifestInfo.StartsWith(versionCheckBytes, HotUpdateDefine.DEPEND_TAG_2_1);
                    fileStream.Close();
				}

                if (!isVersion2_1OrUpper)
				{
                    LoadFromPathOld(path);
                    return _manifestInfo != null;
				}

                long indexTmp = 0;
				bool hasError = false;

                using (var streamReader = new System.IO.StreamReader(path, System.Text.Encoding.UTF8))
				{
                    //2.1以上版本使用stream读取
                    if (isVersion2_1OrUpper)
					{
						//过滤版本信息头
                        streamReader.ReadLine();
                        while (!hasError)
                        {
                            var assetPath = streamReader.ReadLine();
                            if (string.IsNullOrEmpty(assetPath))
							{
								break;
							}

                            var countString = streamReader.ReadLine();
                            if (string.IsNullOrEmpty(countString))
                            {
                                hasError = true;
                                Log.Error("HotUpdateManifest LoadFromPath error: not found count string, index=" + indexTmp + " path=" + path);
                                break;
                            }

                            var dependCount = countString.ToInt();
                            if (dependCount <= 0)
                            {
                                hasError = true;
                                Log.Error("HotUpdateManifest LoadFromPath error: count <= 0 assetPath=" + assetPath + " path=" + path);
                                break;
                            }

                            var newDependInfo = new string[dependCount];
                            for (int i = 0; i < dependCount; ++i)
                            {
                                var dependString = streamReader.ReadLine();
                                if (string.IsNullOrEmpty(dependString))
                                {
                                    hasError = true;
                                    Log.Error("HotUpdateManifest LoadFromPath error: not found depend string, assetPath=" + assetPath + " subIndex=" + i + " path=" + path);
                                    break;
                                }
                                newDependInfo[i] = dependString;
                            }
                            if (hasError)
                                break;

                            if (_manifestInfo.dependenies.ContainsKey(assetPath))
                                Log.Error("HotUpdateManifest LoadFromPath error: duplicate assetPath=" + assetPath + " path=" + path);
                            else
                                _manifestInfo.dependenies.Add(assetPath, newDependInfo);
                            ++indexTmp;
                        }
                        streamReader.Close();
                        shaco.Log.Info("HotUpdateManifest LoadFromPath: depend count=" + _manifestInfo.dependenies.Count);
                    }

                    if (hasError)
						_manifestInfo = null;
                }
            }
			catch (System.Exception e)
			{
                _manifestInfo = null;
				Debug.LogError("HotUpdateManifestInfo LoadFromString erorr: e=" + e);
			}
			return _manifestInfo != null;
		}

		public string[] GetAllDependencies(string assetbundleName)
		{
            return _manifestInfo.dependenies.ContainsKey(assetbundleName) ? _manifestInfo.dependenies[assetbundleName] : null;
		}

		public string[] GetAllAssetBundles()
		{
            string[] retValue = new string[_manifestInfo.dependenies.Count];
			int index = 0;
			foreach (var iter in _manifestInfo.dependenies.Keys)
			{
                retValue[index++] = iter;
			}
			return retValue;
		}

		public bool ContainsKey(string key)
		{
			return _manifestInfo.dependenies.ContainsKey(key);
		}

		public void MergeFrom(string key, string[] assetbundleDependenies)
		{
			string[] findValue = null;
			if (_manifestInfo.dependenies.TryGetValue(key, out findValue))
			{
				bool isChanged = false;
				var newAssetBundleDependenies = findValue.ToList();
				for (int i = assetbundleDependenies.Length - 1; i >= 0; --i)
				{
                    if (!newAssetBundleDependenies.Contains(assetbundleDependenies[i]))
					{
                        newAssetBundleDependenies.Add(assetbundleDependenies[i]);
                        isChanged = true;
					}
				}

				if (isChanged)
				{
					_manifestInfo.dependenies[key] = newAssetBundleDependenies.ToArray();
				}
			}
			else 
			{
                _manifestInfo.dependenies.Add(key, assetbundleDependenies);
			}
		}

		public void MergeTo(HotUpdateManifestInfo to)
		{
			foreach (var iter in this._manifestInfo.dependenies)
			{
                to.MergeFrom(iter.Key, iter.Value);
			}
		}

        static public void SaveToFile(string filePath, HotUpdateDefine.SerializeVersionControl versionControl, shaco.HotUpdateManifestInfo manifest)
		{
            SaveToFile(filePath, versionControl, manifest._manifestInfo);
		}

        static public void SaveToFile(string filePath, HotUpdateDefine.SerializeVersionControl versionControl, shaco.HotUpdateManifestInfo.ManifestInfo manifestInfo)
        {
            switch (versionControl.baseVersion)
            {
                case HotUpdateDefine.BaseVersionType.VER_1_0:
                case HotUpdateDefine.BaseVersionType.VER_2_0:
                    {
                        shaco.Base.FileHelper.WriteAllByUserPath(filePath, shaco.LitJson.JsonMapper.ToJson(manifestInfo));
                        break;
                    }
                default:
                    {
                        //2.1及以上版本均采用stream方式
                        var fileListStr = new System.Text.StringBuilder();
                        shaco.Base.FileHelper.WriteAllByUserPath(filePath, fileListStr.ToString());

						//写入第一行版本信息
						fileListStr.AppendLine(HotUpdateDefine.DEPEND_TAG_2_1);

						foreach (var iter in manifestInfo.dependenies)
						{
							fileListStr.AppendLine(iter.Key);
                            fileListStr.Append(iter.Value.Length); fileListStr.AppendLine();

							for (int i = 0; i < iter.Value.Length; ++i)
							{
                                fileListStr.AppendLine(iter.Value[i]);
                            }
						}

						if (fileListStr.Length > 0)
                            fileListStr.Remove(fileListStr.Length - 1, 1);
                        shaco.Base.FileHelper.WriteAllByUserPath(filePath, fileListStr.ToString());
                        break;
                    }
            }
        }

        /// <summary>
        /// 删除在版本控制文件中找不到的引用关系
        /// <param name="manifestPath">引用关系文件路径</param>
        /// <param name="versionControl">版本控制文件</param>
        /// </summary>
        public void DeleteUnusedDependenciesInManifest(string manifestPath, HotUpdateDefine.SerializeVersionControl versionControl)
		{
			Dictionary<string, object> mapExistsKeys = new Dictionary<string, object>();
			for (int i = 0; i < versionControl.ListAssetBundles.Count; ++i)
			{
                mapExistsKeys.Add(shaco.HotUpdateHelper.AssetBundleKeyToPath(versionControl.ListAssetBundles[i].AssetBundleName), null);
			}

			var removeKeys = new List<string>();
			foreach (var iter in _manifestInfo.dependenies)
			{
				if (!mapExistsKeys.ContainsKey(iter.Key))
				{
                    removeKeys.Add(iter.Key);
				}
			}

			if (removeKeys.Count > 0)
			{
				for (int i = removeKeys.Count - 1; i >= 0; --i)
				{
					_manifestInfo.dependenies.Remove(removeKeys[i]);
				}
				SaveToFile(manifestPath, versionControl, _manifestInfo);
			}
		}

		//ver1.0和2.0旧版本加载方法
		private void LoadFromPathOld(string path)
		{
			var readString = shaco.Base.FileHelper.ReadAllByUserPath(path);
			if (!string.IsNullOrEmpty(readString))
			{
				_manifestInfo = shaco.LitJson.JsonMapper.ToObject<ManifestInfo>(readString);
				shaco.Log.Info("HotUpdateManifest LoadFromPathOld: depend count=" + _manifestInfo.dependenies.Count);
			}
		}

        static private bool StartsWith(byte[] buf, string find)
        {
            var len1 = buf.Length;
            var len2 = find.Length;

            for (int i = 0; i < len1; ++i)
            {
                if (i >= len2)
                    return true;

                if (buf[i] != find[i])
                    return false;
            }
            return true;
        }
    }
}