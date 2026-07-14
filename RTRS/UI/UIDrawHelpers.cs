using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RoyalTitlesRoomSettings.UI
{
    public static class UIDrawHelpers
    {
        public static void DrawCenteredText(Rect rect, string text, Color? color = null)
        {
            Color saved = GUI.color;
            if (color.HasValue)
                GUI.color = color.Value;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, text);
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = saved;
        }

        public static void DrawListRow(Rect rect, string label, Action onClickLabel, Action onRemove, string tooltip = null)
        {
            var labelRect = new Rect(rect.x, rect.y, rect.width - 36f, rect.height);
            var removeRect = new Rect(rect.xMax - 32f, rect.y, 32f, rect.height);

            if (Widgets.ButtonInvisible(labelRect))
                onClickLabel?.Invoke();

            Widgets.Label(labelRect, label);
            Widgets.DrawHighlightIfMouseover(labelRect);

            if (!string.IsNullOrEmpty(tooltip))
                TooltipHandler.TipRegion(labelRect, tooltip);

            if (Widgets.ButtonText(removeRect, "×"))
                onRemove?.Invoke();
        }

        public static void DrawIntField(Listing_Standard listing, ref int value, string label, int min = 0, int max = 99999)
        {
            Rect row = listing.GetRect(28f);
            float w = row.width;

            var labelRect = new Rect(row.x, row.y, w * 0.4f, 28f);
            var sliderRect = new Rect(row.x + w * 0.4f, row.y, w * 0.3f, 28f);
            var textRect = new Rect(row.x + w * 0.72f, row.y, w * 0.28f, 28f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.UpperLeft;

            float fVal = value;
            fVal = Widgets.HorizontalSlider(sliderRect, fVal, min, max, middleAlignment: true);
            value = Mathf.Clamp(Mathf.RoundToInt(fVal), min, max);

            string input = Widgets.TextField(textRect, value.ToString());
            if (int.TryParse(input, out int parsed))
                value = Mathf.Clamp(parsed, min, max);

            listing.Gap(4f);
        }

        public static void DrawStringField(Listing_Standard listing, ref string value, string label)
        {
            Rect row = listing.GetRect(28f);
            float w = row.width;

            var labelRect = new Rect(row.x, row.y, w * 0.3f, 28f);
            var textRect = new Rect(row.x + w * 0.3f, row.y, w * 0.7f, 28f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.UpperLeft;

            value = Widgets.TextField(textRect, value ?? "");
            listing.Gap(4f);
        }

        public static void DrawThingDefField(Listing_Standard listing, Func<ThingDef> getter, Action<ThingDef> setter, string label)
        {
            Rect row = listing.GetRect(28f);
            float w = row.width;

            var labelRect = new Rect(row.x, row.y, w * 0.3f, 28f);
            var buttonRect = new Rect(row.x + w * 0.3f, row.y, w * 0.7f, 28f);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Text.Anchor = TextAnchor.UpperLeft;

            var current = getter();
            string currentLabel = current != null ? current.LabelCap : "(" + "RTRS.None".Translate() + ")";
            if (Widgets.ButtonText(buttonRect, currentLabel + " ▼"))
            {
                Find.WindowStack.Add(new DefSelectionDialog<ThingDef>(
                    GetBuildingDefs(),
                    getter(),
                    setter,
                    d => d.LabelCap,
                    d => d.defName,
                    d => d.description));
            }
            listing.Gap(4f);
        }

        public static void DrawThingDefList(Listing_Standard listing, List<ThingDef> list, string label)
        {
            if (list == null)
                return;

            Text.Font = GameFont.Small;
            listing.Label(label);
            listing.Gap(2f);

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                Rect row = listing.GetRect(24f);
                var labelRect = new Rect(row.x + 8f, row.y, row.width - 40f, 24f);
                var removeRect = new Rect(row.xMax - 32f, row.y, 32f, 24f);

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, "• " + (item != null ? item.LabelCap : item?.defName ?? "?"));
                Text.Anchor = TextAnchor.UpperLeft;

                if (Widgets.ButtonText(removeRect, "×"))
                    list.Remove(item);
            }

            if (listing.ButtonText("+ " + "RTRS.Add".Translate()))
            {
                Find.WindowStack.Add(new DefSelectionDialog<ThingDef>(
                    GetBuildingDefs(),
                    null,
                    d => list.Add(d),
                    d => d.LabelCap,
                    d => d.defName,
                    d => d.description,
                    existingFilter: d => !list.Contains(d)));
            }
            listing.Gap(8f);
        }

        public static void DrawStringList(Listing_Standard listing, List<string> list, string label, Func<IEnumerable<string>> allValuesSource)
        {
            if (list == null)
                return;

            Text.Font = GameFont.Small;
            listing.Label(label);
            listing.Gap(2f);

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                Rect row = listing.GetRect(24f);
                var labelRect = new Rect(row.x + 8f, row.y, row.width - 40f, 24f);
                var removeRect = new Rect(row.xMax - 32f, row.y, 32f, 24f);

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, "• " + (item ?? "?"));
                Text.Anchor = TextAnchor.UpperLeft;

                if (Widgets.ButtonText(removeRect, "×"))
                    list.Remove(item);
            }

            if (listing.ButtonText("+ " + "RTRS.Add".Translate()))
            {
                var existing = new HashSet<string>(list);
                var all = allValuesSource().Where(s => !existing.Contains(s)).OrderBy(s => s).ToList();
                if (all.Count > 0)
                {
                    var options = all.Select(s => new FloatMenuOption(s, () => list.Add(s))).ToList();
                    Find.WindowStack.Add(new FloatMenu(options));
                }
            }
            listing.Gap(8f);
        }

        public static IEnumerable<string> GetAllTerrainTags()
        {
            return DefDatabase<TerrainDef>.AllDefsListForReading
                .Where(t => t.tags != null)
                .SelectMany(t => t.tags)
                .Distinct()
                .OrderBy(s => s);
        }

        public static IEnumerable<string> GetAllBuildingTags()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(t => t.building?.buildingTags != null)
                .SelectMany(t => t.building.buildingTags)
                .Distinct()
                .OrderBy(s => s);
        }

        private static List<ThingDef> cachedBuildingDefs;

        private static List<ThingDef> GetBuildingDefs()
        {
            if (cachedBuildingDefs != null)
                return cachedBuildingDefs;
            cachedBuildingDefs = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.building != null)
                .OrderBy(d => d.LabelCap.ToString())
                .ToList();
            return cachedBuildingDefs;
        }

        public static void DrawThingDefField(Listing_Standard listing, SerializableRoomRequirement req, string label)
        {
            DrawThingDefField(listing,
                () => string.IsNullOrEmpty(req.ThingDefName) ? null : DefDatabase<ThingDef>.GetNamedSilentFail(req.ThingDefName),
                def => req.ThingDefName = def?.defName,
                label);
        }

        public static void DrawThingDefList(Listing_Standard listing, List<string> defNames, string label)
        {
            var list = defNames.Select(DefDatabase<ThingDef>.GetNamedSilentFail).Where(d => d != null).ToList();
            DrawThingDefList(listing, list, label,
                () => { },
                (d) => { if (d != null && !defNames.Contains(d.defName)) defNames.Add(d.defName); },
                (d) => { if (d != null) defNames.Remove(d.defName); });
        }

        public static void DrawInfoLabel(Listing_Standard listing, string text)
        {
            listing.Gap(4f);
            Text.Anchor = TextAnchor.MiddleCenter;
            GUI.color = Color.gray;
            listing.Label(text);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            listing.Gap(4f);
        }

        public static void DrawThingDefList(Listing_Standard listing, List<ThingDef> list, string label,
            Action onAdd, Action<ThingDef> onAddItem, Action<ThingDef> onRemoveItem)
        {
            if (list == null)
                return;
            Text.Font = GameFont.Small;
            listing.Label(label);
            listing.Gap(2f);

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                Rect row = listing.GetRect(24f);
                var labelRect = new Rect(row.x + 8f, row.y, row.width - 40f, 24f);
                var removeRect = new Rect(row.xMax - 32f, row.y, 32f, 24f);

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, "• " + (item != null ? item.LabelCap : item?.defName ?? "?"));
                Text.Anchor = TextAnchor.UpperLeft;

                if (Widgets.ButtonText(removeRect, "×"))
                {
                    onRemoveItem?.Invoke(item);
                    list.Remove(item);
                }
            }

            if (listing.ButtonText("+ " + "RTRS.Add".Translate()))
            {
                onAdd?.Invoke();
                Find.WindowStack.Add(new DefSelectionDialog<ThingDef>(
                    GetBuildingDefs(),
                    null,
                    d => { onAddItem?.Invoke(d); list.Add(d); },
                    d => d.LabelCap,
                    d => d.defName,
                    d => d.description,
                    existingFilter: d => !list.Contains(d)));
            }
            listing.Gap(8f);
        }
    }
}