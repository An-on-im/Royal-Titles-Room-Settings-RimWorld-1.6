using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RoyalTitlesRoomSettings.Services
{
    public static class OriginalRequirementsCache
    {
        private static Dictionary<string, SerializableTitleRequirements> _originalCache;
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;

        public static SerializableTitleRequirements GetOriginal(string titleDefName)
        {
            EnsureCache();
            if (_originalCache.TryGetValue(titleDefName, out var dto))
                return dto.DeepClone();
            return null;
        }

        public static void ResetTitleToOriginal(string titleDefName)
        {
            var settings = SettingsManager.Instance;
            if (settings != null)
                settings.RemoveOverride(titleDefName);
        }

        public static void EnsureCache()
        {
            if (_isInitialized)
                return;
            lock (_lock)
            {
                if (_isInitialized)
                    return;
                BuildCache();
                _isInitialized = true;
            }
        }

        private static void BuildCache()
        {
            _originalCache = new Dictionary<string, SerializableTitleRequirements>();
            var titles = DefDatabase<RoyalTitleDef>.AllDefsListForReading.Where(t => t.seniority > 0);
            foreach (var title in titles)
            {
                var dto = new SerializableTitleRequirements
                {
                    TitleDefName = title.defName
                };

                dto.BedroomRequirements = RequirementConverter.ToDtoList(title.bedroomRequirements);
                dto.ThroneRoomRequirements = RequirementConverter.ToDtoList(title.throneRoomRequirements);

                if (VFEIntegration.IsVfeLoaded)
                {
                    var extDto = VFEIntegration.GetVfeExtensions(title);
                    if (extDto != null)
                    {
                        dto.BallroomRequirements = extDto.BallroomRequirements;
                        dto.GalleryRequirements = extDto.GalleryRequirements;
                        dto.CourtRequirements = extDto.CourtRequirements;
                    }
                }

                _originalCache[title.defName] = dto;
            }

            Log.Message($"[RTRS] OriginalRequirementsCache built for {_originalCache.Count} titles.");
        }

        public static void InvalidateCache()
        {
            lock (_lock)
            {
                _originalCache = null;
                _isInitialized = false;
            }
        }
    }
}