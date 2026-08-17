using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Delegates;

namespace PerfectionHandbook.Models;

public sealed record ArgGetInfo(
    IReadOnlyList<(int, Type, string)> FixedIndex,
    IReadOnlyList<(int, int, int, Type, string)> LoopIndex,
    bool Invert = false
)
{
    public string FormArgDesc(bool negated, string[] args)
    {
        if (Invert)
            negated = !negated;
        StringBuilder sb = new();
        if (negated)
            sb.Append(" NOT");
        for (int i = 1; i < args.Length; i++)
        {
            sb.Append(' ');
            sb.Append(args[i]);
            if (TryMatchFixedIndex(sb, i))
                continue;
            if (TryMatchLoopIndex(sb, i))
                continue;
        }
        return sb.ToString();
    }

    private bool TryMatchFixedIndex(StringBuilder sb, int i)
    {
        foreach ((int idx, Type typ, string name) in FixedIndex)
        {
            int delta = i - idx;
            if (delta < 0)
                continue;
            if (delta == 0 || (typ == typeof(Point) && delta < 2) || (typ == typeof(Rectangle) && delta < 4))
            {
                sb.Append('(');
                sb.Append(name);
                sb.Append(')');
                return true;
            }
        }
        return false;
    }

    private bool TryMatchLoopIndex(StringBuilder sb, int i)
    {
        foreach ((int start, int step, int offset, Type typ, string name) in LoopIndex)
        {
            int delta = (i - start) % step - offset;
            if (delta < 0)
                continue;
            if (delta == 0 || (typ == typeof(Point) && delta < 2) || (typ == typeof(Rectangle) && delta < 4))
            {
                sb.Append('(');
                sb.Append(name);
                sb.Append(' ');
                sb.Append('[');
                sb.Append((i - start) / step);
                sb.Append(']');
                sb.Append(')');
                return true;
            }
        }
        return false;
    }

    internal void LogRepr()
    {
        foreach ((int idx, Type typ, string name) in FixedIndex)
        {
            ModEntry.Log($"{idx}: {typ} {name}");
        }
        foreach ((int start, int step, int offset, Type typ, string name) in LoopIndex)
        {
            ModEntry.Log($"({start}, {step}, {offset}): {typ} {name}");
        }
    }
}

public static class DelegateInspector
{
    public static readonly IReadOnlyDictionary<MethodInfo, Type> ArgUtilityTryGetters = GetArgUtilityTryGetters();

    private static IReadOnlyDictionary<MethodInfo, Type> GetArgUtilityTryGetters()
    {
        Dictionary<MethodInfo, Type> tryGetters = [];
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
            ParameterInfo thirdParam = parameterInfo[2];
            if (!thirdParam.IsOut)
                continue;
            ParameterInfo lastParam = parameterInfo.Last();
            if (lastParam.Name != "name" || lastParam.ParameterType != typeof(string))
                continue;
            tryGetters[methodInfo] = thirdParam.ParameterType.GetElementType() ?? thirdParam.ParameterType;
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
        IList<CodeInstruction> methodBody = PatchProcessor.GetOriginalInstructions(precondHandler.Method).ToList();

        // handle obsolete Not* -> * situations
        if (
            precondHandler.Method.Name.StartsWith("Not")
            && precondHandler.Method.GetCustomAttribute(typeof(ObsoleteAttribute)) != null
        )
        {
            string notless = precondHandler.Method.Name[3..];
            if (Event.TryGetPreconditionHandler(notless, out EventPreconditionDelegate handler))
            {
                ArgGetInfo notlessArgGet = ExtractTryGetPairs(handler);
                return new(notlessArgGet.FixedIndex, notlessArgGet.LoopIndex, true);
            }
        }

        // TODO: actually eval stack eventually :)
        bool hasLoop = TryFindForLoop(methodBody, out CodeInstruction? forLoopLdloc, out int? start, out int? step);

        List<(int, Type, string)> fixedIndex = [];
        List<(int, int, int, Type, string)> loopIndex = [];
        int? indexValue = null;
        int? loopIndexOffset = null;
        for (int i = 1; i < methodBody.Count; i++)
        {
            CodeInstruction previous = methodBody[i - 1];
            CodeInstruction current = methodBody[i];

            if (previous.opcode == OpCodes.Ldarg_2)
            {
                if (forLoopLdloc != null && methodBody.Count > i + 2)
                {
                    if (forLoopLdloc.opcode == current.opcode && forLoopLdloc.operand == current.operand)
                    {
                        if (GetConstI4(methodBody[i + 1]) is int maybeLoopIndex)
                        {
                            CodeInstruction nextnext = methodBody[i + 2];
                            if (nextnext.opcode == OpCodes.Add)
                            {
                                loopIndexOffset = maybeLoopIndex;
                            }
                            else if (nextnext.opcode == OpCodes.Sub)
                            {
                                loopIndexOffset = maybeLoopIndex;
                            }
                        }
                        else
                        {
                            loopIndexOffset = 0;
                        }
                    }
                }
                else
                {
                    indexValue = GetConstI4(current);
                }
            }
            else if (current.opcode == OpCodes.Call)
            {
                if (
                    previous.opcode != OpCodes.Ldstr
                    || current.operand is not MethodInfo method
                    || !ArgUtilityTryGetters.TryGetValue(method, out Type? type)
                )
                    continue;
                if (hasLoop && loopIndexOffset.HasValue)
                {
                    loopIndex.Add((start!.Value, step!.Value, loopIndexOffset.Value, type, (string)previous.operand));
                }
                else if (indexValue.HasValue)
                {
                    fixedIndex.Add((indexValue.Value, type, (string)previous.operand));
                }
                indexValue = null;
            }
            else if (current.opcode == OpCodes.Callvirt)
            {
                indexValue = null;
            }
        }

        return new(fixedIndex, loopIndex);

        static bool TryFindForLoop(
            IList<CodeInstruction> methodBody,
            [NotNullWhen(true)] out CodeInstruction? ldloc,
            [NotNullWhen(true)] out int? start,
            [NotNullWhen(true)] out int? step
        )
        {
            ldloc = null;
            start = null;
            step = null;
            CodeInstruction? stloc = null;
            for (int i = 0; i < methodBody.Count; i++)
            {
                CodeInstruction current = methodBody[i];
                if (current.opcode == OpCodes.Br_S && current.operand is Label label)
                {
                    for (int j = 0; j < methodBody.Count - 5; j++)
                    {
                        if (methodBody[j].labels.Contains(label) && methodBody[j].IsLdloc())
                        {
                            // IL_0059: ldloc.0
                            // IL_005a: ldarg.2
                            // IL_005b: ldlen
                            // IL_005c: conv.i4
                            // IL_005d: blt.s IL_0004
                            if (methodBody[j + 2].opcode == OpCodes.Ldlen)
                            {
                                ldloc = methodBody[j];
                            }
                            // IL_0055: ldloc.0
                            // IL_0056: ldc.i4.2
                            // IL_0057: add
                            // IL_0058: stloc.0
                            if (methodBody[j - 1].IsStloc())
                            {
                                stloc = methodBody[j - 1];
                            }
                            if (methodBody[j - 2].opcode == OpCodes.Add && GetConstI4(methodBody[j - 3]) is int stepVal)
                            {
                                step = stepVal;
                            }
                            goto finish;
                        }
                    }
                }
            }
            finish:
            if (stloc == null)
            {
                return false;
            }
            for (int i = 1; i < methodBody.Count; i++)
            {
                if (methodBody[i].opcode == stloc.opcode && methodBody[i].operand == stloc.operand)
                {
                    start = GetConstI4(methodBody[i - 1]);
                    break;
                }
            }
            return ldloc != null && start != null && step != null;
        }

        static int? GetConstI4(CodeInstruction inst)
        {
            OpCode opcode = inst.opcode;
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
                return (sbyte)inst.operand;
            return null;
        }
    }
}
