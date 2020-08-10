using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace shacoEditor
{
    /// <summary>
    /// 编辑器图集设置接口
    /// </summary>
    [System.Serializable]
    public class SpriteAtlasSettingsDefault : ISpriteAtlasSettings
    {
        //判断是否可以打入图片的条件
        private enum JudgementCondition
        {
            //宽或高，大于x像素值可以打入图集
            GreaterThanAny,
            //宽且高，大于x像素值可以打入图集
            GreaterThanBoth,
            //宽或高，小于x像素值可以打入图集
            LessThanAny,
            //宽且高，小于x像素值可以打入图集
            LessThanBoth,

            //宽，大于x像素值可以打入图集
            GreaterThanWidth,
            //高，大于x像素值可以打入图集
            GreaterThanHeight,
            //宽，小于x像素值可以打入图集
            LessThanWidth,
            //高，小于x像素值可以打入图集
            LessThanHeight
        }

        private readonly GUIContent GUI_CONTENT_MAX_REFERENCE_COUNT = new GUIContent("Max reference count", "if over than max referece count, it will move to subfolder shared atlas or global atlas");
        private readonly GUIContent GUI_CONTENT_CONDITION = new GUIContent("Condition", "only textures meeting this condition will be automatically packed into the atlas, otherwise they will be move to the non atlas directory");

        //判断可以打入图集的条件
        [SerializeField]
        private JudgementCondition _judgementCondition = JudgementCondition.LessThanBoth;

        //判断图片大小像素值
        [SerializeField]
        private Vector2 _judgementPixel = new Vector2(256, 256);

        //最大引用计数，当超出该计数的图片会被规划到共享或者公用图集
        [SerializeField]
        private int _maxReferenceCount = 5;

        public bool CanBuildInAtlas(Texture texture)
        {
            bool retValue = false;
            switch (_judgementCondition)
            {
                case JudgementCondition.GreaterThanAny: retValue = texture.width > _judgementPixel.x || texture.height > _judgementPixel.y; break;
                case JudgementCondition.GreaterThanBoth: retValue = texture.width > _judgementPixel.x && texture.height > _judgementPixel.y; break;
                case JudgementCondition.LessThanAny: retValue = texture.width < _judgementPixel.x || texture.height < _judgementPixel.y; break;
                case JudgementCondition.LessThanBoth: retValue = texture.width < _judgementPixel.x && texture.height < _judgementPixel.y; break;
                case JudgementCondition.GreaterThanWidth: retValue = texture.width > _judgementPixel.x; break;
                case JudgementCondition.GreaterThanHeight: retValue = texture.height > _judgementPixel.y; break;
                case JudgementCondition.LessThanWidth: retValue = texture.width < _judgementPixel.x; break;
                case JudgementCondition.LessThanHeight: retValue = texture.height < _judgementPixel.y; break;
                default: Debug.LogError("SpriteAtlasSettingsDefault CanBuildInNormalAtlas error: unsupport type=" + _judgementCondition); break;
            }
            return retValue;
        }

        public bool CanBuildInGlobalAtlas(Texture texture, System.Collections.Generic.ICollection<string> referenceTargets)
        {
            return referenceTargets.Count > _maxReferenceCount;
        }

        public bool CanBuildInSubSharedAtlas(Texture texture, System.Collections.Generic.ICollection<string> referenceTargets)
        {
            if (referenceTargets.IsNullOrEmpty())
                return false;

            if (referenceTargets.Count <= _maxReferenceCount)
                return false;

            //如果所有文件夹都是在同一目录下则判定为文件夹共享目录
            var retValue = true;
            var assetPathFirst = referenceTargets.First();
            var sameFolderName = shaco.Base.FileHelper.GetFolderNameByPath(assetPathFirst);

            foreach (var iter in referenceTargets)
            {
                var assetPath = iter;
                var folderName = shaco.Base.FileHelper.GetFolderNameByPath(assetPath);
                if (folderName != sameFolderName)
                {
                    retValue = false;
                    break;
                }
            }
            return retValue;
        }

        public void LoadSettings(shaco.Base.IDataSave datasave)
        {
            _judgementCondition = datasave.ReadEnum<JudgementCondition>("SpriteAtlasSettingsDefault._judgementCondition", _judgementCondition);
            _judgementPixel = datasave.ReadVector2("SpriteAtlasSettingsDefault._judgementPixel", _judgementPixel);
            _maxReferenceCount = datasave.ReadInt("SpriteAtlasSettingsDefault._maxReferenceCount", _maxReferenceCount);
        }

        public void SaveSettings(shaco.Base.IDataSave datasave)
        {
            datasave.WriteEnum("SpriteAtlasSettingsDefault._judgementCondition", _judgementCondition);
            datasave.WriteVector2("SpriteAtlasSettingsDefault._judgementPixel", _judgementPixel);
            datasave.WriteInt("SpriteAtlasSettingsDefault._maxReferenceCount", _maxReferenceCount);
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical("box");
            {
                if (GUILayoutHelper.DrawHeader("Editor Settings", "SpriteAtlasSettingsDefault.Settings"))
                {
                    GUILayout.BeginHorizontal();
                    {
                        _judgementCondition = (JudgementCondition)EditorGUILayout.EnumPopup(GUI_CONTENT_CONDITION, _judgementCondition);
                        GUILayout.Label("Pixel", GUILayout.ExpandWidth(false));
                        _judgementPixel = EditorGUILayout.Vector2Field(string.Empty, _judgementPixel);
                    }
                    GUILayout.EndHorizontal();
                    
                    _maxReferenceCount = EditorGUILayout.IntField(GUI_CONTENT_MAX_REFERENCE_COUNT, _maxReferenceCount);
                }
            }
            GUILayout.EndVertical();
        }
    }
}