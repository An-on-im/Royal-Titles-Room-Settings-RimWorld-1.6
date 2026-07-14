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
    public static class RoomTabDrawer
    {
        private static readonly Dictionary<string, Func<SerializableTitleRequirements, List<SerializableRoomRequirement>>> Accessors = new()
        {
            ["bedroom"] = dto => dto.BedroomRequirements,
            ["throneRoom"] = dto => dto.ThroneRoomRequirements,
            ["ballroom"] = dto => dto.BallroomRequirements,
            ["gallery"] = dto => dto.GalleryRequirements,
        };

        public static void Draw(Rect rect, RoyalTitleDef title, string tabId, ref Vector2 scrollPos)
        {
            if (!Accessors.ContainsKey(tabId))
                return;

            var settings = SettingsManager.Instance;
            var requirements = settings.GetRequirements(title.defName, tabId);

            var headerRect = new Rect(rect.x, rect.y, rect.width, 30f);
            DrawHeader(headerRect, title);

            var listRect = new Rect(rect.x, headerRect.yMax + 4f,
                rect.width, rect.yMax - headerRect.yMax - 4f);
            DrawRequirementsList(listRect, requirements, title, tabId, ref scrollPos);
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

        private static void DrawRequirementsList(Rect rect, List<SerializableRoomRequirement> requirements,
            RoyalTitleDef title, string tabId, ref Vector2 scrollPos)
        {
            if (requirements.Count == 0)
            {
                var emptyRect = new Rect(rect.x, rect.y, rect.width, rect.height - 34f);
                UIDrawHelpers.DrawCenteredText(emptyRect, "(" + "RTRS.EmptyList".Translate() + ")", Color.gray);
                DrawAddButton(new Rect(rect.x, rect.yMax - 30f, rect.width, 28f), title, tabId);
                return;
            }

            float viewHeight = requirements.Count * 30f + 40f;
            var viewRect = new Rect(0f, 0f, rect.width - 16f, viewHeight);

            Widgets.BeginScrollView(rect, ref scrollPos, viewRect);
            float y = 0f;

            for (int i = 0; i < requirements.Count; i++)
            {
                var req = requirements[i];
                var rowRect = new Rect(viewRect.x, y, viewRect.width, 28f);
                DrawRequirementRow(rowRect, req, title, tabId, i);
                y += 30f;
            }

            DrawAddButton(new Rect(viewRect.x, y, viewRect.width, 28f), title, tabId);
            Widgets.EndScrollView();
        }

        private static void DrawRequirementRow(Rect rect, SerializableRoomRequirement req,
            RoyalTitleDef title, string tabId, int index)
        {
            string label = GetRequirementLabel(req);
            string tooltip = RequirementTypes.BuildTooltip(req);

            UIDrawHelpers.DrawListRow(rect, label,
                onClickLabel: () =>
                {
                    var settings = SettingsManager.Instance;
                    var dto = settings.EnsureOverride(title.defName);
                    var list = Accessors[tabId](dto);
                    if (list != null && index < list.Count)
                    {
                        var reqFromOverride = list[index];
                        Find.WindowStack.Add(new RequirementEditorWindow(reqFromOverride, title, tabId, index));
                    }
                },
                onRemove: () => RemoveRequirement(title, tabId, index),
                tooltip: tooltip);
        }

        public static string GetRequirementLabel(SerializableRoomRequirement req)
        {
            return RequirementDisplay.GetShortDescription(req);
        }

        private static void RemoveRequirement(RoyalTitleDef title, string tabId, int index)
        {
            var settings = SettingsManager.Instance;
            var dto = settings.EnsureOverride(title.defName);
            if (!Accessors.TryGetValue(tabId, out var accessor))
                return;
            var list = accessor(dto);
            if (list != null && index < list.Count)
                list.RemoveAt(index);
        }

        private static void DrawAddButton(Rect rect, RoyalTitleDef title, string tabId)
        {
            if (!Widgets.ButtonText(rect, "+ " + "RTRS.Add".Translate()))
                return;

            var options = RequirementTypes.AllTypes
                .Select(t => new FloatMenuOption(
                    RequirementTypes.GetLocalizedName(t),
                    () => AddRequirement(title, tabId, t)))
                .ToList();

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void AddRequirement(RoyalTitleDef title, string tabId, Type type)
        {
            var settings = SettingsManager.Instance;
            var dto = settings.EnsureOverride(title.defName);
            if (!Accessors.TryGetValue(tabId, out var accessor))
                return;
            var list = accessor(dto);
            if (list == null)
                return;

            var reqDto = new SerializableRoomRequirement
            {
                TypeName = type.FullName,
                LabelKey = ""
            };
            reqDto.ThingDefNames = new List<string>();
            reqDto.TerrainTags = new List<string>();
            reqDto.BuildingTags = new List<string>();
            reqDto.DisablingPreceptDefNames = new List<string>();

            list.Add(reqDto);
        }
    }
}