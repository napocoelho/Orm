using System;

namespace CoreDll.Bindables
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class AlsoNotifyForAttribute : Attribute
    {
        public string[] PropertyNames { get; private set; }

        public AlsoNotifyForAttribute(params string[] args)
        {
            PropertyNames = args;
        }
    }
}