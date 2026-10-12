using System.Globalization;
using BepInEx.Configuration;
using ModSettingsApi.Api;
using ModSettingsApi.Models;
using ModSettingsApi.Models.Variants;

namespace MdsBuildTools
{
    /// <summary>Registers an in-game "MdsBuildTools" tab via ModSettingsAPI. Values write straight to the BepInEx config.</summary>
    internal static class SettingsMenu
    {
        public static void Register(ModConfig cfg)
        {
            var tab = new TabModel(ModInfo.NAME, new IVariant[]
            {
                Toggle(cfg.Grid),
                FloatBox("Grid step (cm)", cfg.GridStepCm),
                FloatBox("Rotation step (°)", cfg.AngleStepDeg),
                Toggle(cfg.Align),
                FloatBox("Alignment distance (cm)", cfg.AlignThresholdCm),
                Toggle(cfg.Floor),
                Toggle(cfg.Walls),
                new ToggleButtonVariant("Show toggle messages", v => cfg.ShowToggleToasts.Value = v, cfg.ShowToggleToasts.Value),
            });

            ModdedSettingsApi.AddMod(tab);
        }

        private static ToggleButtonVariant Toggle(ModConfig.Feature f) =>
            new ToggleButtonVariant(f.Label, v => f.Enabled.Value = v, f.Enabled.Value);

        private static TextBoxVariant FloatBox(string label, ConfigEntry<float> entry)
        {
            return new TextBoxVariant(label, ((float)entry.DefaultValue).ToString(CultureInfo.InvariantCulture), text =>
            {
                // ConfigEntry clamps to its AcceptableValueRange on assignment.
                if (float.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                    entry.Value = v;
            }, entry.Value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
