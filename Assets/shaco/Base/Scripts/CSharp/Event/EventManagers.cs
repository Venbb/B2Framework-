using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
	public partial class EventManager : IEventManager
    {
		private Dictionary<string, EventManager> _eventManagers = new Dictionary<string, EventManager>();

		private string _currentEventManagerID = string.Empty;
        private EventManager _currentEventManager = null;
		private string _nextEventManagerID = string.Empty;
        private bool _isUsingCurrentEventManager = false;

        /// <summary>
        /// 设置当前使用的事件管理器，同时会激活它
        /// </summary>
        /// <param name="managerID">事件管理器识别符</param>
        public void SetCurrentEventManager(string managerID)
		{
			if (!_eventManagers.ContainsKey(managerID))
			{
				_eventManagers.Add(managerID, new EventManager());
			}

			if (_isUsingCurrentEventManager)
			{
				_nextEventManagerID = managerID;
			}
			else 
			{
				_currentEventManagerID = managerID;
                _currentEventManager = _eventManagers[managerID];
                _currentEventManager._enabled = true;
			}
		}

        /// <summary>
        /// 获取当前使用的事件管理器
        /// </summary>
        public EventManager GetCurrentEventManager()
        {
            if (null == _currentEventManager)
            {
                SetCurrentEventManager("default");
				if (null == _currentEventManager)
                {
                    Log.Error("EventManagers GetCurrentEventManager error: Did you forget to call the 'UseCurrentEventManagerEnd' method at the end of the function?");
                }
			}
			_isUsingCurrentEventManager = true;
            return _currentEventManager;
        }

		public void UseCurrentEventManagerEnd()
		{
			if (_isUsingCurrentEventManager && !string.IsNullOrEmpty(_nextEventManagerID))
			{
				SetCurrentEventManager(_nextEventManagerID);
				_nextEventManagerID = string.Empty;
			}
			_isUsingCurrentEventManager = false;
		}

        /// <summary>
        /// 设置当前使用的事件管理器识别符
        /// </summary>
        public string GetCurrentEventManagerID()
		{
			return _currentEventManagerID;
		}

        /// <summary>
        /// 移除当前事件管理器
        /// *注意* 
        /// 该方法会导致当前没有事件管理器运行，所有的事件派发都会失败 
		/// </summary>
        public bool RemoveCurrentEventManager()
		{
			if (_currentEventManager == null)
			{
				Log.Error("EventManager RemoveCurrentEventManager error: current event manager is null");
			}
			
			if (string.IsNullOrEmpty(_currentEventManagerID))
			{
				Log.Error("EventManager RemoveCurrentEventManager error: current event manager id is empty");
			}

			_eventManagers.Remove(_currentEventManagerID);
			_currentEventManagerID = string.Empty;
			_currentEventManager = null;

			return true;
		}

        /// <summary>
        /// 移除没有运行的事件管理器，释放资源
        /// *注意*
        /// 该方法同时会移除对应的事件管理器添加过的事件监听
		/// </summary>
        public void RemoveUnuseEventManagers()
		{
			var listRemoveKey = new List<string>();
			foreach (var key in _eventManagers.Keys)
			{
				if (key != _currentEventManagerID)
				{
					listRemoveKey.Add(key);
				}
			}

			for (int i = 0; i < listRemoveKey.Count; ++i)
			{
				_eventManagers.Remove(listRemoveKey[i]);
			}
		}

        /// <summary>
        /// 清空所有事件管理器
		/// *注意* 
		/// 该方法清理所有事件，建议只在游戏退出或者重启的时候调用
        /// </summary>
        public void ClearEventManager()
		{
			_eventManagers.Clear();
			_currentEventManagerID = string.Empty;
            _currentEventManager = null;
        }
    }
}

