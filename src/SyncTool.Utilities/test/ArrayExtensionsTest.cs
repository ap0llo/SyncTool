using System;
using System.Linq;
using Xunit;

namespace SyncTool.Utilities.Test
{
    public class ArrayExtensionsTest
    {
        [Fact]
        public void Segment_throws_ArgumentOutOfRangeException_if_maxSegmentSize_is_less_than_1()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<int>().GetSegments(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Array.Empty<int>().GetSegments(-1));
        }

        [Theory]
        [InlineData(0, 1, 0)]
        [InlineData(23, 23, 1)]
        [InlineData(5, 5, 1)]
        [InlineData(5, 10, 1)]
        [InlineData(10, 5, 2)]
        [InlineData(10, 3, 4)]
        public void Segment_returns_expected_segments(int arraySize, int maxSegmentSize, int expectedSegmentCount)
        {
            var array = Enumerable.Range(1, arraySize).ToArray();                

            var segments = array.GetSegments(maxSegmentSize).ToArray();

            Assert.Equal(expectedSegmentCount, segments.Length);
            Assert.True(array.SequenceEqual(segments.SelectMany(x => x).ToArray()));
        }
    }
}
