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
        private readonly PolyphonicAudioEngine _audioEngine;
        private readonly DispatcherTimer _timer;
        private PointCollection _waveformPoints;
        private readonly float[] _waveformBuffer;

        // --- NEW: A property for our diagnostic peak meter ---
        private float _peakValue;

        public PointCollection WaveformPoints
        {
            get => _waveformPoints;
            set { _waveformPoints = value; OnPropertyChanged(); }
        }

        // --- NEW: The public property for the UI to bind to ---
        public float PeakValue
        {
            get => _peakValue;
            set { _peakValue = value; OnPropertyChanged(); }
        }

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

            // --- NEW: Calculate the peak value from the buffer ---
            // We use Linq's Max() function to find the absolute maximum value.
            PeakValue = _waveformBuffer.Select(v => Math.Abs(v)).Max();

            double displayWidth = 1024;
            double displayHeight = 200;
            var newPoints = new PointCollection();

            for (int i = 0; i < _waveformBuffer.Length; i++)
            {
                double x = i;
                double y = (displayHeight / 2) - (_waveformBuffer[i] * (displayHeight / 2));
                newPoints.Add(new System.Windows.Point(x, y));
            }

            newPoints.Freeze();
            WaveformPoints = newPoints;
        }

        public void Dispose()
        {
            _timer.Stop();
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
