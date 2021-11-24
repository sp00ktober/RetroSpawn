using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace RetroSpawn.Patches.Transpilers
{
    [HarmonyPatch(typeof(ScoloManager))]
    class ScoloManager_Transpiler
    {
        // This patch removes the part that deletes exceeding scolos preventing them to stack over the max scolo number over time.
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ScoloManager), nameof(ScoloManager.SpawnScolo))]
        public static IEnumerable<CodeInstruction> SpawnScolo_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int start = -1, stop = -1;

            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ScoloManager), nameof(ScoloManager.ExtraScoloNum))),
                    new CodeMatch(OpCodes.Ldarg_1),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ScoloManager), nameof(ScoloManager.ScoloSpawned))),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_Count"),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Stloc_1)
                    );

            if (matcher.IsInvalid)
            {
                Debug.Log("ScoloManager_Transpiler: failed to find start injection point, not patching!");
                return instructions;
            }

            start = matcher.Pos;

            matcher.MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Stfld, AccessTools.Field(typeof(ScoloManager), nameof(ScoloManager.LastPlayerPosition))),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo)i.operand).Name == "GetNearestSpawnIndex")
                );

            if (matcher.IsInvalid)
            {
                Debug.Log("ScoloManager_Transpiler: failed to find stop injection point, not patching!");
                return instructions;
            }

            stop = matcher.Pos;

            // now start snipping
            matcher.Start();
            matcher.Advance(start + 1);
            for(; matcher.Pos < stop;)
            {
                matcher.SetAndAdvance(OpCodes.Nop, null);
            }

            return matcher.InstructionEnumeration();
        }

        // remove the part that allows scolo spawning only every second day
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(ScoloManager), nameof(ScoloManager.CheckSpawn))]
        public static IEnumerable<CodeInstruction> CheckSpawn_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
#pragma warning disable Harmony003 // Harmony non-ref patch parameters modified
            instructions = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ScoloManager), "GestGen")),
                    new CodeMatch(i => i.opcode == OpCodes.Callvirt && ((MethodInfo)i.operand).Name == "get_DaySurvived"),
                    new CodeMatch(OpCodes.Ldc_I4_2),
                    new CodeMatch(OpCodes.Rem)
                    )
                .SetAndAdvance(OpCodes.Nop, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .SetAndAdvance(OpCodes.Nop, null)
                .InstructionEnumeration();
#pragma warning restore Harmony003 // Harmony non-ref patch parameters modified

            return instructions;
        }
    }
}
