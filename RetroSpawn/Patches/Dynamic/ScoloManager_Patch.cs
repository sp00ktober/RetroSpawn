using HarmonyLib;

namespace RetroSpawn.Patches.Dynamic
{
    [HarmonyPatch(typeof(ScoloManager))]
    class ScoloManager_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ScoloManager), nameof(ScoloManager.Spawn))]
        public static bool Spawn_Prefix(ScoloManager __instance)
        {
            GeneralManager GestGen = (GeneralManager)AccessTools.Field(typeof(ScoloManager), "GestGen").GetValue(__instance);

			__instance.SpawnTimer = 0f;
			if (GestGen.DaySurvived <= 5)
			{
				__instance.SpawnScolo(1);
			}
			else if (GestGen.DaySurvived <= 15)
			{
				__instance.SpawnScolo(2);
			}
			else if (GestGen.DaySurvived <= 30)
			{
				__instance.SpawnScolo(3);
			}
			else if (GestGen.DaySurvived <= 45)
			{
				__instance.SpawnScolo(4);
			}
            else
            {
				__instance.SpawnScolo(5);
			}

			return false;
		}
    }
}
