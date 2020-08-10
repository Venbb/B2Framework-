using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
	/// <summary>
	/// 新手引导通用管理类，用于负责新手引导的触发、执行、记录等
    /// 引导的当前步骤由IGuideManager内部进行管理，自动实现
	/// </summary>
    public interface IGuideManager : IGameInstance
    {
        /// <summary>
        /// 是否需要重新保存配置
        /// 因为用户手动修改过路径或者资源对象导致引导关系错乱
        /// 所以再重新保存一次更保险点
        /// </summary>
        bool isReSaveFileDirty { get; set; }

        /// <summary>
        /// 引导步骤数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 引导开关，当为false时候不会触发引导
        /// </summary>
        bool isOpen { get; set; }

        /// <summary>
        /// 是否正在执行新手引导
        /// </summary>
        bool isGuiding { get; }

        /// <summary>
        /// 是否已经开始引导了(包含引导触发条件和正在执行的引导)
        /// </summary>
        bool isStarting { get; }

        /// <summary>
        /// 准备显示新手引导步骤前回调方法
        /// </summary>
        EventCallBack<IGuideStep> callbackBeforeShowGuide { get; }

        /// <summary>
        /// 单步新手引导开始回调方法
        /// </summary>
        EventCallBack<IGuideStep> callbackBeforeOnceStepStart { get; }
    
        /// <summary>
        /// 单步新手引导结束回调方法
        /// </summary>
        EventCallBack<IGuideStep> callbackAfterOnceStepEnd { get; }

        /// <summary>
        /// 关闭步骤新手引导步骤后回调方法
        /// </summary>
        EventCallBack<IGuideStep> callbackAfterCloseGuide { get; }

        /// <summary>
        /// 新手引导完全停止回调方法
        /// </summary>
        EventCallBack callbackAllStepStop { get; }

        /// <summary>
        /// 保存引导步骤id
        /// <param name="autoRemove">是否在保存id的时候自动关闭正在进行的引导</param>
        /// <param name="stepsID">引导步骤id</param>
        /// </summary>
        void SaveStepsID(bool autoClose, params string[] stepsID);

        /// <summary>
        /// 清空保存过的引导步骤id
        /// </summary>
        void ClearSavedStepsID();

        // <summary>
        /// 从二进制文件中加载引导配置
        /// <param name="text">配置数据</param>
        /// <param name="isForceLoadAll">是否忽略已经执行过的新手引导，强制加载所有引导步骤</param>
        /// </summary>
        void LoadFromString(string text, bool isForceLoadAll = false);

        /// <summary>
        /// 开始新手引导
        /// </summary>
        void Start();

        /// <summary>
        /// 开始一个新手引导
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        void Start(string firstGuideStepID);

        /// <summary>
        /// 停止一个新手引导
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        void Stop(string firstGuideStepID);

        /// <summary>
        /// 该引导步骤是否已经开始了
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        bool IsGuiding(string firstGuideStepID);

        /// <summary>
        /// 保存引导配置内容到文件中
        /// <param name="path">保存文件绝对路径</param>
        /// </summary>
        void SaveAsFile(string path);

        /// <summary>
        /// 是否有该引导步骤
        /// <param name="firstGuideStepID">新手引导id</param>
        /// <return>true：存在 false：不存在</return>
        /// </summary>
        bool HasGuide(string firstGuideStepID);

        /// <summary>
        /// 执行新手引导步骤
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        void Execute(string firstGuideStepID);

        /// <summary>
        /// 引导步骤完成
        /// <param name="guideStep">引导步骤</param>
        /// <param name="forceGotoStepID">强制继续跳转到的引导步骤</param>
        /// </summary>
        void OnGuideStepCompleted(IGuideStep guideStep, string forceGotoStepID = null);

        /// <summary>
        /// 获取正在执行的引导步骤
        /// <param name="firstGuideStepID">第一步引导id</param>
        /// <return>返回值不为空表示有子引导步骤在执行，反之没有引导步骤在执行</return>
        /// </summary>
        IGuideStep GetExecutingStep(string firstGuideStepID);

        /// <summary>
        /// 添加第一步引导步骤
        /// <param name="guideStep">引导步骤</param>
        /// <return>true：添加成功 false:添加失败，可能是参数错误或者引导已经执行过了</return>
        /// </summary>
        bool AddFirstStep(IGuideStep guideStep);

        /// <summary>
        /// 添加后续引导步骤
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// <param name="behindStep">下一步引导步骤</param>
        /// </summary>
        void AddFollowupStep(string firstGuideStepID, IGuideStep behindStep);

        /// <summary>
        /// 修改步骤开始id
        /// <param name="oldStepID">旧id</param>
        /// <param name="newStepID">新id</param>
        /// <return>true: 修改成功 false: 修改失败，可能有重复id或者空id</return>
        /// </summary>
        bool ChangeStepFirstID(string oldStepID, string newStepID);

        /// <summary>
        /// 移除新手引导步骤
        /// <param name="guideStepID">引导步骤id</param>
        /// </summary>
        void RemoveStep(string guideStepID);

        /// <summary>
        /// 清空新手引导
        /// </summary>
        void ClearStep();

        /// <summary>
        /// 获取新手引导步骤数据
        /// <param name="firstStepID">第一步引导步骤id</param>
        /// <param name="stepIndex">引导小标，如果为0表示获取第一步新手引导</param>
        /// <return>引导数据</return>
        /// </summary>
        IGuideStep GetStep(string firstStepID, int stepIndex);

        /// <summary>
        /// 获取第一步引导开始步骤
        /// <param name="firstStepID">第一步引导步骤id</param>
        /// <return>引导数据</return>
        /// </summary>
        IGuideStep GetFirstStep(string firstStepID);

        /// <summary>
        /// 获取所有引导的第一步数据
        /// </summary>
        IGuideStep[] GetAllFirstStep();

        /// <summary>
        /// 判断是否为一步引导
        /// <param name="guideStep">引导数据</param>
        /// </summary>
        bool IsFirstStep(IGuideStep guideStep);

        /// <summary>
        /// 当前步骤引导开关是否打开
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        bool IsStepOpened(string firstGuideStepID);

        /// <summary>
        /// 设置引导步骤开关，主要是可以方便测试
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// <param name="isOpen">引导开关</param>
        /// </summary>
        void SetStepOpen(string firstGuideStepID, bool isOpen);

        /// <summary>
        /// 设置所有引导步骤开关
        /// <param name="isOpen">引导开关</param>
        /// </summary>
        void SetAllStepOpen(bool isOpen);

        /// <summary>
        /// 遍历所有引导步骤
        /// <param name="callback">遍历回调方法<事件ID，事件回调信息, [返回值，true：继续遍历 false：停止遍历]</param>
        /// </summary>
        void ForeachSteps(System.Func<IList<IGuideStep>, bool> callback);

        /// <summary>
        /// 重新开始所有新手引导
        /// <param name="text">配置数据</param>
        /// </summary>
        void ReloadFromString(string text);

        /// <summary>
        /// 判断引导步骤是否已经触发过了
        /// <param name="firstGuideStepID">初始步骤id</param>
        /// <return>true：触发过了 false：尚未触发过</return>
        /// </summary>
        bool IsExecutedStep(string firstGuideStepID);
    }
}	