using System;
using System.Reflection;

namespace Gigya.Http.Telemetry.Consts
{
    [AttributeUsage(validOn: AttributeTargets.Interface | AttributeTargets.Class)]
    public class TypeFriendlyNameAttribute : Attribute
    {
        public string Name { get; set; }

    }


    public class FriendlyName<T>
    {

        public static readonly string Instance = (typeof(T).GetCustomAttribute(typeof(TypeFriendlyNameAttribute)) as TypeFriendlyNameAttribute)?.Name ?? typeof(T).Name;
    }

    
}