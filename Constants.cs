using Avalonia.Logging;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TQDBEditor.Constants
{
    public static class RegionNames
    {
        public const string ViewsRegion = "Views";
    }

    public static class Modules
    {
        public const string ModulesPath = @".\Modules";
    }

    public static class MyLogAreas
    {
        public const string Initialization = nameof(Initialization);
        public const string Critial = nameof(Critial);

        public static IEnumerable<string> All => GetConstFieldsFromType(typeof(MyLogAreas)).Select(fi => fi.GetRawConstantValue()!.ToString()!);
        public static IEnumerable<string> AvaloniaLogAreas => GetConstFieldsFromType(typeof(LogArea)).Select(fi => fi.GetRawConstantValue()!.ToString()!);

        private static IEnumerable<FieldInfo> GetConstFieldsFromType(Type type)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Static).Where(fi => fi.IsLiteral && !fi.IsInitOnly);
        }
    }
}
