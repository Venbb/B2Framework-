using UnityEngine;
using System.Collections;
using System.Reflection;

namespace shaco.Test
{
    public class TestUI : MonoBehaviour
    {
        public shaco.ListViewComponent listView;
        public shaco.PageViewComponent pageView;
        public UnityEngine.UI.Text textOutOfBoundsRate;

        private UnityEngine.Component _openedTestUI = null;

        void Start()
        {
            InitListView();

            //推荐使用SpriteAtlasManager来自动化管理所有图集
            //详情打开图集管理窗口Menu -> shaco -> Tools -> SpriteAtlasSetting
            // UnityEngine.U2D.SpriteAtlasManager.atlasRequested += (name, callback) =>
            // {
            //     Debug.Log("atlas name=" + name);
            //     var atlasNew = shaco.GameHelper.res.LoadResourcesOrLocal<UnityEngine.U2D.SpriteAtlas>("assets/test/resources/atlas.spriteatlas");
            //     callback(atlasNew);

            //     // shaco.GameHelper.res.LoadResourcesOrLocalAsync<UnityEngine.U2D.SpriteAtlas>("assets/test/resources/atlas.spriteatlas", (obj) =>
            //     // {
            //     //     callback(obj);
            //     // });
            // };

            this.gameObject.AddEvent<shaco.UIStateChangedEvents.OnUIOpenEvent>((sender, arg) =>
            {
                _openedTestUI = arg.uiTarget;
                Debug.Log("on ui open sender=" + sender + " key=" + arg.uiKey + " target=" + arg.uiTarget);
            });
        }

        private void RandomTestListView()
        {
            listView.autoUpdateItemMaxIndex += shaco.Base.Utility.Random(5, 20);
            var randIndex = shaco.Base.Utility.Random(listView.autoUpdateItemMinIndex, listView.autoUpdateItemMaxIndex);
            // randIndex = -10;
            Debug.Log("randIndex=" + randIndex);

            if (shaco.Base.Utility.Random(0, 2) == 0)
                listView.LocationByItemIndex(randIndex);
            else
                listView.LocationActionByItemIndex(randIndex, 1.0f);
        }
    
        private void InitListView()
        {
            listView.onItemAutoUpdateCallBack = onItemAutoUpdateCallBack;
            listView.onItemsDragOutOfBoundsCallBack = (rate, direction) =>
            {
                textOutOfBoundsRate.text = rate.ToString() + "_" + direction;
            };
            listView.onItemsDragOutOfBoundsEndCallBack = (rate, direction) =>
            {
                if (rate >= 1)
                {
                    RandomTestListView();
                }
            };

            listView.onItemCanAutoUpdateCallBack = (int index) =>
            {
                return true;
            };
            listView.onItemsCanAutoUpdateCallBack = (int startIndex, int endIndex) =>
            {
                // Debug.Log("prepare update item start=" + startIndex + " end=" + endIndex);
                return true;
            };
            listView.onItemsDidAutoUpdateCallBack = (int startIndex, int endIndex) =>
            {
                // Debug.Log("did update item start=" + startIndex + " end=" + endIndex);
            };
            listView.InitItemWithAutoUpdate(0, 10);
        }

        public void OnClickButton()
        {
            Debug.Log("click target");
        }

        public void onItemAutoUpdateCallBack(int index, GameObject item)
        {
            var scriptTmp = item.GetComponent<TestListViewItem>();
            scriptTmp.num = index;
            item.name = scriptTmp.num.ToString();
            scriptTmp.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = scriptTmp.num.ToString();
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("PageView(add)"))
                {
                    pageView.AddItembyModel();
                }

                if (TestMainMenu.DrawButton("PageView(remove)"))
                {
                    pageView.RemoveItem(0);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("preload_ui"))
                {
                    shaco.GameHelper.ui.PreLoadUI<TestUI_2>();
                }

                if (TestMainMenu.DrawButton("show_ui"))
                {
                    Debug.Log("open ui=" + shaco.GameHelper.ui.OpenUI<TestUI_2>(new TestArg()));
                }

                if (TestMainMenu.DrawButton("show_fullscreen_ui"))
                {
                    Debug.Log("open fullscreen ui=" + shaco.GameHelper.ui.OpenUI<TestUI_3>(new TestArg()));
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("hide_ui"))
                {
                    shaco.GameHelper.ui.HideUI<TestUI_2>();
                }

                if (TestMainMenu.DrawButton("hide_fullscreen_ui"))
                {
                    shaco.GameHelper.ui.HideUI<TestUI_3>();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("close_ui"))
                {
                    shaco.GameHelper.ui.CloseUI<TestUI_2>();
                }

                if (TestMainMenu.DrawButton("close_fullscreen_ui"))
                {
                    shaco.GameHelper.ui.CloseUI<TestUI_3>();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("hide_ui_target"))
                {
                    if (null != _openedTestUI)
                        shaco.GameHelper.ui.HideUITarget(_openedTestUI);
                }

                if (TestMainMenu.DrawButton("close_ui_target"))
                {
                    if (null != _openedTestUI)
                    {
                        shaco.GameHelper.ui.CloseUITarget(_openedTestUI);
                        _openedTestUI = null;
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("popup_hide"))
                {
                    shaco.GameHelper.ui.PopupUIAndHide();
                }

                if (TestMainMenu.DrawButton("popup_close"))
                {
                    shaco.GameHelper.ui.PopupUIAndClose(false);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (TestMainMenu.DrawButton("gettop_ui"))
                {
                    Debug.Log("top_ui=" + shaco.GameHelper.ui.GetTopUI(true));
                }

                if (TestMainMenu.DrawButton("gettop_active_ui"))
                {
                    Debug.Log("top_ui=" + shaco.GameHelper.ui.GetTopUI(false));
                }
            }
            GUILayout.EndHorizontal();

            TestMainMenu.DrawBackToMainMenuButton();
        }
    }
}