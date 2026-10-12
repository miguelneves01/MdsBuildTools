using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace MdsBuildTools
{
    [BepInPlugin(ModInfo.GUID, ModInfo.NAME, ModInfo.VERSION)]
    [BepInDependency(ModSettingsApiGuid, BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string ModSettingsApiGuid = "Edsil.ModSettingsApi";
        private const float ToastSeconds = 1.5f;

        internal static ManualLogSource Log { get; private set; } = null!;
        internal static ModConfig Cfg { get; private set; } = null!;

        private readonly Harmony _harmony = new Harmony(ModInfo.GUID);
        private string _toast = string.Empty;
        private float _toastUntil;
        private GUIStyle? _toastStyle;

        private void Awake()
        {
            Log = Logger;
            Cfg = new ModConfig(Config);

            Patches.PatchRegistry.ApplyAll(_harmony);
            TryRegisterSettingsMenu();

            Log.LogInfo($"{ModInfo.NAME} {ModInfo.VERSION} loaded.");
        }

        private void Update()
        {
            foreach (var feature in Cfg.Features)
            {
                var key = feature.ToggleKey.Value;
                if (key == KeyCode.None || !Input.GetKeyDown(key)) continue;

                var on = feature.Toggle();
                var msg = $"{feature.Label}: {(on ? "ON" : "OFF")}";
                Log.LogInfo(msg);
                ShowToast(msg);
            }
        }

        private void ShowToast(string msg)
        {
            if (!Cfg.ShowToggleToasts.Value) return;
            _toast = msg;
            _toastUntil = Time.unscaledTime + ToastSeconds;
        }

        private void OnGUI()
        {
            if (Time.unscaledTime > _toastUntil) return;

            _toastStyle ??= new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            const float w = 320f, h = 40f;
            GUI.Box(new Rect((Screen.width - w) / 2f, Screen.height * 0.12f, w, h), _toast, _toastStyle);
        }

        private void OnDestroy() => _harmony.UnpatchSelf();

        private static readonly System.Collections.Generic.HashSet<string> LoggedErrors = new System.Collections.Generic.HashSet<string>();

        /// <summary>Logs an exception from a per-frame hook only once, to avoid flooding the log.</summary>
        internal static void LogOnce(string where, System.Exception e)
        {
            if (LoggedErrors.Add(where))
                Log.LogError($"{where}: {e}");
        }

        private static void TryRegisterSettingsMenu()
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModSettingsApiGuid))
            {
                Log.LogInfo("ModSettingsAPI not installed; settings available in BepInEx/config only.");
                return;
            }
            SettingsMenu.Register(Cfg);
        }
    }
}
