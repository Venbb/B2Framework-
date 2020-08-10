using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class Accelerate : ActionBase
    {
        public class ControlPoint
        {
            public float timePercent;
            public float speedRate;

            public ControlPoint(float timePercent = 0, float speedRate = 0)
            {
                this.timePercent = timePercent;
                this.speedRate = speedRate;
            }

            public void checkSafeData()
            {
                if (timePercent < 0)
                    timePercent = 0;
                if (timePercent > 1)
                    timePercent = 1;
                if (speedRate <= 0)
                    speedRate = 1.0f;
            }
        }

        public enum AccelerateMode
        {
            Straight,
            Parabola
        }
        
        protected float Speed;
        protected ActionBase actionTarget;
        protected AccelerateMode actionMode;

        protected ControlPoint controlBegin = new ControlPoint(0, 0);
        protected ControlPoint controlMiddle = new ControlPoint(0, 0);
        protected ControlPoint controlEnd = new ControlPoint(0, 0);

        // static private List<GameObject> _listTestDraw = null;

        static public Accelerate Create(ActionBase action, ControlPoint begin, ControlPoint middle, ControlPoint end, AccelerateMode mode = AccelerateMode.Parabola)
        {
            Accelerate ret = new Accelerate();

            begin.checkSafeData();
            middle.checkSafeData();
            end.checkSafeData();

            ret.controlBegin = begin;
            ret.controlMiddle = middle;
            ret.controlEnd = end;
            ret.actionTarget = action;
            ret.actionMode = mode;

            return ret;
        }

        public override void RunAction(GameObject target)
        {
// #if UNITY_EDITOR
//             _isDebugDrawing = false;
// #endif
            actionTarget.RunActionWithoutPlay(target);

            // if (null != _listTestDraw)
            // {
            //     foreach(var obj in _listTestDraw)
            //     {
            //         GameObject.Destroy(obj);
            //     }
            //     _listTestDraw.Clear();
            //     _listTestDraw = null;
            // }
            base.RunAction(target);
        }

        public void SetActionTarget(ActionBase action)
        {
            actionTarget = action;

            this.elapsed = 0;
            this.duration = actionTarget.duration;
        }

        public override ActionBase Clone()
        {
            return Create(actionTarget, controlBegin, controlMiddle, controlEnd, actionMode);
        }

        override public float UpdateAction(float prePercent)
        {
			if (elapsed >= duration)
			{
				SetActionAlive(false);
				return base.UpdateAction(prePercent);
			}

            float scaleRate = 1.0f;
            float timePercent = actionTarget.elapsed / actionTarget.duration;

            //run action by directly
            if (actionTarget.duration <= 0.0f)
            {
                SetActionAlive(false);
                var completedPercent = base.UpdateAction(1.0f);
                completedPercent = actionTarget.UpdateAction(1.0f);
                return completedPercent;
            }

            if (AccelerateMode.Straight == actionMode)
            {
                if (timePercent >= controlBegin.timePercent && timePercent <= controlMiddle.timePercent)
                {
                    scaleRate = MathS.GetYValueOfLineEquation
                        (controlBegin.timePercent, controlBegin.speedRate,
                        controlMiddle.timePercent, controlMiddle.speedRate, timePercent);
                }
                else if (timePercent >= controlMiddle.timePercent)
                {
                    scaleRate = MathS.GetYValueOfLineEquation
                        (controlMiddle.timePercent, controlMiddle.speedRate,
                        controlEnd.timePercent, controlEnd.speedRate, timePercent);
                }
            }
            else if (AccelerateMode.Parabola == actionMode)
            {
                scaleRate = MathS.GetYValueOfParabolaEquation(
                    controlBegin.timePercent, controlBegin.speedRate,
                    controlMiddle.timePercent, controlMiddle.speedRate,
                    controlEnd.timePercent, controlEnd.speedRate,
                    timePercent);

                //当抛物线加速模式时候，为了避免设置结束控制点过低导致动画的scaleRate几乎无限 == 0的情况
                //则视为动画结束了
                if (Mathf.Abs(scaleRate) <= 0.00001f)
                {
                    this.SetActionAlive(false);
                    return this.duration;
                }
            }

            //check action over
            if (actionTarget.elapsed >= actionTarget.duration)
            {
                SetActionAlive(false);
            }

            float newPercent = actionTarget.GetCurrentPercent() * scaleRate;
            newPercent = base.UpdateAction(newPercent);
            newPercent = actionTarget.UpdateAction(newPercent);

// #if UNITY_EDITOR
//             if (_isDebugDrawing)
//             {
//                 float testScale = 5.0f;
//                 TestDraw(new Vector3(timePercent * testScale, scaleRate * testScale, 0));
//             }
// #endif
            return newPercent;
        }

		public override void PlayEndDirectly ()
		{
			base.PlayEndDirectly ();
			
			if (actionTarget != null)
				actionTarget.PlayEndDirectly();
		}

        public override void Reset(bool isAutoPlay)
        {
            base.Reset(isAutoPlay);

            if (actionTarget != null)
                actionTarget.Reset(isAutoPlay);
        }

        public override ActionBase Reverse(GameObject target)
        {
            Accelerate ret = new Accelerate();
            actionTarget = actionTarget.Reverse(target);
            ret.SetActionTarget(actionTarget);
            return ret;  
        }

// #if UNITY_EDITOR
//         public static ControlPoint testControlBegin = new ControlPoint();
//         public static ControlPoint testControlMiddle = new ControlPoint();
//         public static ControlPoint testControlEnd = new ControlPoint();
//         public static AccelerateMode testAcceMode = AccelerateMode.Parabola;
//         private static string _strAcceMode = "ParabolaMode";

//         private static string _strControlBegin = "0/3";
//         private static string _strControlMiddle = "0.5/2";
//         private static string _strControlEnd = "1/0.2";
//         private static bool _isDebugDrawing = false;
//         static public void TestDrawAccelrateGUI()
//         {
//             _isDebugDrawing = true;
//             float duration = 1.0f;
//             GUILayout.TextArea("ControlBeginPoint");
//             _strControlBegin = GUILayout.TextField(_strControlBegin);
//             GUILayout.TextArea("ControlMiddlePoint");
//             _strControlMiddle = GUILayout.TextField(_strControlMiddle);
//             GUILayout.TextArea("ControlEndPoint");
//             _strControlEnd = GUILayout.TextField(_strControlEnd);

//             if (GUILayout.Button(_strAcceMode))
//             {
//                 if (string.Equals(_strAcceMode, "StraightMode"))
//                 {
//                     _strAcceMode = "ParabolaMode";
//                     testAcceMode = AccelerateMode.Parabola;
//                 }
//                 else
//                 {
//                     _strAcceMode = "StraightMode";
//                     testAcceMode = AccelerateMode.Straight;
//                 }
//             }

//             if (GUILayout.Button("TestDrawAccelerateAction"))
//             {
//                 var scaleby = ScaleBy.Create(Vector3.zero, duration);
//                 TestUpdateParams();
//                 var acceAction = shaco.Accelerate.Create(scaleby, testControlBegin, testControlMiddle, testControlEnd, testAcceMode);

//                 acceAction.RunAction(shaco.GameHelper.action.GetGlobalInvokeTarget().gameObject);
//             }
//         }

//         static public void TestUpdateParams()
//         {
//             var begins = _strControlBegin.Split('/');
//             var middles = _strControlMiddle.Split('/');
//             var ends = _strControlEnd.Split('/');
//             testControlBegin = new ControlPoint(float.Parse(begins[0]), float.Parse(begins[1]));
//             testControlMiddle = new ControlPoint(float.Parse(middles[0]), float.Parse(middles[1]));
//             testControlEnd = new ControlPoint(float.Parse(ends[0]), float.Parse(ends[1]));
//         }
        
//         void TestDraw(Vector3 pos)
//         {
//             var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

//             if (null == _listTestDraw)
//                 _listTestDraw = new List<GameObject>();
//             _listTestDraw.Add(obj);
//             obj.transform.position = pos;
//             float scaleTmp = 0.1f;
//             obj.transform.localScale = new Vector3(scaleTmp, scaleTmp, scaleTmp);
//             obj.transform.SetParent(shaco.GameHelper.action.GetGlobalInvokeTarget().transform);
//         }
// #else 
//         static public void TestDrawAccelrateGUI()
//         {}
// #endif

        private Accelerate() { }
    }
}