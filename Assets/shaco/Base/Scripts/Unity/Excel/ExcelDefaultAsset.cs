using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class ExcelDefaultAsset : ScriptableObject
    {
        [SerializeField]
        public List<Base.ExcelDefine.RowData> datas = new List<Base.ExcelDefine.RowData>();
    }
}