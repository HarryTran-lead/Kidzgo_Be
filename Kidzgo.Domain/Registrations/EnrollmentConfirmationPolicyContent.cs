namespace Kidzgo.Domain.Registrations;

public static class EnrollmentConfirmationPolicyContent
{
    public static IReadOnlyList<string> DefaultNewStudentPolicyLines { get; } =
    [
        "Không áp dụng hoàn phí.",
        "Các buổi con nghỉ phụ huynh báo trước cô 24h trước khi buổi học bắt đầu sẽ được sắp xếp học bù.",
        "Không áp dụng học bù đối với trường hợp nghỉ không báo trước.",
        "Trung tâm sẽ sắp xếp lớp học bù phù hợp.",
        "Chính sách bảo lưu: tối đa 01 lần, trong vòng 03 tháng."
    ];

    public static IReadOnlyList<string> DefaultReservationPolicyLines { get; } =
    [
        "Chỉ áp dụng bảo lưu tối đa 01 lần cho mỗi khóa học.",
        "Sau thời hạn bảo lưu, nếu học viên không quay lại, số buổi còn lại sẽ không còn hiệu lực.",
        "Trung tâm sẽ hỗ trợ sắp xếp lớp phù hợp khi học viên quay lại học.",
        "Học phí đã đóng không được hoàn lại."
    ];

    public static List<string> GetNewStudentPolicyLines(string? value)
        => ParseLines(value, DefaultNewStudentPolicyLines);

    public static List<string> GetReservationPolicyLines(string? value)
        => ParseLines(value, DefaultReservationPolicyLines);

    public static string? SerializeLines(IEnumerable<string>? lines)
    {
        if (lines is null)
        {
            return null;
        }

        var normalizedLines = lines
            .Select(line => line?.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Cast<string>()
            .ToList();

        return normalizedLines.Count == 0
            ? null
            : string.Join('\n', normalizedLines);
    }

    private static List<string> ParseLines(string? value, IReadOnlyList<string> fallback)
    {
        var lines = string.IsNullOrWhiteSpace(value)
            ? new List<string>()
            : value
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

        return lines.Count > 0 ? lines : fallback.ToList();
    }
}
