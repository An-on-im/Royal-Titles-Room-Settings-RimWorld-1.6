using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using System;
using UnityEngine;
using Verse;

namespace RoyalTitlesRoomSettings.UI
{
    public static class RequirementDrawers
    {
        public static void Draw(Listing_Standard listing, SerializableRoomRequirement req)
        {
            if (req == null)
                return;
            var type = Type.GetType(req.TypeName + ", Assembly-CSharp");
            if (type == null)
            {
                UIDrawHelpers.DrawCenteredText(listing.GetRect(28f), "RTRS.NoEditor".Translate(req.TypeName ?? "?"), Color.gray);
                return;
            }

            if (type == typeof(RoomRequirement_Area))
            {
                int val = req.Area ?? 1;
                UIDrawHelpers.DrawIntField(listing, ref val, "RTRS.Area".Translate() + ":", 1, 500);
                req.Area = val;
            }
            else if (type == typeof(RoomRequirement_Impressiveness))
            {
                int val = req.Impressiveness ?? 1;
                UIDrawHelpers.DrawIntField(listing, ref val, "RTRS.Impressiveness".Translate() + ":", 1, 500);
                req.Impressiveness = val;
            }
            else if (type == typeof(RoomRequirement_Thing))
            {
                UIDrawHelpers.DrawThingDefField(listing, req, "RTRS.Thing".Translate() + ":");
            }
            else if (type == typeof(RoomRequirement_ThingCount))
            {
                UIDrawHelpers.DrawThingDefField(listing, req, "RTRS.Thing".Translate() + ":");
                int val = req.Count ?? 1;
                UIDrawHelpers.DrawIntField(listing, ref val, "RTRS.Count".Translate() + ":", 1, 50);
                req.Count = val;
            }
            else if (type == typeof(RoomRequirement_ThingAnyOf) || type == typeof(RoomRequirement_ThingAnyOfCount))
            {
                UIDrawHelpers.DrawThingDefList(listing, req.ThingDefNames, "RTRS.ThingsAnyOf".Translate() + ":");
                if (type == typeof(RoomRequirement_ThingAnyOfCount))
                {
                    int val = req.Count ?? 1;
                    UIDrawHelpers.DrawIntField(listing, ref val, "RTRS.Count".Translate() + ":", 1, 50);
                    req.Count = val;
                }
            }
            else if (type == typeof(RoomRequirement_TerrainWithTags))
            {
                UIDrawHelpers.DrawStringList(listing, req.TerrainTags, "RTRS.TerrainTags".Translate() + ":", UIDrawHelpers.GetAllTerrainTags);
            }
            else if (type == typeof(RoomRequirement_ForbiddenBuildings))
            {
                UIDrawHelpers.DrawStringList(listing, req.BuildingTags, "RTRS.ForbiddenBuildingTags".Translate() + ":", UIDrawHelpers.GetAllBuildingTags);
            }
            else if (type == typeof(RoomRequirement_ForbidAltars))
            {
                UIDrawHelpers.DrawInfoLabel(listing, "RTRS.NoFieldsAltars".Translate());
            }
            else if (type == typeof(RoomRequirement_AllThingsAreGlowing))
            {
                UIDrawHelpers.DrawThingDefField(listing, req, "RTRS.ThingMustGlow".Translate() + ":");
            }
            else if (type == typeof(RoomRequirement_AllThingsAnyOfAreGlowing))
            {
                UIDrawHelpers.DrawThingDefList(listing, req.ThingDefNames, "RTRS.ThingsAllMustGlow".Translate() + ":");
            }
            else if (type == typeof(RoomRequirement_HasAssignedThroneAnyOf))
            {
                UIDrawHelpers.DrawThingDefList(listing, req.ThingDefNames, "RTRS.Thrones".Translate() + ":");
            }
            else if (type.Name == "RoomRequirement_InstrumentSpace")
            {
                UIDrawHelpers.DrawInfoLabel(listing, "RTRS.NoFieldsInstrument".Translate());
            }
            else
            {
                UIDrawHelpers.DrawInfoLabel(listing, "RTRS.NoEditor".Translate(type.Name));
            }
        }
    }
}