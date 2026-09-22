using System.Reflection;

namespace OakMusic
{
    public static class AppInfo
    {
        public static string Version =>
            Assembly.GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString(3)
                ?? "0.0.0";
    }
}