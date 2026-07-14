using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using RoyalTitlesRoomSettings.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RoyalTitlesRoomSettings.UI
{
    public static class CourtTabDrawer
    {
        public static void Draw(Rect rect, RoyalTitleDef title, string tabId, ref Vector2 scrollPos)
        {
            var settings = SettingsManager.Instance;
            var courtRequirements = settings.GetCourt(title.defName);

            var headerRect = new Rect(rect.x, rect.y, rect.width, 30f);
            DrawHeader(headerRect, title);

            var listRect = new Rect(rect.x, headerRect.yMax + 4f,
                rect.width, rect.yMax - headerRect.yMax - 4f);
            DrawCourtList(listRect, courtRequirements, title, ref scrollPos);
        }

        private static void DrawHeader(Rect rect, RoyalTitleDef title)
        {
            var resetRect = new Rect(rect.xMax - 120f, rect.y, 120f, 28f);
            if (Widgets.ButtonText(resetRect, "RTRS.Reset".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "RTRS.ResetConfirm".Translate(title.GetLabelCapForBothGenders()),
                    () =>
                    {
                        OriginalRequirementsCache.ResetTitleToOriginal(title.defName);
                    }));
            }
        }

        private static void DrawCourtList(Rect rect, List<SerializableCourtRequirement> reqs,
            RoyalTitleDef title, ref Vector2 scrollPos)
        {
            if (reqs.Count == 0)
            {
                UIDrawHelpers.DrawCenteredText(
                    new Rect(rect.x, rect.y, rect.width, rect.height - 34f),
                    "(" + "RTRS.EmptyList".Translate() + ")", Color.gray);
                DrawAddButton(new Rect(rect.x, rect.yMax - 30f, rect.width, 28f), title);
                return;
            }

            float viewHeight = reqs.Count * 30f + 40f;
            var viewRect = new Rect(0f, 0f, rect.width - 16f, viewHeight);

            Widgets.BeginScrollView(rect, ref scrollPos, viewRect);
            float y = 0f;

            for (int i = 0; i < reqs.Count; i++)
            {
                var req = reqs[i];
                var rowRect = new Rect(viewRect.x, y, viewRect.width, 28f);
                int index = i;
                DrawCourtRow(rowRect, req, title, index);
                y += 30f;
            }

            DrawAddButton(new Rect(viewRect.x, y, viewRect.width, 28f), title);
            Widgets.EndScrollView();
        }

        private static void DrawCourtRow(Rect rect, SerializableCourtRequirement req,
            RoyalTitleDef title, int index)
        {
            string minLabel = GetTitleLabel(req.MinTitleDefName);
            string maxLabel = GetTitleLabel(req.MaxTitleDefName);
            string label = string.Format("RTRS.CourtRequirement".Translate(), minLabel, maxLabel, req.Count);

            UIDrawHelpers.DrawListRow(rect, label,
                onClickLabel: () =>
                {
                    // ИСПРАВЛЕНИЕ: создаём оверрайд и берём объект из него
                    var settings = SettingsManager.Instance;
                    var dto = settings.EnsureOverride(title.defName);
                    if (dto.CourtRequirements != null && index < dto.CourtRequirements.Count)
                    {
                        var reqFromOverride = dto.CourtRequirements[index];
                        Find.WindowStack.Add(new CourtRequirementEditorWindow(reqFromOverride, title, index));
                    }
                },
                onRemove: () => RemoveCourtReq(title, index));
        }

        private static void RemoveCourtReq(RoyalTitleDef title, int index)
        {
            var settings = SettingsManager.Instance;
            var dto = settings.EnsureOverride(title.defName);
            if (index < dto.CourtRequirements.Count)
                dto.CourtRequirements.RemoveAt(index);
        }

        private static void DrawAddButton(Rect rect, RoyalTitleDef title)
        {
            if (Widgets.ButtonText(rect, "+ " + "RTRS.Add".Translate()))
            {
                var settings = SettingsManager.Instance;
                var dto = settings.EnsureOverride(title.defName);
                dto.CourtRequirements.Add(new SerializableCourtRequirement
                {
                    Count = 1,
                    MinTitleDefName = "Freeholder",
                    MaxTitleDefName = "Freeholder",
                    Enabled = true
                });
            }
        }

        private static string GetTitleLabel(string defName)
        {
            if (string.IsNullOrEmpty(defName))
                return "—";
            var def = DefDatabase<RoyalTitleDef>.GetNamedSilentFail(defName);
            return def?.GetLabelCapForBothGenders() ?? defName;
        }
    }

    public class CourtRequirementEditorWindow : Window
    {
        private SerializableCourtRequirement req;
        private readonly RoyalTitleDef title;
        private readonly int index;
        private List<RoyalTitleDef> allTitles;

        public override Vector2 InitialSize => new Vector2(400f, 280f);

        public CourtRequirementEditorWindow(SerializableCourtRequirement req, RoyalTitleDef title, int index)
        {
            this.req = req;
            this.title = title;
            this.index = index;
            doCloseX = true;
            draggable = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            allTitles = DefDatabase<RoyalTitleDef>.AllDefsListForReading
                .Where(t => t.seniority > 0)
                .OrderBy(t => t.seniority)
                .ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("Min title:");
            DrawTitleDropdown(listing,
                () => req.MinTitleDefName,
                v => req.MinTitleDefName = v);

            listing.Label("Max title:");
            DrawTitleDropdown(listing,
                () => req.MaxTitleDefName,
                v => req.MaxTitleDefName = v);

            UIDrawHelpers.DrawIntField(listing, ref req.Count, "Count:", 1, 50);

            listing.Gap(12f);
            DrawResetButton(listing);

            listing.End();
        }

        private void DrawTitleDropdown(Listing_Standard listing, Func<string> getter, Action<string> setter)
        {
            string currentLabel = GetTitleLabel(getter());
            Rect row = listing.GetRect(28f);

            if (Widgets.ButtonText(row, currentLabel + " ▼"))
            {
                Find.WindowStack.Add(new DefSelectionDialog<RoyalTitleDef>(
                    allTitles,
                    DefDatabase<RoyalTitleDef>.GetNamedSilentFail(getter()),
                    d => setter(d.defName),
                    d => d.GetLabelCapForBothGenders(),
                    d => d.defName,
                    d => d.description));
            }

            listing.Gap(6f);
        }

        private void DrawResetButton(Listing_Standard listing)
        {
            if (!listing.ButtonText("RTRS.Reset".Translate()))
                return;

            var orig = OriginalRequirementsCache.GetOriginal(title.defName);
            if (orig?.CourtRequirements == null || index >= orig.CourtRequirements.Count)
                return;

            var origReq = orig.CourtRequirements[index];
            req.Count = origReq.Count;
            req.MinTitleDefName = origReq.MinTitleDefName;
            req.MaxTitleDefName = origReq.MaxTitleDefName;
            req.Enabled = origReq.Enabled;
        }

        private static string GetTitleLabel(string defName)
        {
            if (string.IsNullOrEmpty(defName))
                return "—";
            var def = DefDatabase<RoyalTitleDef>.GetNamedSilentFail(defName);
            return def?.GetLabelCapForBothGenders() ?? defName;
        }
    }
}