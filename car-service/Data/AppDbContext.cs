using car_service.Models;
using Microsoft.EntityFrameworkCore;

namespace car_service.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Slide> Slides => Set<Slide>();
    public DbSet<FeedbackMessage> FeedbackMessages => Set<FeedbackMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<BlockedDate> BlockedDates => Set<BlockedDate>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<Appointment>(e =>
        {
            e.HasIndex(x => new { x.Date, x.Time });
            e.HasOne(x => x.User).WithMany(u => u.Appointments)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Service).WithMany()
                .HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Review>(e =>
        {
            e.HasIndex(x => x.Status);
            e.HasOne(x => x.User).WithMany(u => u.Reviews)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Notification>(e =>
        {
            e.HasOne(x => x.User).WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<BlockedDate>().HasIndex(x => x.Date).IsUnique();

        // SQLite не умеет сравнивать decimal — храним цену как double.
        b.Entity<ServiceItem>().Property(x => x.PriceFrom).HasConversion<double>();
    }
}
