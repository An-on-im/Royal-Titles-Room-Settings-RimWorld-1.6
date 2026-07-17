using RoyalTitlesRoomSettings.Services;
using RoyalTitlesRoomSettings.Workers;
using UnityEngine;
using Verse;

namespace RoyalTitlesRoomSettings
{
    public class RTRSMod : Mod
    {
        public static SettingsManager Settings;

        public RTRSMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SettingsManager>();
        }

        public override string SettingsCategory()
        {
            return "RTRS.SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            RTRSSettingsGUI.Draw(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            RequirementsApplier.PatchAll();
        }
    }

    [StaticConstructorOnStartup]
    public static class ModBootstrap
    {
        static ModBootstrap()
        {
            RequirementsApplier.PatchAll();
        }
    }
}