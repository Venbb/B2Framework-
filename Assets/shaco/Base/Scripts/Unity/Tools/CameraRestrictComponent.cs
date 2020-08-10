using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 限制摄像机在对象范围内移动组件 
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraRestrictComponent : MonoBehaviour
{
    //限制摄像机移动区域的对象
    [SerializeField] [Header("限制区域对象")]
    private GameObject _target;

    //是否根据限制区域设置摄像机大小
    [SerializeField] [Header("是否自动根据限制区域设定摄像机大小")]
    private bool _isAutoSetCameraSize = false;

    //地图格子宽高
    [SerializeField] [Header("地图格子大小")]
    private Vector2 _gridSize;

    //仅在isInternalRestrictRect为true下生效
    //-------------------------------
    //|           remain            |
    //| r  ---------------------  r |
    //| e  |                   |  e |
    //| m  |      camera       |  m |
    //| a  |     position      |  a |
    //| i  |                   |  i |
    //| b  ---------------------  n |
    //|           remain            |
    //-------------------------------
    [SerializeField] [Header("留空格子数量")]
    private Vector2 _remainGridSize = new Vector2(1.5f, 0);

    //限制对象的矩形 - 与实际像素默认是1 : 100比率
    [SerializeField] [shaco.ReadOnly] [Header("限制区域矩形大小")]
    private Rect _restrictFullRect;

    private Camera _targetCamera;

    //摄像机拍摄区域大小
    private Vector2 _cameraSize;

    void Start()
    {
        if (null != _target)
        {
            var spriterRenderer = _target.GetComponent<SpriteRenderer>();
            if (InitWithSpriteRenderer(spriterRenderer, _gridSize))
                return;
        }
    }

    /// <summary>
    /// 判断当前位置是否在内部设定矩形范围内
    /// <param name="pos">当前位置</param>
    /// <return>是否在内部设定矩形范围内</return>
    /// </summary>
    public bool IsInternalRestrictRect(Vector2 pos)
    {
        var restrictRect = GetInternalRestrictRect();
        var retValue = restrictRect.Contains(pos);
        return retValue;
    }

    /// <summary>
    /// 摄像机是否超出限制区域并自动回正
    /// <param name="pos">当前位置</param>
    /// <param name="isInternalRestrictRect">是否为内部限制矩形区域</param>
    /// isInternalRestrictRect参数模式如下
    //-------------------------------
    //|           false             |
    //| f  ---------------------  f |
    //| a  |                   |  a |
    //| l  |                   |  l |
    //| s  |       true        |  s |
    //| e  |                   |  e |
    //|    ---------------------    |
    //|           false             |
    //-------------------------------
    /// <return>true: 超出了限制区域并回正了 false：没有超出限制区域</return>
    /// </summary>
    public bool CheckCameraRestrict(ref Vector3 pos)
    {
        var isOutOfRestrictX = false;
        var isOutOfRestrictY = false;

        //使用内部限制矩形区域
        Rect restrictRect = _restrictFullRect;
        if (pos.x - _cameraSize.x / 2 < restrictRect.xMin)
        {
            pos.x = restrictRect.xMin + _cameraSize.x / 2;
            isOutOfRestrictX = true;
        }
        else if (pos.x + _cameraSize.x / 2 > restrictRect.xMax)
        {
            pos.x = restrictRect.xMax - _cameraSize.x / 2;
            isOutOfRestrictX = true;
        }

        if (pos.y - _cameraSize.y / 2 < restrictRect.yMin)
        {
            pos.y = restrictRect.yMin + _cameraSize.y / 2;
            isOutOfRestrictY = true;
        }
        else if (pos.y + _cameraSize.y / 2 > restrictRect.yMax)
        {
            pos.y = restrictRect.yMax - _cameraSize.y / 2;
            isOutOfRestrictY = true;
        }

        return isOutOfRestrictX || isOutOfRestrictY;
    }

    /// <summary>
    /// 通过SpriterRenderer获取限制区域
    /// </summary>
    public bool InitWithSpriteRenderer(SpriteRenderer target, Vector3 gridSize)
    {
        _targetCamera = this.GetComponent<Camera>();
        if (!_targetCamera.orthographic)
        {
            shaco.Log.Error("CameraRestrictComponent InitWithSpriteRenderer error: only support ortho graphic mode", this);
            return false;   
        }

        var pos = new Vector2(target.transform.position.x - target.size.x / 2, target.transform.position.y - target.size.y / 2);
        _restrictFullRect = new Rect(pos, target.size);
        CheckAutoSetCameraSize(_targetCamera, _restrictFullRect);

        var cameraHeight = _targetCamera.orthographicSize * 2;
        var cameraWidth = cameraHeight * GetScreenAspect();
        _cameraSize = new Vector2(cameraWidth, cameraHeight);

        _target = target.gameObject;
        _gridSize = gridSize;
        return true;
    }

    private Rect GetInternalRestrictRect()
    {
        Rect restrictRect = new Rect();
        Vector2 cameraPosition = _targetCamera.transform.position;
        var offsetRemainGridSize = _gridSize * _remainGridSize;
        restrictRect.position = (cameraPosition - _cameraSize / 2) + offsetRemainGridSize;
        restrictRect.size = _cameraSize - offsetRemainGridSize * 2;
        return restrictRect;
    }

    private void CheckAutoSetCameraSize(Camera camera, Rect restrictRect)
    {
        if (_isAutoSetCameraSize)
        {
            camera.orthographicSize = restrictRect.height / 2;
        }
    }

    /// <summary>
    /// 获取屏幕分辨率宽高比
    /// </summary>
    private float GetScreenAspect()
    {
        return (float)Screen.width / (float)Screen.height;
    }
}