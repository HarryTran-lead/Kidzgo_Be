using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.Shared;

public static class VietnameseEnumText
{
    public static string ForPauseEnrollmentOutcome(PauseEnrollmentOutcome? outcome)
    {
        return outcome switch
        {
            PauseEnrollmentOutcome.ContinueSameClass => "Tiếp tục học lại lớp cũ",
            PauseEnrollmentOutcome.ReassignEquivalentClass => "Xếp lại lớp tương đương",
            PauseEnrollmentOutcome.ContinueWithTutoring => "Tiếp tục với chương trình học kèm",
            _ => string.Empty
        };
    }

    public static string ForProfileType(ProfileType profileType)
    {
        return profileType switch
        {
            ProfileType.Parent => "Phụ huynh",
            ProfileType.Student => "Học sinh",
            _ => profileType.ToString()
        };
    }

    public static string ForGender(Gender? gender)
    {
        return gender switch
        {
            Gender.Male => "Nam",
            Gender.Female => "Nữ",
            _ => string.Empty
        };
    }

    public static string ForMediaType(MediaType mediaType)
    {
        return mediaType switch
        {
            MediaType.Photo => "Ảnh",
            MediaType.Video => "Video",
            MediaType.Document => "Tài liệu",
            _ => mediaType.ToString()
        };
    }
}
