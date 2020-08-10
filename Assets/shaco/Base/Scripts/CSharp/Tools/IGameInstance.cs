namespace shaco.Base
{
    /// <summary>
    /// 框架单例接口
    /// </summary>
    public interface IGameInstance
	{

	}

    /// <summary>
    /// 框架单例创建方法，如果没有则默认使用shaco_ExtensionsUtility.Instantiate扩展方法进行创建
    /// </summary>
    public interface IGameInstanceCreator
	{
        //继承该接口后需要实现以下方法，以替换掉new class
        // static public object Create()
		// {
		// 	return xxx;	
		// }
    }
}