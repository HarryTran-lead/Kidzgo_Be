using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions.Shared;

internal static class SlotTypeSectionTypeMapper
{
    internal static SectionType Map(SlotUsageType usageType)
    {
        return usageType switch
        {
            SlotUsageType.Makeup => SectionType.Makeup,
            SlotUsageType.Remedial => SectionType.Remedial,
            SlotUsageType.Review => SectionType.Review,
            _ => SectionType.Normal
        };
    }
}
