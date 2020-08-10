using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class GuideManager : IGuideManager
    {
        /// <summary>
        /// 是否需要重新保存配置
        /// 因为用户手动修改过路径或者资源对象导致引导关系错乱
        /// 所以再重新保存一次更保险点
        /// </summary>
        public bool isReSaveFileDirty { get; set; }
        
        /// <summary>
        /// 引导步骤数量
        /// </summary>
        public int Count { get { return _guideSteps.Count; } }

        /// <summary>
        /// 引导开关，当为false时候不会触发任何引导
        /// </summary>
        public bool isOpen
        {
            get
            {
                if (!_isOpenCached)
                {
                    _isOpenCached = true;
                    _isOpen = !GameHelper.datasave.ReadBool(ALL_GUIDE_STEP_END_KEY, false);
                }
                return _isOpen;
            }
            set
            {
                _isOpenCached = false;
                GameHelper.datasave.WriteBool(ALL_GUIDE_STEP_END_KEY, !value);
            }
        }
        private bool _isOpen = false;
        private bool _isOpenCached = false;

        /// <summary>
        /// 是否正在执行新手引导
        /// </summary>
        public bool isGuiding { get { return _executingSteps.Count > 0; } }

        /// <summary>
        /// 是否已经开始引导了(包含引导触发条件和正在执行的引导)
        /// </summary>
        public bool isStarting { get { return this.isGuiding; } }

        /// <summary>
        /// 准备显示新手引导步骤前回调方法
        /// </summary>
        public EventCallBack<IGuideStep> callbackBeforeShowGuide { get { return _callbackBeforeShowGuide; } }
		private EventCallBack<IGuideStep> _callbackBeforeShowGuide = new EventCallBack<IGuideStep>();

        /// <summary>
        /// 单步新手引导开始回调方法
        /// </summary>
        public EventCallBack<IGuideStep> callbackBeforeOnceStepStart { get { return _callbackBeforeOnceStepStart; } }
        private EventCallBack<IGuideStep> _callbackBeforeOnceStepStart = new EventCallBack<IGuideStep>();

        /// <summary>
        /// 单步新手引导结束回调方法
        /// </summary>
        public EventCallBack<IGuideStep> callbackAfterOnceStepEnd { get { return _callbackAfterOnceStepOver; } }
        private EventCallBack<IGuideStep> _callbackAfterOnceStepOver = new EventCallBack<IGuideStep>();

        /// <summary>
        /// 关闭步骤新手引导步骤后回调方法
        /// </summary>
        public EventCallBack<IGuideStep> callbackAfterCloseGuide { get { return _callbackAfterCloseGuide; } }
        private EventCallBack<IGuideStep> _callbackAfterCloseGuide = new EventCallBack<IGuideStep>();

        /// <summary>
        /// 新手引导完全停止回调方法
        /// </summary>
        public EventCallBack callbackAllStepStop { get { return _callbackAllStepStop; } }
        private EventCallBack _callbackAllStepStop = new EventCallBack();

        /// <summary>
        /// 所有新手引导步骤信息
        /// </summary>
        private Dictionary<string, List<IGuideStep>> _guideSteps = new Dictionary<string, List<IGuideStep>>();

        /// <summary>
        /// 正在进行中的引导步骤
        /// </summary>
        private Dictionary<string, int> _executingSteps = new Dictionary<string, int>();

        private const string ALL_GUIDE_STEP_END_KEY = "AllGuideStepEndKey";

        /// <summary>
        /// 保存引导步骤id
        /// <param name="autoRemove">是否在保存id的时候自动关闭正在进行的引导</param>
        /// <param name="stepsID">引导步骤id</param>
        /// </summary>
        public void SaveStepsID(bool autoClose, params string[] stepsID)
        {
            //没有开启引导总开关
            if (!this.isOpen)
                return;

            if (stepsID.IsNullOrEmpty())
            {
                // Log.Error("GuideManager SaveStepsID erorr: steps id is empty");
                return;
            }

            for (int i = stepsID.Length - 1; i >= 0; --i)
            {
                var stepID = stepsID[i];
                SaveExecutedStep(stepID);

                if (autoClose)
                    _RemoveStep(stepID);
            }
        }

        /// <summary>
        /// 清空保存过的引导步骤id
        /// </summary>
        public void ClearSavedStepsID()
        {
            var prefixKey = GetStepPrefixSaveKey();
            GameHelper.datasave.RemoveStartWith(prefixKey);
            this.isOpen = true;
        }

        // <summary>
        /// 从二进制文件中加载引导配置
        /// <param name="text">配置数据</param>
        /// <param name="isForceLoadAll">是否忽略已经执行过的新手引导，强制加载所有引导步骤</param>
        /// </summary>
        public void LoadFromString(string text, bool isForceLoadAll = false)
		{
            CheckInit();
          
            //如果引导已经全部结束了，则不再加载任何配置
            if (!this.isOpen && !isForceLoadAll)
            {
                Log.Info("GuideManager LoadFromString: all guide step is end");
                return;
            }

			if (string.IsNullOrEmpty(text))
			{
				Log.Error("GuideManager LoadFromString error: text is empty");
				return;
			}

            var jsonDataRead = shaco.LitJson.JsonMapper.ToObject(text);
            if (!jsonDataRead.IsObject)
            {
                Log.Error("GuideManager LoadFromString error: root data not 'Json Object'");
                return;
            }

            var jsonStepsArray = jsonDataRead["steps"];
            // this.isOpen = jsonDataRead["isOpen"].ToString().ToBool();

            foreach (var iter in jsonStepsArray)
			{
                var jsonSubData = iter as shaco.LitJson.JsonData;
                var firstGuideStepID = jsonSubData["firstGuideStepID"].ToString();
                var guideStepJsonData = jsonSubData["sub_steps"];

                foreach (var iter2 in guideStepJsonData)
                {
                    var guideStep = shaco.Base.GameHelper.settinghelper.FromDataString<shaco.Base.IGuideStep>(iter2.ToString());
                    if (null == guideStep)
                    {
                        guideStep = new shaco.Base.GuideStepPlaceholder();
                        guideStep.firstStepID = firstGuideStepID;
                    }
                    guideStep.isEnd = true;

                    shaco.Base.GameHelper.guideSettingHelper.LoadFrom(guideStep, guideStep.settingValue);

                    if (isForceLoadAll || !IsExecutedStep(guideStep.firstStepID))
                    {
                        if (isForceLoadAll)
                        {
                            guideStep.LoadFrom(guideStep.settingValue);
                        }
                        AddStep(guideStep);
                    }
                }
            }
		}

        /// <summary>
        /// 开始新手引导
        /// </summary>
        public void Start()
        {
            if (!this.isOpen)
                return;

            foreach (var iter in _guideSteps)
            {
                if (iter.Value.Count == 0)
                    continue;
                
                var firstStep = iter.Value[0];
                Execute(firstStep.firstStepID);
            }
        }

        /// <summary>
        /// 开始一个新手引导
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        public void Start(string firstGuideStepID)
        {
            if (!this.isOpen)
                return;

            Execute(firstGuideStepID);
        }

        /// <summary>
        /// 停止一个新手引导
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        public void Stop(string firstGuideStepID)
        {
            UnRegisterAllStep(firstGuideStepID);
        }

        /// <summary>
        /// 该引导步骤是否已经开始了
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        public bool IsGuiding(string firstGuideStepID)
        {
            return _executingSteps.ContainsKey(firstGuideStepID);
            // //正在执行中的步骤
            // if (_executingSteps.ContainsKey(firstGuideStepID))
            // {
            //     return true;
            // }

            // //正在执行中的步骤条件
            // var firstStep = GetFirstStep(firstGuideStepID);
            // if (null == firstStep || null == firstStep.condition)
            //     return false;
            // return _conditionToFirstStepID.ContainsKey(firstStep.condition);
        }

        /// <summary>
        /// 保存引导配置内容到文件中
        /// <param name="path">保存文件绝对路径</param>
        /// </summary>
        public void SaveAsFile(string path)
		{
			var jsonDataSave = new shaco.LitJson.JsonData();
            jsonDataSave.SetJsonType(shaco.LitJson.JsonType.Object);

            var jsonStepsArray = new shaco.LitJson.JsonData();
            jsonDataSave["steps"] = jsonStepsArray;
            // jsonDataSave["isOpen"] = this.isOpen;

			foreach (var iter in _guideSteps)
			{
                if (iter.Value.IsNullOrEmpty())
                {
                    Log.Error("GuideManager SaveAsFile error: invalid guide list, key=" + iter.Key);
                    continue;
                }

                var jsonSubData = new shaco.LitJson.JsonData();
                jsonStepsArray.Add(jsonSubData);
                
                jsonSubData.SetJsonType(shaco.LitJson.JsonType.Object);
                jsonSubData["firstGuideStepID"] = iter.Value[0].firstStepID;

                var jsonDataSteps = new shaco.LitJson.JsonData(); 
                jsonSubData["sub_steps"] = jsonDataSteps;
                for (int i = 0; i < iter.Value.Count; ++i)
                {
                    var guideStep = iter.Value[i];
                    var valueJsonData = new shaco.LitJson.JsonData();

                    shaco.Base.GameHelper.guideSettingHelper.SaveTo(guideStep, valueJsonData);
                    jsonDataSteps.Add(shaco.Base.GameHelper.settinghelper.ToDataString(guideStep, valueJsonData));
                }
			}

            var jsonWrriter = new shaco.LitJson.JsonWriter(new System.Text.StringBuilder());
            jsonWrriter.PrettyPrint = true;
            jsonDataSave.ToJson(jsonWrriter);
            shaco.Base.FileHelper.WriteAllByUserPath(path, jsonWrriter.TextWriter.ToString());
		}

        /// <summary>
        /// 当前是否存在新手引导数据
        /// <param name="firstGuideStepID">新手引导id</param>
        /// <return>true：存在 false：不存在</return>
        /// </summary>
        public bool HasGuide(string firstGuideStepID)
		{
			return _guideSteps.ContainsKey(firstGuideStepID);
		}

        /// <summary>
        /// 执行新手引导步骤
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        public void Execute(string firstGuideStepID)
		{   
            //新手引导关闭，不执行任何引导
            if (!isOpen)
                return;

			//当前没有新手引导
			if (string.IsNullOrEmpty(firstGuideStepID))
            {
                Log.Error("GuideManager Execute erorr: invalid guide, step id=" + firstGuideStepID);
                return;
            }

            var firstGuideStep = GetFirstStep(firstGuideStepID);
            if (null == firstGuideStep)
            {
                Log.Error("GuideManager Execute error: not found step, step id=" + firstGuideStepID);
                return;
            }

            //该引导是关闭状态，不执行
            if (!IsStepOpened(firstGuideStepID))
            {
                Log.Info("GuideManager Execute info: is custom closed step, id=" + firstGuideStepID);

                if (_executingSteps.ContainsKey(firstGuideStepID))
                    _executingSteps.Remove(firstGuideStepID);
                return;
            }

            //该引导已经在执行了，不允许重复执行
            if (null != GetExecutingStep(firstGuideStepID))
            {
                Log.Error("GuideManager Execute error: step is executing, can't execute again, step id=" + firstGuideStepID);
                return;
            }

            _executingSteps.Add(firstGuideStep.firstStepID, 0);
            ExecuteLoop(firstGuideStep);
		}

        /// <summary>
        /// 引导步骤完成
        /// <param name="guideStep">引导步骤</param>
        /// <param name="forceGotoStepID">强制继续跳转到的引导步骤</param>
        /// </summary>
        public void OnGuideStepCompleted(IGuideStep guideStep, string forceGotoStepID = null)
        {
            _callbackAfterOnceStepOver.InvokeAllCallBack(guideStep);

            //注销引导步骤
            UnRegisterStep(guideStep);

            //跳转到下一步引导步骤
            var currentGuideStepIndex = 0;
            if (!_executingSteps.TryGetValue(guideStep.firstStepID, out currentGuideStepIndex))
            {
                //发生错误则强制结束引导
                guideStep.End();
                Log.Error("GuideManager OnGuideStepCompleted error: not found current guide step, first id=" + guideStep.firstStepID);
                return;
            }
            
            var nextStepIndex = currentGuideStepIndex + 1;
            IGuideStep nextStepTmp = null;

            if (string.IsNullOrEmpty(forceGotoStepID))
            {
                nextStepTmp = GetStep(guideStep.firstStepID, nextStepIndex);
            }
            else
            {
                var findSteps = GetSteps(guideStep.firstStepID);
                if (!findSteps.IsNullOrEmpty())
                {
                    nextStepIndex = findSteps.IndexOf(v => v.guideStepID == forceGotoStepID);
                    if (nextStepIndex < 0)
                        Log.Error("GuideManager OnGuideStepCompleted error: not found goto step id=" + forceGotoStepID + " first step id=" + guideStep.firstStepID);
                    else
                        nextStepTmp = findSteps[nextStepIndex];
                }
                else
                    Log.Error("GuideManager OnGuideStepCompleted error: not found first step id=" + guideStep.firstStepID);
            }

            if (null != nextStepTmp)
                _executingSteps[guideStep.firstStepID] = nextStepIndex;

            //清理配置数据
            guideStep.settingValue = null;

            if (null != nextStepTmp)
            {
                //继续下一步新手引导
                this.ExecuteLoop(nextStepTmp);
            }
            else
            {
                //删除正在执行的引导记录
                _executingSteps.Remove(guideStep.firstStepID);
                

                //记录已经执行过的步骤id
                SaveExecutedStep(guideStep.firstStepID);
                _RemoveStep(guideStep.firstStepID);

                //引导步骤结束
                _callbackAfterCloseGuide.InvokeAllCallBack(guideStep);

                //检查所有引导是否结束
                if (!isGuiding)
                {
                    _callbackAllStepStop.InvokeAllCallBack();
                }
            }
        }

        /// <summary>
        /// 获取正在执行的引导步骤
        /// <param name="firstGuideStepID">第一步引导id</param>
        /// <return>返回值不为空表示有子引导步骤在执行，反之没有引导步骤在执行</return>
        /// </summary>
        public IGuideStep GetExecutingStep(string firstGuideStepID)
        {
            IGuideStep retValue = null;
            var firstGuideStep = GetFirstStep(firstGuideStepID);
            int findStepIndex = -1;
            if (_executingSteps.TryGetValue(firstGuideStep.firstStepID, out findStepIndex))
            {
                retValue = GetStep(firstGuideStepID, findStepIndex);
            }

            return retValue;
        }

        /// <summary>
        /// 添加第一步引导步骤
        /// <param name="guideStep">引导步骤</param>
        /// <return>true：添加成功 false:添加失败，可能是参数错误或者引导已经执行过了</return>
        /// </summary>
        public bool AddFirstStep(IGuideStep guideStep)
        {
            if (null == guideStep || string.IsNullOrEmpty(guideStep.guideStepID))
            {
                Log.Error("GuideManager AddFirstStep error: invalid guide step");
                return false;
            }

            if (HasGuide(guideStep.guideStepID))
            {
                Log.Error("GuideManager AddFirstStep error: duplicate first guide, id=" + guideStep.guideStepID);
                return false;
            }

            UnRegisterAllStep(guideStep.firstStepID);

            guideStep.firstStepID = guideStep.guideStepID;
            AddStep(guideStep);
            return true;
        }

        /// <summary>
        /// 添加后续引导步骤
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// <param name="behindStep">下一步引导步骤</param>
        /// </summary>
        public void AddFollowupStep(string firstGuideStepID, IGuideStep behindStep)
        {
            if (string.IsNullOrEmpty(firstGuideStepID))
            {
                Log.Error("GuideManager AddFollowupStep error: invalid first guide step");
                return;
            }

            if (null == behindStep)
            {
                Log.Error("GuideManager AddFollowupStep error: invalid behind guide step");
                return;
            }

            if (!HasGuide(firstGuideStepID))
            {
                Log.Error("GuideManager AddFollowupStep error: not found step by first id=" + firstGuideStepID);
                return;
            }

            behindStep.firstStepID = firstGuideStepID;
            AddStep(behindStep);
        }

        /// <summary>
        /// 修改步骤开始id
        /// <param name="oldStepID">旧id</param>
        /// <param name="newStepID">新id</param>
        /// <return>true: 修改成功 false: 修改失败，可能有重复id或者空id</return>
        /// </summary>
        public bool ChangeStepFirstID(string oldStepID, string newStepID)
        {
            if (oldStepID == newStepID)
            {
                Log.Warning("GuideManager ChangeStepFirstID warning: is same, step id=" + oldStepID);
            }

            var firstStepOld = GetFirstStep(oldStepID);
            if (null == firstStepOld)
            {
                Log.Error("GuideManager ChangeStepFirstID error: not found old step id=" + oldStepID);
                return false;
            }

            if (_guideSteps.ContainsKey(newStepID))
            {
                Log.Error("GuideManager ChangeStepFirstID error: duplicate new step id=" + newStepID);
                return false;
            }

            firstStepOld.firstStepID = oldStepID;
            var allSteps = GetSteps(oldStepID);
            firstStepOld.guideStepID = newStepID;
            for (int i = allSteps.Count - 1; i >= 0; --i)
            {
                allSteps[i].firstStepID = newStepID;
            }

            _guideSteps.Remove(oldStepID);
            _guideSteps.Add(firstStepOld.firstStepID, allSteps);
            return true;
        }

        /// <summary>
        /// 移除新手引导步骤
        /// <param name="guideStepID">引导步骤id</param>
        /// </summary>
        public void RemoveStep(string firstGuideStepID)
        {
			if (!HasGuide(firstGuideStepID))
            {
                Log.Error("GuideManager RemoveStep error: not found guide id=" + firstGuideStepID);
                return;
            }

            _RemoveStep(firstGuideStepID);
        }

        /// <summary>
        /// 清空新手引导
        /// </summary>
        public void ClearStep()
        {
            foreach (var iter in _executingSteps)
            {
                UnRegisterAllStep(iter.Key);
            }

            _guideSteps.Clear();
            _executingSteps.Clear();
        }

        /// <summary>
        /// 获取新手引导步骤数据
        /// <param name="firstStepID">第一步引导步骤id</param>
        /// <param name="stepIndex">引导小标，如果为0表示获取第一步新手引导</param>
        /// <return>引导数据</return>
        /// </summary>
        public IGuideStep GetStep(string firstStepID, int stepIndex)
        {
            IGuideStep retValue = null;
            if (!_guideSteps.IsOutOfRange(firstStepID))
            {
                var listSteps = _guideSteps[firstStepID];
                if (!listSteps.IsOutOfRange(stepIndex))
                {
                    retValue = listSteps[stepIndex];
                }
            }
            else
                _guideSteps.DebugLogOutOfRange(firstStepID, "GuideManager GetStep 1 error");
            return retValue;
        }

        /// <summary>
        /// 获取第一步引导开始步骤
        /// <param name="firstStepID">第一步引导步骤id</param>
        /// <return>引导数据</return>
        /// </summary>
        public IGuideStep GetFirstStep(string firstStepID)
        {
            return GetStep(firstStepID, 0);
        }

        /// <summary>
        /// 获取所有引导的第一步数据
        /// </summary>
        public IGuideStep[] GetAllFirstStep()
        {
            var retValue = new IGuideStep[_guideSteps.Count];
            int index = 0;
            foreach (var iter in _guideSteps)
            {
                retValue[index++] = iter.Value.Count > 0 ? iter.Value[0] : null;
            }
            return retValue;
        }

        /// <summary>
        /// 判断是否为一步引导
        /// <param name="guideStep">引导数据</param>
        /// </summary>
        public bool IsFirstStep(IGuideStep guideStep)
        {
            return null == guideStep ? false : guideStep.guideStepID == guideStep.firstStepID; 
        }

        /// <summary>
        /// 当前步骤引导开关是否打开
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// </summary>
        public bool IsStepOpened(string firstGuideStepID)
        {
            IGuideStep stepFind = GetFirstStep(firstGuideStepID);
            return isOpen && (null != stepFind && stepFind.isOpen);
        }

        /// <summary>
        /// 设置引导步骤开关，主要是可以方便测试
        /// <param name="firstGuideStepID">第一步新手引导步骤ID</param>
        /// <param name="isOpen">引导开关</param>
        /// </summary>
        public void SetStepOpen(string firstGuideStepID, bool isOpen)
        {
            IGuideStep stepFind = GetFirstStep(firstGuideStepID);
            if (null != stepFind)
            {
                stepFind.isOpen = isOpen;
            }
        }

        /// <summary>
        /// 设置所有引导步骤开关
        /// <param name="isOpen">引导开关</param>
        /// </summary>
        public void SetAllStepOpen(bool isOpen)
        {
            var allFirstStep = GetAllFirstStep();
            if (allFirstStep.IsNullOrEmpty())
                return;
                
            for (int i = allFirstStep.Length - 1; i >= 0; --i)
            {
                var stepTmp = allFirstStep[i];
                if (null != stepTmp)
                {
                    SetStepOpen(stepTmp.firstStepID, isOpen);
                }
            }
        }

        /// <summary>
        /// 遍历所有引导步骤
        /// <param name="callback">遍历回调方法<事件ID，事件回调信息, [返回值，true：继续遍历 false：停止遍历]</param>
        /// </summary>
        public void ForeachSteps(System.Func<IList<IGuideStep>, bool> callback)
        {
            foreach (var iter in _guideSteps)
            {
                if (!callback(iter.Value))
                    break;
            }
        }

        /// <summary>
        /// 重新开始所有新手引导
        /// <param name="text">配置数据</param>
        /// </summary>
        public void ReloadFromString(string text)
        {
            ClearSavedStepsID();

            //强制读取所有引导数据(忽略是否已经进行过了)
            this.LoadFromString(text, true);
        }

        /// <summary>
        /// 判断引导步骤是否已经触发过了
        /// <param name="firstGuideStepID">初始步骤id</param>
        /// <return>true：触发过了 false：尚未触发过</return>
        /// </summary>
        public bool IsExecutedStep(string firstGuideStepID)
        {
            var keyTmp = GetStepSaveKey(firstGuideStepID);
            return shaco.Base.GameHelper.datasave.ContainsKey(keyTmp);
        }

        /// <summary>
        /// 移除新手引导步骤
        /// <param name="guideStepID">引导步骤id</param>
        /// </summary>
        private void _RemoveStep(string firstGuideStepID)
        {
            if (!_guideSteps.ContainsKey(firstGuideStepID))
                return;

            UnRegisterAllStep(firstGuideStepID);
            _guideSteps.Remove(firstGuideStepID);

            //移除可能正在执行的引导步骤
            if (_executingSteps.ContainsKey(firstGuideStepID))
                _executingSteps.Remove(firstGuideStepID);

            //引导步骤被删除完毕，记录引导全部完成的标记
            if (_guideSteps.Count == 0)
            {
                this.isOpen = false;
            }
        }

        /// <summary>
        /// 注销引导
        /// <param name="step">引导步骤</param>
        /// </summary>
        private void UnRegisterStep(IGuideStep step)
        {
            if (null != step)
            {
                if (!step.isEnd)
                {
                    step.isEnd = true;
                    try
                    {
                        step.End();
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("GuideManager UnRegisterStep error: first step id=" + step.firstStepID + " step id=" + step.guideStepID + " " + e);
                    }
                }
            }
        }

        /// <summary>
        /// 注销引导
        /// <param name="firstStep">初始引导步骤</param>
        /// </summary>
        private void UnRegisterAllStep(string firstGuideStepID)
        {
            var guideSteps = GetSteps(firstGuideStepID);
            if (guideSteps.IsNullOrEmpty())
            {
                return;
            }

            for (int i = guideSteps.Count - 1; i >= 0; --i)
            {
                var stepTmp = guideSteps[i];
                UnRegisterStep(stepTmp);
            }
        }

        /// <summary>
        /// 获取一段新手引导
        /// <param name="firstStepID">第一步引导id</param>
        /// <return>一段引导</return>
        /// </summary>
        private List<IGuideStep> GetSteps(string firstStepID)
        {
            return _guideSteps.ContainsKey(firstStepID) ? _guideSteps[firstStepID] : null;
        }

        /// <summary>
        /// 执行新手引导步骤
        /// <param name="guideStep">新手引导步骤</param>
        /// </summary>
        private void ExecuteLoop(IGuideStep guideStep)
        {
            //当前没有新手引导
            if (null == guideStep)
                return;

            _callbackBeforeOnceStepStart.InvokeAllCallBack(guideStep);

            //开始执行引导
            _callbackBeforeShowGuide.InvokeAllCallBack(guideStep);
            guideStep.LoadFrom(guideStep.settingValue);

            try
            {
                guideStep.isEnd = false;
                guideStep.Execute();
            }
            catch (System.Exception e)
            {
                Log.Error("GuideManager ExecuteLoop error: first step id=" + guideStep.firstStepID + " step id=" + guideStep.guideStepID + " " + e);
            }
        }

        /// <summary>
        /// 添加引导步骤
        /// </summary>
        private void AddStep(IGuideStep step)
        {
            if (null == step || string.IsNullOrEmpty(step.firstStepID))
            {
                Log.Error("GuideManager AddStep error: invalid step");
                return;
            }

            List<IGuideStep> listTmp = null;
            if (!_guideSteps.TryGetValue(step.firstStepID, out listTmp))
            {
                listTmp = new List<IGuideStep>();
                _guideSteps.Add(step.firstStepID, listTmp);
            }
            listTmp.Add(step);
        }

        /// <summary>
        /// 检查新手引导管理类的初始化
        /// </summary>
        private void CheckInit()
        {
            ClearStep();
        }

        /// <summary>
        /// 获取新手引导转换后的键值
        /// <param name="firstGuideStepID">初始步骤id</param>
        /// <return>转换后的键值</return>
        /// </summary>
        private string GetStepSaveKey(string firstGuideStepID)
		{
			return GetStepPrefixSaveKey() + "step_" + firstGuideStepID;
		}

        /// <summary>
        /// 获取保存引导步骤前缀键值
        /// <return>前缀键值</return>
        /// </summary>
        private string GetStepPrefixSaveKey()
        {
            return typeof(shaco.Base.IGuideManager).ToTypeString();
        }

        /// <summary>
        /// 记录执行过的新手引导id
        /// <param name="firstGuideStepID">初始步骤id</param>
        /// </summary>
        private void SaveExecutedStep(string firstGuideStepID)
		{
			var keyTmp = GetStepSaveKey(firstGuideStepID);
			shaco.Base.GameHelper.datasave.WriteString(keyTmp, firstGuideStepID);
        }
    }
}