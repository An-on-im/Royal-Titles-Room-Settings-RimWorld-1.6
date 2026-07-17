using RimWorld;
using RoyalTitlesRoomSettings.Services;
using RoyalTitlesRoomSettings.UI;
using RoyalTitlesRoomSettings.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RoyalTitlesRoomSettings
{
    public static class RTRSSettingsGUI
    {
        private const float LeftPanelWidth = 200f;
        private const float BottomBarHeight = 40f;
        private const float TabBarHeight = 32f;

        private delegate void TabDrawAction(Rect rect, RoyalTitleDef title, string tabId, ref Vector2 scrollPos);

        private static Vector2 leftScrollPos;
        private static Vector2 rightScrollPos;
        private static RoyalTitleDef selectedTitle;
        private static string activeTabId = "bedroom";
        private static List<RoyalTitleDef> cachedTitles;

        private static readonly TabDescriptor[] AllTabs =
        {
            new TabDescriptor("bedroom",    "RTRS.TabBedroom",    () => true,                    RoomTabDrawer.Draw),
            new TabDescriptor("throneRoom", "RTRS.TabThroneRoom", () => true,                    RoomTabDrawer.Draw),
            new TabDescriptor("ballroom",   "RTRS.TabBallroom",   () => VFEIntegration.IsVfeLoaded, RoomTabDrawer.Draw),
            new TabDescriptor("gallery",    "RTRS.TabGallery",    () => VFEIntegration.IsVfeLoaded, RoomTabDrawer.Draw),
            new TabDescriptor("court",      "RTRS.TabCourt",      () => VFEIntegration.IsVfeLoaded, CourtTabDrawer.Draw),
        };

        public static void Draw(Rect inRect)
        {
            EnsureCache();

            var bottomRect = new Rect(inRect.x, inRect.yMax - BottomBarHeight, inRect.width, BottomBarHeight);
            var mainRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - BottomBarHeight - 4f);

            DrawBottomBar(bottomRect);
            DrawMainArea(mainRect);
        }

        private static void EnsureCache()
        {
            if (cachedTitles != null)
                return;

            OriginalRequirementsCache.EnsureCache();
            cachedTitles = DefDatabase<RoyalTitleDef>.AllDefsListForReading
                .Where(t => t.seniority > 0)
                .OrderBy(t => t.seniority)
                .ToList();

            if (cachedTitles.Count > 0)
                selectedTitle = cachedTitles[0];
        }

        private static void DrawBottomBar(Rect rect)
        {
            Widgets.DrawLineHorizontal(rect.x, rect.y, rect.width);

            var btnRect = new Rect(rect.x + 4f, rect.y + 4f, 220f, rect.height - 8f);
            if (Widgets.ButtonText(btnRect, "RTRS.ResetAll".Translate()))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "RTRS.ResetAllConfirm".Translate(),
                    () =>
                    {
                        SettingsManager.Instance.ResetAll();
                        RequirementsApplier.PatchAll();
                    }));
            }

            var helpBtnRect = new Rect(rect.xMax - 44f, rect.y + 4f, 40f, rect.height - 8f);
            if (Widgets.ButtonText(helpBtnRect, "?"))
            {
                Find.WindowStack.Add(new RoomRequirementsHelpWindow());
            }
            TooltipHandler.TipRegion(helpBtnRect, "RTRS.HelpButtonTooltip".Translate());
        }

        private static void DrawMainArea(Rect rect)
        {
            var leftRect = new Rect(rect.x, rect.y, LeftPanelWidth, rect.height);
            var rightRect = new Rect(rect.x + LeftPanelWidth + 6f, rect.y,
                rect.width - LeftPanelWidth - 6f, rect.height);

            Widgets.DrawLineVertical(leftRect.xMax, leftRect.y, leftRect.height);

            DrawLeftPanel(leftRect);
            DrawRightPanel(rightRect);
        }

        private static void DrawLeftPanel(Rect rect)
        {
            float totalHeight = 0f;
            var heights = new List<float>(cachedTitles.Count);
            foreach (var title in cachedTitles)
            {
                float h = CalcTitleRowHeight(title);
                heights.Add(h);
                totalHeight += h;
            }

            var viewRect = new Rect(0f, 0f, rect.width - 16f, totalHeight + 10f);
            Widgets.BeginScrollView(rect, ref leftScrollPos, viewRect);

            float y = 4f;
            for (int i = 0; i < cachedTitles.Count; i++)
            {
                float h = heights[i];
                var rowRect = new Rect(viewRect.x, y, viewRect.width, h);
                DrawTitleRow(rowRect, cachedTitles[i]);
                y += h;
            }

            Widgets.EndScrollView();
        }

        private static float CalcTitleRowHeight(RoyalTitleDef title)
        {
            string label = GetTitleRowLabel(title);
            float textHeight = Text.CalcHeight(label, LeftPanelWidth - 28f);
            return Mathf.Max(28f, textHeight + 4f);
        }

        private static string GetTitleRowLabel(RoyalTitleDef title)
        {
            string name = title.GetLabelCapForBothGenders();
            bool modified = SettingsManager.Instance.IsTitleModified(title.defName);
            return modified ? name + " *" : name;
        }

        private static void DrawTitleRow(Rect rect, RoyalTitleDef title)
        {
            bool isSelected = selectedTitle == title;

            if (isSelected)
                Widgets.DrawBoxSolid(rect, new Color(0.3f, 0.5f, 0.8f, 0.3f));

            if (Widgets.ButtonInvisible(rect))
            {
                selectedTitle = title;
            }

            string fullLabel = GetTitleRowLabel(title);
            var textRect = new Rect(rect.x + 6f, rect.y, rect.width - 12f, rect.height);

            Text.WordWrap = true;
            Text.Anchor = TextAnchor.MiddleLeft;

            string displayLabel = fullLabel;
            float needed = Text.CalcHeight(fullLabel, textRect.width);
            if (needed > textRect.height + 2f)
            {
                for (int cut = fullLabel.Length - 1; cut > 3; cut--)
                {
                    string candidate = fullLabel.Substring(0, cut) + "...";
                    if (Text.CalcHeight(candidate, textRect.width) <= textRect.height + 2f)
                    {
                        displayLabel = candidate;
                        break;
                    }
                }
            }

            Widgets.Label(textRect, displayLabel);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;

            string tooltip = title.GetLabelCapForBothGenders();
            if (!string.IsNullOrEmpty(title.description))
                tooltip += "\n" + title.description;
            TooltipHandler.TipRegion(rect, tooltip);
        }

        private static void DrawRightPanel(Rect rect)
        {
            if (selectedTitle == null)
            {
                UIDrawHelpers.DrawCenteredText(rect, "(нет выбранного титула)", Color.gray);
                return;
            }

            var tabBarRect = new Rect(rect.x, rect.y, rect.width, TabBarHeight);
            var contentRect = new Rect(rect.x, tabBarRect.yMax + 2f,
                rect.width, rect.yMax - tabBarRect.yMax - 2f);

            DrawTabBar(tabBarRect);
            DrawTabContent(contentRect);
        }

        private static void DrawTabBar(Rect rect)
        {
            var visibleTabs = GetVisibleTabs().ToList();
            float x = rect.x;

            foreach (var tab in visibleTabs)
            {
                string label = tab.LabelKey.Translate();
                float w = Text.CalcSize(label).x + 24f;
                var tabRect = new Rect(x, rect.y, w, rect.height - 2f);

                bool isActive = activeTabId == tab.Id;

                if (isActive)
                {
                    Widgets.DrawAltRect(tabRect);
                    Widgets.DrawLineHorizontal(tabRect.x, tabRect.y, tabRect.width);
                    Widgets.DrawLineHorizontal(tabRect.x, tabRect.yMax, tabRect.width);
                    Widgets.DrawLineVertical(tabRect.x, tabRect.y, tabRect.height);
                    Widgets.DrawLineVertical(tabRect.xMax, tabRect.y, tabRect.height);
                }
                else
                {
                    Widgets.DrawHighlightIfMouseover(tabRect);
                }

                if (Widgets.ButtonText(tabRect, label, drawBackground: false))
                    activeTabId = tab.Id;

                x += w + 2f;
            }

            Widgets.DrawLineHorizontal(rect.x, rect.yMax, rect.width);
        }

        private static void DrawTabContent(Rect rect)
        {
            var tab = GetVisibleTabs().FirstOrDefault(t => t.Id == activeTabId);
            if (tab == null)
                return;

            Widgets.DrawMenuSection(rect);
            var inner = rect.ContractedBy(4f);
            tab.DrawAction(inner, selectedTitle, activeTabId, ref rightScrollPos);
        }

        private static IEnumerable<TabDescriptor> GetVisibleTabs()
        {
            return AllTabs.Where(t => t.IsVisible());
        }

        private class TabDescriptor
        {
            public readonly string Id;
            public readonly string LabelKey;
            public readonly Func<bool> IsVisible;
            public readonly TabDrawAction DrawAction;

            public TabDescriptor(string id, string labelKey, Func<bool> isVisible, TabDrawAction drawAction)
            {
                Id = id;
                LabelKey = labelKey;
                IsVisible = isVisible;
                DrawAction = drawAction;
            }
        }
    }
}