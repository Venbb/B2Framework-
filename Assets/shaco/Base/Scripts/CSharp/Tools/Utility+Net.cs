namespace shaco.Base
{
    public static partial class Utility
    {
        /// <summary>
        /// 获取本地ip地址
        /// </summary>
        static public string GetLocalIP()
        {
            var strHostName = System.Net.Dns.GetHostName();
            var ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            var addr = ipEntry.AddressList;
            foreach (var iter in addr)
            {
                if (iter.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return iter.ToString();
            }

            return ipEntry.AddressList[0].ToString();
        }

        /// <summary>
        /// 获取外网ip地址
        /// </summary>
        // static public void GetPublicIP(System.Action<string> callbackEnd)
        // {
        //     string retValue = string.Empty;
        //     try
        //     {
        //         //从网址中获取本机ip数据 
        //         var httpTmp = shaco.Base.GameHelper.http.Instantiate();
        //         httpTmp.Download("http://icanhazip.com/");
        //         shaco.Base.WaitFor.Run(() =>
        //         {
        //             return httpTmp.IsDone();
        //         }, () =>
        //         {
        //             callbackEnd(httpTmp.GetDownloadByte().ToStringArray());
        //         });
        //     }
        //     catch (System.Exception e)
        //     {
        //         Log.Error("Utility+Net GetPublicIP exception: e=" + e);
        //     }
        // }
    }
}