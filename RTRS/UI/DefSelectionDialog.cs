using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RoyalTitlesRoomSettings.UI
{
    public class DefSelectionDialog<T> : Window where T : Def
    {
        private readonly List<T> allDefs;
        private readonly T currentDef;
        private readonly Action<T> onSelect;
        private readonly Func<T, string> labelGetter;
        private readonly Func<T, string> defNameGetter;
        private readonly Func<T, string> descriptionGetter;
        private readonly Func<T, bool> existingFilter;

        private QuickSearchWidget searchWidget;
        private Vector2 scrollPos;
        private List<T> filteredDefs;
        private string lastSearchText = "";

        public override Vector2 InitialSize => new Vector2(450f, 500f);

        public DefSelectionDialog(
            IEnumerable<T> allDefs,
            T currentDef,
            Action<T> onSelect,
            Func<T, string> labelGetter,
            Func<T, string> defNameGetter,
            Func<T, string> descriptionGetter = null,
            Func<T, bool> existingFilter = null)
        {
            this.allDefs = allDefs.OrderBy(d => labelGetter(d)).ToList();
            this.currentDef = currentDef;
            this.onSelect = onSelect;
            this.labelGetter = labelGetter;
            this.defNameGetter = defNameGetter;
            this.descriptionGetter = descriptionGetter;
            this.existingFilter = existingFilter;

            doCloseX = true;
            draggable = true;
            resizeable = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;

            searchWidget = new QuickSearchWidget();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            filteredDefs = allDefs.Where(d =>
            {
                if (existingFilter != null && !existingFilter(d))
                    return false;
                if (!searchWidget.filter.Active)
                    return true;
                string label = labelGetter(d) ?? "";
                string defName = defNameGetter(d) ?? "";
                return searchWidget.filter.Matches(label) || searchWidget.filter.Matches(defName);
            }).ToList();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var searchRect = new Rect(inRect.x, inRect.y, inRect.width, 30f);
            var countRect = new Rect(inRect.x, searchRect.yMax, inRect.width, 20f);
            var listRect = new Rect(inRect.x, countRect.yMax + 2f, inRect.width, inRect.yMax - countRect.yMax - 2f);

            searchWidget.OnGUI(searchRect);

            string currentText = searchWidget.filter.Text ?? "";
            if (currentText != lastSearchText)
            {
                lastSearchText = currentText;
                ApplyFilter();
                scrollPos = Vector2.zero;
            }

            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(countRect, "RTRS.SearchResults".Translate(filteredDefs.Count, allDefs.Count));
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
            Text.Font = GameFont.Small;

            DrawDefList(listRect);
        }

        private void DrawDefList(Rect rect)
        {
            float rowHeight = 28f;
            float viewHeight = filteredDefs.Count * rowHeight;
            var viewRect = new Rect(0f, 0f, rect.width - 16f, Mathf.Max(viewHeight, rect.height));

            Widgets.BeginScrollView(rect, ref scrollPos, viewRect);

            int firstVisible = Mathf.Max(0, Mathf.FloorToInt(scrollPos.y / rowHeight));
            int lastVisible = Mathf.Min(filteredDefs.Count - 1,
                Mathf.CeilToInt((scrollPos.y + rect.height) / rowHeight));

            for (int i = firstVisible; i <= lastVisible; i++)
            {
                var def = filteredDefs[i];
                float y = i * rowHeight;
                var rowRect = new Rect(viewRect.x, y, viewRect.width, rowHeight);

                if (def == currentDef)
                    Widgets.DrawBoxSolid(rowRect, new Color(0.3f, 0.5f, 0.8f, 0.3f));

                bool clicked = Widgets.ButtonInvisible(rowRect);
                Widgets.DrawHighlightIfMouseover(rowRect);

                var textRect = new Rect(rowRect.x + 8f, rowRect.y, rowRect.width - 16f, rowRect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(textRect, labelGetter(def));
                Text.Anchor = TextAnchor.UpperLeft;

                if (descriptionGetter != null)
                {
                    string tooltip = labelGetter(def) + " (" + defNameGetter(def) + ")";
                    string desc = descriptionGetter(def);
                    if (!string.IsNullOrEmpty(desc))
                        tooltip += "\n\n" + desc;
                    TooltipHandler.TipRegion(rowRect, tooltip);
                }

                if (clicked)
                {
                    onSelect(def);
                    Close();
                    Widgets.EndScrollView();
                    return;
                }
            }

            Widgets.EndScrollView();
        }

        public override void OnAcceptKeyPressed()
        {
            if (searchWidget.CurrentlyFocused())
                searchWidget.Unfocus();
            else
                base.OnAcceptKeyPressed();
        }

        public override void OnCancelKeyPressed()
        {
            if (searchWidget.CurrentlyFocused())
                searchWidget.Unfocus();
            else
                base.OnCancelKeyPressed();
        }
    }
}