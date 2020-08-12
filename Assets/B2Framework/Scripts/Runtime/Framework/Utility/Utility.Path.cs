namespace B2Framework
{
    public static partial class Utility
    {
        public static partial class Path
        {
            /// <summary>
            /// 获取规范的路径
            /// </summary>
            /// <param name="path"></param>
            /// <param name="forward">是否正斜线</param>
            /// <returns></returns>
            public static string GetRegularPath(string path, bool forward = true)
            {
                if (string.IsNullOrEmpty(path)) return path;
                return forward ? path.Replace("\\", "/") : path.Replace("/", "\\");
            }
            /// <summary>
            /// 验证路径后缀
            /// </summary>
            /// <param name="path"></param>
            /// <param name="extents"></param>
            /// <returns></returns>
            public static bool IsExtension(string path, params string[] extents)
            {
                if (string.IsNullOrEmpty(path)) return false;
                var idx = System.Array.FindIndex(extents, ex => path.EndsWith(ex));
                return idx != -1;
                // return extents.Contains(System.IO.Path.GetExtension(path));
            }
            /// <summary>
            /// 获取路径扩展名
            /// </summary>
            /// <param name="path"></param>
            /// <param name="extents"></param>
            /// <returns></returns>
            public static string GetExtension(string path, params string[] extents)
            {
                if (string.IsNullOrEmpty(path)) return string.Empty;
                if (extents != null && extents.Length > 0)
                {
                    var idx = System.Array.FindIndex(extents, ex => path.EndsWith(ex));
                    return idx != -1 ? extents[idx] : string.Empty;
                }
                else
                    return System.IO.Path.GetExtension(path);
            }
            /// <summary>
            /// 获取全路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetFullPath(string path)
            {
                return System.IO.Path.GetFullPath(path).ToPath();
            }
            /// <summary>
            /// 拼接路径
            /// </summary>
            /// <param name="path1"></param>
            /// <param name="path2"></param>
            /// <returns></returns>
            public static string Combine(string path1, string path2)
            {
                return System.IO.Path.Combine(path1, path2).ToPath();
            }
            /// <summary>
            /// 获取文件夹路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetDirectoryName(string path)
            {
                return System.IO.Path.GetDirectoryName(path).ToPath();
            }
            /// <summary>
            /// 返回去掉后缀的路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetFilePathWithoutExtension(string path)
            {
                return Combine(System.IO.Path.GetDirectoryName(path), System.IO.Path.GetFileNameWithoutExtension(path));
            }
        }
    }
}