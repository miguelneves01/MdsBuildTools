using System.Linq;
using MdsBuildTools.Core;
using Xunit;

namespace MdsBuildTools.Core.Tests
{
    public class SnapMathTests
    {
        [Theory]
        [InlineData(1.23f, 0.05f, 1.25f)]
        [InlineData(1.22f, 0.05f, 1.20f)]
        [InlineData(-0.03f, 0.05f, -0.05f)]
        [InlineData(3.47f, 0.01f, 3.47f)]
        public void Snap_RoundsToNearestStep(float value, float step, float expected) =>
            Assert.Equal(expected, SnapMath.Snap(value, step), 4);

        [Theory]
        [InlineData(0f)]
        [InlineData(-1f)]
        [InlineData(float.NaN)]
        public void Snap_InvalidStep_ReturnsValueUnchanged(float step) =>
            Assert.Equal(1.234f, SnapMath.Snap(1.234f, step));

        [Theory]
        [InlineData(7f, 15f, 0f)]
        [InlineData(8f, 15f, 15f)]
        [InlineData(359f, 15f, 0f)]
        [InlineData(-20f, 15f, 345f)]
        [InlineData(95f, 90f, 90f)]
        public void SnapAngle_SnapsAndNormalises(float deg, float step, float expected) =>
            Assert.Equal(expected, SnapMath.SnapAngle(deg, step), 3);
    }

    public class FloorClampTests
    {
        [Fact] public void BelowFloor_ReturnsPenetration() => Assert.Equal(0.12f, FloorClamp.Correction(-0.12f, 0f), 4);
        [Fact] public void AboveFloor_NoCorrection() => Assert.Equal(0f, FloorClamp.Correction(0.3f, 0f));
        [Fact] public void WithinTolerance_NoCorrection() => Assert.Equal(0f, FloorClamp.Correction(-0.0005f, 0f));
        [Fact] public void RaisedFloor_Works() => Assert.Equal(0.05f, FloorClamp.Correction(0.95f, 1f), 4);
    }

    public class LengthParserTests
    {
        [Theory]
        [InlineData("3.47", 3.47f)]
        [InlineData("3,47", 3.47f)]
        [InlineData("3,47 m", 3.47f)]
        [InlineData("347cm", 3.47f)]
        [InlineData("3470 mm", 3.47f)]
        [InlineData(" 2 M ", 2f)]
        public void ParsesValidInputs(string input, float expected)
        {
            Assert.True(LengthParser.TryParseMetres(input, out var m));
            Assert.Equal(expected, m, 4);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("-2")]
        [InlineData("3.4.7")]
        [InlineData("1 km")]
        [InlineData("0.01")]   // below min
        [InlineData("120")]    // above max
        public void RejectsInvalidInputs(string? input) =>
            Assert.False(LengthParser.TryParseMetres(input, out _));
    }

    public class AlignmentTests
    {
        private static Alignment.Span S(float min, float max) => new Alignment.Span(min, max);

        [Fact]
        public void AlignsEdgeToNearbyEdge() =>
            Assert.Equal(0.02f, Alignment.FindShift(S(0.98f, 1.98f), new[] { S(2f, 3f) }, 0.03f)!.Value, 4);

        [Fact]
        public void AlignsCentres() =>
            Assert.Equal(-0.01f, Alignment.FindShift(S(0f, 1f), new[] { S(-0.51f, 1.49f) }, 0.03f)!.Value, 4);

        [Fact]
        public void PicksSmallestShift() =>
            Assert.Equal(0.005f, Alignment.FindShift(S(0f, 1f), new[] { S(1.02f, 2f), S(1.005f, 3f) }, 0.03f)!.Value, 4);

        [Fact]
        public void NothingInRange_ReturnsNull() =>
            Assert.Null(Alignment.FindShift(S(0f, 1f), new[] { S(5f, 6f) }, 0.03f));

        [Fact]
        public void NoTargets_ReturnsNull() =>
            Assert.Null(Alignment.FindShift(S(0f, 1f), Enumerable.Empty<Alignment.Span>(), 0.03f));
    }
}
