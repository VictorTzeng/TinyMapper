﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TinyMapper.Configs;
using TinyMapper.DataStructures;
using TinyMapper.Extensions;
using TinyMapper.Mappers.Types.Members;
using TinyMapper.Nelibur.Sword.Extensions;
using TinyMapper.TypeConverters;

namespace TinyMapper.Mappers.Types
{
    internal sealed class MappingTypeBuilder
    {
        private readonly MapConfig _config = new MapConfig();
        private readonly Func<string, string, bool> _memberMatcher;

        public MappingTypeBuilder()
        {
            _memberMatcher = _config.Match;
        }

        public MappingType Build(TypePair typePair)
        {
            var result = new MappingType(typePair);

            SelectMembers(result.RootMember, typePair, new HashSet<TypePair>());
            return result;
        }

        private static List<MemberInfo> GetPublicMembers(Type type)
        {
            return type.GetMembers(BindingFlags.Instance | BindingFlags.Public)
                       .Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field)
                       .ToList();
        }

        private List<MemberInfo> GetSourceMembers(Type sourceType)
        {
            var result = new List<MemberInfo>();

            List<MemberInfo> members = GetPublicMembers(sourceType);
            foreach (MemberInfo member in members)
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    MethodInfo method = ((PropertyInfo)member).GetGetMethod();
                    if (method.IsNull())
                    {
                        continue;
                    }
                }
                result.Add(member);
            }
            return result;
        }

        private List<MemberInfo> GetTargetMembers(Type targetType)
        {
            var result = new List<MemberInfo>();

            List<MemberInfo> members = GetPublicMembers(targetType);
            foreach (MemberInfo member in members)
            {
                if (member.MemberType == MemberTypes.Property)
                {
                    MethodInfo method = ((PropertyInfo)member).GetSetMethod();
                    if (method.IsNull() || method.GetParameters().Length != 1)
                    {
                        continue;
                    }
                }
                result.Add(member);
            }
            return result;
        }

        private bool IsPrimitiveMember(TypePair typePair)
        {
            return PrimitiveTypeConverter.IsSupported(typePair);
        }

        private void SelectMembers(CompositeMappingMember composite, TypePair typePair, HashSet<TypePair> processed)
        {
            processed.Add(typePair);

            List<MemberInfo> sourceMembers = GetSourceMembers(typePair.Source);
            List<MemberInfo> targetMembers = GetTargetMembers(typePair.Target);

            foreach (MemberInfo targetMember in targetMembers)
            {
                MemberInfo sourceMember = sourceMembers.FirstOrDefault(x => _memberMatcher(x.Name, targetMember.Name));
                if (sourceMember.IsNull())
                {
                    continue;
                }
                var mappingPair = new TypePair(sourceMember.GetMemberType(), targetMember.GetMemberType());
                if (IsPrimitiveMember(mappingPair))
                {
                    MappingMember mappingMember = new SimpleMappingMember(sourceMember, targetMember);
                    composite.Add(mappingMember);
                }
                else
                {
                    if (processed.Contains(mappingPair))
                    {
                        return;
                    }
                    MappingMember mappingMember = new CompositeMappingMember(sourceMember, targetMember);
                    composite.Add(mappingMember);
                    SelectMembers(composite, mappingPair, processed);
                }
            }
            processed.Remove(typePair);
        }
    }
}