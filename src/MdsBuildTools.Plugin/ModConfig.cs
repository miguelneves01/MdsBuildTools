using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace MdsBuildTools
{
    /// <summary>
    /// All user-facing settings, persisted by BepInEx in BepInEx/config/com.miguelneves.mdsbuildtools.cfg.
    /// Every feature has an Enabled flag and a hotkey to toggle it in-game (set the key to None to disable the hotkey).
    /// </summary>
    internal sealed class ModConfig
    {
        public sealed class Feature
        {
            public Feature(string label, ConfigEntry<bool> enabled, ConfigEntry<KeyCode> toggleKey)
            {
                Label = label;
                Enabled = enabled;
                ToggleKey = toggleKey;
            }

            public string Label { get; }
            public ConfigEntry<bool> Enabled { get; }
            public ConfigEntry<KeyCode> ToggleKey { get; }

            /// <summary>Flips the feature; returns the new state.</summary>
            public bool Toggle() => Enabled.Value = !Enabled.Value;
        }

        public Feature Grid { get; }
        public Feature Align { get; }
        public Feature Floor { get; }
        public IReadOnlyList<Feature> Features { get; }

        public ConfigEntry<float> GridStepCm { get; }
        public ConfigEntry<float> AngleStepDeg { get; }
        public ConfigEntry<float> AlignThresholdCm { get; }
        public ConfigEntry<KeyCode> BypassKey { get; }
        public ConfigEntry<bool> ShowToggleToasts { get; }

        public ModConfig(ConfigFile cfg)
        {
            Grid = new Feature("Grid snapping",
                cfg.Bind("Grid", "Enabled", true, "Snap object position and rotation to a grid while placing."),
                cfg.Bind("Grid", "ToggleKey", KeyCode.G, "Toggle grid snapping on/off."));
            GridStepCm = cfg.Bind("Grid", "StepCm", 5f,
                new ConfigDescription("Grid step in centimetres.", new AcceptableValueRange<float>(0.5f, 100f)));
            AngleStepDeg = cfg.Bind("Grid", "AngleStepDeg", 15f,
                new ConfigDescription("Rotation snap step in degrees.", new AcceptableValueRange<float>(1f, 90f)));

            Align = new Feature("Alignment",
                cfg.Bind("Alignment", "Enabled", true, "Snap edges/centres to nearby objects and walls."),
                cfg.Bind("Alignment", "ToggleKey", KeyCode.H, "Toggle alignment on/off."));
            AlignThresholdCm = cfg.Bind("Alignment", "ThresholdCm", 3f,
                new ConfigDescription("Max distance at which alignment kicks in.", new AcceptableValueRange<float>(0.5f, 20f)));

            Floor = new Feature("Keep above floor",
                cfg.Bind("Floor", "Enabled", true, "Push objects that sink into the floor back onto its surface."),
                cfg.Bind("Floor", "ToggleKey", KeyCode.J, "Toggle floor correction on/off."));


            BypassKey = cfg.Bind("General", "BypassKey", KeyCode.LeftAlt, "Hold to temporarily disable grid snapping and alignment.");
            ShowToggleToasts = cfg.Bind("General", "ShowToggleToasts", true, "Show a short on-screen message when a feature is toggled.");

            Features = new[] { Grid, Align, Floor };
        }

        public float GridStepMetres => GridStepCm.Value / 100f;
        public float AlignThresholdMetres => AlignThresholdCm.Value / 100f;

        private bool Bypassed => Input.GetKey(BypassKey.Value);
        public bool SnapActive => Grid.Enabled.Value && !Bypassed;
        public bool AlignActive => Align.Enabled.Value && !Bypassed;
    }
}
