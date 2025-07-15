using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceenth.Model
{

    /// <summary>
    /// Esta feature ainda não foi implementada, por isso é irrelevante
    /// O código semi funciona, mas simplesmente não faz sentido gastar o meu tempo com isto agora
    /// </summary>
    public class MidiManager : IDisposable
    {
        private MidiIn _midiIn;

        // Eventos para o Viewmodel
        public event EventHandler<MidiNoteEventArgs> NoteOnReceived;
        public event EventHandler<MidiNoteEventArgs> NoteOffReceived;

        public static Dictionary<int, string> GetInputDevices()
        {
            var devices = new Dictionary<int, string>();
            for (int deviceId = 0; deviceId < MidiIn.NumberOfDevices; deviceId++)
            {
                devices.Add(deviceId, MidiIn.DeviceInfo(deviceId).ProductName);
            }
            return devices;
        }

        // Código que começa a "ouvir" para eventos de MIDI
        public void StartListening(int deviceId)
        {
            // Para de ouvir o dispositivo anterior (implementado para safe-keeping)
            StopListening();

            if (deviceId >= 0 && deviceId < MidiIn.NumberOfDevices)
            {
                _midiIn = new MidiIn(deviceId);
                _midiIn.MessageReceived += MidiIn_MessageReceived;
                _midiIn.Start();
            }
        }

        // Para de "ouvir" eventos vindo de MIDI
        public void StopListening()
        {
            if (_midiIn != null)
            {
                _midiIn.MessageReceived -= MidiIn_MessageReceived;
                _midiIn.Stop();
                _midiIn.Dispose();
                _midiIn = null;
            }
        }

        private void MidiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            // Check if the message is a NoteOn event
            if (e.MidiEvent is NoteOnEvent noteOnEvent && noteOnEvent.Velocity > 0)
            {
                // Convert MIDI note number to frequency
                double frequency = 440.0 * Math.Pow(2.0, (noteOnEvent.NoteNumber - 69.0) / 12.0);
                // Raise our custom event
                NoteOnReceived?.Invoke(this, new MidiNoteEventArgs(noteOnEvent.NoteNumber, frequency));
            }
            // Check if the message is a NoteOff event (or a NoteOn with 0 velocity)
            else if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOff || (e.MidiEvent is NoteOnEvent noteOnEventOff && noteOnEventOff.Velocity == 0))
            {
                int noteNumber = (e.MidiEvent as NoteEvent).NoteNumber;
                double frequency = 440.0 * Math.Pow(2.0, (noteNumber - 69.0) / 12.0);
                // Raise our custom event
                NoteOffReceived?.Invoke(this, new MidiNoteEventArgs(noteNumber, frequency));
            }
        }

        public void Dispose()
        {
            StopListening();
        }
    }

    /// <summary>
    /// A custom EventArgs class to pass MIDI note data.
    /// </summary>
    public class MidiNoteEventArgs : EventArgs
    {
        public int NoteNumber { get; }
        public double Frequency { get; }

        public MidiNoteEventArgs(int noteNumber, double frequency)
        {
            NoteNumber = noteNumber;
            Frequency = frequency;
        }
    }
}
