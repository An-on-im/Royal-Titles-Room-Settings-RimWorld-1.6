using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RoyalTitlesRoomSettings.UI
{
    public class DisablingPreceptsWindow : Window
    {
        private readonly List<string> preceptDefNames;
        private Vector2 scrollPos;

        public override Vector2 InitialSize => new Vector2(500f, 450f);

        public DisablingPreceptsWindow(List<string> preceptDefNames)
        {
            this.preceptDefNames = preceptDefNames;
            doCloseX = true;
            draggable = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var viewRect = new Rect(0f, 0f, inRect.width - 16f, Mathf.Max(inRect.height, preceptDefNames.Count * 30f + 150f));
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            DrawHeader(listing);
            DrawPreceptsList(listing);
            DrawAddButton(listing);

            listing.End();
            Widgets.EndScrollView();
        }

        private void DrawHeader(Listing_Standard listing)
        {
            Text.Font = GameFont.Medium;
            listing.Label("RTRS.DisablingPreceptsTitle".Translate());
            Text.Font = GameFont.Small;
            listing.GapLine();

            GUI.color = Color.gray;
            listing.Label("RTRS.DisablingPreceptsDesc".Translate());
            GUI.color = Color.white;
            listing.Gap(8f);
        }

        private void DrawPreceptsList(Listing_Standard listing)
        {
            if (preceptDefNames.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.gray;
                listing.Label("(" + "RTRS.PreceptsEmpty".Translate() + ")");
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                listing.Gap(8f);
                return;
            }

            for (int i = preceptDefNames.Count - 1; i >= 0; i--)
            {
                var defName = preceptDefNames[i];
                var def = DefDatabase<PreceptDef>.GetNamedSilentFail(defName);
                string displayName = def != null ? GetPreceptDisplayName(def) : defName;
                Rect row = listing.GetRect(26f);
                var labelRect = new Rect(row.x + 8f, row.y, row.width - 40f, 26f);
                var removeRect = new Rect(row.xMax - 32f, row.y, 32f, 26f);

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, "• " + displayName);
                Text.Anchor = TextAnchor.UpperLeft;

                if (Widgets.ButtonText(removeRect, "×"))
                    preceptDefNames.RemoveAt(i);
            }
            listing.Gap(8f);
        }

        private void DrawAddButton(Listing_Standard listing)
        {
            if (!listing.ButtonText("+ " + "RTRS.Add".Translate()))
                return;

            var allDefs = DefDatabase<PreceptDef>.AllDefsListForReading;
            var existing = new HashSet<string>(preceptDefNames);
            var available = allDefs.Where(d => !existing.Contains(d.defName)).ToList();

            Find.WindowStack.Add(new DefSelectionDialog<PreceptDef>(
                available,
                null,
                d => preceptDefNames.Add(d.defName),
                d => GetPreceptDisplayName(d),
                d => d.defName,
                d => d.description,
                existingFilter: d => !preceptDefNames.Contains(d.defName)));
        }

        private static string GetPreceptDisplayName(PreceptDef p)
        {
            if (p == null)
                return "?";
            string label = p.LabelCap;
            if (string.IsNullOrEmpty(label))
                label = p.defName;

            string issue = p.issue?.LabelCap;
            if (!string.IsNullOrEmpty(issue))
                return issue + ": " + label;

            return label;
        }
    }
}