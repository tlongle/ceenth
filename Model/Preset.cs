using System.ComponentModel.DataAnnotations;

namespace ceenth.Model
{
    public class Preset
    {
        [Key]
        public int IdPreset { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsUserPreset { get; set; }

        public int Waveform { get; set; }

        public float AttackSeconds { get; set; }

        public float ReleaseSeconds { get; set; }

        public float FilterCutoff { get; set; }

        public float FilterQ { get; set; }

        public float VibratoRate { get; set; }

        public float VibratoDepth { get; set; }

        public float TremoloRate { get; set; }

        public float TremoloDepth { get; set; }
    }
}