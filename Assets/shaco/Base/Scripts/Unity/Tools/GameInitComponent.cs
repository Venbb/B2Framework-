using UnityEngine;
using System.Collections;

namespace shaco
{
    /// <summary>
    /// 游戏的启动组件，必须在整个游戏启动前运行，建议放到初始化场景中
    /// 如果是Unity5.3以上版本，则会自动执行，不需要手动放置
    /// </summary>
    [DisallowMultipleComponent]
    public class GameInitComponent : MonoBehaviour
    {
        static private bool _isInited = false;

        void Awake()
        {
            InitFrameworkEnvironment();
        }

        void Update()
        {
            MainUpdate(Time.deltaTime);
        }

        /// <summary>
        /// 初始化框架运行环境，如果是Unity5.3以下的版本，请在初始场景中手动添加GameInitComponent
        /// </summary>
#if UNITY_5_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static public void InitFrameworkEnvironment()
        {
             if (_isInited)
                return;

            _isInited = true;

            //这里需要临时获取一次来初始化框架组件配置读取
            var configTmp = shaco.GameHelper.gameConfig;

            //自动创建游戏初始化组件到场景中
            //已经场景中已经存在该组件，则将它属性修改为不销毁
            GameEntry.GetComponentInstance<GameInitComponent>();

            //初始化内存池回收对象
            shaco.GameEntry.GetComponentInstance<UnityObjectPoolCompnnet>();
        }

        /// <summary>
        /// 整个游戏计时器刷新方法
        /// </summary>
        static public void MainUpdate(float deltaTime)
        {
            shaco.Base.BehaviourRootTree.BaseUpdate(Time.deltaTime);
            shaco.Base.WaitFor.Update();
        }
    }
}
