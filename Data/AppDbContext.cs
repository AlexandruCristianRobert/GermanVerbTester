using GermanVerbTester.Models;
using Microsoft.EntityFrameworkCore;

namespace GermanVerbTester.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Verb> Verbs { get; set; }
        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<Preposition> Prepositions { get; set; }
        public DbSet<PrepositionTestResult> PrepositionTestResults { get; set; }
        public DbSet<VerbSelection> VerbSelections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VerbSelection>()
                .HasOne(vs => vs.Verb)
                .WithMany()
                .HasForeignKey(vs => vs.VerbId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}