namespace Kidzgo.Domain.Sessions;

[Flags]
public enum SlotUsageType
{
    None = 0,
    Standard = 1,
    Makeup = 2,
    Remedial = 4,
    Review = 8,
    Custom = 16
}
