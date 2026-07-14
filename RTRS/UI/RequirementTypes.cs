using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RoyalTitlesRoomSettings.UI
{
    public static class RequirementTypes
    {
        public static readonly List<Type> AllTypes;

        private static readonly Dictionary<Type, string> LocalizedNames = new();

        static RequirementTypes()
        {
            AllTypes = GenTypes.AllSubclasses(typeof(RoomRequirement))
                .Where(t => !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToList();

            RegisterLocalized<RoomRequirement_Area>("RTRS.ReqType_Area");
            RegisterLocalized<RoomRequirement_Impressiveness>("RTRS.ReqType_Impressiveness");
            RegisterLocalized<RoomRequirement_Thing>("RTRS.ReqType_Thing");
            RegisterLocalized<RoomRequirement_ThingCount>("RTRS.ReqType_ThingCount");
            RegisterLocalized<RoomRequirement_ThingAnyOf>("RTRS.ReqType_ThingAnyOf");
            RegisterLocalized<RoomRequirement_ThingAnyOfCount>("RTRS.ReqType_ThingAnyOfCount");
            RegisterLocalized<RoomRequirement_TerrainWithTags>("RTRS.ReqType_TerrainWithTags");
            RegisterLocalized<RoomRequirement_ForbiddenBuildings>("RTRS.ReqType_ForbiddenBuildings");
            RegisterLocalized<RoomRequirement_ForbidAltars>("RTRS.ReqType_ForbidAltars");
            RegisterLocalized<RoomRequirement_AllThingsAreGlowing>("RTRS.ReqType_AllThingsAreGlowing");
            RegisterLocalized<RoomRequirement_AllThingsAnyOfAreGlowing>("RTRS.ReqType_AllThingsAnyOfAreGlowing");
            RegisterLocalized<RoomRequirement_HasAssignedThroneAnyOf>("RTRS.ReqType_HasAssignedThroneAnyOf");

            if (Services.VFEIntegration.IsVfeLoaded)
                TryRegisterVFEETypes();
        }

        private static void TryRegisterVFEETypes()
        {
            var instrumentType = Type.GetType("VFEEmpire.RoomRequirement_InstrumentSpace, VFEEmpire");
            if (instrumentType != null)
                LocalizedNames[instrumentType] = "RTRS.ReqType_InstrumentSpace";
        }

        private static void RegisterLocalized<T>(string translationKey)
        {
            LocalizedNames[typeof(T)] = translationKey;
        }

        public static string GetLocalizedName(Type type)
        {
            if (LocalizedNames.TryGetValue(type, out var key))
            {
                string translated = key.Translate();
                if (!string.IsNullOrEmpty(translated) && translated != key)
                    return translated;
            }
            return GetFriendlyName(type);
        }

        public static string GetFriendlyName(Type type)
        {
            return type.Name
                .Replace("RoomRequirement_", "")
                .Replace("RoomRequirement", "Base");
        }

        public static string BuildTooltip(SerializableRoomRequirement req)
        {
            return RequirementDisplay.GetDetailedTooltip(req);
        }
    }
}