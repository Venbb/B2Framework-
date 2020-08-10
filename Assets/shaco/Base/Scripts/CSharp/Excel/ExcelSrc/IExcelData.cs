using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public interface IExcelData 
    {
        void CheckInitAsync(System.Action<float> callbackProgress = null);
    }
}