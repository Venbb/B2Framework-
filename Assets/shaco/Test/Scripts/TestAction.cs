using UnityEngine;
using System.Collections;

namespace shaco.Test
{
    public class TestAction : MonoBehaviour
    {

        public shaco.SwapMoveActionComponent actionSwapMove;
        public shaco.CurveMoveComponent actionCurveMove;

        public GameObject actionTarget;
        public GameObject curveMoveEndTarget;
        public shaco.NumberLoopScrollActionComponent loopScrollAction;

        void Start()
        {
            actionSwapMove.onMoveEndCallBack += (target, next, isRightToLeft) =>
            {
                Debug.Log("move end target=" + target + " next=" + next + " isRightToLeft=" + isRightToLeft);
            };

            actionSwapMove.onWillMoveCallBack += (target, next, isRightToLeft) =>
            {
                Debug.Log("will move target=" + target + " next=" + next + " isRightToLeft=" + isRightToLeft);
            };
        }

        void OnGUI()
        {
            if (TestMainMenu.DrawButton("CurveMove"))
            {
                if (null != curveMoveEndTarget)
                    actionCurveMove.PlayMoveAction(actionTarget.transform, curveMoveEndTarget.transform.position, 1.0f);
                else
                {
                    actionCurveMove.moveTarget = actionTarget.transform;
                    actionCurveMove.moveDuration = -2;
                    actionCurveMove.PlayMoveAction();
                }
            }
            if (TestMainMenu.DrawButton("LoopScroll"))
            {
                loopScrollAction.text = shaco.Base.Utility.Random(10, 100000).ToString();
            }

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("LeftToRight"))
                {
                    actionSwapMove.DoAction(false);
                }
                if (TestMainMenu.DrawButton("RightToLeft"))
                {
                    actionSwapMove.DoAction(true);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("ScaleTo_Big"))
                {
                    actionTarget.ScaleTo(new Vector3(1.2f, 1.2f, 1.2f), 0.5f);
                }
                if (TestMainMenu.DrawButton("ScaleTo_Small"))
                {
                    actionTarget.ScaleTo(new Vector3(0.8f, 0.8f, 0.8f), 0.5f);
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal(); 
            {
                if (TestMainMenu.DrawButton("RepeatMove"))
                {
                    actionTarget.StopAllAction(true);
                    var move1 = shaco.MoveBy.Create(new Vector3(1, 0, 0), 0.5f);
                    var move2 = move1.Reverse(actionTarget);
                    var seq = shaco.Sequeue.Create(move1, move2);
                    seq.RunAction(actionTarget);
                }
                if (TestMainMenu.DrawButton("RepeatRotate"))
                {
                    var rotate1 = shaco.RotateBy.Create(new Vector3(0, 0, 50), 1.0f);
                    var rotate2 = rotate1.Reverse(actionTarget);
                    var seq = shaco.Sequeue.Create(rotate1, rotate2);
                    seq.RunAction(actionTarget);
                }
                if (TestMainMenu.DrawButton("RepeatScale"))
                {
                    actionTarget.StopAllAction(true);

                    var scale1 = shaco.ScaleTo.Create(new Vector3(1.2f, 1.2f, 1.2f), 0.5f);
                    var scale2 = shaco.ScaleTo.Create(new Vector3(1.0f, 1.0f, 1.0f), 0.5f);
                    var seq = shaco.Sequeue.Create(scale1, scale2);
                    var actionRepeat = shaco.Repeat.Create(seq, 4);
                    actionRepeat.RunAction(actionTarget);
                }
                if (TestMainMenu.DrawButton("RepeatShake"))
                {
                    actionTarget.gameObject.StopActionByType<shaco.ShakeRepeat>(true);
                    var action = actionTarget.ShakeRepeat(new Vector3(2.0f, 0, 0), 3, 1.0f);
                    action.onCompleteFunc += (ac) => 
                    {
                        Debug.Log("shake end...");
                    };
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("TransparentBy"))
                {
                    actionTarget.gameObject.StopActionByType<shaco.TransparentBy>(true);
                    actionTarget.TransparentBy(-1, 1);
                }
                if (TestMainMenu.DrawButton("TransparentTo"))
                {
                    actionTarget.gameObject.StopActionByType<shaco.TransparentBy>(true);
                    actionTarget.TransparentBy(1, 1);
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("CombineAction"))
                {
                    actionTarget.ScaleTo(new Vector3(2.0f, 2.0f, 2.0f), 0.5f)
                                .ScaleTo(new Vector3(1.0f, 1.0f, 1.0f), 0.5f);
                }
                if (TestMainMenu.DrawButton("CombineActionRepeat"))
                {
                    actionTarget.ScaleTo(new Vector3(2.0f, 2.0f, 2.0f), 0.5f)
                                .ScaleTo(new Vector3(1.0f, 1.0f, 1.0f), 0.5f)
                                .RepeatForever();
                }
            }
            GUILayout.EndHorizontal();

            if (TestMainMenu.DrawButton("PinpongAction"))
            {
                actionTarget.MoveBy(new Vector3(1, 0, 0), 0.5f)
                            .Delay(0.5f)
                            .MoveBy(new Vector3(1, 0, 0), 0.5f)
                            .Pingpong()
                            .Repeat(3);
            }
            
            if (TestMainMenu.DrawButton("Accelerate"))
            {
                actionTarget.MoveBy(new Vector3(3, 0, 0), 1f)
                            .MoveBy(new Vector3(-3, 0, 0), 1f)
                            .MoveBy(new Vector3(3, 0, 0), 1f)
                            //                        0.1倍速缓慢启动                    运动到50%时间后到达10倍速                    最终回落到1倍速
                            .Accelerate(new Accelerate.ControlPoint(0, 0.1f), new Accelerate.ControlPoint(0.5f, 10f), new Accelerate.ControlPoint(1, 1f));
            }

            if (TestMainMenu.DrawButton("StopActions"))
            {
                actionTarget.StopAllAction(true);
            }

            TestMainMenu.DrawBackToMainMenuButton();
        }
    }
}