using System.ComponentModel;

namespace Kidzgo.Domain.Registrations;

public enum EntryType
{
    Immediate,  // Vao hoc ngay
    Wait,       // Cho lop moi
    Retake,     // Thi lai len lop cao hon

    [EditorBrowsable(EditorBrowsableState.Never)]
    Makeup      // Legacy only: giu de doc du lieu cu, khong dung cho luong moi
}
