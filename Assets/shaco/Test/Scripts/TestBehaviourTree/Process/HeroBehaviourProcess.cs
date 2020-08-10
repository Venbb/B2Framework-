using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Test
{
    //查找在视野中的敌人，并向它移动
    public class FindEnemyInVision : shaco.Base.IBehaviourProcess
    {
        public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
        {
            shaco.Log.Info("find...");
            var gameData = tree.GetRoot().GetParameter<TestGameData>();
            var hero = tree.GetParameter<TestBehaivourTree>();

            //查找最近的敌人
            Enemy findEnemy = null;
            float minInstance = float.MaxValue;
            for (int i = gameData.enemys.Count - 1; i >= 0; --i)
            {
                var enemyTmp = gameData.enemys[i];
                var distance = Vector3.Distance(enemyTmp.transform.position, hero.transform.position);

                if (!enemyTmp.isTackedTarget && distance < minInstance)
                {
                    minInstance = distance;
                    findEnemy = enemyTmp;
                    break;
                }
            }

            //设置向敌人移动的量
            if (findEnemy != null)
            {
                tree.SetParameter(new TestMove()
                {
                    moveTarget = hero.transform,
                    moveOffset = (findEnemy.transform.position - hero.transform.position).normalized * hero.moveSpeed,
                    enemyTarget = findEnemy
                });
            }
            //没有找到敌人暂时原地不动
            else 
            {
                Debug.Log("standby...");
                yield return false;
            }
        }

        public void Dispose()
        {

        }
    }

    public class MoveProcess : shaco.Base.IBehaviourProcess
    {
        public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
        {
            Debug.Log("move start");
            var testMove = tree.GetParameter<TestMove>();
            var hero = tree.GetParameter<TestBehaivourTree>();

            //持续向目标单位移动，直到碰撞
            yield return shaco.Base.WaitUntil.Create(() => 
            {
                testMove.moveTarget.position += new Vector3(testMove.moveOffset.x, 0, testMove.moveOffset.z);

                var distance = Vector3.Distance(testMove.enemyTarget.transform.position, hero.transform.position);
                if (distance < hero.minDistanceToEnemy)
                {
                    tree.SetParameter(new TestAttackData() { target = testMove.enemyTarget });
                    testMove.enemyTarget.isTackedTarget = true;
                    return true;
                }
                else
                    return false;
            });
        }

        public void Dispose()
        {
            Debug.Log("move end");
        }
    }

    public class AttackProcess : shaco.Base.IBehaviourProcess
    {
        public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
        {
            var attackData = tree.GetParameter<TestAttackData>();
            if (null == attackData)
            {
                yield return false;
            }

            Debug.Log("will attack target=" + attackData.target.name);
            tree.RemoveParameter<TestAttackData>();

            var testMove = tree.GetParameter<TestMove>();
            var action1 = shaco.RotateBy.Create(new Vector3(0, 50, 0), 0.5f);
            var action2 = action1.Reverse(testMove.moveTarget.gameObject);
            var moveAction = shaco.Sequeue.Create(action1, action2);
            moveAction.RunAction(testMove.moveTarget.gameObject);

            //等待条件完毕
            // yield return shaco.Base.WaitUntil.Create(() =>
            // {
            // });

            // 等待5.0秒
            // yield return 5.0f;
            // yield return shaco.Base.WaitforSeconds.Create(5.0f);

            //等待动画执行完毕
            yield return shaco.WaitForActionEnd.Create(moveAction);
        }

        public void Dispose()
        {
        }
    }

    public class AttackDamege : shaco.Base.IBehaviourProcess
    {
        public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
        {
            Debug.Log("AttackDamege...");
            yield return null;
        }

        public void Dispose()
        {

        }
    }

    public class Test1 : shaco.Base.IBehaviourProcess
    {
        public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
        {
            shaco.Log.Info("Test1", Color.blue);
            yield return null;
        }

        public void Dispose()
        {

        }
    }

    public class Test2 : shaco.Base.IBehaviourProcess
    {
        public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
        {
            shaco.Log.Info("Test2", Color.blue);
            yield return null;
        }

        public void Dispose()
        {

        }
    }

    public class Test3 : shaco.Base.IBehaviourProcess
    {
        public IEnumerator<shaco.Base.IBehaviourEnumerator> Process(shaco.Base.BehaviourTree tree)
        {
            shaco.Log.Info("Test3", Color.blue);
            yield return null;
        }

        public void Dispose()
        {

        }
    }
}