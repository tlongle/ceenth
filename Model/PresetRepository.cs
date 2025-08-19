using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ceenth.Model
{
    public class PresetRepository
    {
        private readonly CeenthDbContext _context;

        public PresetRepository()
        {
            _context = new CeenthDbContext();

            // Ensure database is created and migrations are applied
            _context.Database.EnsureCreated();
        }

        // Get all presets
        public List<Preset> GetAllPresets()
        {
            return _context.Presets.ToList();
        }

        // Get preset by ID
        public Preset? GetPresetById(int id)
        {
            return _context.Presets.FirstOrDefault(p => p.IdPreset == id);
        }

        // Add new preset
        public void AddPreset(Preset preset)
        {
            _context.Presets.Add(preset);
            _context.SaveChanges();
        }

        // Update existing preset
        public void UpdatePreset(Preset preset)
        {
            _context.Presets.Update(preset);
            _context.SaveChanges();
        }

        // Delete preset (only user presets)
        public bool DeletePreset(int id)
        {
            var preset = _context.Presets.FirstOrDefault(p => p.IdPreset == id);

            if (preset != null && preset.IsUserPreset)
            {
                _context.Presets.Remove(preset);
                _context.SaveChanges();
                return true;
            }

            return false; // Cannot delete factory presets
        }

        // Get only user-created presets
        public List<Preset> GetUserPresets()
        {
            return _context.Presets.Where(p => p.IsUserPreset).ToList();
        }

        // Get only factory presets
        public List<Preset> GetFactoryPresets()
        {
            return _context.Presets.Where(p => !p.IsUserPreset).ToList();
        }

        // Dispose of the context
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}