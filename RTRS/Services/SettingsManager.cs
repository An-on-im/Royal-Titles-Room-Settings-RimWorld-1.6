using RoyalTitlesRoomSettings.DTO;
using System.Collections.Generic;
using Verse;

namespace RoyalTitlesRoomSettings.Services
{
    public class SettingsManager : ModSettings
    {
        private Dictionary<string, SerializableTitleRequirements> _overrides = new();
        private List<string> _tmpKeys = new();
        private List<SerializableTitleRequirements> _tmpValues = new();

        public static SettingsManager Instance
        {
            get; private set;
        }

        public SettingsManager()
        {
            Instance = this;
        }

        public bool TryGetTitleData(string titleDefName, out SerializableTitleRequirements data)
        {
            return _overrides.TryGetValue(titleDefName, out data);
        }

        public List<SerializableRoomRequirement> GetRequirements(string titleDefName, string roomId)
        {
            if (_overrides.TryGetValue(titleDefName, out var overridden))
                return GetRoomList(overridden, roomId);

            var original = OriginalRequirementsCache.GetOriginal(titleDefName);
            if (original != null)
                return GetRoomList(original, roomId);

            return new List<SerializableRoomRequirement>();
        }

        public List<SerializableCourtRequirement> GetCourt(string titleDefName)
        {
            if (_overrides.TryGetValue(titleDefName, out var overridden))
                return overridden.CourtRequirements;

            var original = OriginalRequirementsCache.GetOriginal(titleDefName);
            return original?.CourtRequirements ?? new List<SerializableCourtRequirement>();
        }

        public SerializableTitleRequirements EnsureOverride(string titleDefName)
        {
            if (_overrides.TryGetValue(titleDefName, out var existing))
                return existing;

            var original = OriginalRequirementsCache.GetOriginal(titleDefName);
            var newDto = original?.DeepClone() ?? new SerializableTitleRequirements { TitleDefName = titleDefName };
            _overrides[titleDefName] = newDto;
            return newDto;
        }

        public void RemoveOverride(string titleDefName)
        {
            _overrides.Remove(titleDefName);
        }

        public void ResetAll()
        {
            _overrides.Clear();
        }

        public bool IsTitleModified(string titleDefName)
        {
            return _overrides.ContainsKey(titleDefName);
        }

        private static List<SerializableRoomRequirement> GetRoomList(SerializableTitleRequirements dto, string roomId)
        {
            switch (roomId)
            {
                case "bedroom":
                    return dto.BedroomRequirements;
                case "throneRoom":
                    return dto.ThroneRoomRequirements;
                case "ballroom":
                    return dto.BallroomRequirements;
                case "gallery":
                    return dto.GalleryRequirements;
                default:
                    return new List<SerializableRoomRequirement>();
            }
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref _overrides, "overrides", LookMode.Value, LookMode.Deep, ref _tmpKeys, ref _tmpValues);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _overrides ??= new Dictionary<string, SerializableTitleRequirements>();
                _tmpKeys ??= new List<string>();
                _tmpValues ??= new List<SerializableTitleRequirements>();
            }
        }
    }
}