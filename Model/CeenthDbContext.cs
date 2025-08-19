using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace ceenth.Model
{
    public class CeenthDbContext : DbContext
    {
        public DbSet<Preset> Presets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // SQLite database file will be created in the app's directory
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ceenth.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        private void ShowDatabaseLocation()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ceenth.db");

            string message = $"Database path: {dbPath}\n" +
                            $"File exists: {File.Exists(dbPath)}\n" +
                            $"Base directory: {AppDomain.CurrentDomain.BaseDirectory}";

            System.Windows.MessageBox.Show(message, "Database Location");

            // Copy to clipboard
            if (File.Exists(dbPath))
            {
                System.Windows.Clipboard.SetText(dbPath);
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Preset entity
            modelBuilder.Entity<Preset>(entity =>
            {
                entity.HasKey(e => e.IdPreset);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Seed the database with factory presets
            modelBuilder.Entity<Preset>().HasData(
                new Preset
                {
                    IdPreset = 1,
                    Name = "Default Sine",
                    IsUserPreset = false,
                    Waveform = 0,
                    AttackSeconds = 0.01f,
                    ReleaseSeconds = 0.3f,
                    FilterCutoff = 1.0f,
                    FilterQ = 1.0f,
                    VibratoRate = 5f,
                    VibratoDepth = 0f,
                    TremoloRate = 5f,
                    TremoloDepth = 0f
                },
                new Preset
                {
                    IdPreset = 2,
                    Name = "Simple Bass",
                    IsUserPreset = false,
                    Waveform = 1,
                    AttackSeconds = 0.02f,
                    ReleaseSeconds = 0.4f,
                    FilterCutoff = 0.3f,
                    FilterQ = 2.0f,
                    VibratoRate = 0f,
                    VibratoDepth = 0f,
                    TremoloRate = 0f,
                    TremoloDepth = 0f
                },
                new Preset
                {
                    IdPreset = 3,
                    Name = "Wobble Pad",
                    IsUserPreset = false,
                    Waveform = 2,
                    AttackSeconds = 0.8f,
                    ReleaseSeconds = 1.5f,
                    FilterCutoff = 0.6f,
                    FilterQ = 4.0f,
                    VibratoRate = 6f,
                    VibratoDepth = 8f,
                    TremoloRate = 0f,
                    TremoloDepth = 0f
                },
                new Preset
                {
                    IdPreset = 4,
                    Name = "Laser Sweep",
                    IsUserPreset = false,
                    Waveform = 1,
                    AttackSeconds = 0.01f,
                    ReleaseSeconds = 0.01f,
                    FilterCutoff = 0.98f,
                    FilterQ = 7.3f,
                    VibratoRate = 5.0f,
                    VibratoDepth = 20.0f,
                    TremoloRate = 5.0f,
                    TremoloDepth = 0.5f
                }
            );
        }
    }
}