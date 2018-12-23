using System;
using System.Reflection;

namespace LuckSkill
{
    class Util
    {
        // http://stackoverflow.com/questions/3303126/how-to-get-the-value-of-private-field-in-c
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }

        public static void CallInstanceMethod(Type type, object instance, string name, object[] args)
        {
            // TODO: Support method overloading
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            MethodInfo func = type.GetMethod(name, bindFlags);
            func.Invoke(instance, args);
        }

        public static void DecompileComment( string str )
        {
        }
    }
}
