using KoiBet.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq; // Đảm bảo bao gồm namespace này để sử dụng LINQ

namespace KoiBet.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Roles> Roles { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<Transactions> Transactions { get; set; }
        public DbSet<Referee> Referee { get; set; }
        public DbSet<FishKoi> FishKoi { get; set; }
        public DbSet<KoiStandard> KoiStandard { get; set; }
        public DbSet<KoiCategory> KoiCategory { get; set; }
        public DbSet<Award> Award { get; set; }
        public DbSet<CompetitionKoi> CompetitionKoi { get; set; }
        public DbSet<KoiRegistration> KoiRegistration { get; set; }
        public DbSet<CompetitionRound> CompetitionRound { get; set; }
        public DbSet<CompetitionMatch> CompetitionMatch { get; set; }
        public DbSet<KoiScore> KoiScore { get; set; }
        public DbSet<BetKoi> KoiBet { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set precision for decimal types
            var decimalProps = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => (System.Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType) == typeof(decimal));

            foreach (var property in decimalProps)
            {
                property.SetPrecision(10);
                property.SetScale(2);
            }

            // Define relationships if needed
            modelBuilder.Entity<Users>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.role_id);

            modelBuilder.Entity<Transactions>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.users_id);

            modelBuilder.Entity<Referee>()
                .HasOne(r => r.User)
                .WithMany(u => u.Referees)
                .HasForeignKey(r => r.user_id);

            modelBuilder.Entity<FishKoi>()
                .HasOne(k => k.User)
                .WithMany(u => u.FishKoi)
                .HasForeignKey(k => k.users_id);

            modelBuilder.Entity<KoiCategory>()
                .HasOne(c => c.KoiStandard)
                .WithMany(s => s.KoiCategories)
                .HasForeignKey(c => c.standard_id);

            modelBuilder.Entity<CompetitionKoi>()
                .HasOne(c => c.Category)
                .WithMany(c => c.Competitions)
                .HasForeignKey(c => c.category_id);

            //modelBuilder.Entity<CompetitionKoi>()
            //    .HasOne(c => c.Koi)
            //    .WithMany(k => k.Competitions)
            //    .HasForeignKey(c => c.koi_id);

            modelBuilder.Entity<CompetitionKoi>()
                .HasOne(c => c.Referee)
                .WithMany(r => r.Competitions)
                .HasForeignKey(c => c.referee_id);

            modelBuilder.Entity<CompetitionKoi>()
                .HasOne(c => c.Award)
                .WithMany(a => a.Competitions)
                .HasForeignKey(c => c.award_id);

            modelBuilder.Entity<KoiRegistration>()
                .HasOne(r => r.FishKoi)
                .WithMany(k => k.KoiRegistrations)
                .HasForeignKey(r => r.koi_id);

            modelBuilder.Entity<KoiRegistration>()
                .HasOne(r => r.CompetitionKoi)
                .WithMany(c => c.KoiRegistrations)
                .HasForeignKey(r => r.competition_id);

            modelBuilder.Entity<CompetitionMatch>()
                .HasOne(m => m.Round)
                .WithMany(r => r.Matches)
                .HasForeignKey(m => m.round_id);

            modelBuilder.Entity<CompetitionMatch>()
                .HasOne(m => m.FirstKoi)
                .WithMany()
                .HasForeignKey(m => m.first_koiId1);

            modelBuilder.Entity<CompetitionMatch>()
                .HasOne(m => m.SecondKoi)
                .WithMany()
                .HasForeignKey(m => m.first_koiId2);

            modelBuilder.Entity<KoiScore>()
                .HasOne(s => s.FishKoi)
                .WithMany(k => k.Scores)
                .HasForeignKey(s => s.koi_id);

            modelBuilder.Entity<KoiScore>()
                .HasOne(s => s.Referee)
                .WithMany(r => r.Scores)
                .HasForeignKey(s => s.referee_id);

            modelBuilder.Entity<KoiScore>()
                .HasOne(s => s.CompetitionMatch)
                .WithMany(m => m.Scores)
                .HasForeignKey(s => s.match_id);

            modelBuilder.Entity<BetKoi>()
                .HasOne(b => b.User)
                .WithMany(u => u.BetKoi)
                .HasForeignKey(b => b.users_id);

            modelBuilder.Entity<BetKoi>()
                .HasOne(b => b.KoiRegistration)
                .WithMany(r => r.Bets)
                .HasForeignKey(b => b.registration_id);

            modelBuilder.Entity<BetKoi>()
                .HasOne(b => b.Competition)
                .WithMany(c => c.Bets)
                .HasForeignKey(b => b.competition_id);

            modelBuilder.Entity<CompetitionRound>()
                .HasOne(b => b.CompetitionKoi)
                .WithMany()
                .HasForeignKey(b => b.competition_id)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
