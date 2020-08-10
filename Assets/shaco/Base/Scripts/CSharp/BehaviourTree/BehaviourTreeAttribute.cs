namespace shaco.Base
{
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class BehaviourProcessTreeAttribute : System.Attribute
    {
        public string path { get { return _path; }}
        private string _path;

        public BehaviourProcessTreeAttribute() {}
        public BehaviourProcessTreeAttribute(System.Type type) 
        {
            var currentPath = shaco.Base.FileHelper.GetCurrentSourceFilePath();
            var unityPath = "Assets/" + currentPath.RemoveFront("/Assets/");
            var oldExtentsion = shaco.Base.FileHelper.GetFilNameExtension(unityPath);
            unityPath = shaco.Base.FileHelper.ReplaceLastFileName(unityPath, type.Name);
            unityPath = shaco.Base.FileHelper.ReplaceAllExtension(unityPath, oldExtentsion);
            this._path = unityPath;
        }
    }
}