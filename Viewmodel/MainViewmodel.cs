using ceenth.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ceenth.Viewmodel
{
    public class MainViewmodel : INotifyPropertyChanged, IDisposable
    {
        #region Fields
        private readonly PolyphonicAudioEngine _audioEngine;
        private readonly PresetRepository _presetRepository;
        private readonly Dictionary<Key, PianoKeyViewmodel> _keyMappings;

        // --- NEW: A placeholder object for the "New Preset" option ---
        private readonly Preset _newPresetPlaceholder = new Preset { IdPreset = -1, Name = "[ New Preset ]", IsUserPreset = false };

        private Preset _selectedPreset;
        private string _newPresetName = "New Preset";
        private int _octave = 0;

        // Backing fields for synth parameters
        private WaveformTypes _selectedWaveform;
        private float _attackSeconds;
        private float _releaseSeconds;
        private float _logarithmicFilterCutoff = 0.75f;
        private float _filterQ;
        private float _vibratoRate;
        private float _vibratoDepth;
        private float _tremoloRate;
        private float _tremoloDepth;
        #endregion

        #region Constructor
        public MainViewmodel()
        {
            _audioEngine = new PolyphonicAudioEngine();
            _presetRepository = new PresetRepository();
            _keyMappings = new Dictionary<Key, PianoKeyViewmodel>();

            // Collections for UI
            PianoKeys = new ObservableCollection<PianoKeyViewmodel>();
            Presets = new ObservableCollection<Preset>();

            // Commands
            SavePresetCommand = new RelayCommand(_ => SaveCurrentSettings());
            DeletePresetCommand = new RelayCommand(_ => DeleteSelectedPreset(), _ => SelectedPreset != null && SelectedPreset.IsUserPreset);
            IncreaseOctaveCommand = new RelayCommand(_ => SetOctave(_octave + 1));
            DecreaseOctaveCommand = new RelayCommand(_ => SetOctave(_octave - 1));

            // Initialization
            OscilloscopeViewmodel = new OscilloscopeViewmodel(_audioEngine);
            InitializePianoKeys();
            LoadPresets();
        }
        #endregion

        #region Properties
        public OscilloscopeViewmodel OscilloscopeViewmodel { get; }
        public ObservableCollection<PianoKeyViewmodel> PianoKeys { get; }
        public ObservableCollection<Preset> Presets { get; }
        public ICommand SavePresetCommand { get; }
        public ICommand DeletePresetCommand { get; }
        public ICommand IncreaseOctaveCommand { get; }
        public ICommand DecreaseOctaveCommand { get; }

        public string NewPresetName
        {
            get => _newPresetName;
            set { _newPresetName = value; OnPropertyChanged(); }
        }

        public Preset SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                if (_selectedPreset != value)
                {
                    _selectedPreset = value;

                    // --- UPDATED: Logic to handle preset selection ---
                    if (_selectedPreset == _newPresetPlaceholder)
                    {
                        ClearSettingsForNewPreset();
                    }
                    else if (_selectedPreset != null)
                    {
                        LoadPreset(_selectedPreset);
                    }
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<WaveformTypes> Waveforms => Enum.GetValues(typeof(WaveformTypes)).Cast<WaveformTypes>();

        public int Octave
        {
            get => _octave;
            set
            {
                if (_octave != value)
                {
                    _octave = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        #region Synth Parameter Properties
        public WaveformTypes SelectedWaveform { get => _selectedWaveform; set { _selectedWaveform = value; _audioEngine.Waveform = value; OnPropertyChanged(); } }
        public float AttackSeconds { get => _attackSeconds; set { _attackSeconds = value; _audioEngine.AttackSeconds = value; OnPropertyChanged(); } }
        public float ReleaseSeconds { get => _releaseSeconds; set { _releaseSeconds = value; _audioEngine.ReleaseSeconds = value; OnPropertyChanged(); } }
        public float FilterCutoff { get => _logarithmicFilterCutoff; set { _logarithmicFilterCutoff = value; float minFreq = 100f; float maxFreq = 20000f; float calculatedFreq = minFreq * (float)Math.Pow(maxFreq / minFreq, _logarithmicFilterCutoff); _audioEngine.FilterCutoff = calculatedFreq; OnPropertyChanged(); OnPropertyChanged(nameof(FilterCutoffDisplay)); } }
        public string FilterCutoffDisplay { get { float minFreq = 100f; float maxFreq = 20000f; float calculatedFreq = minFreq * (float)Math.Pow(maxFreq / minFreq, _logarithmicFilterCutoff); return calculatedFreq < 1000 ? $"{(int)calculatedFreq} Hz" : $"{calculatedFreq / 1000:F1} kHz"; } }
        public float FilterQ { get => _filterQ; set { _filterQ = value; _audioEngine.FilterQ = value; OnPropertyChanged(); } }
        public float VibratoRate { get => _vibratoRate; set { _vibratoRate = value; _audioEngine.VibratoRate = value; OnPropertyChanged(); } }
        public float VibratoDepth { get => _vibratoDepth; set { _vibratoDepth = value; _audioEngine.VibratoDepth = value; OnPropertyChanged(); } }
        public float TremoloRate { get => _tremoloRate; set { _tremoloRate = value; _audioEngine.TremoloRate = value; OnPropertyChanged(); } }
        public float TremoloDepth { get => _tremoloDepth; set { _tremoloDepth = value; _audioEngine.TremoloDepth = value; OnPropertyChanged(); } }
        #endregion

        #region Methods
        private void LoadPresets()
        {
            var currentId = SelectedPreset?.IdPreset;
            Presets.Clear();

            // --- UPDATED: Always add the "New Preset" option first ---
            Presets.Add(_newPresetPlaceholder);

            var presetsFromDb = _presetRepository.GetAllPresets();
            foreach (var preset in presetsFromDb)
            {
                Presets.Add(preset);
            }

            SelectedPreset = Presets.FirstOrDefault(p => p.IdPreset == currentId) ?? Presets.FirstOrDefault(p => p.IdPreset != -1);
        }

        private void LoadPreset(Preset preset)
        {
            this.SelectedWaveform = (WaveformTypes)preset.Waveform;
            this.AttackSeconds = preset.AttackSeconds;
            this.ReleaseSeconds = preset.ReleaseSeconds;
            this.FilterCutoff = preset.FilterCutoff;
            this.FilterQ = preset.FilterQ;
            this.VibratoRate = preset.VibratoRate;
            this.VibratoDepth = preset.VibratoDepth;
            this.TremoloRate = preset.TremoloRate;
            this.TremoloDepth = preset.TremoloDepth;
            this.NewPresetName = preset.Name;
        }

        // --- NEW: Method to reset all sliders to a default state ---
        private void ClearSettingsForNewPreset()
        {
            this.SelectedWaveform = WaveformTypes.Sine;
            this.AttackSeconds = 0.01f;
            this.ReleaseSeconds = 0.3f;
            this.FilterCutoff = 1.0f;
            this.FilterQ = 1.0f;
            this.VibratoRate = 5f;
            this.VibratoDepth = 0f;
            this.TremoloRate = 5f;
            this.TremoloDepth = 0f;
            this.NewPresetName = "My New Sound";
        }

        private void SaveCurrentSettings()
        {
            Preset presetToSave;
            bool isNew = false;

            // If a user preset is selected, UPDATE it by loading it fresh from the database
            if (SelectedPreset != null && SelectedPreset.IsUserPreset && SelectedPreset.IdPreset > 0)
            {
                // Use the selected preset directly - UpdatePreset will handle the tracking
                presetToSave = new Preset
                {
                    IdPreset = SelectedPreset.IdPreset,
                    IsUserPreset = true
                };

                if (presetToSave == null)
                {
                    // Fallback: create new if somehow the preset doesn't exist anymore
                    presetToSave = new Preset { IsUserPreset = true };
                    isNew = true;
                }
            }
            // Otherwise, CREATE a new preset
            else
            {
                presetToSave = new Preset { IsUserPreset = true };
                isNew = true;
            }

            // Update the preset with current UI values
            presetToSave.Name = this.NewPresetName;
            presetToSave.Waveform = (int)this.SelectedWaveform;
            presetToSave.AttackSeconds = this.AttackSeconds;
            presetToSave.ReleaseSeconds = this.ReleaseSeconds;
            presetToSave.FilterCutoff = this.FilterCutoff;
            presetToSave.FilterQ = this.FilterQ;
            presetToSave.VibratoRate = this.VibratoRate;
            presetToSave.VibratoDepth = this.VibratoDepth;
            presetToSave.TremoloRate = this.TremoloRate;
            presetToSave.TremoloDepth = this.TremoloDepth;

            if (isNew)
            {
                _presetRepository.AddPreset(presetToSave);
            }
            else
            {
                _presetRepository.UpdatePreset(presetToSave);
            }

            LoadPresets();
        }

        private void DeleteSelectedPreset()
        {
            if (SelectedPreset != null && SelectedPreset.IsUserPreset)
            {
                if (MessageBox.Show($"Are you sure you want to delete '{SelectedPreset.Name}'?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _presetRepository.DeletePreset(SelectedPreset.IdPreset);
                    LoadPresets();
                }
            }
        }

        private void SetOctave(int newOctave) { if (newOctave < -2 || newOctave > 2) return; Octave = newOctave; UpdateKeyFrequencies(); }
        private void UpdateKeyFrequencies() { double octaveMultiplier = Math.Pow(2, _octave); foreach (var keyViewModel in PianoKeys) { keyViewModel.Frequency = keyViewModel.BaseFrequency * octaveMultiplier; } }
        public void HandleKeyDown(Key key) { if (_keyMappings.TryGetValue(key, out var pianoKey) && !pianoKey.IsPressed) { pianoKey.IsPressed = true; pianoKey.NoteOnCommand.Execute(null); } }
        public void HandleKeyUp(Key key) { if (_keyMappings.TryGetValue(key, out var pianoKey) && pianoKey.IsPressed) { pianoKey.IsPressed = false; pianoKey.NoteOffCommand.Execute(null); } }
        public void PlayNote(double frequency) { _audioEngine.NoteOn(frequency); }
        public void StopNote(double frequency) { _audioEngine.NoteOff(frequency); }
        private void InitializePianoKeys()
        {
            var notes = new[]
            {
                new { Name = "C4", Freq = 261.63, Type = KeyType.White, Key = Key.Z }, new { Name = "C#4", Freq = 277.18, Type = KeyType.Black, Key = Key.S },
                new { Name = "D4", Freq = 293.66, Type = KeyType.White, Key = Key.X }, new { Name = "D#4", Freq = 311.13, Type = KeyType.Black, Key = Key.D },
                new { Name = "E4", Freq = 329.63, Type = KeyType.White, Key = Key.C }, new { Name = "F4", Freq = 349.23, Type = KeyType.White, Key = Key.V },
                new { Name = "F#4", Freq = 369.99, Type = KeyType.Black, Key = Key.G }, new { Name = "G4", Freq = 392.00, Type = KeyType.White, Key = Key.B },
                new { Name = "G#4", Freq = 415.30, Type = KeyType.Black, Key = Key.H }, new { Name = "A4", Freq = 440.00, Type = KeyType.White, Key = Key.N },
                new { Name = "A#4", Freq = 466.16, Type = KeyType.Black, Key = Key.J }, new { Name = "B4", Freq = 493.88, Type = KeyType.White, Key = Key.M },
                new { Name = "C5", Freq = 523.25, Type = KeyType.White, Key = Key.OemComma }
            };
            foreach (var note in notes) { var keyViewModel = new PianoKeyViewmodel(note.Name, note.Type, note.Freq, this); PianoKeys.Add(keyViewModel); _keyMappings.Add(note.Key, keyViewModel); }
        }
        #endregion

        #region INotifyPropertyChanged & IDisposable
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        public void Dispose() { _audioEngine.Dispose(); OscilloscopeViewmodel.Dispose(); }
        #endregion
    }
}
