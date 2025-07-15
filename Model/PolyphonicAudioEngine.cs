using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ceenth.Model
{
    /// Nota sobre o projeto:
    /// 
    /// Este projeto deu me bastante trabalho a fazer.
    /// A biblioteca de audio que eu utilizei (NAudio) está deprecada e é MUITO confusa.
    /// Para ver como certas coisas funcionavam, tive de ir "sailing the high seas",
    /// para achar um PDF de um livro lançado à anos atrás, para só semi compreender como as coisas funcionam.
    /// Como isto tem, muita, mas MUITA matemática, 
    /// tive de pedir assistencia ao meu amigo Google Gemini em conjunto com o livro.

    #region Classes de Apoio para Efeitos

    /// Esta classe é uma classe especial para mostrar no osciloscópio,
    /// o som que está a tocar nas colunas do dispositivo
    internal class VisualizerSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly float[] _circularBuffer;
        private readonly object _lockObject = new object();
        private int _writePosition;
        public WaveFormat WaveFormat => _source.WaveFormat;
        public VisualizerSampleProvider(ISampleProvider source, int bufferSize = 1024) { _source = source; _circularBuffer = new float[bufferSize]; }
        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _source.Read(buffer, offset, count);
            lock (_lockObject)
            {
                for (int i = 0; i < samplesRead; i++)
                {
                    _circularBuffer[_writePosition] = buffer[offset + i];
                    _writePosition = (_writePosition + 1) % _circularBuffer.Length;
                }
            }
            return samplesRead;
        }
        public void GetWaveformData(float[] destinationBuffer)
        {
            lock (_lockObject)
            {
                int bufferLength = _circularBuffer.Length;
                int readPosition = (_writePosition + 1) % bufferLength;
                for (int i = 0; i < bufferLength; i++)
                {
                    destinationBuffer[i] = _circularBuffer[readPosition];
                    readPosition = (readPosition + 1) % bufferLength;
                }
            }
        }
    }

    /// Aplica um filtro "Low-Pass", que corta as frequências altas do som.
    /// É o que faz o som ficar mais "abafado"
    public class FilterSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly int _channels;
        private double a0, a1, a2, b1, b2;
        private readonly float[] x1, x2, y1, y2;
        public WaveFormat WaveFormat => _source.WaveFormat;
        public FilterSampleProvider(ISampleProvider source) { _source = source; _channels = source.WaveFormat.Channels; x1 = new float[_channels]; x2 = new float[_channels]; y1 = new float[_channels]; y2 = new float[_channels]; }
        public void SetLowPassFilter(float sampleRate, float cutoff, float q) { q = Math.Max(0.001f, q); double w0 = 2 * Math.PI * cutoff / sampleRate; double cosw0 = Math.Cos(w0); double sinw0 = Math.Sin(w0); double alpha = sinw0 / (2 * q); double b0_temp = (1 - cosw0) / 2; b1 = 1 - cosw0; b2 = (1 - cosw0) / 2; a0 = 1 + alpha; a1 = -2 * cosw0; a2 = 1 - alpha; b2 = b2 / a0; b1 = b1 / a0; a2 = a2 / a0; a1 = a1 / a0; a0 = b0_temp / a0; }
        public int Read(float[] buffer, int offset, int count) { int samplesRead = _source.Read(buffer, offset, count); for (int i = 0; i < samplesRead; i++) { int channel = i % _channels; float input = buffer[offset + i]; float result = (float)(a0 * input + a1 * x1[channel] + a2 * x2[channel] - b1 * y1[channel] - b2 * y2[channel]); x2[channel] = x1[channel]; x1[channel] = input; y2[channel] = y1[channel]; y1[channel] = result; buffer[offset + i] = result; } return samplesRead; }
    }

    /// Aplica o efeito Tremolo, que faz o volume do som "pulsar" ritmicamente.
    /// Usa um oscilador de baixa frequência (LFO) para modular o volume.
    internal class TremoloSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly Oscillator _lfo;
        private float _depth;
        private readonly float[] _lfoBuffer;
        public WaveFormat WaveFormat => _source.WaveFormat;
        public float Depth { get => _depth; set => _depth = Math.Max(0, Math.Min(1, value)); }
        public TremoloSampleProvider(ISampleProvider source, Oscillator lfo) { _source = source; _lfo = lfo; Depth = 0f; _lfoBuffer = new float[4096]; }
        public int Read(float[] buffer, int offset, int count) { int lfoSamplesToRead = Math.Min(count, _lfoBuffer.Length); _lfo.Read(_lfoBuffer, 0, lfoSamplesToRead); int samplesRead = _source.Read(buffer, offset, count); for (int i = 0; i < samplesRead; i++) { float lfoSample = (_lfoBuffer[i] + 1) / 2; float multiplier = 1 - (lfoSample * Depth); buffer[offset + i] *= multiplier; } return samplesRead; }
    }
    #endregion

    internal class SynthVoice
    {
        public AdsrSampleProvider Adsr { get; set; }
        public ISampleProvider SignalChain { get; set; }
        public bool IsOn { get; set; }
        public double Frequency { get; set; }

        public SynthVoice()
        {
            IsOn = false;
        }
    }

    public class PolyphonicAudioEngine : IDisposable
    {
        private readonly WaveOutEvent _waveOut;
        private readonly MixingSampleProvider _mixer;
        private readonly List<SynthVoice> _voices;
        private readonly int _sampleRate;
        private readonly VisualizerSampleProvider _visualizerProvider;

        #region Properties
        public float AttackSeconds { get; set; } = 0.01f;
        public float ReleaseSeconds { get; set; } = 0.05f;
        public WaveformTypes Waveform { get; set; }
        public float FilterCutoff { get; set; } = 20000f;
        public float FilterQ { get; set; } = 1.0f;
        public float VibratoRate { get; set; } = 5f;
        public float VibratoDepth { get; set; } = 0f;
        public float TremoloRate { get; set; } = 5f;
        public float TremoloDepth { get; set; } = 0f;
        #endregion

        public PolyphonicAudioEngine(int sampleRate = 44100, int channelCount = 1, int voiceCount = 16)
        {
            _sampleRate = sampleRate;
            _waveOut = new WaveOutEvent { DesiredLatency = 100 };
            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount)) { ReadFully = true };
            _voices = new List<SynthVoice>();

            for (int i = 0; i < voiceCount; i++)
            {
                _voices.Add(new SynthVoice());
            }

            _visualizerProvider = new VisualizerSampleProvider(_mixer, 1024);

            _waveOut.Init(_visualizerProvider);
            _waveOut.Play();

            // Subscribe to the event that tells us when a sound has finished
            _mixer.MixerInputEnded += OnMixerInputEnded;
        }

        public void GetWaveformData(float[] buffer)
        {
            _visualizerProvider.GetWaveformData(buffer);
        }

        public void NoteOn(double frequency)
        {
            var freeVoice = _voices.FirstOrDefault(v => v.SignalChain == null);
            if (freeVoice == null) return;

            freeVoice.IsOn = true;
            freeVoice.Frequency = frequency;

            var oscillator = new Oscillator(_sampleRate, 1)
            {
                Waveform = this.Waveform,
                Frequency = frequency,
                Amplitude = 0.25
            };
            oscillator.Reset();

            var adsr = new AdsrSampleProvider(oscillator)
            {
                AttackSeconds = this.AttackSeconds,
                ReleaseSeconds = this.ReleaseSeconds
            };
            freeVoice.Adsr = adsr;

            var filter = new FilterSampleProvider(adsr);
            filter.SetLowPassFilter(_sampleRate, this.FilterCutoff, this.FilterQ);

            var vibratoLfo = new Oscillator(_sampleRate, 1) { Frequency = this.VibratoRate, Amplitude = 1.0 };
            oscillator.LfoSource = vibratoLfo;
            oscillator.LfoDepth = this.VibratoDepth;

            var tremoloLfo = new Oscillator(_sampleRate, 1) { Frequency = this.TremoloRate, Amplitude = 1.0 };
            var tremolo = new TremoloSampleProvider(filter, tremoloLfo) { Depth = this.TremoloDepth };

            // --- FIX: Store a reference to the final link in the chain ---
            freeVoice.SignalChain = tremolo;

            _mixer.AddMixerInput(freeVoice.SignalChain);
        }

        public void NoteOff(double frequency)
        {
            var activeVoice = _voices.FirstOrDefault(v => v.IsOn && Math.Abs(v.Frequency - frequency) < 0.001);
            if (activeVoice != null)
            {
                activeVoice.IsOn = false;
                // This starts the release phase. The MixerInputEnded event will handle cleanup.
                activeVoice.Adsr?.Stop();
            }
        }

        private void OnMixerInputEnded(object sender, SampleProviderEventArgs e)
        {
            // --- FIX: Find the voice by matching the entire signal chain ---
            var finishedVoice = _voices.FirstOrDefault(v => v.SignalChain == e.SampleProvider);
            if (finishedVoice != null)
            {
                // Mark the voice as free so it can be used by a new note
                finishedVoice.SignalChain = null;
                finishedVoice.Frequency = 0;
                finishedVoice.Adsr = null;
            }
        }

        public void Dispose()
        {
            _waveOut.Dispose();
        }
    }
}
