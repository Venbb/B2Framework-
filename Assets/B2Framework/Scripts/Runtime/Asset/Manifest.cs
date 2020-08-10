using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace B2Framework
{
    /// <summary>
    /// 封装AssetBundleManifest，并扩展一些接口
    /// </summary>
    public class Manifest
    {
        string[] nullArr = new string[] { };
        public AssetBundleManifest manifest { get; protected set; }
        /// <summary>
        /// 拥有的AssetBundle包个数
        /// </summary>
        /// <value></value>
        public int Length
        {
            get
            {
                return manifest == null ? 0 : manifest.GetAllAssetBundles().Length;
            }
        }
        public Manifest(AssetBundleManifest manifest)
        {
            this.manifest = manifest;
        }
        /// <summary>
        /// 获取指定AssetBundle的Hash值
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public Hash128 GetAssetBundleHash(string assetbundleName)
        {
            return manifest == null ? default(Hash128) : manifest.GetAssetBundleHash(assetbundleName);
        }
        /// <summary>
        /// 获取所有AssetBundle的名字
        /// </summary>
        /// <returns></returns>
        public string[] GetAllAssetBundles()
        {
            return manifest == null ? nullArr : manifest.GetAllAssetBundles();
        }
        /// <summary>
        /// 获取所有变体
        /// </summary>
        /// <returns></returns>
        public string[] GetAllAssetBundlesWithVariant()
        {
            return manifest == null ? nullArr : manifest.GetAllAssetBundlesWithVariant();
        }
        /// <summary>
        /// 获取所有依赖
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public string[] GetAllDependencies(string assetbundleName)
        {
            return manifest == null ? nullArr : manifest.GetAllDependencies(assetbundleName);
        }
        /// <summary>
        /// 获取所有直接依赖
        /// </summary>
        /// <param name="assetbundleName"></param>
        /// <returns></returns>
        public string[] GetDirectDependencies(string assetbundleName)
        {
            return manifest == null ? nullArr : manifest.GetDirectDependencies(assetbundleName);
        }
        /// <summary>
        /// 对比差异，返回需要更新的AssetBundle列表
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public List<string> CompareTo(Manifest other)
        {
            List<string> result = new List<string>();
            if (other.manifest == null) return result;

            if (manifest == null)
            {
                result.AddRange(other.GetAllAssetBundles());
                return result;
            }

            string[] others = other.GetAllAssetBundles();
            string[] mines = GetAllAssetBundles();
            foreach (string name in others)
            {
                int idx = System.Array.FindIndex(mines, element => element.Equals(name));
                if (idx == -1)
                {
                    result.Add(name);
                }
                else if (!GetAssetBundleHash(mines[idx]).Equals(other.GetAssetBundleHash(name)))
                {
                    result.Add(name);
                }
            }
            return result;
        }
    }
}