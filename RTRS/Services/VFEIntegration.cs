using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Verse;

namespace RoyalTitlesRoomSettings.Services
{
    internal static class VFEIntegration
    {
        public static bool IsVfeLoaded { get; } = ModsConfig.IsActive("OskarPotocki.VFE.Empire");

        private static readonly Lazy<Type> ExtType = new(() =>
        {
            if (!IsVfeLoaded)
                return null;
            var asm = LoadedModManager.RunningMods
                .SelectMany(m => m.assemblies?.loadedAssemblies ?? Enumerable.Empty<Assembly>())
                .FirstOrDefault(a => a.GetName().Name == "VFEEmpire");
            return asm?.GetType("VFEEmpire.RoyalTitleDefExtension");
        });

        private static readonly Lazy<FieldInfo> BallroomField = new(() =>
            ExtType.Value?.GetField("ballroomRequirements"));

        private static readonly Lazy<FieldInfo> GalleryField = new(() =>
            ExtType.Value?.GetField("galleryRequirements"));

        private static readonly Lazy<FieldInfo> CourtField = new(() =>
            ExtType.Value?.GetField("courtRequirments"));

        private static readonly Lazy<Type> CourtReqType = new(() =>
        {
            if (!IsVfeLoaded)
                return null;
            var asm = LoadedModManager.RunningMods
                .SelectMany(m => m.assemblies?.loadedAssemblies ?? Enumerable.Empty<Assembly>())
                .FirstOrDefault(a => a.GetName().Name == "VFEEmpire");
            return asm?.GetType("VFEEmpire.RoyalCourtRequirment");
        });

        public static SerializableTitleRequirements GetVfeExtensions(RoyalTitleDef title)
        {
            if (!IsVfeLoaded || ExtType.Value == null)
                return null;
            var ext = title.modExtensions?.FirstOrDefault(e => ExtType.Value.IsInstanceOfType(e));
            if (ext == null)
                return null;

            var dto = new SerializableTitleRequirements { TitleDefName = title.defName };

            if (BallroomField.Value != null)
            {
                var list = BallroomField.Value.GetValue(ext) as IList;
                dto.BallroomRequirements = RequirementConverter.ToDtoList(list?.Cast<RoomRequirement>());
            }

            if (GalleryField.Value != null)
            {
                var list = GalleryField.Value.GetValue(ext) as IList;
                dto.GalleryRequirements = RequirementConverter.ToDtoList(list?.Cast<RoomRequirement>());
            }

            if (CourtField.Value != null && CourtReqType.Value != null)
            {
                var list = CourtField.Value.GetValue(ext) as IList;
                dto.CourtRequirements = RequirementConverter.CourtListToDto(list);
            }

            return dto;
        }

        public static void SetVfeExtensions(RoyalTitleDef title, SerializableTitleRequirements dto)
        {
            if (!IsVfeLoaded || ExtType.Value == null || dto == null)
                return;
            var ext = title.modExtensions?.FirstOrDefault(e => ExtType.Value.IsInstanceOfType(e));
            if (ext == null)
                return;

            if (BallroomField.Value != null)
            {
                var list = RequirementConverter.FromDtoList(dto.BallroomRequirements);
                BallroomField.Value.SetValue(ext, list);
            }

            if (GalleryField.Value != null)
            {
                var list = RequirementConverter.FromDtoList(dto.GalleryRequirements);
                GalleryField.Value.SetValue(ext, list);
            }

            if (CourtField.Value != null && CourtReqType.Value != null)
            {
                var list = RequirementConverter.CourtListFromDto(dto.CourtRequirements, CourtReqType.Value);
                CourtField.Value.SetValue(ext, list);
            }
        }
    }
}