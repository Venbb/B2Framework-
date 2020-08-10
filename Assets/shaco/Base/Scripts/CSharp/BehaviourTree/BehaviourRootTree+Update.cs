using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public partial class BehaviourRootTree : BehaviourTree
    {
        static private Dictionary<int, BehaviourRootTree> _runningTrees = new Dictionary<int, BehaviourRootTree>();

        static private bool _isForeachingUpdate = false;
        static private Dictionary<int, BehaviourRootTree> _willAddTrees = new Dictionary<int, BehaviourRootTree>();
        static private List<int> _willRemoveTrees = new List<int>();

        /// <summary>
        /// 如果使用该方法用于启动树的协程方法，则必须在工程的刷新方法用调用BehaviourRootTree.BaseUpdate
        /// 否则Start不会生效的
        /// </summary>
        public void Start()
        {
            var key = this.GetHashCode();
            if (_willAddTrees.ContainsKey(key))
            {
                Log.Error("BehaviourRootTree+Update StartCoroutine error: already started, tree=" + this + " key=" + key);
                return;
            }

            if (_isForeachingUpdate)
            {
                _willAddTrees.Add(key, this);
            }
            else
            {
                _runningTrees.Add(key, this);
            }
            this.StartRunning();
        }
        
        static public void BaseUpdate(float delayTime)
        {
            if (_willAddTrees.IsNullOrEmpty() && _runningTrees.IsNullOrEmpty())
                return;

            if (_willAddTrees.Count > 0)
            {
                _runningTrees.AddRange(_willAddTrees);
                _willAddTrees.Clear();
            }

            _isForeachingUpdate = true;
            foreach (var iter in _runningTrees)
            {
                try
                {
                    iter.Value.Update(delayTime);
                }
                catch (System.Exception e)
                {
                    Log.Error("e=" + e);
                    iter.Value.Stop();
                }
            }
            _isForeachingUpdate = false;

            if (_willRemoveTrees.Count > 0)
            {
                _runningTrees.RemoveRange(_willRemoveTrees);
                _willRemoveTrees.Clear();
            }
        }

        static public void StopAll()
        {
            foreach (var iter in _runningTrees)
            {
                iter.Value.Stop();
            }
            _runningTrees.Clear();
        }

        private void StopUpdateSelf()
        {
            var key = this.GetHashCode();

            if (!_runningTrees.ContainsKey(key))
            {
                Log.Error("BehaviourRootTree+Update StopUpdateSelf error: not found type=" + this.ToTypeString());
                return;
            }

            if (_isForeachingUpdate)
            {
                _willRemoveTrees.Add(key);
            }
            else
            {
                _runningTrees.Remove(key);
            }
        }
    }
}

