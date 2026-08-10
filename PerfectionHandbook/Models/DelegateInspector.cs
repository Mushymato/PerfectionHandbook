using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;
using StardewValley.Delegates;

namespace PerfectionHandbook.Models;

public static class DelegateInspector
{
    public static readonly IReadOnlyList<MethodInfo> ArgUtilityTryGetters = GetArgUtilityTryGetters();

    private static IReadOnlyList<MethodInfo> GetArgUtilityTryGetters()
    {
        List<MethodInfo> tryGetters = [];
        foreach (MethodInfo methodInfo in typeof(ArgUtility).GetMethods())
        {
            ParameterInfo[] parameterInfo = methodInfo.GetParameters();
            if (parameterInfo.Length < 3)
                continue;
            ParameterInfo secondParam = parameterInfo[1];
            if (secondParam.Name != "index" || secondParam.ParameterType != typeof(int))
            {
                continue;
            }
            ParameterInfo lastParam = parameterInfo.Last();
            if (lastParam.Name != "name" || lastParam.ParameterType != typeof(string))
            {
                continue;
            }
            tryGetters.Add(methodInfo);
        }
        return tryGetters;
    }

    private static readonly Dictionary<EventPreconditionDelegate, IReadOnlyList<string>> TryGetPairsCached = [];

    public static IReadOnlyList<string> ExtractTryGetPairs(EventPreconditionDelegate precondHandler)
    {
        if (TryGetPairsCached.TryGetValue(precondHandler, out IReadOnlyList<string>? cached))
        {
            return cached;
        }
        // TODO: identify the for loop pattern as well
        List<string> tryGetPairs = [];
        int? indexValue = null;
        KeyValuePair<OpCode, object>? previous = null;
        foreach (KeyValuePair<OpCode, object> kv in PatchProcessor.ReadMethodBody(precondHandler.Method))
        {
            if (previous?.Key == OpCodes.Ldarg_2)
            {
                indexValue = GetConstI4(kv.Key, kv.Value);
            }
            else if (kv.Key == OpCodes.Call)
            {
                if (indexValue.HasValue && previous?.Key == OpCodes.Ldstr && ArgUtilityTryGetters.Contains(kv.Value))
                {
                    while (tryGetPairs.Count < indexValue.Value)
                        tryGetPairs.Add(string.Empty);
                    tryGetPairs.Add((string)previous.Value.Value);
                }
                indexValue = null;
            }
            else if (kv.Key == OpCodes.Callvirt)
            {
                indexValue = null;
            }
            previous = kv;
        }
        return tryGetPairs;

        static int? GetConstI4(OpCode opcode, object operand)
        {
            if (opcode == OpCodes.Ldc_I4_0)
                return 0;
            if (opcode == OpCodes.Ldc_I4_1)
                return 1;
            if (opcode == OpCodes.Ldc_I4_2)
                return 2;
            if (opcode == OpCodes.Ldc_I4_3)
                return 3;
            if (opcode == OpCodes.Ldc_I4_4)
                return 4;
            if (opcode == OpCodes.Ldc_I4_5)
                return 5;
            if (opcode == OpCodes.Ldc_I4_6)
                return 6;
            if (opcode == OpCodes.Ldc_I4_7)
                return 7;
            if (opcode == OpCodes.Ldc_I4_8)
                return 8;
            if (opcode == OpCodes.Ldc_I4_S)
                return (sbyte)operand;
            return null;
        }
    }
}
