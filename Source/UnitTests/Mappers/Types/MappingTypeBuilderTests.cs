﻿using System.Linq;
using TinyMapper.DataStructures;
using TinyMapper.Mappers.Types;
using Xunit;

namespace UnitTests.Mappers.Types
{
    public sealed class MappingTypeBuilderTests
    {
        [Fact]
        public void Buid_Recursion_Ok()
        {
            var builder = new MappingTypeBuilder();
            MappingType mappingType = builder.Build(new TypePair(typeof(MyClass), typeof(MyClass1)));
            Assert.Equal(2, mappingType.RootMember.Members.Count());
        }
    }


    public class MyClass
    {
        public MyClass1 Class { get; set; }
        public int Id { get; set; }
    }


    public class MyClass1
    {
        public MyClass Class { get; set; }
        public int Id { get; set; }
    }
}