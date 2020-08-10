using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    /// <summary>
    /// 批量修改Unity组建属性委托，可继承自该委托来自定义修改逻辑
    /// </summary>
    public interface IChangeComponentData
    {
        /// <summary>
        /// 获取需要查找的组件类型名字
        /// </summary>
        string GetSearchComponentTypeName();

        /// <summary>
        /// 获取需要查找的参数名字表
        /// </summary>
        List<string> GetSerachPropertyNames();

        /// <summary>
        /// 获取需要改变的数据类型
        /// </summary>
        ChangeComponentDataHelper.ValueType GetChangePropertyValueType();

        /// <summary>
        /// 修改参数
        /// </summary>
        void ChangePropertyValue(UnityEngine.Object unityObject, SerializedObject obj, SerializedProperty property, shaco.AutoValue autoValue);

        /// <summary>
        /// 绘制自定义搜索条件
        /// </summary>
        void DrawCustomSearchCondition();

        /// <summary>
        /// 是否满足自定义搜索条件
        /// <param name="targetInfo">搜索对象信息</param>
        /// <return>true or false</return>
        /// </summary>
        bool IsCustomSearchCondition(ChangeComponentDataWindow.TargetObjectInfo targetInfo);
    }
}