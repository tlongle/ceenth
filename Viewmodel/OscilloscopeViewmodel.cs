using ceenth.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace ceenth.Viewmodel
{
    public class OscilloscopeViewmodel : INotifyPropertyChanged, IDisposable
    {
        // Referência o engine de audio polifónico
        private readonly PolyphonicAudioEngine _audioEngine;
        // Timer para atualizar o osciloscópio a cada 33ms (~30 FPS)
        private readonly DispatcherTimer _timer;
        // Buffer para armazenar os dados do waveform
        private PointCollection _waveformPoints;
        // Buffer para armazenar os dados do waveform em formato de float
        private readonly float[] _waveformBuffer;

        // Uma propriedade que segura o valor do pico do waveform (para debug)
        private float _peakValue;

        // Propriedade para os pontos do waveform
        public PointCollection WaveformPoints
        {
            get => _waveformPoints;
            set { _waveformPoints = value; OnPropertyChanged(); }
        }

        // Propriedade para o valor do pico do waveform
        public float PeakValue
        {
            get => _peakValue;
            set { _peakValue = value; OnPropertyChanged(); }
        }

        // Construtor que recebe o engine de audio polifónico
        public OscilloscopeViewmodel(PolyphonicAudioEngine audioEngine)
        {
            _audioEngine = audioEngine;
            _waveformBuffer = new float[1024];
            WaveformPoints = new PointCollection();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(33); // ~30 FPS
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateWaveform();
        }

        private void UpdateWaveform()
        {
            _audioEngine.GetWaveformData(_waveformBuffer);

            // Calcula o valor máximo do waveform para exibição
            // Usamos a função Max() do LINQ para encontrar o valor absoluto máximo
            PeakValue = _waveformBuffer.Select(v => Math.Abs(v)).Max();
            double displayHeight = 200;
            var newPoints = new PointCollection();

            // Limpa os pontos antigos e cria novos
            for (int i = 0; i < _waveformBuffer.Length; i++)
            {
                double x = i;
                double y = (displayHeight / 2) - (_waveformBuffer[i] * (displayHeight / 2));
                newPoints.Add(new System.Windows.Point(x, y));
            }

            // Adiciona um ponto extra para fechar o loop do osciloscópio
            newPoints.Freeze();
            // Atualiza a coleção de pontos do osciloscópio
            WaveformPoints = newPoints;
        }

        // Método para parar o timer e liberar recursos, porque se não explodia quase de certeza
        public void Dispose()
        {
            _timer.Stop();
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
