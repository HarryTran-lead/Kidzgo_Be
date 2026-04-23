using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Payroll;
using Kidzgo.Domain.Tickets;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.Schools;

public class Branch : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
    public ICollection<Class> Classes { get; set; } = new List<Class>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
    public ICollection<MonthlyReportJob> MonthlyReportJobs { get; set; } = new List<MonthlyReportJob>();
    public ICollection<Lead> PreferredLeads { get; set; } = new List<Lead>();
    public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<CashbookEntry> CashbookEntries { get; set; } = new List<CashbookEntry>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<PayrollRun> PayrollRuns { get; set; } = new List<PayrollRun>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<BranchProgram> BranchPrograms { get; set; } = new List<BranchProgram>();
    public ICollection<TuitionPlan> TuitionPlans { get; set; } = new List<TuitionPlan>();
}
