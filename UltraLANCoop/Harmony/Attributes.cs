namespace UltraLANCoop.Harmony
{
    using HarmonyLib;
    using System;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Patch : Attribute
    {
        private Type type;
        private string name;
        private Type[] args;
        private MethodType? sign;

        public MethodBase Target => sign switch
        {
            MethodType.Constructor => AccessTools.Constructor(type, args),
            MethodType.Getter => AccessTools.PropertyGetter(type, name),
            MethodType.Setter => AccessTools.PropertySetter(type, name),
            _ => AccessTools.Method(type, name, args),
        };

        public Patch(Type type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public Patch(Type type, string name, params Type[] args) : this(type, name)
        {
            this.args = args;
        }

        public Patch(Type type, string name, MethodType sign) : this(type, name)
        {
            this.sign = sign;
        }

        public HarmonyMethod GetPatch(MethodInfo method) => new HarmonyMethod(method, Priority.High) { methodType = sign };
    }

    public class DynamicPatch : Patch
    {
        public DynamicPatch(Type type, string name) : base(type, name) { }
        public DynamicPatch(Type type, string name, params Type[] args) : base(type, name, args) { }
        public DynamicPatch(Type type, string name, MethodType sign) : base(type, name, sign) { }
    }

    public class StaticPatch : Patch
    {
        public StaticPatch(Type type, string name) : base(type, name) { }
        public StaticPatch(Type type, string name, params Type[] args) : base(type, name, args) { }
        public StaticPatch(Type type, string name, MethodType sign) : base(type, name, sign) { }
    }

    public class Prefix : Attribute { }
    public class Postfix : Attribute { }
    public class Transpiler : Attribute { }
}