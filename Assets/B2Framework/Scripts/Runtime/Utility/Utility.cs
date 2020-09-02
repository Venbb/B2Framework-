namespace B2Framework
{
    public static partial class Utility
    {
        public static T ToEnum<T>(string e)
        {
            return (T)System.Enum.Parse(typeof(T), e);
        }
    }
}