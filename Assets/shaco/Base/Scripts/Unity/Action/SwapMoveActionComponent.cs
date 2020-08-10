using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class SwapMoveActionComponent : MonoBehaviour
    {
        //参数1：当前显示的组件
        //参数2：下个显示的组件
        //参数3：true表示从左往右，false表示从右往左移动
        public System.Action<GameObject, GameObject, bool> onWillMoveCallBack = null;
        public System.Action<GameObject, GameObject, bool> onMoveEndCallBack = null;

        [System.Serializable]
        public class ListSelect
        {
            public GameObject target;

            [HideInInspector]
            public int oldDepth = 0;

            public ListSelect prev = null;
            public ListSelect next = null;

            public Vector3 nextPos;
            public Quaternion nextRotation;
        }

        //是否仅允许旋转Y轴
        public bool onlyRotateYAxis = true;
        //当正在执行交换动画时候是否锁定动画输入，避免多次DoAction导致的动画播放跳跃问题
        public bool lockActionWhenMoving = true;
        //是否当执行交换动画时候自动隐藏在背部的对象
        public bool autoHideBackMove = true;
        //交换动画执行一次的时间(单位：秒)
        public float actionDuration = 1.0f;

        //显示在最前面的对象，所有的交换动画会围绕这个对象来执行 
        private ListSelect _centerTarget = new ListSelect();

        private bool _isSuccess = true;
        private int _iMaxDepth = 0;
        private int _iMinDepth = 0;
        private ListSelect _firstSelect = null;

        void Start()
        {
            UpdateSelect();
        }

        void OnDisable()
        {
            DoFirstSelectFunction((ListSelect select) =>
            {
                SetDepthSafe(select.target, select.oldDepth);
            });
        }

        public bool DoAction(bool isRightToLeft)
        {
            if (transform.childCount <= 1)
                return false;

            if (!_isSuccess)
            {
                if (lockActionWhenMoving)
                    return false;
                else
                {
                    DoFirstSelectFunction((ListSelect select) =>
                                     {
                                         shaco.GameHelper.action.StopAllAction(select.target, true);
                                     });
                }
            }

            if (autoHideBackMove && actionDuration > 0)
            {
                var setActiveSelect = isRightToLeft ? _firstSelect : _firstSelect.prev;
                setActiveSelect.target.SetActive(false);
            }

            DoFirstSelectFunction((ListSelect select) =>
            {
                DoSwapMoveAction(select, isRightToLeft);
            });

            //finish move in direct when action duration is 0
            if (actionDuration == 0)
            {
                DoFirstSelectFunction((ListSelect select) =>
                {
                    select.target.transform.position = select.nextPos;
                    select.target.transform.rotation = select.nextRotation;
                });
            }
            else
            {
                _isSuccess = false;
            }

            _centerTarget = isRightToLeft ? _centerTarget.next : _centerTarget.prev;
            UpdateDepth(isRightToLeft);

            if (onWillMoveCallBack != null)
            {
                onWillMoveCallBack(_centerTarget.target, _centerTarget.next.target, isRightToLeft);
            }

            return true;
        }

        public bool IsMoving()
        {
            return !_isSuccess;
        }

        public void UpdateSelect()
        {
            if (this.transform.childCount == 0)
            {
                return;
            }

            ResetSelect();
            _centerTarget.target = transform.GetChild(0).gameObject;
            _centerTarget.oldDepth = GetDepthSafe(_centerTarget.target);

            //link children list
            var listChildren = new List<GameObject>();
            for (int i = 0; i < transform.childCount; ++i)
            {
                GameObject child = transform.GetChild(i).gameObject;
                listChildren.Add(child);
            }

            ListSelect tmpCurrent = CreateSelect(listChildren[0]);
            _firstSelect = tmpCurrent;
            LinkSelect(tmpCurrent, tmpCurrent);

            for (int i = 1; i < listChildren.Count; ++i)
            {
                var child = listChildren[i];

                ListSelect nextSelect = CreateSelect(child);
                LinkSelect(nextSelect, tmpCurrent);

                //update min & max depth
                var depth = GetDepthSafe(child);
                if (depth > _iMaxDepth)
                    _iMaxDepth = depth;
                if (depth < _iMinDepth)
                    _iMinDepth = depth;

                tmpCurrent = nextSelect;
            }

            //set current
            DoFirstSelectFunction((ListSelect select) =>
            {
                if (select.target == _centerTarget.target)
                    _centerTarget = select;
            });
        }

        public void ResetSelect()
        {
            if (_firstSelect != null)
            {
                var nextSelect = _firstSelect;
                while (nextSelect != _firstSelect)
                {
                    var nextTmp = nextSelect.next;
                    nextSelect.target = null;
                    nextSelect.prev = null;
                    nextSelect.next = null;
                    nextSelect = nextTmp;
                }
            }
        }

        public override string ToString()
        {
            var ret = this.name + " : ";

            DoFirstSelectFunction((ListSelect select) =>
            {
                ret += select.target.name;
                ret += "→";
            });
            ret = ret.Remove(ret.Length - 1, 1);
            return ret;
        }

        public bool IsActionCompleted()
        {
            return lockActionWhenMoving ? _isSuccess : true;
        }

        private void DoSelectFunction(System.Action<ListSelect> func, bool isNextForeach = true, ListSelect first = null)
        {
            if (_centerTarget.target == null)
            {
                return;
            }

            if (first == null)
                first = _centerTarget;

            ListSelect tmpCurrent = first;

            if (isNextForeach)
            {
                do
                {
                    func(tmpCurrent);
                    tmpCurrent = tmpCurrent.next;
                } while (tmpCurrent != first);
            }
            else
            {
                do
                {
                    func(tmpCurrent);
                    tmpCurrent = tmpCurrent.prev;
                } while (tmpCurrent != first);
            }
        }

        private ListSelect CreateSelect(GameObject selectObject)
        {
            ListSelect ret = new ListSelect();
            ret.oldDepth = GetDepthSafe(selectObject);
            ret.target = selectObject;
            ret.prev = null;
            ret.next = null;

            return ret;
        }

        private void LinkSelect(ListSelect select, ListSelect prev)
        {
            select.prev = prev;
            select.next = prev.next;

            prev.next = select;

            if (select.next == _firstSelect)
                _firstSelect.prev = select;
        }

        private void DoSwapMoveAction(ListSelect select, bool isRightToLeft)
        {
            Vector3 Pos = new Vector3();

            Quaternion RotationTmp = new Quaternion();
            if (isRightToLeft)
            {
                Pos = select.prev.target.transform.position;
                RotationTmp = select.prev.target.transform.rotation;
            }
            else
            {
                Pos = select.next.target.transform.position;
                RotationTmp = select.next.target.transform.rotation;
            }

            select.nextPos = Pos;
            select.nextRotation = RotationTmp;

            if (actionDuration != 0)
            {
                var moveTo = shaco.MoveTo.Create(Pos, actionDuration);
                var rotateEulderAngle = RotationTmp.eulerAngles;
                if (onlyRotateYAxis)
                {
                    var rotateEulderSrc = select.target.transform.rotation.eulerAngles;
                    rotateEulderAngle.x = rotateEulderSrc.x;
                    rotateEulderAngle.z = rotateEulderSrc.z;
                }

                var acceAction = shaco.Accelerate.Create(moveTo,
                    new shaco.Accelerate.ControlPoint(0, 3.0f),
                    new shaco.Accelerate.ControlPoint(0.5f, 2.0f),
                    new shaco.Accelerate.ControlPoint(1, 0.2f),
                    shaco.Accelerate.AccelerateMode.Parabola);

                acceAction.RunAction(select.target);

                acceAction.onCompleteFunc += (shaco.ActionBase action) =>
                {
                    if (_isSuccess)
                        return;
                        
                    _isSuccess = true;

                    if (onMoveEndCallBack != null)
                    {
                        onMoveEndCallBack(_centerTarget.target, _centerTarget.next.target, isRightToLeft);
                    }

                    if (autoHideBackMove)
                    {
                        ListSelect setActiveSelect = isRightToLeft ? _firstSelect.prev : _firstSelect;
                        setActiveSelect.target.SetActive(true);
                    }
                };
            }
        }

        private void UpdateDepth(bool isNextForeach)
        {
            if (!isNextForeach)
            {
                ListSelect minDepthSelect = _firstSelect.prev;
                int depth = _iMinDepth;
                _firstSelect = minDepthSelect;

                DoFirstSelectFunction((ListSelect select) =>
                {
                    SetDepthSafe(select.target, depth++);
                });
            }
            else
            {
                ListSelect maxDepthSelect = _firstSelect;
                int depth = _iMaxDepth;
                _firstSelect = maxDepthSelect.next;

                DoFirstSelectFunction((ListSelect select) =>
                {
                    SetDepthSafe(select.target, depth--);
                });
            }
        }

        static private int GetDepthSafe(GameObject obj)
        {
            int ret = 0;

#if SUPPORT_NGUI
            var widgetTmp = obj.GetComponent<UIWidget>();
			ret = widgetTmp ? widgetTmp.depth : 0;
#else
            var spriteRender = obj.GetComponent<Renderer>();

            if (spriteRender != null)
            {
                ret = spriteRender.sortingOrder;
            }
            else
            {
                var parent = obj.transform.parent;
                if (parent == null)
                {
                    Log.Error("SwapMoveAction getDepthSafe error: parent is null");
                    return 0;
                }

                for (int i = 0; i < parent.childCount; ++i)
                {
                    var child = parent.GetChild(i).gameObject;
                    if (obj == child)
                    {
                        ret = i;
                        break;
                    }
                }
            }
#endif
            return ret;

        }

//         public ListSelect GetDepthSafe(int depth)
//         {
//             ListSelect ret = null;

//             var tmpCurrent = centerTarget;

//             do
//             {
// #if SUPPORT_NGUI
//                 var widgetTmp = tmpCurrent.selectObject.GetComponent<UIWidget>();
//                 if (widgetTmp.depth == depth)
//                     ret = tmpCurrent;
// #else
//                 var spriteRender = tmpCurrent.selectObject.GetComponent<Renderer>();

//                 if (spriteRender != null && spriteRender.sortingOrder == depth)
//                 {
//                     ret = tmpCurrent;
//                 }
//                 else
//                 {
//                     if (depth == tmpCurrent.selectObject.transform.GetSiblingIndex())
//                     {
//                         ret = tmpCurrent;
//                     }
//                 }
// #endif

//                 tmpCurrent = tmpCurrent.next;
//             } while (tmpCurrent != centerTarget);

//             return ret;
//         }

        static private void SetDepthSafe(GameObject obj, int depth)
        {
#if SUPPORT_NGUI
            var widgetTmp = obj.GetComponent<UIWidget>();
            if (widgetTmp)
                widgetTmp.depth = depth;
#else
            var spriteRender = obj.GetComponent<Renderer>();

            if (spriteRender != null)
            {
                spriteRender.sortingOrder = depth;
            }
            else
            {
                var parent = obj.transform.parent;
                if (parent == null)
                {
                    Log.Error("SwapMoveAction setDepthSafe error: parent is null");
                    return;
                }

                obj.transform.SetSiblingIndex(depth);
            }
#endif
        }

        private void DoFirstSelectFunction(System.Action<ListSelect> func, bool isNextForeach = true)
        {
            DoSelectFunction(func, isNextForeach, _firstSelect);
        }

        // void OnGUI()
        // {
        //     if (OpenDebugMode)
        //     {
        //         if (Input.GetKeyUp(KeyCode.LeftArrow))
        //         {
        //             DoAction(false);
        //         }
        //         if (Input.GetKeyUp(KeyCode.RightArrow))
        //         {
        //             DoAction(true);
        //         }
        //     }
        // }
    }
}