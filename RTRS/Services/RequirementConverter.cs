using RimWorld;
using RoyalTitlesRoomSettings.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace RoyalTitlesRoomSettings.Services
{
    public static class RequirementConverter
    {
        private static readonly Dictionary<string, Type> TypeCache = new();
        private static readonly Dictionary<Type, List<FieldInfo>> FieldCache = new();

        static RequirementConverter()
        {
            RegisterType(typeof(RoomRequirement_Area));
            RegisterType(typeof(RoomRequirement_Impressiveness));
            RegisterType(typeof(RoomRequirement_Thing));
            RegisterType(typeof(RoomRequirement_ThingCount));
            RegisterType(typeof(RoomRequirement_ThingAnyOf));
            RegisterType(typeof(RoomRequirement_ThingAnyOfCount));
            RegisterType(typeof(RoomRequirement_TerrainWithTags));
            RegisterType(typeof(RoomRequirement_ForbiddenBuildings));
            RegisterType(typeof(RoomRequirement_ForbidAltars));
            RegisterType(typeof(RoomRequirement_AllThingsAreGlowing));
            RegisterType(typeof(RoomRequirement_AllThingsAnyOfAreGlowing));
            RegisterType(typeof(RoomRequirement_HasAssignedThroneAnyOf));

            var instrumentType = Type.GetType("VFEEmpire.RoomRequirement_InstrumentSpace, VFEEmpire");
            if (instrumentType != null)
                RegisterType(instrumentType);
        }

        private static void RegisterType(Type type)
        {
            if (type != null && !TypeCache.ContainsKey(type.FullName))
                TypeCache[type.FullName] = type;
        }

        internal static Type GetTypeByName(string fullName)
        {
            if (TypeCache.TryGetValue(fullName, out var type))
                return type;

            type = Type.GetType(fullName + ", Assembly-CSharp");
            if (type != null)
            {
                TypeCache[fullName] = type;
                return type;
            }

            type = Type.GetType(fullName + ", VFEEmpire");
            if (type != null)
            {
                TypeCache[fullName] = type;
                return type;
            }

            return null;
        }

        private static List<FieldInfo> GetFields(Type type)
        {
            if (FieldCache.TryGetValue(type, out var fields))
                return fields;

            fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance).ToList();
            FieldCache[type] = fields;
            return fields;
        }

        public static SerializableRoomRequirement ToDto(RoomRequirement req)
        {
            if (req == null)
                return null;

            var dto = new SerializableRoomRequirement
            {
                TypeName = req.GetType().FullName,
                LabelKey = req.labelKey
            };

            if (req.disablingPrecepts != null && req.disablingPrecepts.Count > 0)
                dto.DisablingPreceptDefNames.AddRange(req.disablingPrecepts.Select(p => p?.defName).Where(s => !string.IsNullOrEmpty(s)));

            var fields = GetFields(req.GetType());
            foreach (var field in fields)
            {
                if (field.Name == "labelKey" || field.Name == "disablingPrecepts")
                    continue;

                var value = field.GetValue(req);
                if (value == null)
                    continue;

                if (field.FieldType == typeof(int))
                {
                    int intVal = (int)value;
                    if (field.Name == "area")
                        dto.Area = intVal;
                    else if (field.Name == "impressiveness")
                        dto.Impressiveness = intVal;
                    else if (field.Name == "count")
                        dto.Count = intVal;
                }
                else if (field.FieldType == typeof(ThingDef))
                {
                    var def = value as ThingDef;
                    if (def != null)
                        dto.ThingDefName = def.defName;
                }
                else if (typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType.IsGenericType)
                {
                    var list = value as IList;
                    if (list == null || list.Count == 0)
                        continue;

                    var elemType = field.FieldType.GetGenericArguments()[0];
                    if (elemType == typeof(ThingDef))
                    {
                        dto.ThingDefNames.AddRange(list.Cast<ThingDef>().Select(d => d.defName));
                    }
                    else if (elemType == typeof(string))
                    {
                        var strings = list.Cast<string>().ToList();
                        if (field.Name == "tags" || field.Name == "terrainTags")
                            dto.TerrainTags.AddRange(strings);
                        else if (field.Name == "buildingTags")
                            dto.BuildingTags.AddRange(strings);
                        else if (field.Name == "things")
                        {
                            if (strings.All(s => !string.IsNullOrEmpty(s)))
                                dto.ThingDefNames.AddRange(strings);
                        }
                    }
                }
            }

            return dto;
        }

        public static RoomRequirement FromDto(SerializableRoomRequirement dto)
        {
            if (dto == null)
                return null;

            var type = GetTypeByName(dto.TypeName);
            if (type == null)
            {
                Log.Warning($"[RTRS] Unknown requirement type: {dto.TypeName}");
                return null;
            }

            var req = (RoomRequirement)Activator.CreateInstance(type);
            req.labelKey = dto.LabelKey;

            if (dto.DisablingPreceptDefNames.Count > 0)
            {
                req.disablingPrecepts = new List<PreceptDef>();
                foreach (var defName in dto.DisablingPreceptDefNames)
                {
                    var def = DefDatabase<PreceptDef>.GetNamedSilentFail(defName);
                    if (def != null)
                        req.disablingPrecepts.Add(def);
                }
            }

            var fields = GetFields(type);
            foreach (var field in fields)
            {
                if (field.Name == "labelKey" || field.Name == "disablingPrecepts")
                    continue;

                if (field.FieldType == typeof(int))
                {
                    int? value = null;
                    if (field.Name == "area")
                        value = dto.Area;
                    else if (field.Name == "impressiveness")
                        value = dto.Impressiveness;
                    else if (field.Name == "count")
                        value = dto.Count;

                    if (value.HasValue)
                        field.SetValue(req, value.Value);
                }
                else if (field.FieldType == typeof(ThingDef))
                {
                    if (!string.IsNullOrEmpty(dto.ThingDefName))
                    {
                        var def = DefDatabase<ThingDef>.GetNamedSilentFail(dto.ThingDefName);
                        if (def != null)
                            field.SetValue(req, def);
                    }
                }
                else if (typeof(IList).IsAssignableFrom(field.FieldType) && field.FieldType.IsGenericType)
                {
                    var elemType = field.FieldType.GetGenericArguments()[0];
                    if (elemType == typeof(ThingDef))
                    {
                        var list = (IList)Activator.CreateInstance(field.FieldType);
                        foreach (var defName in dto.ThingDefNames)
                        {
                            var def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
                            if (def != null)
                                list.Add(def);
                        }
                        field.SetValue(req, list);
                    }
                    else if (elemType == typeof(string))
                    {
                        var list = (IList)Activator.CreateInstance(field.FieldType);
                        if (field.Name == "tags" || field.Name == "terrainTags")
                        {
                            foreach (var tag in dto.TerrainTags)
                                list.Add(tag);
                        }
                        else if (field.Name == "buildingTags")
                        {
                            foreach (var tag in dto.BuildingTags)
                                list.Add(tag);
                        }
                        else
                        {
                            foreach (var s in dto.ThingDefNames)
                                list.Add(s);
                        }
                        field.SetValue(req, list);
                    }
                }
            }

            return req;
        }

        public static List<SerializableRoomRequirement> ToDtoList(IEnumerable<RoomRequirement> list)
        {
            if (list == null)
                return new List<SerializableRoomRequirement>();
            return list.Select(ToDto).Where(d => d != null).ToList();
        }

        public static List<RoomRequirement> FromDtoList(IEnumerable<SerializableRoomRequirement> dtos)
        {
            if (dtos == null)
                return new List<RoomRequirement>();
            return dtos.Select(FromDto).Where(r => r != null).ToList();
        }

        public static SerializableCourtRequirement CourtToDto(object courtReq)
        {
            if (courtReq == null)
                return null;

            var type = courtReq.GetType();
            var countField = type.GetField("count");
            var minField = type.GetField("minTitle");
            var maxField = type.GetField("maxTitle");

            if (countField == null || minField == null || maxField == null)
                return null;

            var dto = new SerializableCourtRequirement
            {
                Count = (int)countField.GetValue(courtReq),
                MinTitleDefName = (minField.GetValue(courtReq) as RoyalTitleDef)?.defName,
                MaxTitleDefName = (maxField.GetValue(courtReq) as RoyalTitleDef)?.defName,
                Enabled = true
            };

            return dto;
        }

        public static object CourtFromDto(SerializableCourtRequirement dto, Type courtType)
        {
            if (dto == null || courtType == null)
                return null;

            var obj = Activator.CreateInstance(courtType);
            var countField = courtType.GetField("count");
            var minField = courtType.GetField("minTitle");
            var maxField = courtType.GetField("maxTitle");

            if (countField == null || minField == null || maxField == null)
                return null;

            countField.SetValue(obj, dto.Count);

            if (!string.IsNullOrEmpty(dto.MinTitleDefName))
            {
                var minTitle = DefDatabase<RoyalTitleDef>.GetNamedSilentFail(dto.MinTitleDefName);
                if (minTitle != null)
                    minField.SetValue(obj, minTitle);
            }

            if (!string.IsNullOrEmpty(dto.MaxTitleDefName))
            {
                var maxTitle = DefDatabase<RoyalTitleDef>.GetNamedSilentFail(dto.MaxTitleDefName);
                if (maxTitle != null)
                    maxField.SetValue(obj, maxTitle);
            }

            return obj;
        }

        public static List<SerializableCourtRequirement> CourtListToDto(IList courtList)
        {
            var result = new List<SerializableCourtRequirement>();
            if (courtList == null)
                return result;

            foreach (var item in courtList)
            {
                var dto = CourtToDto(item);
                if (dto != null)
                    result.Add(dto);
            }

            return result;
        }

        public static IList CourtListFromDto(IEnumerable<SerializableCourtRequirement> dtos, Type courtType)
        {
            if (courtType == null)
                return null;

            var listType = typeof(List<>).MakeGenericType(courtType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var dto in dtos)
            {
                if (!dto.Enabled)
                    continue;

                var obj = CourtFromDto(dto, courtType);
                if (obj != null)
                    list.Add(obj);
            }

            return list;
        }
    }
}