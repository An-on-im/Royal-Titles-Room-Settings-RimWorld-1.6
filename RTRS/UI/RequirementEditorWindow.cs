using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using RoyalTitlesRoomSettings.Services;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RoyalTitlesRoomSettings.UI
{
    public class RequirementEditorWindow : Window
    {
        private readonly SerializableRoomRequirement req;
        private readonly RoyalTitleDef title;
        private readonly string tabId;
        private readonly int index;
        private Vector2 scrollPos;

        public override Vector2 InitialSize => new Vector2(520f, 520f);

        public RequirementEditorWindow(SerializableRoomRequirement req, RoyalTitleDef title, string tabId, int index)
        {
            this.req = req;
            this.title = title;
            this.tabId = tabId;
            this.index = index;
            doCloseX = true;
            draggable = true;
            resizeable = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var viewRect = new Rect(0f, 0f, inRect.width - 16f, 2000f);
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            DrawHeader(listing);
            DrawCommonFields(listing);

            listing.GapLine();
            listing.Gap(8f);

            RequirementDrawers.Draw(listing, req);

            listing.Gap(12f);
            DrawResetButton(listing);

            listing.End();
            Widgets.EndScrollView();
        }

        private void DrawHeader(Listing_Standard listing)
        {
            var type = Type.GetType(req.TypeName + ", Assembly-CSharp");
            string typeName = type != null ? RequirementTypes.GetLocalizedName(type) : req.TypeName ?? "?";
            Text.Font = GameFont.Medium;
            listing.Label(typeName);
            Text.Font = GameFont.Small;
            listing.GapLine();
        }

        private void DrawCommonFields(Listing_Standard listing)
        {
            listing.Gap(4f);
            DrawPreceptsButton(listing);
        }

        private void DrawPreceptsButton(Listing_Standard listing)
        {
            int count = req.DisablingPreceptDefNames.Count;

            string statusText = count > 0
                ? "RTRS.PreceptsStatusSet".Translate(count)
                : "RTRS.PreceptsStatusNotSet".Translate();

            string buttonLabel = "RTRS.PreceptsButton".Translate() + ": " + statusText;

            if (listing.ButtonText(buttonLabel))
                Find.WindowStack.Add(new DisablingPreceptsWindow(req.DisablingPreceptDefNames));

            Rect lastRect = listing.GetRect(0f);
            TooltipHandler.TipRegion(new Rect(lastRect.x, lastRect.y - 32f, lastRect.width, 32f),
                "RTRS.PreceptsTooltip".Translate());

            listing.Gap(4f);
        }

        private void DrawResetButton(Listing_Standard listing)
        {
            if (!listing.ButtonText("RTRS.Reset".Translate()))
                return;

            var orig = GetOriginalRequirement();
            if (orig == null)
                return;

            CopyFrom(orig);
        }

        private SerializableRoomRequirement GetOriginalRequirement()
        {
            var origDto = OriginalRequirementsCache.GetOriginal(title.defName);
            if (origDto == null)
                return null;

            List<SerializableRoomRequirement> origList = null;
            switch (tabId)
            {
                case "bedroom":
                    origList = origDto.BedroomRequirements;
                    break;
                case "throneRoom":
                    origList = origDto.ThroneRoomRequirements;
                    break;
                case "ballroom":
                    origList = origDto.BallroomRequirements;
                    break;
                case "gallery":
                    origList = origDto.GalleryRequirements;
                    break;
            }

            if (origList == null || index >= origList.Count)
                return null;
            return origList[index];
        }

        private void CopyFrom(SerializableRoomRequirement source)
        {
            req.Area = source.Area;
            req.Impressiveness = source.Impressiveness;
            req.ThingDefName = source.ThingDefName;
            req.Count = source.Count;
            req.LabelKey = source.LabelKey;
            req.ThingDefNames.Clear();
            req.ThingDefNames.AddRange(source.ThingDefNames);
            req.TerrainTags.Clear();
            req.TerrainTags.AddRange(source.TerrainTags);
            req.BuildingTags.Clear();
            req.BuildingTags.AddRange(source.BuildingTags);
            req.DisablingPreceptDefNames.Clear();
            req.DisablingPreceptDefNames.AddRange(source.DisablingPreceptDefNames);
        }
    }
}