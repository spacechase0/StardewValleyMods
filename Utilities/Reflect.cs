using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceCore.Utilities
{
    public class Reflect
    {
        // Originally I was just going to do this directly myself, but since SMAPI
        // caches these things I might as well take advantage of it.
        public static T getField<T>(object obj, string field)
        {
            return SpaceCore.instance.Helper.Reflection.GetPrivateValue< T >(obj, field);
        }

        public static T getField<T>(Type type, string field)
        {
            return SpaceCore.instance.Helper.Reflection.GetPrivateValue< T >(type, field);
        }

        public static T getProperty<T>(object obj, string field)
        {
            return SpaceCore.instance.Helper.Reflection.GetPrivateProperty<T>(obj, field).GetValue();
        }

        public static T getProperty<T>(Type type, string field)
        {
            return SpaceCore.instance.Helper.Reflection.GetPrivateProperty<T>(type, field).GetValue();
        }

        public static void setField<T>(object obj, string field, T value)
        {
            SpaceCore.instance.Helper.Reflection.GetPrivateField<T>(obj, field).SetValue(value);
        }

        public static void setField<T>(Type type, string field, T value)
        {
            SpaceCore.instance.Helper.Reflection.GetPrivateField<T>(type, field).SetValue(value);
        }

        public static void setProperty<T>(object obj, string field, T value)
        {
            SpaceCore.instance.Helper.Reflection.GetPrivateProperty<T>(obj, field).SetValue(value);
        }

        public static void setProperty<T>(Type type, string field, T value)
        {
            SpaceCore.instance.Helper.Reflection.GetPrivateProperty<T>(type, field).SetValue(value);
        }

        public static void callMethod(object obj, string method, object[] args)
        {
            SpaceCore.instance.Helper.Reflection.GetPrivateMethod(obj, method).Invoke(args);
        }

        public static T callMethod< T >( object obj, string method, object[] args )
        {
            return SpaceCore.instance.Helper.Reflection.GetPrivateMethod(obj, method).Invoke< T >(args);
        }

        public static void callMethod(Type type, string method, object[] args)
        {
            SpaceCore.instance.Helper.Reflection.GetPrivateMethod(type, method).Invoke(args);
        }

        public static T callMethod<T>(Type type, string method, object[] args)
        {
            return SpaceCore.instance.Helper.Reflection.GetPrivateMethod(type, method).Invoke<T>(args);
        }
    }
}
