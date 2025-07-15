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
        // _source trata de ir buscar a waveform
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
        // _source vai buscar a waveform ao ISampleProvider
        private readonly ISampleProvider _source;
        // parametros necessários para o cálculo final
        private readonly int _channels;
        private double a0, a1, a2, b1, b2;
        private readonly float[] x1, x2, y1, y2;
        public WaveFormat WaveFormat => _source.WaveFormat;
        public FilterSampleProvider(ISampleProvider source) { _source = source; _channels = source.WaveFormat.Channels; x1 = new float[_channels]; x2 = new float[_channels]; y1 = new float[_channels]; y2 = new float[_channels]; }
        /// <summary>
        /// Este código trata de criar o low-pass filter
        /// Estes cálculos são demasiado complicados para eu compreender
        /// E só os consegui executar com ajuda da documentação e de inteligência artificial
        /// Funciona! dentro dos possíveis
        /// </summary>
        public void SetLowPassFilter(float sampleRate, float cutoff, float q) { q = Math.Max(0.001f, q); double w0 = 2 * Math.PI * cutoff / sampleRate; double cosw0 = Math.Cos(w0); double sinw0 = Math.Sin(w0); double alpha = sinw0 / (2 * q); double b0_temp = (1 - cosw0) / 2; b1 = 1 - cosw0; b2 = (1 - cosw0) / 2; a0 = 1 + alpha; a1 = -2 * cosw0; a2 = 1 - alpha; b2 = b2 / a0; b1 = b1 / a0; a2 = a2 / a0; a1 = a1 / a0; a0 = b0_temp / a0; }
        /// Este código aplica o low-pass filter à função "Read"
        public int Read(float[] buffer, int offset, int count) { int samplesRead = _source.Read(buffer, offset, count); for (int i = 0; i < samplesRead; i++) { int channel = i % _channels; float input = buffer[offset + i]; float result = (float)(a0 * input + a1 * x1[channel] + a2 * x2[channel] - b1 * y1[channel] - b2 * y2[channel]); x2[channel] = x1[channel]; x1[channel] = input; y2[channel] = y1[channel]; y1[channel] = result; buffer[offset + i] = result; } return samplesRead; }
    }

    /// Aplica o efeito Tremolo, que faz o volume do som "pulsar" ritmicamente.
    /// Usa um oscilador de baixa frequência (LFO) para modular o volume.
    internal class TremoloSampleProvider : ISampleProvider
    {
        // Mesma coisa do código de cima - o ISampleProvider vai buscar a waveform
        private readonly ISampleProvider _source;
        // Faz um oscillator novo, que neste caso é o LFO (oscilador de baixa frequência)
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
        // Esta classe representa uma "voz" do sintetizador.
        // Cada voz pode tocar uma nota diferente ao mesmo tempo (polifonia).
        public AdsrSampleProvider Adsr { get; set; }
        public ISampleProvider SignalChain { get; set; }
        public bool IsOn { get; set; }
        public double Frequency { get; set; }

        public SynthVoice()
        {
            IsOn = false; // Inicialmente, a voz está desligada.
        }
    }

    public class PolyphonicAudioEngine : IDisposable
    {
        // _waveOut é o dispositivo de saída de áudio (colunas, auscultadores, etc.)
        private readonly WaveOutEvent _waveOut;
        // _mixer mistura todas as vozes para tocar ao mesmo tempo
        private readonly MixingSampleProvider _mixer;
        // Lista de todas as vozes disponíveis para tocar notas
        private readonly List<SynthVoice> _voices;
        private readonly int _sampleRate;
        // _visualizerProvider serve para mostrar a waveform no osciloscópio
        private readonly VisualizerSampleProvider _visualizerProvider;

        #region Propriedades
        // Propriedades para controlar o som gerado pelo sintetizador
        public float AttackSeconds { get; set; } = 0.01f; // Tempo de ataque da nota
        public float ReleaseSeconds { get; set; } = 0.05f; // Tempo de libertação da nota
        public WaveformTypes Waveform { get; set; } // Tipo de waveform (seno, quadrada, etc.)
        public float FilterCutoff { get; set; } = 20000f; // Frequência de corte do filtro
        public float FilterQ { get; set; } = 1.0f; // Qualidade do filtro
        public float VibratoRate { get; set; } = 5f; // Velocidade do vibrato
        public float VibratoDepth { get; set; } = 0f; // Intensidade do vibrato
        public float TremoloRate { get; set; } = 5f; // Velocidade do tremolo
        public float TremoloDepth { get; set; } = 0f; // Intensidade do tremolo
        #endregion

        public PolyphonicAudioEngine(int sampleRate = 44100, int channelCount = 1, int voiceCount = 16)
        {
            // Inicialização dos componentes principais do motor de áudio
            _sampleRate = sampleRate;
            _waveOut = new WaveOutEvent { DesiredLatency = 100 };
            _mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount)) { ReadFully = true };
            _voices = new List<SynthVoice>();

            for (int i = 0; i < voiceCount; i++)
            {
                _voices.Add(new SynthVoice()); // Cria as vozes disponíveis
            }

            _visualizerProvider = new VisualizerSampleProvider(_mixer, 1024);

            _waveOut.Init(_visualizerProvider);
            _waveOut.Play();

            // Subscreve o evento para saber quando um som termina
            _mixer.MixerInputEnded += OnMixerInputEnded;
        }

        public void GetWaveformData(float[] buffer)
        {
            // Vai buscar os dados da waveform para o osciloscópio
            _visualizerProvider.GetWaveformData(buffer);
        }

        public void NoteOn(double frequency)
        {
            // Procura uma voz livre para tocar a nova nota
            var freeVoice = _voices.FirstOrDefault(v => v.SignalChain == null);
            if (freeVoice == null) return;

            freeVoice.IsOn = true;
            freeVoice.Frequency = frequency;

            // Cria o oscilador para gerar o som da nota
            var oscillator = new Oscillator(_sampleRate, 1)
            {
                Waveform = this.Waveform,
                Frequency = frequency,
                Amplitude = 0.25
            };
            oscillator.Reset();

            // Aplica o envelope ADSR à nota
            var adsr = new AdsrSampleProvider(oscillator)
            {
                AttackSeconds = this.AttackSeconds,
                ReleaseSeconds = this.ReleaseSeconds
            };
            freeVoice.Adsr = adsr;

            // Aplica o filtro low-pass
            var filter = new FilterSampleProvider(adsr);
            filter.SetLowPassFilter(_sampleRate, this.FilterCutoff, this.FilterQ);

            // Adiciona vibrato (modulação de frequência)
            var vibratoLfo = new Oscillator(_sampleRate, 1) { Frequency = this.VibratoRate, Amplitude = 1.0 };
            oscillator.LfoSource = vibratoLfo;
            oscillator.LfoDepth = this.VibratoDepth;

            // Adiciona tremolo (modulação de volume)
            var tremoloLfo = new Oscillator(_sampleRate, 1) { Frequency = this.TremoloRate, Amplitude = 1.0 };
            var tremolo = new TremoloSampleProvider(filter, tremoloLfo) { Depth = this.TremoloDepth };

            freeVoice.SignalChain = tremolo;

            // Adiciona a voz ao mixer para tocar
            _mixer.AddMixerInput(freeVoice.SignalChain);
        }

        public void NoteOff(double frequency)
        {
            // Procura a voz que está a tocar a nota e inicia a fase de libertação
            var activeVoice = _voices.FirstOrDefault(v => v.IsOn && Math.Abs(v.Frequency - frequency) < 0.001);
            if (activeVoice != null)
            {
                activeVoice.IsOn = false;
                // Isto inicia a fase de release. O evento MixerInputEnded trata da limpeza.
                activeVoice.Adsr?.Stop();
            }
        }

        private void OnMixerInputEnded(object sender, SampleProviderEventArgs e)
        {
            // Quando uma voz termina, marca-a como livre para ser reutilizada
            var finishedVoice = _voices.FirstOrDefault(v => v.SignalChain == e.SampleProvider);
            if (finishedVoice != null)
            {
                finishedVoice.SignalChain = null;
                finishedVoice.Frequency = 0;
                finishedVoice.Adsr = null;
            }
        }

        public void Dispose()
        {
            // Liberta os recursos do dispositivo de áudio
            _waveOut.Dispose();
        }
    }
}
