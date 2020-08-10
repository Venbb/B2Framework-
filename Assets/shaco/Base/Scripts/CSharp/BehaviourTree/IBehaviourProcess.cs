using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public interface IBehaviourProcess
    {
        /// <summary>
        /// 任务进度执行方法
        /// <param name="tree">节点对象，可以通过GetParameter获取节点数据</param>
        /// <return>支持各种yield return方法，用法与Unity协程一致</return>
        /// </summary>
        IEnumerator<shaco.Base.IBehaviourEnumerator> Process(BehaviourTree tree);

        /// <summary>
        /// 任务进度结束，清理资源方法
        /// </summary>
        void Dispose();
    }
}

