using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

static public class shaco_ExtensionsDataSave
{
    static public void WriteVector2(this shaco.Base.IDataSave datasave, string key, Vector2 value) { datasave.WriteArguments(key, value.x, value.y); }
    static public void WriteVector3(this shaco.Base.IDataSave datasave, string key, Vector3 value) { datasave.WriteArguments(key, value.x, value.y, value.z); }
    static public void WriteVector4(this shaco.Base.IDataSave datasave, string key, Vector4 value) { datasave.WriteArguments(key, value.x, value.y, value.z, value.w); }
    static public void WriteColor(this shaco.Base.IDataSave datasave, string key, Color value) { datasave.WriteArguments(key, value.r, value.g, value.b, value.a); }
    static public void WriteRect(this shaco.Base.IDataSave datasave, string key, Rect value) { datasave.WriteArguments(key, value.x, value.y, value.width, value.height); }

    static public Vector2 ReadVector2(this shaco.Base.IDataSave datasave, string key) { return datasave.ReadVector2(key, Vector2.zero); }
    static public Vector2 ReadVector2(this shaco.Base.IDataSave datasave, string key, Vector2 defaultValue)
    {
        var strTmp = datasave.ReadString(key);
        if (string.IsNullOrEmpty(strTmp))
        {
            return defaultValue;
        }
        else
        {
            var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, datasave.SPLIT_FLAG);
            return new Vector2(float.Parse(splitTmp[0]), float.Parse(splitTmp[1]));
        }
    }

    static public Vector3 ReadVector3(this shaco.Base.IDataSave datasave, string key) { return datasave.ReadVector3(key, Vector3.zero); }
    static public Vector3 ReadVector3(this shaco.Base.IDataSave datasave, string key, Vector3 defaultValue)
    {
        var strTmp = datasave.ReadString(key);
        if (string.IsNullOrEmpty(strTmp))
        {
            return defaultValue;
        }
        else
        {
            var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, datasave.SPLIT_FLAG);
            return new Vector3(float.Parse(splitTmp[0]), float.Parse(splitTmp[1]), float.Parse(splitTmp[2]));
        }
    }

    static public Vector4 ReadVector4(this shaco.Base.IDataSave datasave, string key) { return datasave.ReadVector4(key, Vector3.zero); }
    static public Vector4 ReadVector4(this shaco.Base.IDataSave datasave, string key, Vector3 defaultValue)
    {
        var strTmp = datasave.ReadString(key);
        if (string.IsNullOrEmpty(strTmp))
        {
            return defaultValue;
        }
        else
        {
            var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, datasave.SPLIT_FLAG);
            return new Vector4(float.Parse(splitTmp[0]), float.Parse(splitTmp[1]), float.Parse(splitTmp[2]), float.Parse(splitTmp[3]));
        }
    }

    static public Color ReadColor(this shaco.Base.IDataSave datasave, string key) { return datasave.ReadColor(key, Color.black); }
    static public Color ReadColor(this shaco.Base.IDataSave datasave, string key, Color defaultValue)
    {
        var strTmp = datasave.ReadString(key);
        if (string.IsNullOrEmpty(strTmp))
        {
            return defaultValue;
        }
        else
        {
            var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, datasave.SPLIT_FLAG);
            return new Color(float.Parse(splitTmp[0]), float.Parse(splitTmp[1]), float.Parse(splitTmp[2]), float.Parse(splitTmp[3]));
        }
    }


    static public Rect ReadRect(this shaco.Base.IDataSave datasave, string key) { return datasave.ReadRect(key, new Rect()); }
    static public Rect ReadRect(this shaco.Base.IDataSave datasave, string key, Rect defaultValue)
    {
        var strTmp = datasave.ReadString(key);
        if (string.IsNullOrEmpty(strTmp))
        {
            return defaultValue;
        }
        else
        {
            var splitTmp = System.Text.RegularExpressions.Regex.Split(strTmp, datasave.SPLIT_FLAG);
            return new Rect(float.Parse(splitTmp[0]), float.Parse(splitTmp[1]), float.Parse(splitTmp[2]), float.Parse(splitTmp[3]));
        }
    }
}