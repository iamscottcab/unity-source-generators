using System;

namespace Scott.Cab.Initialization
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InitializableAttribute : Attribute { }
}
