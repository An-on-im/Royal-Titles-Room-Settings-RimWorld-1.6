using System;
using System.Collections.Generic;
using Verse;

namespace RoyalTitlesRoomSettings.DTO
{
    [Serializable]
    public class SerializableTitleRequirements : IExposable
    {
        public string TitleDefName;
        public List<SerializableRoomRequirement> BedroomRequirements = new();
        public List<SerializableRoomRequirement> ThroneRoomRequirements = new();
        public List<SerializableRoomRequirement> BallroomRequirements = new();
        public List<SerializableRoomRequirement> GalleryRequirements = new();
        public List<SerializableCourtRequirement> CourtRequirements = new();

        public void ExposeData()
        {
            Scribe_Values.Look(ref TitleDefName, "TitleDefName");
            Scribe_Collections.Look(ref BedroomRequirements, "BedroomRequirements", LookMode.Deep);
            Scribe_Collections.Look(ref ThroneRoomRequirements, "ThroneRoomRequirements", LookMode.Deep);
            Scribe_Collections.Look(ref BallroomRequirements, "BallroomRequirements", LookMode.Deep);
            Scribe_Collections.Look(ref GalleryRequirements, "GalleryRequirements", LookMode.Deep);
            Scribe_Collections.Look(ref CourtRequirements, "CourtRequirements", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                BedroomRequirements ??= new();
                ThroneRoomRequirements ??= new();
                BallroomRequirements ??= new();
                GalleryRequirements ??= new();
                CourtRequirements ??= new();
            }
        }

        public SerializableTitleRequirements DeepClone()
        {
            var clone = new SerializableTitleRequirements
            {
                TitleDefName = this.TitleDefName
            };
            clone.BedroomRequirements.AddRange(this.BedroomRequirements.ConvertAll(r => r.DeepClone()));
            clone.ThroneRoomRequirements.AddRange(this.ThroneRoomRequirements.ConvertAll(r => r.DeepClone()));
            clone.BallroomRequirements.AddRange(this.BallroomRequirements.ConvertAll(r => r.DeepClone()));
            clone.GalleryRequirements.AddRange(this.GalleryRequirements.ConvertAll(r => r.DeepClone()));
            clone.CourtRequirements.AddRange(this.CourtRequirements.ConvertAll(c => c.DeepClone()));
            return clone;
        }
    }

    [Serializable]
    public class SerializableRoomRequirement : IExposable
    {
        public string TypeName;
        public string LabelKey;

        public int? Area;
        public int? Impressiveness;
        public string ThingDefName;
        public int? Count;
        public List<string> ThingDefNames = new();
        public List<string> TerrainTags = new();
        public List<string> BuildingTags = new();
        public List<string> DisablingPreceptDefNames = new();

        public void ExposeData()
        {
            Scribe_Values.Look(ref TypeName, "TypeName");
            Scribe_Values.Look(ref LabelKey, "LabelKey");
            Scribe_Values.Look(ref Area, "Area");
            Scribe_Values.Look(ref Impressiveness, "Impressiveness");
            Scribe_Values.Look(ref ThingDefName, "ThingDefName");
            Scribe_Values.Look(ref Count, "Count");
            Scribe_Collections.Look(ref ThingDefNames, "ThingDefNames", LookMode.Value);
            Scribe_Collections.Look(ref TerrainTags, "TerrainTags", LookMode.Value);
            Scribe_Collections.Look(ref BuildingTags, "BuildingTags", LookMode.Value);
            Scribe_Collections.Look(ref DisablingPreceptDefNames, "DisablingPreceptDefNames", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ThingDefNames ??= new();
                TerrainTags ??= new();
                BuildingTags ??= new();
                DisablingPreceptDefNames ??= new();
            }
        }

        public SerializableRoomRequirement DeepClone()
        {
            var clone = new SerializableRoomRequirement
            {
                TypeName = this.TypeName,
                LabelKey = this.LabelKey,
                Area = this.Area,
                Impressiveness = this.Impressiveness,
                ThingDefName = this.ThingDefName,
                Count = this.Count,
            };
            clone.ThingDefNames.AddRange(this.ThingDefNames);
            clone.TerrainTags.AddRange(this.TerrainTags);
            clone.BuildingTags.AddRange(this.BuildingTags);
            clone.DisablingPreceptDefNames.AddRange(this.DisablingPreceptDefNames);
            return clone;
        }
    }

    [Serializable]
    public class SerializableCourtRequirement : IExposable
    {
        public string MinTitleDefName;
        public string MaxTitleDefName;
        public int Count;
        public bool Enabled = true;

        public void ExposeData()
        {
            Scribe_Values.Look(ref MinTitleDefName, "MinTitleDefName");
            Scribe_Values.Look(ref MaxTitleDefName, "MaxTitleDefName");
            Scribe_Values.Look(ref Count, "Count");
            Scribe_Values.Look(ref Enabled, "Enabled", true);
        }

        public SerializableCourtRequirement DeepClone()
        {
            return new SerializableCourtRequirement
            {
                MinTitleDefName = this.MinTitleDefName,
                MaxTitleDefName = this.MaxTitleDefName,
                Count = this.Count,
                Enabled = this.Enabled
            };
        }
    }
}