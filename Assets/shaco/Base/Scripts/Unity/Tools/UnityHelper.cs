using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public partial class UnityHelper
    {
        static private List<ShaderBindInfo> _cachedShaderBindInfo = null;

        static public void ChangeParent(GameObject target, GameObject parent)
        {
            var oldPos = target.transform.position;
            var oldScale = target.transform.localScale;
            var oldAngle = target.transform.eulerAngles;

            target.transform.SetParent(parent == null ? null : parent.transform);

            target.transform.position = oldPos;
            target.transform.localScale = oldScale;
            target.transform.eulerAngles = oldAngle;
        }

        static public void ChangeParentLocalPosition(GameObject target, GameObject parent)
        {
            var oldPos = target.transform.localPosition;
            var oldScale = target.transform.localScale;
            var oldAngle = target.transform.localEulerAngles;
            var oldOffsetMin = Vector2.zero;
            var oldOffsetMax = Vector2.zero;
            var rectTrans = target.GetComponent<RectTransform>();
            if (null != rectTrans)
            {
                oldOffsetMin = rectTrans.offsetMin;
                oldOffsetMax = rectTrans.offsetMax;
            }

            target.transform.SetParent(parent == null ? null : parent.transform);

            target.transform.localPosition = oldPos;
            target.transform.localScale = oldScale;
            target.transform.localEulerAngles = oldAngle;
            if (null != rectTrans)
            {
                rectTrans.offsetMin = oldOffsetMin;
                rectTrans.offsetMax = oldOffsetMax;
            }
        }

        /// <summary>
        /// get the top of the root node, and this node not have parent
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        static public GameObject GetRoot(GameObject child)
        {
            return GetRoot(child, null);
        }

        /// <summary>
        /// get the top of the root node by parentName
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        static public GameObject GetRoot(GameObject child, string parentName)
        {
            if (child == null)
                return null;

            Transform prevParent = child.transform;
            var parent = prevParent.parent;

            while (parent != null)
            {
                prevParent = parent;
                if (!string.IsNullOrEmpty(parentName) && parent.name == parentName)
                    break;

                parent = parent.transform.parent;
            }

            return null != prevParent ? prevParent.gameObject : null;
        }

        /// <summary>
        /// 获取根节点
        /// <param name="child">子节点对象</param>
        /// </summary>
        static public T GetRootWithComponent<T>(GameObject child) where T : Component
        {
            if (child == null)
                return null;

            Transform prevParent = child.transform;
            var parent = prevParent.parent;

            while (parent != null)
            {
                prevParent = parent;
                if (parent.GetComponent<T>() != null)
                    break;

                parent = parent.transform.parent;
            }

            return null != prevParent ? prevParent.gameObject.GetComponent<T>() : null;
        }

        /// <summary>
        /// call 'callfunc' when animation play end
        /// </summary>
        /// <param name="ani"></param> animation target
        /// <param name="animationName"></param> animation name
        /// <param name="callfunc"></param> event call function
        static public void CallOnAnimationEnd(Animation ani, System.Action callfunc)
        {
            var timeUpdate = shaco.Repeat.CreateRepeatForever(shaco.DelayTime.Create(60.0f));

            timeUpdate.RunAction(ani.gameObject);
            timeUpdate.onFrameFunc = (float percent) =>
            {
                if (!ani.isPlaying)
                {
                    timeUpdate.StopMe();
                    try
                    {
                        callfunc();
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("UnityHelper CallOnAnimationEnd error: animation=" + ani + " callfunc=" + callfunc + " e=" + e);
                    }
                }
            };
        }

        static public void CallOnAnimationEnd(Animator ani, System.Action callfunc)
        {
            int animationindex = 0;
            var timeUpdate = shaco.Repeat.CreateRepeatForever(shaco.DelayTime.Create(60.0f));

            //这里必须要刷新一桢，否则Play切换动画后，normalizedTime却没有及时更新导致判断出错
            ani.Update(0);

            timeUpdate.RunAction(ani.gameObject);
            timeUpdate.onFrameFunc = (float percent) =>
            {
                bool isStop = false;
                var animationTmp = ani.GetCurrentAnimatorStateInfo(animationindex);
                if (animationTmp.normalizedTime >= 1.0f)
                {
                    isStop = true;
                }

                if (isStop)
                {
                    timeUpdate.StopMe();
                    try
                    {
                        callfunc();
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("UnityHelper CallOnAnimationEnd error: animation=" + ani + " callfunc=" + callfunc + " e=" + e);
                    }
                }
            };
        }

        /// <summary>
        /// 监听粒子特效播放结束回调(如果是loop模式的回调无法接受到回调的)
        /// <param name="particle">特效对象根节点</param>
        /// <param name="callfunc">播放完毕回调方法</param>
        /// <return></return>
        /// </summary>
        static public void CallOnParticleEnd(GameObject particleRoot, System.Action callfunc)
        {
            var timeUpdate = shaco.Repeat.CreateRepeatForever(shaco.DelayTime.Create(60.0f));
            var particleSystems = particleRoot.GetComponentsInChildren<ParticleSystem>();

            if (particleSystems.IsNullOrEmpty() && null != callfunc)
            {
                try
                {
                    callfunc();
                }
                catch (System.Exception e)
                {
                    Log.Error("UnityHelper CallOnParticleEnd 1 error: root=" + particleRoot + " callfunc=" + callfunc + " e=" + e);
                }
                return;
            }

            timeUpdate.RunAction(particleRoot.gameObject);
            timeUpdate.onFrameFunc = (float percent) =>
            {
                bool allStopped = true;

                for (int i = particleSystems.Length - 1; i >= 0; --i)
                {
                    var ps = particleSystems[i];
    #if UNITY_5_6_OR_NEWER
                    if ((!ps.isStopped && ps.time < ps.main.duration) && !ps.main.loop)
    #else
                    if ((!ps.isStopped && ps.time < ps.duration) && !ps.loop)
    #endif
                    {
                        allStopped = false;
                    }
                }

                if (allStopped && null != callfunc)
                {
                    try
                    {
                        callfunc();
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("UnityHelper CallOnParticleEnd 2 error: root=" + particleRoot + " callfunc=" + callfunc + " e=" + e);
                    }
                }
            };
        }

        static public void SetLocalPositionByPivot(GameObject target, Vector3 newPosition, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("setLocalPositionByArchor error: target dose not contain RectTransform !");
                return;
            }
            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);

            target.transform.localPosition = new Vector3(
                newPosition.x - sizeTmp.x * newPivot.x,
                newPosition.y - sizeTmp.y * newPivot.y,
                newPosition.z);
        }

        static public Vector3 GetLocalPositionByPivot(GameObject target, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("getLocalPositioByArchor error: dose not contain RectTransform !");
                return Vector3.zero;
            }
            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);

            return new Vector3(
                rectTrans.localPosition.x + sizeTmp.x * newPivot.x,
                rectTrans.localPosition.y + sizeTmp.y * newPivot.y,
                rectTrans.localPosition.z);
        }

        static public void SetPivotByLocalPosition(GameObject target, Vector2 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("SetPivotByLocalPosition error: dose not contain RectTransform !");
                return;
            }

            var pivotOffset = pivot - rectTrans.pivot;
            var sizeTmp = GetRealSize(rectTrans);
            rectTrans.pivot = pivot;
            target.transform.localPosition = new Vector3(
                target.transform.localPosition.x + sizeTmp.x * pivotOffset.x,
                target.transform.localPosition.y + sizeTmp.y * pivotOffset.y,
                target.transform.localPosition.z);
        }

        static public Vector3 GetWorldPositionByPivot(GameObject target, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("GetWorldPositionByPivot error: dose not contain RectTransform !");
                return Vector3.zero;
            }

            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);
            var t1 = target.transform.TransformPoint(Vector3.zero);
            var t2 = target.transform.TransformPoint(new Vector3(sizeTmp.x, sizeTmp.y));
            var t3 = t2 - t1;
            var t4 = new Vector3(t3.x * newPivot.x, t3.y * newPivot.y, 0);

            var ret = new Vector3(
                rectTrans.position.x + t4.x,
                rectTrans.position.y + t4.y,
                rectTrans.position.z);

            return ret;
        }

        static public void SetWorldPositionByPivot(GameObject target, Vector3 newPosition, Vector3 pivot)
        {
            var rectTrans = target.GetComponent<RectTransform>();
            if (rectTrans == null)
            {
                Log.Error("SetWorldPositionByPivot error: target dose not contain RectTransform !");
                return;
            }
            var newPivot = new Vector3(pivot.x - rectTrans.pivot.x, pivot.y - rectTrans.pivot.y, pivot.z);
            var sizeTmp = GetRealSize(rectTrans);
            var newOffset = new Vector3(sizeTmp.x * newPivot.x, sizeTmp.y * newPivot.y, 0);
            newOffset = target.transform.TransformPoint(newOffset);

            target.transform.position += new Vector3(
                newPosition.x - newOffset.x,
                newPosition.y - newOffset.y,
                0);
        }

        static public Vector2 GetRealPivot(RectTransform transform)
        {
            var retValue = Vector2.zero;
            var textTmp = transform.GetComponent<UnityEngine.UI.Text>();
            if (null == textTmp)
            {
                retValue = transform.pivot;
            }
            else
            {
                retValue = textTmp.alignment.ToPivot();
            }
            return retValue;
        }

        static public Vector2 GetRealSize(RectTransform transform)
        {
            var retValue = Vector2.zero;
            var textTmp = transform.GetComponent<UnityEngine.UI.Text>();
            if (null == textTmp)
            {
                retValue = transform.sizeDelta;
            }
            else
            {
                retValue = GetTextRealSize(textTmp);
            }
            return retValue;
        }

        static public Vector2 GetTextRealSize(UnityEngine.UI.Text textTarget)
        {
            var retValue = Vector2.zero;
            // if (textTarget.resizeTextForBestFit && textTarget.horizontalOverflow != HorizontalWrapMode.Overflow)
            // {
            //     TextGenerationSettings generationSettings = textTarget.GetGenerationSettings(Vector2.zero);
            //     generationSettings.fontSize = textTarget.cachedTextGeneratorForLayout.fontSizeUsedForBestFit;

            //     retValue.x = textTarget.cachedTextGeneratorForLayout.GetPreferredWidth(textTarget.text, generationSettings) / textTarget.pixelsPerUnit;
            //     retValue.y = textTarget.preferredHeight;
            // }
            // else
            // {
            retValue = new Vector2(textTarget.preferredWidth, textTarget.preferredHeight);
            // }

            if (textTarget.horizontalOverflow == HorizontalWrapMode.Wrap)
            {
                retValue.x = Mathf.Min(retValue.x, textTarget.rectTransform.sizeDelta.x);
            }
            if (textTarget.verticalOverflow == VerticalWrapMode.Truncate)
            {
                retValue.y = Mathf.Min(retValue.y, textTarget.rectTransform.sizeDelta.y);
            }

            return retValue;
        }

        static public Rect GetLocalRect(RectTransform target)
        {
            var pos = GetLocalPositionByPivot(target.gameObject, shaco.Pivot.LowerLeft);
            var size = GetRealSize(target);
            return new Rect(pos.x, pos.y, size.x, size.y);
        }

        static public Rect GetWorldRect(RectTransform target)
        {
            var pos1 = GetWorldPositionByPivot(target.gameObject, shaco.Pivot.LowerLeft);
            var pos2 = GetWorldPositionByPivot(target.gameObject, shaco.Pivot.UpperRight);
            return new Rect(pos1.x, pos1.y, pos2.x - pos1.x, pos2.y - pos1.y);
        }

        /// <summary>
        /// Iterate through all the child
        /// </summary>
        /// <param name="index"></param> current child index
        /// <param name="child"></param> child target
        static public void ForeachChildren(GameObject target, System.Func<int, GameObject, bool> callfunc)
        {
            int index = 0;
            ForeachChildren(target, callfunc, ref index);

        }
        static private void ForeachChildren(GameObject target, System.Func<int, GameObject, bool> callfunc, ref int index)
        {
            bool result = true;
            for (int i = 0; i < target.transform.childCount; ++i)
            {
                GameObject child = target.transform.GetChild(i).gameObject;

                try
                {
                    result = callfunc(index++, child);
                }
                catch (System.Exception e)
                {
                    Log.Error("UnityHelper ForeachChildren error: target=" + target + " callfunc=" + callfunc + " e=" + e);
                }
                if (!result)
                {
                    break;
                }
                ForeachChildren(child, callfunc, ref index);
            }
        }

        static public Vector3 GetColliderSize(GameObject target)
        {
            Vector3 ret = Vector3.zero;
            var collider = target.GetComponent<Collider>();

            if (collider == null)
            {
                Log.Error("not find 'Collider' Component in target=" + target);
                return ret;
            }

            var oldRotation = target.transform.rotation;
            target.transform.eulerAngles = Vector3.zero;
            ret = collider.bounds.size;
            target.transform.rotation = oldRotation;

            return ret;
        }

        //fixed missing shader when import asset bundle
        private class ShaderBindInfo : shaco.Base.IObjectPoolData
        {
            public GameObject target = null;
            public Material material = null;

            public void Dispose()
            {
                target = null;
                material = null;
            }
        }

        public static void ResetShader(UnityEngine.Object obj)
        {
            if (null == _cachedShaderBindInfo)
                _cachedShaderBindInfo = new List<ShaderBindInfo>();

            lock (_cachedShaderBindInfo)
            {
                _cachedShaderBindInfo.Clear();
                var bindInfofullTypeName = typeof(ShaderBindInfo).ToTypeString();

                if (obj is Material)
                {
                    Material m = obj as Material;
                    var newBindInfo = shaco.GameHelper.objectpool.Instantiate(bindInfofullTypeName, () => new ShaderBindInfo());
                    newBindInfo.target = null;
                    newBindInfo.material = m;
                    _cachedShaderBindInfo.Add(newBindInfo);
                }
                else if (obj is GameObject)
                {
                    GameObject go = obj as GameObject;
                    var renders = go.GetComponentsInChildren<Renderer>(true);

                    if (null != renders)
                    {
                        foreach (Renderer item in renders)
                        {
                            Material[] materialsArr = item.sharedMaterials;
                            foreach (Material m in materialsArr)
                            {
                                var newBindInfo = shaco.GameHelper.objectpool.Instantiate(bindInfofullTypeName, () => new ShaderBindInfo());
                                newBindInfo.target = item.gameObject;
                                newBindInfo.material = m;
                                _cachedShaderBindInfo.Add(newBindInfo);
                            }
                        }
                    }

                    var graphics = go.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
                    if (null != graphics)
                    {
                        foreach (var item in graphics)
                        {
                            var newBindInfo = shaco.GameHelper.objectpool.Instantiate(bindInfofullTypeName, () => new ShaderBindInfo());
                            newBindInfo.target = item.gameObject;
                            newBindInfo.material = item.material;
                            _cachedShaderBindInfo.Add(newBindInfo);
                        }
                    }
                }

                for (int i = _cachedShaderBindInfo.Count - 1; i >= 0; --i)
                {
                    Material m = _cachedShaderBindInfo[i].material;

                    if (null == m || null == m.shader)
                        continue;

                    var shaderName = m.shader.name;
                    var newShader = Shader.Find(shaderName);
                    if (newShader != null)
                    {
                        m.shader = newShader;
                    }
                    else if (!string.IsNullOrEmpty(shaderName))
                    {
                        Log.Warning("missing shader obj=" + obj + " shader name=" + shaderName + " material target=" + _cachedShaderBindInfo[i].target + " material name=" + m);
                    }
                }

                if (_cachedShaderBindInfo.Count > 0)
                {
                    _cachedShaderBindInfo.Clear();

                    var totalCountInMemory = shaco.GameHelper.objectpool.GetUnusedCount(bindInfofullTypeName) + shaco.GameHelper.objectpool.GetInstantiatedCount(bindInfofullTypeName);
                    if (totalCountInMemory > 10)
                        shaco.GameHelper.objectpool.DestroyAllObjects(bindInfofullTypeName);
                    else
                        shaco.GameHelper.objectpool.RecyclingAllObjects(bindInfofullTypeName);
                }
            }
        }

        static public void SafeDontDestroyOnLoad(Object target)
        {
            if (Application.isPlaying)
                MonoBehaviour.DontDestroyOnLoad(target);
        }

        static public void SafeDestroy(Object target, float delayTime = 0)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                MonoBehaviour.Destroy(target, delayTime);
            }
            else
            {
                MonoBehaviour.DestroyImmediate(target);
            }
#else
            MonoBehaviour.Destroy(target, delayTime);
#endif
        }

        /// <summary>
        /// 获取一个对象在场景对象面板中的路径
        /// <param name="target">查找对象</param>
        /// <return>对象路径</return>
        /// </summary>
        static public string GetObjectPathInHierarchy(UnityEngine.Object target)
        {
            UnityEngine.GameObject targetGameObject = null;
            if (target is UnityEngine.GameObject)
                targetGameObject = target as GameObject;
            else if (target is UnityEngine.Component)
                targetGameObject = (target as UnityEngine.Component).gameObject;
            else if (target is UnityEngine.Behaviour)
                targetGameObject = (target as UnityEngine.Behaviour).gameObject;

            if (null == targetGameObject)
            {
                return target.name;
            }

            var retValue = new System.Text.StringBuilder();
            var parentsTmp = new List<UnityEngine.Transform>();
            var parentTmp = targetGameObject.transform.parent;
            while (null != parentTmp)
            {
                parentsTmp.Add(parentTmp);
                parentTmp = parentTmp.parent;
            }

            for (int i = parentsTmp.Count - 1; i >= 0; --i)
            {
                retValue.Append(parentsTmp[i].name);
                retValue.Append("/");
            }
            retValue.Append(targetGameObject.name);

            return retValue.ToString();
        }

        /// <summary>
        /// 获取绝对路径
        /// <param name="relativePath">unity相对路径</param>
        /// <return>绝对路径</return>
        /// </summary>
        static public string UnityPathToFullPath(string relativePath)
        {
            var currentPath = shaco.Base.FileHelper.GetCurrentSourceFilePath();
            var applicationPath = currentPath.RemoveBehind("/Assets/");
            if (!relativePath.StartsWith(applicationPath))
            {
                relativePath = shaco.Base.FileHelper.ContactPath(applicationPath, relativePath);
            }
            return relativePath;
        }

        /// <summary>
        /// 获取unity对象相对路径
        /// <param name="fullPath">绝对路径</param>
        /// <return>unity对象相对路径</return>
        /// </summary>
        static public string FullPathToUnityPath(string fullPath)
        {
            var currentPath = shaco.Base.FileHelper.GetCurrentSourceFilePath();
            var applicationPath = currentPath.RemoveBehind("Assets/");
            return fullPath.RemoveFront(applicationPath).Replace("\\", "/");
        }

        static public bool HaveCopyDataInClipBoard()
        {
            return !string.IsNullOrEmpty(GUIUtility.systemCopyBuffer);
        }

        static public void CopyToClipBoard(string data)
        {
            GUIUtility.systemCopyBuffer = data;
        }

        static public string PasteFromClipBoard()
        {
            return GUIUtility.systemCopyBuffer;
        }

        /// <summary>
        /// 获取Mac地址
        /// </summary>
        static public string GetMacAddress()
        {
            string physicalAddress = string.Empty;

#if UNITY_WEBGL
            Log.Error("UnityHelper GetMacAddress error: unsupport on webgl !");
            return "not found mac address";   
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
            //Android需要通过native方法获取
            AndroidJavaObject context = new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            var utilityClass = new AndroidJavaClass("com.shaco.Utility");
            physicalAddress = utilityClass.CallStatic<string>("GetMacAddress", context);
            return physicalAddress;
#else
            var nice = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            foreach (System.Net.NetworkInformation.NetworkInterface adaper in nice)
            {
                if (adaper.Description == "en0")
                {
                    physicalAddress = adaper.GetPhysicalAddress().ToString();
                    break;
                }
                else
                {
                    physicalAddress = adaper.GetPhysicalAddress().ToString();
                    if (physicalAddress != string.Empty)
                    {
                        break;
                    };
                }
            }

            //如果id地址没有:号隔开，则手动加一个
            if (!string.IsNullOrEmpty(physicalAddress) && !physicalAddress.Contains(":"))
            {
                int index = 0;
                while (index < physicalAddress.Length - 2)
                {
                    index += 2;
                    physicalAddress = physicalAddress.Insert(index, ":");
                    index += 1;
                }
            }
            return physicalAddress;
#endif
        }

        /// <summary>
        /// 绘制调试模式的矩形
        /// </summary>
        static public void DrawDebugRect(GameObject target, Rect rect)
        {
            var lineRenderer = target.GetOrAddComponent<LineRenderer>();
            var sizeArea = rect.size.x * rect.size.y;
            lineRenderer.startWidth = sizeArea * 0.0005f;
            lineRenderer.endWidth = sizeArea * 0.0005f;
            lineRenderer.positionCount = 5;
            lineRenderer.SetPosition(0, rect.min);
            lineRenderer.SetPosition(1, new Vector3(rect.xMax, rect.yMin));
            lineRenderer.SetPosition(2, rect.max);
            lineRenderer.SetPosition(3, new Vector3(rect.xMin, rect.yMax));
            lineRenderer.SetPosition(4, rect.min);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void BeginSample(string tag)
        {
            UnityEngine.Profiling.Profiler.BeginSample(tag);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void EndSample()
        {
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}