using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RoyalTitlesRoomSettings.Services
{
    public static class RoomRulesCache
    {
        private static List<RoomRuleSet> _cache;
        private static readonly object _lock = new();
        private static readonly System.Type BedType = typeof(Building_Bed);

        public static List<RoomRuleSet> GetAll()
        {
            if (_cache != null)
            {
                return _cache;
            }
            lock (_lock)
            {
                if (_cache != null)
                {
                    return _cache;
                }
                _cache = BuildCache();
            }
            return _cache;
        }

        public static void Invalidate()
        {
            lock (_lock)
            {
                _cache = null;
            }
        }

        private static List<RoomRuleSet> BuildCache()
        {
            List<RoomRuleSet> result =
            [
                new RoomRuleSet
                {
                    RoleDefName = "Bedroom",
                    DescriptionKey = "RTRS.RoomDesc_Bedroom",
                    Rules =
                    [
                        new RequiredBedConfigRule
                        {
                            LabelKey = "RTRS.Rule_RequiredHumanBed",
                            TooltipKey = "RTRS.Rule_RequiredHumanBed_Desc",
                            Humanlike = true, CountsForBedroom = true, Medical = false, ForPrisoners = false, MinCount = 1,
                            Description = "RTRS.BedDesc_HumanBed",
                            AllowedThingDefNames = GetHumanBedsForBedroom()
                        },
                        new BedroomLogicRule
                        {
                            LabelKey = "RTRS.Rule_BedroomLogic",
                            TooltipKey = "RTRS.Rule_BedroomLogic_Desc",
                            Description = "RTRS.BedroomLogicDesc"
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "ThroneRoom",
                    DescriptionKey = "RTRS.RoomDesc_ThroneRoom",
                    Rules =
                    [
                        new RequiredThingClassCountRule
                        {
                            LabelKey = "RTRS.Rule_RequiredThrone",
                            TooltipKey = "RTRS.Rule_RequiredThrone_Desc",
                            ClassName = "Building_Throne", ClassLabelKey = "RTRS.Class_Throne", MinCount = 1,
                            AllowedThingDefNames = GetThrones()
                        },
                        new ForbiddenOutdoorsRule
                        {
                            LabelKey = "RTRS.Rule_MustBeIndoors",
                            TooltipKey = "RTRS.Rule_MustBeIndoors_Desc"
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "DiningRoom",
                    DescriptionKey = "RTRS.RoomDesc_DiningRoom",
                    Rules =
                    [
                        new RequiredSurfaceTypeRule
                        {
                            LabelKey = "RTRS.Rule_RequiredEatSurface",
                            TooltipKey = "RTRS.Rule_RequiredEatSurface_Desc",
                            SurfaceType = "Eat",
                            AllowedThingDefNames = GetEatSurfaces()
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "RecRoom",
                    DescriptionKey = "RTRS.RoomDesc_RecRoom",
                    Rules =
                    [
                        new RequiredJoyGiverRule
                        {
                            LabelKey = "RTRS.Rule_RequiredJoyGiver",
                            TooltipKey = "RTRS.Rule_RequiredJoyGiver_Desc",
                            AllowedThingDefNames = GetJoyGiverThings()
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "Hospital",
                    DescriptionKey = "RTRS.RoomDesc_Hospital",
                    Rules =
                    [
                        new RequiredBedConfigRule
                        {
                            LabelKey = "RTRS.Rule_RequiredMedicalBed",
                            TooltipKey = "RTRS.Rule_RequiredMedicalBed_Desc",
                            Humanlike = true, Medical = true, ForPrisoners = false, MinCount = 1,
                            Description = "RTRS.BedDesc_MedicalBed",
                            AllowedThingDefNames = GetMedicalBeds()
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "PrisonCell",
                    DescriptionKey = "RTRS.RoomDesc_PrisonCell",
                    Rules =
                    [
                        new RequiredBedConfigRule
                        {
                            LabelKey = "RTRS.Rule_RequiredPrisonerBed",
                            TooltipKey = "RTRS.Rule_RequiredPrisonerBed_Desc",
                            Humanlike = true, ForPrisoners = true, MinCount = 1, MaxCount = 1,
                            Description = "RTRS.BedDesc_PrisonerBed",
                            AllowedThingDefNames = GetHumanBeds()
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "PrisonBarracks",
                    DescriptionKey = "RTRS.RoomDesc_PrisonBarracks",
                    Rules =
                    [
                        new RequiredBedConfigRule
                        {
                            LabelKey = "RTRS.Rule_RequiredPrisonerBeds",
                            TooltipKey = "RTRS.Rule_RequiredPrisonerBeds_Desc",
                            Humanlike = true, ForPrisoners = true, MinCount = 2,
                            Description = "RTRS.BedDesc_PrisonerBed",
                            AllowedThingDefNames = GetHumanBeds()
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "Barn",
                    DescriptionKey = "RTRS.RoomDesc_Barn",
                    Rules =
                    [
                        new RequiredBedConfigRule
                        {
                            LabelKey = "RTRS.Rule_RequiredAnimalBed",
                            TooltipKey = "RTRS.Rule_RequiredAnimalBed_Desc",
                            Humanlike = false, MinCount = 1,
                            Description = "RTRS.BedDesc_AnimalBed",
                            AllowedThingDefNames = GetAnimalBeds()
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "Barracks",
                    DescriptionKey = "RTRS.RoomDesc_Barracks",
                    Rules =
                    [
                        new RequiredBedConfigRule
                        {
                            LabelKey = "RTRS.Rule_RequiredHumanBeds",
                            TooltipKey = "RTRS.Rule_RequiredHumanBeds_Desc",
                            Humanlike = true, CountsForBedroom = true, Medical = false, ForPrisoners = false, MinCount = 2,
                            Description = "RTRS.BedDesc_HumanBed",
                            AllowedThingDefNames = GetHumanBedsForBedroom()
                        },
                        new BedroomLogicRule
                        {
                            LabelKey = "RTRS.Rule_NotBedroom",
                            TooltipKey = "RTRS.Rule_NotBedroom_Desc",
                            Inverted = true,
                            Description = "RTRS.BarracksLogicDesc"
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "Tomb",
                    DescriptionKey = "RTRS.RoomDesc_Tomb",
                    Rules =
                    [
                        new RequiredSarcophagusRule
                        {
                            LabelKey = "RTRS.Rule_RequiredSarcophagus",
                            TooltipKey = "RTRS.Rule_RequiredSarcophagus_Desc",
                            AllowedThingDefNames = GetSarcophagi()
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "StoreRoom",
                    DescriptionKey = "RTRS.RoomDesc_StoreRoom",
                    Rules =
                    [
                        new RequiredStorageRule
                        {
                            LabelKey = "RTRS.Rule_RequiredStorage",
                            TooltipKey = "RTRS.Rule_RequiredStorage_Desc",
                            AllowedThingDefNames = GetStorages()
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "Kitchen",
                    DescriptionKey = "RTRS.RoomDesc_Kitchen",
                    Rules =
                    [
                        new RequiredProductionRecipeRule
                        {
                            LabelKey = "RTRS.Rule_RequiredKitchen",
                            TooltipKey = "RTRS.Rule_RequiredKitchen_Desc",
                            AllowedThingDefNames = GetKitchenWorkTables()
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "Workshop",
                    DescriptionKey = "RTRS.RoomDesc_Workshop",
                    Rules =
                    [
                        new RequiredWorkTableRoleRule
                        {
                            LabelKey = "RTRS.Rule_RequiredWorkshop",
                            TooltipKey = "RTRS.Rule_RequiredWorkshop_Desc",
                            RoleDefName = "Workshop",
                            AllowedThingDefNames = GetWorkTablesByRole("Workshop")
                        },
                    ]
                },
                new RoomRuleSet
                {
                    RoleDefName = "Laboratory",
                    DescriptionKey = "RTRS.RoomDesc_Laboratory",
                    Rules =
                    [
                        new RequiredWorkTableRoleRule
                        {
                            LabelKey = "RTRS.Rule_RequiredLaboratory",
                            TooltipKey = "RTRS.Rule_RequiredLaboratory_Desc",
                            RoleDefName = "Laboratory",
                            AllowedThingDefNames = GetWorkTablesByRole("Laboratory")
                        },
                    ]
                }
            ];

            // ==================== Ideology ====================
            if (ModsConfig.IdeologyActive)
            {
                result.Add(new RoomRuleSet
                {
                    RoleDefName = "WorshipRoom",
                    DescriptionKey = "RTRS.RoomDesc_WorshipRoom",
                    Rules =
                    [
                        new RequiredAltarWithMemeRule
                        {
                            LabelKey = "RTRS.Rule_RequiredAltar",
                            TooltipKey = "RTRS.Rule_RequiredAltar_Desc",
                            AllowedThingDefNames = GetAltars()
                        },
                    ]
                });
            }

            // ==================== Biotech ====================
            if (ModsConfig.BiotechActive)
            {
                result.Add(new RoomRuleSet
                {
                    RoleDefName = "Nursery",
                    DescriptionKey = "RTRS.RoomDesc_Nursery",
                    Rules =
                    [
                        new RequiredBedConfigRule
                        {
                            LabelKey = "RTRS.Rule_RequiredChildBeds",
                            TooltipKey = "RTRS.Rule_RequiredChildBeds_Desc",
                            Humanlike = true, ChildSize = true, Medical = false, ForPrisoners = false, MinCount = 2,
                            Description = "RTRS.BedDesc_ChildBed",
                            AllowedThingDefNames = GetChildBeds()
                        },
                    ]
                });

                result.Add(new RoomRuleSet
                {
                    RoleDefName = "Playroom",
                    DescriptionKey = "RTRS.RoomDesc_Playroom",
                    Rules =
                    [
                        new ForbiddenThingClassRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenHumanBeds",
                            TooltipKey = "RTRS.Rule_ForbiddenHumanBeds_Desc",
                            ClassName = "Building_Bed", ClassLabelKey = "RTRS.Class_HumanBed"
                        },
                        new RequiredBabyPlayRule
                        {
                            LabelKey = "RTRS.Rule_RequiredBabyPlay",
                            TooltipKey = "RTRS.Rule_RequiredBabyPlay_Desc",
                            AllowedThingDefNames = GetBabyPlayThings()
                        },
                    ]
                });

                result.Add(new RoomRuleSet
                {
                    RoleDefName = "Classroom",
                    DescriptionKey = "RTRS.RoomDesc_Classroom",
                    Rules =
                    [
                        new ForbiddenThingClassRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenHumanBeds",
                            TooltipKey = "RTRS.Rule_ForbiddenHumanBeds_Desc",
                            ClassName = "Building_Bed", ClassLabelKey = "RTRS.Class_HumanBed"
                        },
                        new RequiredThingDefRule
                        {
                            LabelKey = "RTRS.Rule_RequiredSchoolFurniture",
                            TooltipKey = "RTRS.Rule_RequiredSchoolFurniture_Desc",
                            DefName = "SchoolDesk", IsAnyOf = true, AltDefName = "Blackboard",
                            AllowedThingDefNames = ["SchoolDesk", "Blackboard"]
                        },
                    ]
                });

                result.Add(new RoomRuleSet
                {
                    RoleDefName = "DeathrestChamber",
                    DescriptionKey = "RTRS.RoomDesc_DeathrestChamber",
                    Rules =
                    [
                        new RequiredDeathrestRule
                        {
                            LabelKey = "RTRS.Rule_RequiredDeathrest",
                            TooltipKey = "RTRS.Rule_RequiredDeathrest_Desc",
                            AllowedThingDefNames = GetDeathrestThings()
                        },
                    ]
                });
            }

            // ==================== Anomaly ====================
            if (ModsConfig.AnomalyActive)
            {
                result.Add(new RoomRuleSet
                {
                    RoleDefName = "ContainmentCell",
                    DescriptionKey = "RTRS.RoomDesc_ContainmentCell",
                    Rules =
                    [
                        new RequiredEntityHolderRule
                        {
                            LabelKey = "RTRS.Rule_RequiredEntityHolder",
                            TooltipKey = "RTRS.Rule_RequiredEntityHolder_Desc",
                            AllowedThingDefNames = GetEntityHolderThings()
                        },
                    ]
                });

                result.Add(new RoomRuleSet
                {
                    RoleDefName = "CeremonialChamber",
                    DescriptionKey = "RTRS.RoomDesc_CeremonialChamber",
                    Rules =
                    [
                        new RequiredThingDefRule
                        {
                            LabelKey = "RTRS.Rule_RequiredPsychicRitualSpot",
                            TooltipKey = "RTRS.Rule_RequiredPsychicRitualSpot_Desc",
                            DefName = "PsychicRitualSpot",
                            AllowedThingDefNames = ["PsychicRitualSpot"]
                        },
                    ]
                });
            }

            // ==================== VFE Empire ====================
            if (VFEIntegration.IsVfeLoaded)
            {
                result.Add(new RoomRuleSet
                {
                    RoleDefName = "VFEE_Ballroom",
                    DescriptionKey = "RTRS.RoomDesc_Ballroom",
                    Rules =
                    [
                        new ForbiddenThingRequestGroupRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenEntityHolder",
                            TooltipKey = "RTRS.Rule_ForbiddenEntityHolder_Desc",
                            GroupName = "EntityHolder",
                            ExampleDefNames = ["Sarcophagus", "CryptosleepCasket", "AncientCryptosleepCasket"]
                        },
                        new ForbiddenThingClassRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenBeds",
                            TooltipKey = "RTRS.Rule_ForbiddenBeds_Desc",
                            ClassName = "Building_Bed", ClassLabelKey = "RTRS.Class_Bed"
                        },
                        new ForbiddenThingClassRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenThrones",
                            TooltipKey = "RTRS.Rule_ForbiddenThrones_Desc",
                            ClassName = "Building_Throne", ClassLabelKey = "RTRS.Class_Throne"
                        },
                        new ForbiddenThingClassRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenWorkTables",
                            TooltipKey = "RTRS.Rule_ForbiddenWorkTables_Desc",
                            ClassName = "Building_WorkTable", ClassLabelKey = "RTRS.Class_WorkTable"
                        },
                        new ForbiddenWorkTableRoleRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenOtherWorkTableRole",
                            TooltipKey = "RTRS.Rule_ForbiddenOtherWorkTableRole_Desc",
                            AllowedRoleDefName = "VFEE_Ballroom"
                        },
                        new RequiredThingClassCountRule
                        {
                            LabelKey = "RTRS.Rule_RequiredMusicalInstrument",
                            TooltipKey = "RTRS.Rule_RequiredMusicalInstrument_Desc",
                            ClassName = "Building_MusicalInstrument", ClassLabelKey = "RTRS.Class_MusicalInstrument", MinCount = 1,
                            AllowedThingDefNames = GetMusicalInstruments()
                        },
                    ]
                });

                result.Add(new RoomRuleSet
                {
                    RoleDefName = "VFEE_Gallery",
                    DescriptionKey = "RTRS.RoomDesc_Gallery",
                    Rules =
                    [
                        new ForbiddenThingRequestGroupRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenEntityHolder",
                            TooltipKey = "RTRS.Rule_ForbiddenEntityHolder_Desc",
                            GroupName = "EntityHolder",
                            ExampleDefNames = ["Sarcophagus", "CryptosleepCasket", "AncientCryptosleepCasket"]
                        },
                        new ForbiddenThingClassRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenBeds",
                            TooltipKey = "RTRS.Rule_ForbiddenBeds_Desc",
                            ClassName = "Building_Bed", ClassLabelKey = "RTRS.Class_Bed"
                        },
                        new ForbiddenThingClassRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenThrones",
                            TooltipKey = "RTRS.Rule_ForbiddenThrones_Desc",
                            ClassName = "Building_Throne", ClassLabelKey = "RTRS.Class_Throne"
                        },
                        new ForbiddenThingClassRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenWorkTables",
                            TooltipKey = "RTRS.Rule_ForbiddenWorkTables_Desc",
                            ClassName = "Building_WorkTable", ClassLabelKey = "RTRS.Class_WorkTable"
                        },
                        new ForbiddenWorkTableRoleRule
                        {
                            LabelKey = "RTRS.Rule_ForbiddenOtherWorkTableRole",
                            TooltipKey = "RTRS.Rule_ForbiddenOtherWorkTableRole_Desc",
                            AllowedRoleDefName = "VFEE_Gallery"
                        },
                        new RequiredThingClassCountRule
                        {
                            LabelKey = "RTRS.Rule_RequiredArt",
                            TooltipKey = "RTRS.Rule_RequiredArt_Desc",
                            ClassName = "Building_Art", ClassLabelKey = "RTRS.Class_Art", MinCount = 4,
                            AllowedThingDefNames = GetArtThings()
                        },
                    ]
                });
            }

            return result;
        }

        // ==================== Helper methods for collecting ThingDefs ====================

        private static List<string> GetHumanBedsForBedroom()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.building != null
                    && d.thingClass != null
                    && BedType.IsAssignableFrom(d.thingClass)
                    && d.building.bed_humanlike
                    && d.building.bed_countsForBedroomOrBarracks
                    && !d.building.bed_defaultMedical)
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetHumanBeds()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.building != null
                    && d.thingClass != null
                    && BedType.IsAssignableFrom(d.thingClass)
                    && d.building.bed_humanlike)
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetMedicalBeds()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.building != null
                    && d.building.bed_humanlike
                    && d.building.bed_defaultMedical)
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetAnimalBeds()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.building != null && !d.building.bed_humanlike)
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetChildBeds()
        {
            float childSizeFactor = LifeStageDefOf.HumanlikeChild?.bodySizeFactor ?? 0.5f;
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.building != null
                    && d.building.bed_humanlike
                    && !d.building.bed_defaultMedical
                    && d.building.bed_maxBodySize < childSizeFactor)
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetThrones()
        {
            var throneType = typeof(RimWorld.Building_Throne);
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.thingClass != null && throneType.IsAssignableFrom(d.thingClass))
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetEatSurfaces()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.category == ThingCategory.Building && d.surfaceType == SurfaceType.Eat)
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetJoyGiverThings()
        {
            var result = new HashSet<string>();
            foreach (var jg in DefDatabase<JoyGiverDef>.AllDefsListForReading)
            {
                if (jg.countsForRecRoom && jg.thingDefs != null)
                {
                    foreach (var t in jg.thingDefs)
                    {
                        _ = result.Add(t.defName);
                    }
                }
            }
            return result.OrderBy(n => n).ToList();
        }

        private static List<string> GetSarcophagi()
        {
            var sarcophagusType = typeof(RimWorld.Building_Sarcophagus);
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.thingClass != null && sarcophagusType.IsAssignableFrom(d.thingClass))
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetStorages()
        {
            var storageType = typeof(RimWorld.Building_Storage);
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.thingClass != null && storageType.IsAssignableFrom(d.thingClass))
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetKitchenWorkTables()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.designationCategory == DesignationCategoryDefOf.Production
                    && d.AllRecipes != null
                    && d.AllRecipes.Any(r => r.products != null
                        && r.products.Any(p => p.thingDef != null
                            && p.thingDef.IsNutritionGivingIngestible
                            && p.thingDef.ingestible != null
                            && p.thingDef.ingestible.HumanEdible)))
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetWorkTablesByRole(string roleDefName)
        {
            var roleDef = DefDatabase<RoomRoleDef>.GetNamedSilentFail(roleDefName);
            return roleDef == null
                ? []
                : DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(d => d.building != null && d.building.workTableRoomRole == roleDef)
                    .Select(d => d.defName)
                    .OrderBy(n => n)
                    .ToList();
        }

        private static List<string> GetAltars()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.isAltar)
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetBabyPlayThings()
        {
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.GetStatValueAbstract(StatDefOf.BabyPlayGainFactor) > 1f)
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetDeathrestThings()
        {
            var result = new HashSet<string>();
            var deathrestCasket = DefDatabase<ThingDef>.GetNamedSilentFail("DeathrestCasket");
            if (deathrestCasket != null)
            {
                _ = result.Add("DeathrestCasket");
            }
            var deathrestBindableType = typeof(RimWorld.CompDeathrestBindable);
            foreach (var d in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (d.comps != null && d.comps.Any(c => c.compClass != null && deathrestBindableType.IsAssignableFrom(c.compClass)))
                {
                    _ = result.Add(d.defName);
                }
            }
            return result.OrderBy(n => n).ToList();
        }

        private static List<string> GetEntityHolderThings()
        {
            var result = new HashSet<string>();
            foreach (var d in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (ThingRequestGroup.EntityHolder.Includes(d))
                {
                    _ = result.Add(d.defName);
                }
            }
            var electroharvester = DefDatabase<ThingDef>.GetNamedSilentFail("Electroharvester");
            if (electroharvester != null)
            {
                _ = result.Add("Electroharvester");
            }
            var electricInhibitor = DefDatabase<ThingDef>.GetNamedSilentFail("ElectricInhibitor");
            if (electricInhibitor != null)
            {
                _ = result.Add("ElectricInhibitor");
            }
            var bioferriteHarvester = DefDatabase<ThingDef>.GetNamedSilentFail("BioferriteHarvester");
            if (bioferriteHarvester != null)
            {
                _ = result.Add("BioferriteHarvester");
            }
            return result.OrderBy(n => n).ToList();
        }

        private static List<string> GetMusicalInstruments()
        {
            var instrumentType = GenTypes.GetTypeInAnyAssembly("Building_MusicalInstrument");
            if (instrumentType == null)
            {
                return DefDatabase<ThingDef>.AllDefsListForReading
                    .Where(d => d.defName is "Harp" or "Harpsichord" or "Piano"
                        or "VFEE_PipeOrgan")
                    .Select(d => d.defName)
                    .OrderBy(n => n)
                    .ToList();
            }
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.thingClass != null && instrumentType.IsAssignableFrom(d.thingClass))
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }

        private static List<string> GetArtThings()
        {
            var artType = typeof(RimWorld.Building_Art);
            return DefDatabase<ThingDef>.AllDefsListForReading
                .Where(d => d.thingClass != null && artType.IsAssignableFrom(d.thingClass))
                .Select(d => d.defName)
                .OrderBy(n => n)
                .ToList();
        }
    }

    public class RoomRuleSet
    {
        public string RoleDefName;
        public string DescriptionKey;
        public List<RoomRule> Rules = [];

        public RoomRoleDef GetRoleDef()
        {
            return DefDatabase<RoomRoleDef>.GetNamedSilentFail(RoleDefName);
        }
    }

    public abstract class RoomRule
    {
        public string LabelKey;
        public string TooltipKey;
    }

    public class ForbiddenThingRequestGroupRule : RoomRule
    {
        public string GroupName;
        public List<string> ExampleDefNames;
    }

    public class ForbiddenThingClassRule : RoomRule
    {
        public string ClassName;
        public string ClassLabelKey;
    }

    public class ForbiddenWorkTableRoleRule : RoomRule
    {
        public string AllowedRoleDefName;
    }

    public class ForbiddenOutdoorsRule : RoomRule
    {
    }

    public class RequiredThingClassCountRule : RoomRule
    {
        public string ClassName;
        public string ClassLabelKey;
        public int MinCount;
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredThingDefRule : RoomRule
    {
        public string DefName;
        public bool IsAnyOf;
        public string AltDefName;
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredBedConfigRule : RoomRule
    {
        public bool Humanlike;
        public bool CountsForBedroom;
        public bool Medical;
        public bool ForPrisoners;
        public bool ChildSize;
        public int MinCount;
        public int MaxCount = -1;
        public string Description;
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredSurfaceTypeRule : RoomRule
    {
        public string SurfaceType;
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredJoyGiverRule : RoomRule
    {
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredAltarWithMemeRule : RoomRule
    {
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredStorageRule : RoomRule
    {
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredSarcophagusRule : RoomRule
    {
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredEntityHolderRule : RoomRule
    {
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredDeathrestRule : RoomRule
    {
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredProductionRecipeRule : RoomRule
    {
        public List<string> AllowedThingDefNames = [];
    }

    public class RequiredWorkTableRoleRule : RoomRule
    {
        public string RoleDefName;
        public List<string> AllowedThingDefNames = [];
    }

    public class BedroomLogicRule : RoomRule
    {
        public bool Inverted;
        public string Description;
    }

    public class RequiredBabyPlayRule : RoomRule
    {
        public List<string> AllowedThingDefNames = [];
    }
}