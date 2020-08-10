using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;

namespace shacoEditor
{
   public interface ISpineBinaryReader : shaco.Base.IGameInstance
   {
        /// <summary>
        /// 从spine二进制文件中获取动画名字
        /// 如果需要通过Spine检查窗口来检查spine二进制文件中的animations匹配有效度，则必须重载该方法
        /// <param name="binary">spine二进制文件</param>
        /// <return>spine动画名字列表</return>
        /// </summary>
        string[] ReadSpineAimationsFromBinary(byte[] binary);
   }
}