using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
namespace StringAnalyzer.Models
{
    public class ApiContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(databaseName: "StringAnalyserDB");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var dictionaryConverter = new ValueConverter<Dictionary<char, int>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<char, int>>(v, (JsonSerializerOptions)null));

            modelBuilder.Entity<StringProperty>().Property(sp => sp.CharacterFrequencyMap).HasConversion(dictionaryConverter);
        }

        public DbSet<StringProperty> StringProperties { get; set; }
    }
}
