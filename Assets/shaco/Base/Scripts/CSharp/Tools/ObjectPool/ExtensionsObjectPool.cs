using System.Collections;
using System.Collections.Generic;

static public class shaco_ExtensionsObjectPool
{
    static public object RecyclingWithPool(this object obj)
    {
        return shaco.Base.GameHelper.objectpool.RecyclingObject(obj);
    }

    static public object DestroyWithPool(this object obj)
    {
        return shaco.Base.GameHelper.objectpool.DestroyObject(obj);
    }
}
