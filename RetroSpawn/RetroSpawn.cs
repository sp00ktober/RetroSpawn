using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace RetroSpawn
{
    [BepInPlugin("com.sp00ktober.RetroSpawn", "RetroSpawn", "0.0.1")]
    public class RetroSpawn : BaseUnityPlugin
    {
        private void Awake()
        {
            InitPatches();
        }

        private static void InitPatches()
        {
            Debug.Log("Patching Starsand...");

            try
            {
                Debug.Log("Applying patches from RetroSpawn");
#if DEBUG
                if (Directory.Exists("./mmdump"))
                {
                    foreach (FileInfo file in new DirectoryInfo("./mmdump").GetFiles())
                    {
                        file.Delete();
                    }

                    Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "cecil");
                    Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "./mmdump");
                }
#endif
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.sp00ktober.RetroSpawn");
#if DEBUG
                Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "");
#endif

                Debug.Log("Patching completed successfully");
            }
            catch (Exception ex)
            {
                Debug.Log("Unhandled exception occurred while patching the game: " + ex);
            }
        }
    }
}
