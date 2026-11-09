using Pos.Domain.Common;
using Pos.Domain.Rubrics;
using Pos.Domain.Students;

namespace Pos.Tests;

public class ValueObjectTests
{
    [Theory]
    [InlineData(0.0)]
    [InlineData(7.5)]
    [InlineData(10.0)]
    public void Score_WithinRange_IsCreated(double value)
    {
        var score = new Score((decimal)value, 10);

        Assert.Equal((decimal)value, score.Value);
        Assert.Equal((decimal)value / 10, score.Ratio);
    }

    [Theory]
    [InlineData(-0.5)]
    [InlineData(10.01)]
    [InlineData(12.0)]
    public void Score_OutOfRange_Throws(double value)
    {
        Assert.Throws<DomainException>(() => new Score((decimal)value, 10));
    }

    [Fact]
    public void Score_NonPositiveMax_Throws()
    {
        Assert.Throws<DomainException>(() => new Score(0, 0));
    }

    [Fact]
    public void Score_WithValue_KeepsMaxAndChecksRange()
    {
        var score = new Score(5, 10);

        Assert.Equal(new Score(8, 10), score.WithValue(8));
        Assert.Throws<DomainException>(() => score.WithValue(11));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Weight_NonPositive_Throws(int value)
    {
        Assert.Throws<DomainException>(() => new Weight(value));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void StudentName_Empty_Throws(string name)
    {
        Assert.Throws<DomainException>(() => new StudentName(name));
    }

    [Fact]
    public void StudentName_IsTrimmed_AndComparedByValue()
    {
        Assert.Equal(new StudentName("Анна"), new StudentName("  Анна "));
    }
}
