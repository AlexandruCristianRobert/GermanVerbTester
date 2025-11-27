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
            modelBuilder.Entity<Verb>().HasData(
                new Verb { Id = 1, German = "sein", English = "to be" },
                new Verb { Id = 2, German = "haben", English = "to have" },
                new Verb { Id = 3, German = "werden", English = "to become" },
                new Verb { Id = 4, German = "können", English = "to be able to" },
                new Verb { Id = 5, German = "müssen", English = "to have to" },
                new Verb { Id = 6, German = "sagen", English = "to say" },
                new Verb { Id = 7, German = "machen", English = "to do" },
                new Verb { Id = 8, German = "geben", English = "to give" },
                new Verb { Id = 9, German = "kommen", English = "to come" },
                new Verb { Id = 10, German = "sollen", English = "should" },
                new Verb { Id = 11, German = "wollen", English = "to want" },
                new Verb { Id = 12, German = "gehen", English = "to go" },
                new Verb { Id = 13, German = "wissen", English = "to know" },
                new Verb { Id = 14, German = "sehen", English = "to see" },
                new Verb { Id = 15, German = "lassen", English = "to let" },
                new Verb { Id = 16, German = "stehen", English = "to stand" },
                new Verb { Id = 17, German = "finden", English = "to find" },
                new Verb { Id = 18, German = "bleiben", English = "to stay" },
                new Verb { Id = 19, German = "liegen", English = "to lie" },
                new Verb { Id = 20, German = "heißen", English = "to be called" },
                new Verb { Id = 21, German = "denken", English = "to think" },
                new Verb { Id = 22, German = "nehmen", English = "to take" },
                new Verb { Id = 23, German = "tun", English = "to do" },
                new Verb { Id = 24, German = "dürfen", English = "to be allowed to" },
                new Verb { Id = 25, German = "glauben", English = "to believe" },
                new Verb { Id = 26, German = "halten", English = "to hold" },
                new Verb { Id = 27, German = "nennen", English = "to name" },
                new Verb { Id = 28, German = "mögen", English = "to like" },
                new Verb { Id = 29, German = "zeigen", English = "to show" },
                new Verb { Id = 30, German = "führen", English = "to lead" }
            );
        }
    }
}