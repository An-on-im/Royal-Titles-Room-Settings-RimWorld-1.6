using RimWorld;
using RoyalTitlesRoomSettings.Services;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RoyalTitlesRoomSettings.UI
{
    public class RoomRequirementsHelpWindow : Window
    {
        private const float LeftPanelWidth = 200f;
        private readonly List<RoomRuleSet> _rules;
        private RoomRuleSet _selected;
        private Vector2 _leftScrollPos;
        private Vector2 _rightScrollPos;

        public override Vector2 InitialSize => new Vector2(900f, 600f);

        public RoomRequirementsHelpWindow()
        {
            doCloseX = true;
            draggable = true;
            resizeable = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            _rules = RoomRulesCache.GetAll();
            _selected = _rules.FirstOrDefault();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            var titleRect = new Rect(inRect.x, inRect.y, inRect.width, 30f);
            Widgets.Label(titleRect, "RTRS.HelpWindowTitle".Translate());
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            var contentRect = new Rect(inRect.x, inRect.y + 34f, inRect.width, inRect.height - 34f);
            var leftRect = new Rect(contentRect.x, contentRect.y, LeftPanelWidth, contentRect.height);
            var rightRect = new Rect(contentRect.x + LeftPanelWidth + 6f, contentRect.y,
                contentRect.width - LeftPanelWidth - 6f, contentRect.height);

            Widgets.DrawLineVertical(leftRect.xMax, leftRect.y, leftRect.height);

            DrawLeftPanel(leftRect);
            DrawRightPanel(rightRect);
        }

        private void DrawLeftPanel(Rect rect)
        {
            float totalHeight = _rules.Count * 30f + 10f;
            var viewRect = new Rect(0f, 0f, rect.width - 16f, totalHeight);
            Widgets.BeginScrollView(rect, ref _leftScrollPos, viewRect);

            float y = 4f;
            foreach (var ruleSet in _rules)
            {
                var rowRect = new Rect(viewRect.x, y, viewRect.width, 28f);
                bool isSelected = _selected == ruleSet;
                if (isSelected)
                    Widgets.DrawBoxSolid(rowRect, new Color(0.3f, 0.5f, 0.8f, 0.3f));

                if (Widgets.ButtonInvisible(rowRect))
                {
                    _selected = ruleSet;
                    _rightScrollPos = Vector2.zero;
                }

                Widgets.DrawHighlightIfMouseover(rowRect);

                string label = GetRoomLabel(ruleSet);
                var textRect = new Rect(rowRect.x + 6f, rowRect.y, rowRect.width - 12f, rowRect.height);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(textRect, label);
                Text.Anchor = TextAnchor.UpperLeft;

                y += 30f;
            }

            Widgets.EndScrollView();
        }

        private string GetRoomLabel(RoomRuleSet ruleSet)
        {
            var def = ruleSet.GetRoleDef();
            if (def != null)
                return def.LabelCap;
            return ruleSet.RoleDefName;
        }

        private void DrawRightPanel(Rect rect)
        {
            if (_selected == null)
            {
                UIDrawHelpers.DrawCenteredText(rect, "RTRS.HelpNoRoomSelected".Translate(), Color.gray);
                return;
            }

            Widgets.DrawMenuSection(rect);
            var inner = rect.ContractedBy(8f);

            float contentHeight = CalcContentHeight(_selected);
            var viewRect = new Rect(0f, 0f, inner.width - 16f, Mathf.Max(contentHeight, inner.height));
            Widgets.BeginScrollView(inner, ref _rightScrollPos, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            Text.Font = GameFont.Medium;
            listing.Label(GetRoomLabel(_selected));
            Text.Font = GameFont.Small;
            listing.Gap(4f);

            if (!string.IsNullOrEmpty(_selected.DescriptionKey))
            {
                string desc = _selected.DescriptionKey.Translate();
                var savedColor = GUI.color;
                GUI.color = Color.gray;
                listing.Label(desc);
                GUI.color = savedColor;
                listing.Gap(12f);
            }

            listing.GapLine();
            listing.Gap(8f);

            Text.Font = GameFont.Small;
            listing.Label("RTRS.HelpRulesHeader".Translate());
            listing.Gap(8f);

            foreach (var rule in _selected.Rules)
            {
                DrawRule(listing, rule);
                listing.Gap(6f);
            }

            listing.End();
            Widgets.EndScrollView();
        }

        private float CalcContentHeight(RoomRuleSet ruleSet)
        {
            float h = 80f; // title + description + gap
            foreach (var rule in ruleSet.Rules)
            {
                h += 50f; // label + details
                h += GetRuleDetailsHeight(rule);
            }
            return h;
        }

        private float GetRuleDetailsHeight(RoomRule rule)
        {
            float h = 0f;

            if (rule is RequiredBedConfigRule rb)
            {
                h += 20f; // description line
                h += rb.AllowedThingDefNames.Count * 18f + 4f; // list
                if (rb.MaxCount > 0)
                    h += 18f;
            }
            else if (rule is BedroomLogicRule bl)
            {
                if (!string.IsNullOrEmpty(bl.Description))
                    h += 36f;
            }
            else if (rule is RequiredThingClassCountRule rc)
            {
                h += 20f; // min count line
                h += rc.AllowedThingDefNames.Count * 18f + 4f; // list
            }
            else if (rule is RequiredThingDefRule rd)
            {
                h += rd.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredSurfaceTypeRule rs)
            {
                h += rs.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredJoyGiverRule rj)
            {
                h += rj.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredAltarWithMemeRule ra)
            {
                h += ra.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredStorageRule rst)
            {
                h += rst.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredSarcophagusRule rsa)
            {
                h += rsa.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredEntityHolderRule reh)
            {
                h += reh.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredDeathrestRule rde)
            {
                h += rde.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredProductionRecipeRule rpr)
            {
                h += rpr.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredWorkTableRoleRule rwt)
            {
                h += rwt.AllowedThingDefNames.Count * 18f + 4f;
            }
            else if (rule is RequiredBabyPlayRule rbp)
            {
                h += rbp.AllowedThingDefNames.Count * 18f + 4f;
            }

            return h;
        }

        private void DrawRule(Listing_Standard listing, RoomRule rule)
        {
            string label = rule.LabelKey.Translate();

            // Маркер и название
            Text.Font = GameFont.Small;
            var labelRect = listing.GetRect(22f);
            Widgets.Label(labelRect, "• " + label);

            if (!string.IsNullOrEmpty(rule.TooltipKey))
            {
                TooltipHandler.TipRegion(labelRect, rule.TooltipKey.Translate());
            }

            // Дополнительные детали
            DrawRuleDetails(listing, rule);
        }

        private void DrawRuleDetails(Listing_Standard listing, RoomRule rule)
        {
            var savedFont = Text.Font;
            var savedColor = GUI.color;
            Text.Font = GameFont.Tiny;
            GUI.color = new Color(0.65f, 0.65f, 0.65f);

            if (rule is ForbiddenThingRequestGroupRule fg)
            {
                if (fg.ExampleDefNames != null && fg.ExampleDefNames.Count > 0)
                {
                    string examples = string.Join(", ", fg.ExampleDefNames.Select(GetThingLabel));
                    var r = listing.GetRect(18f);
                    Widgets.Label(new Rect(r.x + 16f, r.y, r.width - 16f, r.height),
                        "RTRS.HelpExamples".Translate() + ": " + examples);
                }
            }
            else if (rule is RequiredThingClassCountRule rc)
            {
                string className = rc.ClassLabelKey.Translate();
                var r = listing.GetRect(18f);
                Widgets.Label(new Rect(r.x + 16f, r.y, r.width - 16f, r.height),
                    "RTRS.HelpMinCount".Translate(rc.MinCount, className));

                DrawThingList(listing, rc.AllowedThingDefNames);
            }
            else if (rule is RequiredBedConfigRule rb)
            {
                // Description line
                if (!string.IsNullOrEmpty(rb.Description))
                {
                    var r = listing.GetRect(18f);
                    Widgets.Label(new Rect(r.x + 16f, r.y, r.width - 16f, r.height),
                        rb.Description.Translate());
                }

                // Allowed things list
                DrawThingList(listing, rb.AllowedThingDefNames);

                if (rb.MaxCount > 0)
                {
                    var r2 = listing.GetRect(18f);
                    Widgets.Label(new Rect(r2.x + 16f, r2.y, r2.width - 16f, r2.height),
                        "RTRS.HelpMaxCount".Translate(rb.MaxCount));
                }
            }
            else if (rule is RequiredThingDefRule rd)
            {
                DrawThingList(listing, rd.AllowedThingDefNames);
            }
            else if (rule is RequiredWorkTableRoleRule rw)
            {
                var roleDef = DefDatabase<RoomRoleDef>.GetNamedSilentFail(rw.RoleDefName);
                string roleName = roleDef != null ? roleDef.LabelCap : rw.RoleDefName;
                var r = listing.GetRect(18f);
                Widgets.Label(new Rect(r.x + 16f, r.y, r.width - 16f, r.height),
                    "RTRS.HelpWorkTableRole".Translate() + ": " + roleName);

                DrawThingList(listing, rw.AllowedThingDefNames);
            }
            else if (rule is ForbiddenWorkTableRoleRule fw)
            {
                var roleDef = DefDatabase<RoomRoleDef>.GetNamedSilentFail(fw.AllowedRoleDefName);
                string roleName = roleDef != null ? roleDef.LabelCap : fw.AllowedRoleDefName;
                var r = listing.GetRect(18f);
                Widgets.Label(new Rect(r.x + 16f, r.y, r.width - 16f, r.height),
                    "RTRS.HelpAllowedRole".Translate() + ": " + roleName);
            }
            else if (rule is BedroomLogicRule bl)
            {
                if (!string.IsNullOrEmpty(bl.Description))
                {
                    var r = listing.GetRect(36f);
                    Widgets.Label(new Rect(r.x + 16f, r.y, r.width - 16f, r.height),
                        bl.Description.Translate());
                }
            }
            else if (rule is RequiredSurfaceTypeRule rs)
            {
                DrawThingList(listing, rs.AllowedThingDefNames);
            }
            else if (rule is RequiredJoyGiverRule rj)
            {
                DrawThingList(listing, rj.AllowedThingDefNames);
            }
            else if (rule is RequiredAltarWithMemeRule ra)
            {
                DrawThingList(listing, ra.AllowedThingDefNames);
            }
            else if (rule is RequiredStorageRule rst)
            {
                DrawThingList(listing, rst.AllowedThingDefNames);
            }
            else if (rule is RequiredSarcophagusRule rsa)
            {
                DrawThingList(listing, rsa.AllowedThingDefNames);
            }
            else if (rule is RequiredEntityHolderRule reh)
            {
                DrawThingList(listing, reh.AllowedThingDefNames);
            }
            else if (rule is RequiredDeathrestRule rde)
            {
                DrawThingList(listing, rde.AllowedThingDefNames);
            }
            else if (rule is RequiredProductionRecipeRule rpr)
            {
                DrawThingList(listing, rpr.AllowedThingDefNames);
            }
            else if (rule is RequiredBabyPlayRule rbp)
            {
                DrawThingList(listing, rbp.AllowedThingDefNames);
            }

            GUI.color = savedColor;
            Text.Font = savedFont;
        }

        private void DrawThingList(Listing_Standard listing, List<string> defNames)
        {
            if (defNames == null || defNames.Count == 0)
                return;

            var r = listing.GetRect(18f);
            Widgets.Label(new Rect(r.x + 16f, r.y, r.width - 16f, r.height),
                "RTRS.HelpAllowedThings".Translate() + ":");

            foreach (var defName in defNames)
            {
                var itemRect = listing.GetRect(18f);
                Widgets.Label(new Rect(itemRect.x + 32f, itemRect.y, itemRect.width - 32f, itemRect.height),
                    "  • " + GetThingLabel(defName));
            }
        }

        private string GetThingLabel(string defName)
        {
            if (string.IsNullOrEmpty(defName))
                return "?";
            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def != null)
                return def.LabelCap;
            return defName;
        }
    }
}