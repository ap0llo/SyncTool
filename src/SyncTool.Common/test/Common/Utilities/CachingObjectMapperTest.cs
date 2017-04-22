using System;
using Xunit;

namespace SyncTool.Common.Utilities
{
    /// <summary>
    /// Tests for <see cref="CachingObjectMapper{T,T}"/>
    /// </summary>
    public class CachingObjectMapperTest
    {
         
        [Fact(DisplayName = "CachingObjectMapper: Constructor: Mapping function must not be null")]
        public void Constructor_MappingFunction_must_not_be_null()
        {
            Assert.Throws<ArgumentNullException>(() => new CachingObjectMapper<object, object>(null));
        }


        [Fact(DisplayName = "CachingObjectMapper.MapObject() invokes the mapping function")]
        public void MapObject_Invokes_the_mapping_function()
        {
            var instance = new CachingObjectMapper<string, string>(str => str.ToUpper());

            var value = "str1";            
            var actual = instance.MapObject(value);

            Assert.Equal(value.ToUpper(), actual);
        }

        [Fact(DisplayName = "CachingObjectMapper.MapObject() reuses cached instances")]
        public void MapObject_reuses_cached_instances()
        {
            var value = new object();

            var instance = new CachingObjectMapper<object, object>(_ => new object());
            
            var mappedValue1 = instance.MapObject(value);
            var mappedValue2 = instance.MapObject(value);

            Assert.Same(mappedValue1, mappedValue2);
        }

        [Fact(DisplayName = "CachingObjectMapper.MapObject() uses the specified equality comparer")]
        public void MapObject_uses_the_specified_equality_comparer()
        {
            var value = "foo";

            var instance = new CachingObjectMapper<string, object>(str => new object(), StringComparer.CurrentCultureIgnoreCase);

            var mapped1 = instance.MapObject(value);
            var mapped2 = instance.MapObject(value.ToUpper());


            Assert.Same(mapped1, mapped2);
        }


        [Fact(DisplayName = "CachingObjectMapper.CleanCache() removes specified items from cache")]
        public void CleanCache_removes_specified_items_from_cache()
        {            
            var value1 = new object();
            var value2 = new object();

            var instance = new CachingObjectMapper<object, object>(_ => new object());

            var value1_mapped1 = instance.MapObject(value1);
            var value2_mapped1 = instance.MapObject(value2);


            instance.CleanCache(new[] { value1 });

            var value1_mapped2 = instance.MapObject(value1);
            var value2_mapped2 = instance.MapObject(value2);

            Assert.Same(value1_mapped1, value1_mapped2);
            Assert.NotSame(value2_mapped1, value2_mapped2);

        }

        [Fact(DisplayName = "CachingObjectMapper.CleanCache() uses the specified equality comparer")]
        public void CleanCache_uses_the_specified_equality_comparer()
        {
            var value1 = "foo";
            var value2 = "bar";

            var instance = new CachingObjectMapper<string, object>(str => new object(), StringComparer.CurrentCultureIgnoreCase);

            var value1_mapped1 = instance.MapObject(value1);
            var value2_mapped1 = instance.MapObject(value2);


            instance.CleanCache(new[] { value1.ToUpper() });

            var value1_mapped2 = instance.MapObject(value1);
            var value2_mapped2 = instance.MapObject(value2);

        }


    }
}