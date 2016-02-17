using System;
using System.Reflection;

namespace HelpersLib
{
    public static class AppInfo
    {
        public static string Version
        {
            get
            {
                var assembly = typeof(AppInfo).GetTypeInfo().Assembly;
                // In some PCL profiles the above line is: var assembly = typeof(MyType).Assembly;
                var assemblyName = new AssemblyName(assembly.FullName);
                return assemblyName.Version.ToString();
            }
        }
    }
}