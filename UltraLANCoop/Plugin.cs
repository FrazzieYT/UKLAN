using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UltraLANCoop.Network;
using UltraLANCoop.UI;
using UnityEngine;

namespace UltraLANCoop
{
    [BepInPlugin("com.frazzie.ultralancoop", "UltraLANCoop", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }
        internal new ManualLogSource Logger => base.Logger;

        private MainMenu mainMenu;
        private GameObject menuObject;
        private GameObject eventsObject;

        private void Awake()
        {
            Instance = this;
            Logger.LogInfo("[UltraLANCoop] Loading v1.0.0...");

            try
            {
                NetCore.Load();
                Logger.LogInfo("[UltraLANCoop] NetCore initialized");

                eventsObject = new GameObject("[UltraLANCoop] Events");
                eventsObject.AddComponent<Events>();
                DontDestroyOnLoad(eventsObject);
                Logger.LogInfo("[UltraLANCoop] Events initialized");

                Harmony.Patches.Load();
                Harmony.Patches.LoadStatic();
                Logger.LogInfo("[UltraLANCoop] Harmony patches system loaded");

                CreateMenu();

                Logger.LogInfo("[UltraLANCoop] Loaded successfully");
            }
            catch (System.Exception e)
            {
                Logger.LogError("[UltraLANCoop] Failed to load: " + e.Message);
                Logger.LogError(e.StackTrace);
            }
        }

        private void CreateMenu()
        {
            if (menuObject != null)
            {
                Destroy(menuObject);
            }

            menuObject = new GameObject("[UltraLANCoop] MainMenu");
            mainMenu = menuObject.AddComponent<MainMenu>();
            DontDestroyOnLoad(menuObject);
            Logger.LogInfo("[UltraLANCoop] MainMenu created");
        }

        private void Update()
        {
            try
            {
                NetCore.Update();

                if (mainMenu == null || menuObject == null)
                {
                    CreateMenu();
                }

                if (Input.GetKeyDown(KeyCode.F1))
                {
                    if (mainMenu != null)
                    {
                        mainMenu.Toggle();
                    }
                }
            }
            catch (System.Exception e)
            {
                Logger.LogError("[UltraLANCoop] Update error: " + e.Message);
            }
        }

        private void OnDestroy()
        {
            NetCore.Unload();
        }
    }
}