using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using RoyalTitlesRoomSettings.Services;
using Verse;

namespace RoyalTitlesRoomSettings.Workers
{
    public static class RequirementsApplier
    {
        public static void PatchAll()
        {
            OriginalRequirementsCache.EnsureCache();
            var settings = SettingsManager.Instance;
            if (settings == null)
                return;

            int applied = 0;
            foreach (var title in DefDatabase<RoyalTitleDef>.AllDefsListForReading)
            {
                if (title.seniority <= 0)
                    continue;

                SerializableTitleRequirements source;
                if (settings.TryGetTitleData(title.defName, out var overridden))
                    source = overridden;
                else
                    source = OriginalRequirementsCache.GetOriginal(title.defName);

                if (source == null)
                    continue;

                title.bedroomRequirements = RequirementConverter.FromDtoList(source.BedroomRequirements);
                title.throneRoomRequirements = RequirementConverter.FromDtoList(source.ThroneRoomRequirements);

                if (VFEIntegration.IsVfeLoaded)
                    VFEIntegration.SetVfeExtensions(title, source);

                applied++;
            }

            Log.Message($"[RTRS] PatchAll: patched {applied} titles.");
        }

        public static void PatchSingle(RoyalTitleDef title)
        {
            if (title == null || title.seniority <= 0)
                return;
            OriginalRequirementsCache.EnsureCache();
            var settings = SettingsManager.Instance;
            if (settings == null)
                return;

            SerializableTitleRequirements source;
            if (settings.TryGetTitleData(title.defName, out var overridden))
                source = overridden;
            else
                source = OriginalRequirementsCache.GetOriginal(title.defName);

            if (source == null)
                return;

            title.bedroomRequirements = RequirementConverter.FromDtoList(source.BedroomRequirements);
            title.throneRoomRequirements = RequirementConverter.FromDtoList(source.ThroneRoomRequirements);

            if (VFEIntegration.IsVfeLoaded)
                VFEIntegration.SetVfeExtensions(title, source);

            Log.Message($"[RTRS] PatchSingle: patched {title.defName}.");
        }
    }
}