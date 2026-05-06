namespace Kidzgo.Domain.Registrations;

public enum OperationType
{
    Initial,       // Dang ky lan dau
    Upgrade,       // Nang goi
    Renewal,       // Gia han
    Transfer,      // Chuyen lop
    TransferBranch, // Chuyen chi nhanh
    Retake,         // Thi lai de len lop cao hon
    Promotion       // Len chuong trinh ke tiep theo ket qua danh gia
}
