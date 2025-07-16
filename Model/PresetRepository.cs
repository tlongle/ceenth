using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceenth.Model
{
    public class PresetRepository
    {
        private readonly DB_CeenthEntities _context;

        // puxa contexto das Entities do Entity Framework
        public PresetRepository()
        {
            _context = new DB_CeenthEntities();
        }

        // Métodos CRUD para Presets
        public List<Preset> GetAllPresets()
        {
            return _context.Presets.ToList();
        }

        public void AddPreset(Preset preset)
        {
            preset.IsUserPreset = true;
            _context.Presets.Add(preset);
            _context.SaveChanges();
        }

        public void UpdatePreset(Preset preset)
        {
            var existingPreset = _context.Presets.Find(preset.IdPreset);
            if (existingPreset != null && existingPreset.IsUserPreset)
            {
                _context.Entry(existingPreset).CurrentValues.SetValues(preset);
                _context.SaveChanges();
            }
        }

        public void DeletePreset(int presetId)
        {
            var presetToDelete = _context.Presets.Find(presetId);
            if (presetToDelete != null && presetToDelete.IsUserPreset)
            {
                _context.Presets.Remove(presetToDelete);
                _context.SaveChanges();
            }
        }
    }
}
