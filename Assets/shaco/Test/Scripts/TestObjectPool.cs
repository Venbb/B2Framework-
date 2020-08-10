using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Test
{
    public class TestObjectPool : MonoBehaviour
    {
		public GameObject unityObjectModel;
		private TestPoolData cSharpObject = null;
        private GameObject _instantiateUnitObject = null;

		private class TestPoolData : shaco.Base.IObjectPoolData
		{
			public string str = string.Empty;
			public void Dispose()
			{
				str = string.Empty;
			}
		}
        
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                _instantiateUnitObject = unityObjectModel.InstantiateWithPool(unityObjectModel.name);
                shaco.UnityHelper.ChangeParentLocalPosition(_instantiateUnitObject, this.gameObject);
            }
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                _instantiateUnitObject.RecyclingWithPool();
                _instantiateUnitObject = null;
            }
        }

        void OnGUI()
		{
            GUILayout.BeginHorizontal();
            {
                //对于所有引用池对象建议配套AutoDestroyWithPool自动销毁方法使用
                //该方法需要绑定一个组件对象，当组件销毁后自动回收内存池对象
                if (TestMainMenu.DrawButton("Instantiate(c#)"))
                {
                    cSharpObject = shaco.GameHelper.objectpool.Instantiate(() => new TestPoolData()).AutoDestroyWithPool(this);
                }

                if (TestMainMenu.DrawButton("Instantiate(unity)"))
                {
                    var instantiateObj = shaco.GameHelper.objectpool.Instantiate(() => new GameObject().AddComponent<TestPoolData2>()).AutoRecyclingWithPool(this);
                    Debug.Log("instantiateObj=" + instantiateObj);
                }

                if (TestMainMenu.DrawButton("Instantiate(target)"))
                {
                    _instantiateUnitObject = unityObjectModel.InstantiateWithPool(unityObjectModel.name).AutoDestroyWithPool(this);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("RecyclingObject(c#)"))
                {
                    cSharpObject.RecyclingWithPool();
                }

                if (TestMainMenu.DrawButton("RecyclingAllObjects(c#)"))
                {
                    shaco.GameHelper.objectpool.RecyclingAllObjects(typeof(TestPoolData).ToTypeString());
                }

                if (TestMainMenu.DrawButton("RecyclingObject(target)"))
                {
                    if (null != _instantiateUnitObject)
                    {
                        _instantiateUnitObject.RecyclingWithPool();
                        _instantiateUnitObject = null;
                    }
                }

                if (TestMainMenu.DrawButton("RecyclingAllObjects(unity)"))
                {
                    shaco.GameHelper.objectpool.RecyclingAllObjects(typeof(TestPoolData2).ToTypeString());
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("DestroyObject(c#)"))
                {
                    if (null != cSharpObject)
                    {
                        cSharpObject.DestroyWithPool();
                        cSharpObject = null;
                    }
                }

                if (TestMainMenu.DrawButton("DestroyAllObjects(c#)"))
                {
                    shaco.GameHelper.objectpool.DestroyAllObjects(typeof(TestPoolData).ToTypeString());
                }

                if (TestMainMenu.DrawButton("DestroyObject(target)"))
                {
                    if (null != _instantiateUnitObject)
                    {
                        _instantiateUnitObject.DestroyWithPool();
                        _instantiateUnitObject = null;
                    }
                }

                if (TestMainMenu.DrawButton("DestroyAllObjects(unity)"))
                {
                    shaco.GameHelper.objectpool.DestroyAllObjects(typeof(TestPoolData2).ToTypeString());
                }
            }
            GUILayout.EndHorizontal();

            TestMainMenu.DrawBackToMainMenuButton();
		}
    }
}