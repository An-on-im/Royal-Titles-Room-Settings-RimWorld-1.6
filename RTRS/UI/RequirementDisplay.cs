using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RoyalTitlesRoomSettings.UI
{
    public static class RequirementDisplay
    {
        public static string GetShortDescription(SerializableRoomRequirement req)
        {
            if (req == null)
                return "?";
            var type = Type.GetType(req.TypeName + ", Assembly-CSharp");
            if (type == null)
                return req.TypeName ?? "?";

            string localizedTypeName = RequirementTypes.GetLocalizedName(type);

            if (type == typeof(RoomRequirement_Area))
                return "RTRS.Area".Translate() + ": " + (req.Area?.ToString() ?? "?");
            if (type == typeof(RoomRequirement_Impressiveness))
                return "RTRS.Impressiveness".Translate() + ": " + (req.Impressiveness?.ToString() ?? "?");
            if (type == typeof(RoomRequirement_Thing))
                return "RTRS.Thing".Translate() + ": " + GetThingDefLabel(req.ThingDefName);
            if (type == typeof(RoomRequirement_ThingCount))
                return "RTRS.Thing".Translate() + ": " + GetThingDefLabel(req.ThingDefName) + " x" + (req.Count?.ToString() ?? "?");
            if (type == typeof(RoomRequirement_ThingAnyOf))
                return "RTRS.ThingsAnyOf".Translate() + ": " + GetThingsListLabel(req.ThingDefNames);
            if (type == typeof(RoomRequirement_ThingAnyOfCount))
                return "RTRS.ThingsAnyOf".Translate() + ": " + GetThingsListLabel(req.ThingDefNames) + " x" + (req.Count?.ToString() ?? "?");
            if (type == typeof(RoomRequirement_TerrainWithTags))
                return "RTRS.TerrainTags".Translate() + ": " + GetStringListLabel(req.TerrainTags);
            if (type == typeof(RoomRequirement_ForbiddenBuildings))
                return "RTRS.ForbiddenBuildingTags".Translate() + ": " + GetStringListLabel(req.BuildingTags);
            if (type == typeof(RoomRequirement_ForbidAltars))
                return localizedTypeName; // нет параметров
            if (type == typeof(RoomRequirement_AllThingsAreGlowing))
                return "RTRS.ThingMustGlow".Translate() + ": " + GetThingDefLabel(req.ThingDefName);
            if (type == typeof(RoomRequirement_AllThingsAnyOfAreGlowing))
                return "RTRS.ThingsAllMustGlow".Translate() + ": " + GetThingsListLabel(req.ThingDefNames);
            if (type == typeof(RoomRequirement_HasAssignedThroneAnyOf))
                return "RTRS.Thrones".Translate() + ": " + GetThingsListLabel(req.ThingDefNames);
            if (type.Name == "RoomRequirement_InstrumentSpace")
                return localizedTypeName; // нет параметров

            return localizedTypeName;
        }

        public static string GetDetailedTooltip(SerializableRoomRequirement req)
        {
            if (req == null)
                return "";
            var lines = new List<string>();

            var type = Type.GetType(req.TypeName + ", Assembly-CSharp");
            string typeName = type != null ? RequirementTypes.GetLocalizedName(type) : (req.TypeName ?? "?");
            lines.Add(typeName);

            if (req.Area.HasValue)
                lines.Add("  " + "RTRS.Area".Translate() + ": " + req.Area.Value);
            if (req.Impressiveness.HasValue)
                lines.Add("  " + "RTRS.Impressiveness".Translate() + ": " + req.Impressiveness.Value);
            if (!string.IsNullOrEmpty(req.ThingDefName))
                lines.Add("  " + "RTRS.Thing".Translate() + ": " + GetThingDefLabel(req.ThingDefName));
            if (req.Count.HasValue)
                lines.Add("  " + "RTRS.Count".Translate() + ": " + req.Count.Value);
            if (req.ThingDefNames.Count > 0)
                lines.Add("  " + "RTRS.Things".Translate() + ": " + GetThingsListLabel(req.ThingDefNames));
            if (req.TerrainTags.Count > 0)
                lines.Add("  " + "RTRS.TerrainTags".Translate() + ": " + GetStringListLabel(req.TerrainTags));
            if (req.BuildingTags.Count > 0)
                lines.Add("  " + "RTRS.ForbiddenBuildingTags".Translate() + ": " + GetStringListLabel(req.BuildingTags));

            if (req.DisablingPreceptDefNames.Count > 0)
            {
                lines.Add("");
                lines.Add("RTRS.DisablingPreceptsTitle".Translate() + ":");
                foreach (var pn in req.DisablingPreceptDefNames)
                {
                    var def = DefDatabase<PreceptDef>.GetNamedSilentFail(pn);
                    string label;
                    if (def != null)
                    {
                        label = !string.IsNullOrEmpty(def.LabelCap) ? def.LabelCap.ToString() : def.defName;
                    }
                    else
                    {
                        label = pn;
                    }
                    lines.Add("  • " + label);
                }
            }

            lines.Add("");
            lines.Add("RTRS.OriginalType".Translate() + ": " + req.TypeName);

            return string.Join("\n", lines);
        }

        private static string GetThingDefLabel(string defName)
        {
            if (string.IsNullOrEmpty(defName))
                return "(" + "RTRS.None".Translate() + ")";
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def != null)
                return !string.IsNullOrEmpty(def.LabelCap) ? def.LabelCap.ToString() : def.defName;
            return defName;
        }

        private static string GetThingsListLabel(List<string> defNames)
        {
            if (defNames == null || defNames.Count == 0)
                return "(" + "RTRS.None".Translate() + ")";
            var labels = defNames.Select(name =>
            {
                var def = DefDatabase<ThingDef>.GetNamedSilentFail(name);
                if (def != null)
                    return !string.IsNullOrEmpty(def.LabelCap) ? def.LabelCap.ToString() : def.defName;
                return name;
            });
            return string.Join(", ", labels);
        }

        private static string GetStringListLabel(List<string> strings)
        {
            if (strings == null || strings.Count == 0)
                return "(" + "RTRS.None".Translate() + ")";
            return string.Join(", ", strings);
        }
    }
}