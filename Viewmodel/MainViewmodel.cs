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
        // Instâncias dos componentes principais
        private readonly PolyphonicAudioEngine _audioEngine;
        private readonly PresetRepository _presetRepository;
        private readonly Dictionary<Key, PianoKeyViewmodel> _keyMappings;

        // Objeto de placeholder para novos presets
        private readonly Preset _newPresetPlaceholder = new Preset { IdPreset = -1, Name = "[ New Preset ]", IsUserPreset = false };
        
        // Propriedades do Preset selecionado e novo nome
        private Preset _selectedPreset;
        private string _newPresetName = "New Preset";
        private int _octave = 0;

        // Parâmetros do synth
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

        #region Construtor
        public MainViewmodel()
        {
            // Inicializa os componentes principais
            _audioEngine = new PolyphonicAudioEngine();
            _presetRepository = new PresetRepository();
            _keyMappings = new Dictionary<Key, PianoKeyViewmodel>();

            // Coleções para o UI
            PianoKeys = new ObservableCollection<PianoKeyViewmodel>();
            Presets = new ObservableCollection<Preset>();

            // Commandos para ações do usuário
            // NOTA: Eu utilizei um RelayCommand modificado pois quis fazer isto de uma maneira diferente
            // e mais straight forward, sem necessidade de passar o _canExecute
            // Se isto dá me menos pontos, so be it, a este ponto só quero que o código funcione
            // e que seja fácil de ler... Dentro dos possiveis.

            SavePresetCommand = new RelayCommand(_ => SaveCurrentSettings());
            DeletePresetCommand = new RelayCommand(_ => DeleteSelectedPreset(), _ => SelectedPreset != null && SelectedPreset.IsUserPreset);
            IncreaseOctaveCommand = new RelayCommand(_ => SetOctave(_octave + 1));
            DecreaseOctaveCommand = new RelayCommand(_ => SetOctave(_octave - 1));

            // Inicialização do OscilloscopeViewmodel + Piano Keys e Presets
            OscilloscopeViewmodel = new OscilloscopeViewmodel(_audioEngine);
            InitializePianoKeys();
            LoadPresets();
        }
        #endregion

        #region Propriedades
        // Propriedades públicas para o ViewModel
        public OscilloscopeViewmodel OscilloscopeViewmodel { get; }
        public ObservableCollection<PianoKeyViewmodel> PianoKeys { get; }
        public ObservableCollection<Preset> Presets { get; }
        public ICommand SavePresetCommand { get; }
        public ICommand DeletePresetCommand { get; }
        public ICommand IncreaseOctaveCommand { get; }
        public ICommand DecreaseOctaveCommand { get; }

        // get set para o Preset selecionado e novo nome
        public string NewPresetName
        {
            get => _newPresetName;
            set { _newPresetName = value; OnPropertyChanged(); }
        }
        // Propriedade para o Preset selecionado, com lógica para lidar com a seleção do placeholder
        public Preset SelectedPreset
        {
            get => _selectedPreset;
            set
            {
                // Verifica se o _selectedPreset é diferente do novo placeholder
                if (_selectedPreset != value)
                {
                    // Se o novo valor for o placeholder, limpa as configurações para um novo preset
                    _selectedPreset = value;

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

        // Propriedade para a lista de tipos de waveforms
        public IEnumerable<WaveformTypes> Waveforms => Enum.GetValues(typeof(WaveformTypes)).Cast<WaveformTypes>();

        // Propriedade para o Octave, com lógica para notificar mudanças para o UI
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

        #region Parametros do Synth/Propriedades do Synth
        // Propriedades para os parâmetros do sintetizador, com lógica para notificar mudanças
        // e atualizar o áudio engine
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

        #region Métodos
        // O método LoadPresets carrega os presets do repositório e atualiza a coleção de presets
        private void LoadPresets()
        {
            var currentId = SelectedPreset?.IdPreset;
            Presets.Clear();

            // Adiciona o placeholder para novos presets
            Presets.Add(_newPresetPlaceholder);

            var presetsFromDb = _presetRepository.GetAllPresets();
            foreach (var preset in presetsFromDb)
            {
                Presets.Add(preset);
            }

            SelectedPreset = Presets.FirstOrDefault(p => p.IdPreset == currentId) ?? Presets.FirstOrDefault(p => p.IdPreset != -1);
        }

        // O método LoadPreset carrega os valores do preset selecionado para as propriedades do ViewModel
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

        // Este método limpa as configurações atuais para preparar a criação de um novo preset
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

        // O método SaveCurrentSettings salva o preset atual ou cria um novo, dependendo se um preset já está selecionado
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
            // Caso contrário, cria um novo preset.
            else
            {
                presetToSave = new Preset { IsUserPreset = true };
                isNew = true;
            }

            // Define os valores do preset a partir das propriedades do ViewModel
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

            // Se for um novo preset, adiciona ao repositório, caso contrário, atualiza o existente
            if (isNew)
            {
                _presetRepository.AddPreset(presetToSave);
            }
            else
            {
                _presetRepository.UpdatePreset(presetToSave);
            }

            // Bora! Corre essa miséria!
            LoadPresets();
        }

        // O método DeleteSelectedPreset remove o preset selecionado, se for um preset de usuário
        private void DeleteSelectedPreset()
        {
            // Verifica se o preset selecionado é um preset de usuário e se não é o placeholder
            if (SelectedPreset != null && SelectedPreset.IsUserPreset)
            {
                // Confirmação antes de deletar
                // A MessageBox está em inglês porque why not... that's why
                if (MessageBox.Show($"Are you sure you want to delete '{SelectedPreset.Name}'?", "Confirm Delete", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _presetRepository.DeletePreset(SelectedPreset.IdPreset);
                    LoadPresets();
                }
            }
        }
        // O método SetOctave ajusta o oitavo atual e atualiza as frequências das teclas do piano
        private void SetOctave(int newOctave) { if (newOctave < -2 || newOctave > 2) return; Octave = newOctave; UpdateKeyFrequencies(); }
        // O método UpdateKeyFrequencies atualiza as frequências de todas as teclas do piano com base no oitavo atual
        private void UpdateKeyFrequencies() { double octaveMultiplier = Math.Pow(2, _octave); foreach (var keyViewModel in PianoKeys) { keyViewModel.Frequency = keyViewModel.BaseFrequency * octaveMultiplier; } }
        // Os métodos HandleKeyDown e HandleKeyUp lidam com os eventos de pressionar e soltar teclas do teclado
        public void HandleKeyDown(Key key) { if (_keyMappings.TryGetValue(key, out var pianoKey) && !pianoKey.IsPressed) { pianoKey.IsPressed = true; pianoKey.NoteOnCommand.Execute(null); } }
        public void HandleKeyUp(Key key) { if (_keyMappings.TryGetValue(key, out var pianoKey) && pianoKey.IsPressed) { pianoKey.IsPressed = false; pianoKey.NoteOffCommand.Execute(null); } }
        // Os métodos PlayNote e StopNote são usados para tocar e parar notas com base na frequência
        public void PlayNote(double frequency) { _audioEngine.NoteOn(frequency); }
        public void StopNote(double frequency) { _audioEngine.NoteOff(frequency); }

        // Método que inicializa as teclas do piano com suas respectivas notas, frequências e tipos
        private void InitializePianoKeys()
        {
            // Cria um array de notas com suas propriedades
            // Segue o clássico Dó Ré Mi Fá Sol Lá Si Dó (C4 a C5)
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
            // Inicializa as teclas do piano com as notas definidas
            foreach (var note in notes) { var keyViewModel = new PianoKeyViewmodel(note.Name, note.Type, note.Freq, this); PianoKeys.Add(keyViewModel); _keyMappings.Add(note.Key, keyViewModel); }
        }
        #endregion

        #region INotifyPropertyChanged & IDisposable
        // Implementação do INotifyPropertyChanged para notificar mudanças nas propriedades
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        public void Dispose() { _audioEngine.Dispose(); OscilloscopeViewmodel.Dispose(); }
        #endregion
    }
}
