using EProtokoll.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EProtokoll.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<ExternalInstitution> ExternalInstitutions => Set<ExternalInstitution>();
    public DbSet<ProtocolBook> ProtocolBooks => Set<ProtocolBook>();
    public DbSet<ProtocolCounter> ProtocolCounters => Set<ProtocolCounter>();
    public DbSet<Letter> Letters => Set<Letter>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<ResponseEntry> Responses => Set<ResponseEntry>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<LetterAccess> LetterAccesses => Set<LetterAccess>();
    public DbSet<LetterDepartmentAccess> LetterDepartmentAccesses => Set<LetterDepartmentAccess>();
    public DbSet<DocumentHistory> DocumentHistories => Set<DocumentHistory>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.UserName)
            .IsUnique();

        modelBuilder.Entity<ProtocolCounter>()
            .HasIndex(x => x.Year)
            .IsUnique();

        modelBuilder.Entity<Letter>()
            .HasIndex(x => x.ProtocolNumber)
            .IsUnique();

        modelBuilder.Entity<Document>()
            .HasIndex(x => x.HashSha256);

        modelBuilder.Entity<Letter>()
            .HasMany(x => x.Documents)
            .WithOne(x => x.Letter)
            .HasForeignKey(x => x.LetterId);

        modelBuilder.Entity<LetterAccess>()
            .HasIndex(x => new { x.LetterId, x.UserId })
            .IsUnique();

        modelBuilder.Entity<LetterDepartmentAccess>()
            .HasIndex(x => new { x.LetterId, x.DepartmentId })
            .IsUnique();
    }
}
