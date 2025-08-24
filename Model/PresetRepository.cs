using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ceenth.Model
{
    public class PresetRepository
    {
        // Cria um context novo para evitar merdas
        private CeenthDbContext CreateContext()
        {
            var context = new CeenthDbContext();
            context.Database.EnsureCreated();
            return context;
        }

        // Vai buscar todas as presets da DB
        public List<Preset> GetAllPresets()
        {
            using var context = CreateContext();
            return context.Presets.AsNoTracking().ToList();
        }

        // Busca preset por ID
        public Preset? GetPresetById(int id)
        {
            using var context = CreateContext();
            return context.Presets.AsNoTracking().FirstOrDefault(p => p.IdPreset == id);
        }

        // Adiciona nova preset
        public void AddPreset(Preset preset)
        {
            using var context = CreateContext();
            preset.IsUserPreset = true; // Ensure it's marked as user preset
            context.Presets.Add(preset);
            context.SaveChanges();
        }

        // Dá update do preset selecionado
        public void UpdatePreset(Preset preset)
        {
            using var context = CreateContext();

            var existingPreset = context.Presets.Find(preset.IdPreset);
            if (existingPreset != null && existingPreset.IsUserPreset)
            {
                // O actual update
                existingPreset.Name = preset.Name;
                existingPreset.Waveform = preset.Waveform;
                existingPreset.AttackSeconds = preset.AttackSeconds;
                existingPreset.ReleaseSeconds = preset.ReleaseSeconds;
                existingPreset.FilterCutoff = preset.FilterCutoff;
                existingPreset.FilterQ = preset.FilterQ;
                existingPreset.VibratoRate = preset.VibratoRate;
                existingPreset.VibratoDepth = preset.VibratoDepth;
                existingPreset.TremoloRate = preset.TremoloRate;
                existingPreset.TremoloDepth = preset.TremoloDepth;

                context.SaveChanges();
            }
        }

        // Apagar preset (apenas de users)
        public bool DeletePreset(int id)
        {
            using var context = CreateContext();

            var preset = context.Presets.FirstOrDefault(p => p.IdPreset == id);
            if (preset != null && preset.IsUserPreset)
            {
                context.Presets.Remove(preset);
                context.SaveChanges();
                return true;
            }

            return false;
        }

        public List<Preset> GetUserPresets()
        {
            using var context = CreateContext();
            return context.Presets.AsNoTracking().Where(p => p.IsUserPreset).ToList();
        }

        public List<Preset> GetFactoryPresets()
        {
            using var context = CreateContext();
            return context.Presets.AsNoTracking().Where(p => !p.IsUserPreset).ToList();
        }
    }
}