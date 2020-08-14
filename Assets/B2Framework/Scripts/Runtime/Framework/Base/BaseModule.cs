namespace B2Framework
{
    internal abstract class BaseModule
    {
        internal virtual int Priority
        {
            get
            {
                return 0;
            }
        }
        internal abstract void Update(float deltaTime, float unscaledDeltaTime);

        internal abstract void Dispose();
    }
}