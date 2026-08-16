using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewValley;
using StardewValley.Delegates;

namespace PerfectionHandbook.Models;

public sealed record ArgGetInfo(IReadOnlyList<string> FixedIndex, IReadOnlyList<(int, int, string)> LoopIndex);

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
            ParameterInfo firstParam = parameterInfo[0];
            if (firstParam.Name != "array")
                continue;
            Type firstParamType = firstParam.GetType();
            if (firstParamType.IsGenericType) { }
            ParameterInfo secondParam = parameterInfo[1];
            if (secondParam.Name != "index" || secondParam.ParameterType != typeof(int))
                continue;
            ParameterInfo lastParam = parameterInfo.Last();
            if (lastParam.Name != "name" || lastParam.ParameterType != typeof(string))
                continue;
            tryGetters.Add(methodInfo);
        }
        return tryGetters;
    }

    private static readonly Dictionary<EventPreconditionDelegate, ArgGetInfo> TryGetInfoCached = [];

    public static ArgGetInfo ExtractTryGetPairs(EventPreconditionDelegate precondHandler)
    {
        if (!TryGetInfoCached.TryGetValue(precondHandler, out ArgGetInfo? cached))
        {
            cached = InnerExtractTryGetPairs(precondHandler);
            TryGetInfoCached[precondHandler] = cached;
        }
        return cached;
    }

    private static ArgGetInfo InnerExtractTryGetPairs(EventPreconditionDelegate precondHandler)
    {
        // pass 1
        List<string> fixedIndex = [];
        int? indexValue = null;
        IList<KeyValuePair<OpCode, object>> methodBody = PatchProcessor.ReadMethodBody(precondHandler.Method).ToList();
        for (int i = 1; i < methodBody.Count; i++)
        {
            KeyValuePair<OpCode, object> previous = methodBody[i - 1];
            KeyValuePair<OpCode, object> current = methodBody[i];

            if (previous.Key == OpCodes.Ldarg_2)
            {
                indexValue = GetConstI4(current.Key, current.Value);
            }
            else if (current.Key == OpCodes.Call)
            {
                if (
                    indexValue.HasValue
                    && previous.Key == OpCodes.Ldstr
                    && ArgUtilityTryGetters.Contains(current.Value)
                )
                {
                    while (fixedIndex.Count < indexValue.Value)
                        fixedIndex.Add(string.Empty);
                    fixedIndex.Add((string)previous.Value);
                }
                indexValue = null;
            }
            else if (current.Key == OpCodes.Callvirt)
            {
                indexValue = null;
            }
        }
        return new(fixedIndex, []);

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
