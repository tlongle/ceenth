using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace ceenth.Model
{
    /// <summary>
    /// A classe Oscillator, vai tratar de geraçao de som, a partir de Waveforms
    /// Chama sempre a classe ISampleProvider da biblioteca NAudio, para a geraçao do som 
    /// e os cálculos necessários a serem feitos para gerar N waveform
    /// </summary>

    // Tipos de waveform

    public enum WaveformTypes
    {
        Sine,
        Square,
        Saw,
        Triangle
    }

    public class Oscillator : ISampleProvider
    {
        // Declarar variáveis essenciais para manipulaçao do waveform no VM/V
        private double _frequency;
        private double _amplitude;
        private WaveformTypes _waveform;
        private double _phase;
        private readonly WaveFormat _waveFormat;
        public Oscillator LfoSource { get; set; }
        private readonly float[] _lfoBuffer;
        public double LfoDepth { get; set; }

        public double Frequency
        {
            // Limita a frequencia para ser SEMPRE positiva, absolutamente necessário para a geraçao do waveform
            get => _frequency;
            set => _frequency = Math.Max(0, value);
        }

        public double Amplitude
        {
            get => _amplitude;
            set => _amplitude = Math.Max(0, Math.Min(1, value)); // valor entre 0 e 1
        }

        public void Reset()
        {
            _phase = 0;
        }

        public WaveformTypes Waveform
        {
            get => _waveform;
            set => _waveform = value;
        }
        public WaveFormat WaveFormat => _waveFormat;

        public Oscillator(int sampleRate = 44100, int channels = 1)
        {
            _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels);
            _frequency = 440.0; // Nota por defeito é A4 (Lá no oitavo 4)
            _amplitude = 0.25;  // Amplitude da wavetable por defeito é 0.25
            _waveform = WaveformTypes.Sine;
            _lfoBuffer = new float[1];
        }

        public int Read(float[] buffer, int offset, int count)
        {
            double lfoValue = 0;
            // Um buffer para ler uma sample de cada vez do LFO, se estiver conectado   
            if (LfoSource != null)
            {
                LfoSource.Read(_lfoBuffer, 0, 1);
                lfoValue = _lfoBuffer[0];
            }

            for (int i = 0; i < count; i++)
            {
                // Se um LFO estiver conectado, ler o valor do LFO
                if (LfoSource != null && _lfoBuffer != null)
                {
                    LfoSource.Read(_lfoBuffer, 0, 1);
                    lfoValue = _lfoBuffer[0];
                }

                // Calcular a frequência modulada pelo LFO
                double modulatedFrequency = _frequency + (lfoValue * LfoDepth);

                // Calcular o incremento de fase baseado na frequência modulada
                double phaseIncrement = modulatedFrequency / _waveFormat.SampleRate;

                float sampleValue = 0;
                // Calcular o valor do sample baseado no tipo de waveform
                // Cada waveform tem uma fórmula diferente para calcular o valor do sample:
                // Dando em sons diferentes, como Sine, Square, Saw e Triangle
                switch (_waveform)
                {
                    case WaveformTypes.Sine:
                        sampleValue = (float)(_amplitude * Math.Sin(2 * Math.PI * _phase));
                        break;
                    case WaveformTypes.Square:
                        sampleValue = (float)(_amplitude * (_phase < 0.5 ? 1 : -1));
                        break;
                    case WaveformTypes.Saw:
                        sampleValue = (float)(_amplitude * (2 * _phase - 1));
                        break;
                    case WaveformTypes.Triangle:
                        sampleValue = (float)(_amplitude * (4 * Math.Abs(_phase - 0.5) - 1));
                        break;
                }
                // Aplicar a amplitude e o valor do LFO
                buffer[offset + i] = sampleValue;
                _phase += phaseIncrement;

                // Normalizar a fase para o intervalo [0, 1)
                if (_phase >= 1.0)
                {
                    _phase -= 1.0;
                }
            }

            return count;
        }
    }
}
    