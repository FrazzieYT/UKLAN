namespace UltraLANCoop.Harmony
{
    using HarmonyLib;
    using System;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using UltraLANCoop.Network;

    public static class Patches
    {
        public static HarmonyLib.Harmony Dynamic, Static;
        public static bool Patched;

        public static void Load()
        {
            NetCore.OnServerStarted += ApplyDynamicPatches;
            NetCore.OnClientConnected += ApplyDynamicPatches;
            NetCore.OnDisconnected += RemoveDynamicPatches;
        }

        private static void ApplyDynamicPatches()
        {
            Dynamic ??= new HarmonyLib.Harmony("com.frazzie.ultralancoop.dynamic");

            if (NetCore.IsConnected && !Patched)
            {
                CustomAttributes((m, attrs) => ApplyCustom<DynamicPatch>(m, attrs, Dynamic));
                StandardHarmonyPatches(Dynamic, isDynamic: true);
                Patched = true;
                Debug.Log($"[HARM] Applied {Dynamic.GetPatchedMethods().Count()} dynamic patches");
            }

            if (!NetCore.IsConnected && Patched)
            {
                Dynamic.UnpatchSelf();
                Patched = false;
                Debug.Log("[HARM] Unapplied all dynamic patches");
            }
        }

        private static void RemoveDynamicPatches()
        {
            if (!Patched) return;
            Dynamic?.UnpatchSelf();
            Patched = false;
            Debug.Log("[HARM] Unapplied all dynamic patches");
        }

        public static void LoadStatic()
        {
            Static ??= new HarmonyLib.Harmony("com.frazzie.ultralancoop.static");
            CustomAttributes((m, attrs) => ApplyCustom<StaticPatch>(m, attrs, Static));
            StandardHarmonyPatches(Static, isDynamic: false);
            Debug.Log($"[HARM] Applied {Static.GetPatchedMethods().Count()} static patches");
        }

        private static void CustomAttributes(Action<MethodInfo, Patch[]> action)
        {
            var assembly = Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var attrs = method.GetCustomAttributes(typeof(Patch), false).Cast<Patch>().ToArray();
                    if (attrs.Length > 0) action(method, attrs);
                }
            }
        }

        private static void ApplyCustom<T>(MethodInfo method, Patch[] attrs, HarmonyLib.Harmony harmony) where T : Patch
        {
            foreach (var attr in attrs.Where(a => a is T))
            {
                var target = attr.Target;
                if (target == null)
                {
                    Debug.LogWarning($"[HARM] Target is null for {method.Name}");
                    continue;
                }

                HarmonyMethod prefix = null, postfix = null, transpiler = null;

                if (method.GetCustomAttribute<Prefix>() != null)
                    prefix = attr.GetPatch(method);
                if (method.GetCustomAttribute<Postfix>() != null)
                    postfix = attr.GetPatch(method);
                if (method.GetCustomAttribute<Transpiler>() != null)
                    transpiler = attr.GetPatch(method);

                try
                {
                    harmony.Patch(target, prefix, postfix, transpiler);
                    Debug.Log($"[HARM] Patched: {target.DeclaringType?.Name}.{target.Name} <- {method.Name}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[HARM] Failed to patch {target.DeclaringType?.Name}.{target.Name}: {e.Message}");
                }
            }
        }

        private static void StandardHarmonyPatches(HarmonyLib.Harmony harmony, bool isDynamic)
        {
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                var classPatchAttr = type.GetCustomAttribute<HarmonyPatch>();
                if (classPatchAttr == null) continue;

                if (type.GetCustomAttribute<DynamicPatch>() != null || type.GetCustomAttribute<StaticPatch>() != null)
                    continue;

                Type targetDeclaringType = null;
                string targetMethodName = null;
                Type[] targetArgs = null;
                MethodType? methodType = null;

                var attrCtorArgs = GetHarmonyPatchArgs(classPatchAttr);
                if (attrCtorArgs.declaringType != null)
                {
                    targetDeclaringType = attrCtorArgs.declaringType;
                    targetMethodName = attrCtorArgs.methodName;
                    targetArgs = attrCtorArgs.args;
                    methodType = attrCtorArgs.methodType;
                }

                if (targetDeclaringType == null)
                {
                    Debug.LogWarning($"[HARM] HarmonyPatch on {type.Name} has no target type");
                    continue;
                }

                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    HarmonyMethod prefix = null, postfix = null, transpiler = null;

                    if (method.GetCustomAttribute<HarmonyPrefix>() != null)
                        prefix = new HarmonyMethod(method);
                    if (method.GetCustomAttribute<HarmonyPostfix>() != null)
                        postfix = new HarmonyMethod(method);
                    if (method.GetCustomAttribute<HarmonyTranspiler>() != null)
                        transpiler = new HarmonyMethod(method);

                    if (prefix == null && postfix == null && transpiler == null)
                        continue;

                    MethodBase target = null;

                    var methodPatchAttr = method.GetCustomAttribute<HarmonyPatch>();
                    if (methodPatchAttr != null)
                    {
                        var methodArgs = GetHarmonyPatchArgs(methodPatchAttr);
                        if (methodArgs.declaringType != null)
                        {
                            target = ResolveTarget(methodArgs.declaringType, methodArgs.methodName, methodArgs.args, methodArgs.methodType);
                        }
                    }

                    if (target == null)
                    {
                        target = ResolveTarget(targetDeclaringType, targetMethodName, targetArgs, methodType);
                    }

                    if (target == null)
                    {
                        Debug.LogWarning($"[HARM] Could not resolve target for {type.Name}.{method.Name}");
                        continue;
                    }

                    try
                    {
                        harmony.Patch(target, prefix, postfix, transpiler);
                        Debug.Log($"[HARM] Standard patched: {target.DeclaringType?.Name}.{target.Name} <- {type.Name}.{method.Name}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[HARM] Failed to standard patch {target.DeclaringType?.Name}.{target.Name}: {e.Message}");
                    }
                }
            }
        }

        private static MethodBase ResolveTarget(Type declaringType, string methodName, Type[] args, MethodType? methodType)
        {
            if (string.IsNullOrEmpty(methodName))
                return null;

            try
            {
                switch (methodType)
                {
                    case MethodType.Constructor:
                        return args != null ? AccessTools.Constructor(declaringType, args) : AccessTools.Constructor(declaringType);
                    case MethodType.Getter:
                        return AccessTools.PropertyGetter(declaringType, methodName);
                    case MethodType.Setter:
                        return AccessTools.PropertySetter(declaringType, methodName);
                    default:
                        return args != null
                            ? AccessTools.Method(declaringType, methodName, args)
                            : AccessTools.Method(declaringType, methodName);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[HARM] ResolveTarget failed for {declaringType.Name}.{methodName}: {e.Message}");
                return null;
            }
        }

        private static (Type declaringType, string methodName, Type[] args, MethodType? methodType) GetHarmonyPatchArgs(HarmonyPatch attr)
        {
            Type declaringType = null;
            string methodName = null;
            Type[] args = null;
            MethodType? methodType = null;

            var infoField = typeof(HarmonyPatch).GetField("info", BindingFlags.Public | BindingFlags.Instance);
            if (infoField != null)
            {
                var info = infoField.GetValue(attr);
                if (info != null)
                {
                    var infoType = info.GetType();
                    var dtProp = infoType.GetField("declaringType");
                    var mnProp = infoType.GetField("methodName");
                    var arProp = infoType.GetField("argumentTypes");
                    var mtProp = infoType.GetField("methodType");

                    declaringType = dtProp?.GetValue(info) as Type;
                    methodName = mnProp?.GetValue(info) as string;
                    args = arProp?.GetValue(info) as Type[];
                    if (mtProp?.GetValue(info) is MethodType mt) methodType = mt;
                }
            }

            return (declaringType, methodName, args, methodType);
        }
    }
}