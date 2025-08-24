using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ceenth.Viewmodel
{
    // Self-explanatory enum for key types
    public enum KeyType
    {
        White,
        Black
    }

    public class PianoKeyViewmodel : INotifyPropertyChanged
    {
        // Propriedades para o estado do teclado
        private bool _isPressed;
        private double _frequency;
        public KeyType KeyType { get; }
        public string NoteName { get; }
        // Guarda o estado da tecla original (sem alterações de oitava)
        public double BaseFrequency { get; }
        // Commands para tocar e parar notas
        public ICommand NoteOnCommand { get; }
        public ICommand NoteOffCommand { get; }

        // Frequência agora é uma propriedade pública que pode ser atualizada externamente.
        // Isto faz com que o MainViewmodel possa alterar a frequência da nota quando necessário.
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

        // tá pressionado? 
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
            // Inicializa as propriedades do teclado
            NoteName = noteName;
            KeyType = keyType;
            BaseFrequency = baseFrequency;
            Frequency = baseFrequency;

            // Os comandos agora vão usar a frequência atualizada em cima
            NoteOnCommand = new RelayCommand(_ => parentViewModel.PlayNote(this.Frequency));
            NoteOffCommand = new RelayCommand(_ => parentViewModel.StopNote(this.Frequency));
        }

        #region INotifyPropertyChanged Implementation
        // Implementação do INotifyPropertyChanged para notificar a UI sobre mudanças nas propriedades
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
