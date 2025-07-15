using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ceenth.Viewmodel // Make sure this namespace matches your project
{
    public enum KeyType
    {
        White,
        Black
    }

    public class PianoKeyViewmodel : INotifyPropertyChanged
    {
        private bool _isPressed;
        private double _frequency; // Add a private backing field for the frequency

        public KeyType KeyType { get; }
        public string NoteName { get; }
        // --- NEW: Stores the key's original, unmodified frequency ---
        public double BaseFrequency { get; }

        public ICommand NoteOnCommand { get; }
        public ICommand NoteOffCommand { get; }

        // --- UPDATED: Frequency now has a public setter ---
        // This allows the MainViewModel to update it when the octave changes.
        public double Frequency
        {
            get => _frequency;
            set
            {
                if (_frequency != value)
                {
                    _frequency = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsPressed
        {
            get => _isPressed;
            set
            {
                if (_isPressed != value)
                {
                    _isPressed = value;
                    OnPropertyChanged();
                }
            }
        }

        public PianoKeyViewmodel(string noteName, KeyType keyType, double baseFrequency, MainViewmodel parentViewModel)
        {
            NoteName = noteName;
            KeyType = keyType;
            // --- UPDATED: Store the base frequency and set the initial frequency ---
            BaseFrequency = baseFrequency;
            Frequency = baseFrequency;

            // The commands will now always use the current value of the public Frequency property
            NoteOnCommand = new RelayCommand(_ => parentViewModel.PlayNote(this.Frequency));
            NoteOffCommand = new RelayCommand(_ => parentViewModel.StopNote(this.Frequency));
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
