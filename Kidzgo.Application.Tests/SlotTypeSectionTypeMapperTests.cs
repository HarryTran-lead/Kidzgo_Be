using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Domain.Sessions;
using Xunit;

namespace Kidzgo.Application.Tests;

public sealed class SlotTypeSectionTypeMapperTests
{
    [Theory]
    [InlineData(SlotUsageType.None, SectionType.Normal)]
    [InlineData(SlotUsageType.Standard, SectionType.Normal)]
    [InlineData(SlotUsageType.Custom, SectionType.Normal)]
    [InlineData(SlotUsageType.Makeup, SectionType.Makeup)]
    [InlineData(SlotUsageType.Remedial, SectionType.Remedial)]
    [InlineData(SlotUsageType.Review, SectionType.Review)]
    public void Map_returns_expected_section_type(SlotUsageType usageType, SectionType expectedSectionType)
    {
        var result = SlotTypeSectionTypeMapper.Map(usageType);

        Assert.Equal(expectedSectionType, result);
    }
}
