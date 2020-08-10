using System.Collections;

namespace shaco.Base
{
    /// <summary>
    /// 事件基础类信息，所有事件都需要继承该类
    /// </summary>
    public class BaseEventArg : System.Object
    {
        virtual public string eventID { get; }

        public BaseEventArg()
        {
            eventID = GetType().FullName;
        }
    }
}
