using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Test
{
    public class TestGuideStepShowImage : shaco.Base.GuideStepDefault
    {
        /// <summary>
        /// 引导图片(Resource目录或者shaco框架支持的更新目录文件对象)
        /// </summary>
        public shaco.GuideResourceObject<Sprite> sprite;

        /// <summary>
        /// 图片显示父节点(Unity Hierachy窗口运行时刻对象，如果当前场景无该对象则显示为字符串)
        /// </summary>
        public shaco.GuideRuntimeObject<GameObject> parentGameObject;

        /// <summary>
        /// 引导中文说明1
        /// </summary>
        public string chineseDecription1 { get{ return "我是一条只读的中文说明\n打开，旋转跳跃，我闭着眼~"; }}

        /// <summary>
        /// 引导中文说明2
        /// </summary>
        public string chineseDecription2 = "我是一条可以修改的中文说明";

        [SerializeField]
        [Header("我是一个序列化成员变量")]
        private string _serializeValue = string.Empty;

        /// <summary>
        /// 新建的图片子节点对象
        /// </summary>
        private GameObject _showGameObject;

        /// <summary>
        /// 新手引导执行逻辑
        /// </summary>
        override public void Execute() 
        {
            if (null == parentGameObject)
            {
                Debug.LogError("GuideStepShowImage Execute error: not found parent name=" + parentGameObject);
                shaco.GameHelper.newguide.OnGuideStepCompleted(this);
                return;
            }

            //创建图片并显示到父节点下
            _showGameObject = new GameObject("guide_show_image_target");
            shaco.UnityHelper.ChangeParentLocalPosition(_showGameObject, parentGameObject);
            var image = _showGameObject.AddComponent<UnityEngine.UI.Image>();
            sprite.GetValue(v => image.sprite = v);
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(128, 128);

            //显示并旋转图片360度
            var rotate = shaco.RotateBy.Create(new Vector3(0, 0, 360), 1.0f);
            rotate.RunAction(_showGameObject);

            rotate.onCompleteFunc = (action) =>
            {
                //完成新手引导步骤
                shaco.GameHelper.newguide.OnGuideStepCompleted(this);
            };
        }

        /// <summary>
        /// 新手引导执行完毕逻辑
        /// </summary>
        override public void End()
        {
            if (null != _showGameObject)
            {
                MonoBehaviour.Destroy(_showGameObject);
                _showGameObject = null;
            }
        }
    }
}