using GermanVerbTester.Models;
using Microsoft.EntityFrameworkCore;

namespace GermanVerbTester.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Verb> Verbs { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}