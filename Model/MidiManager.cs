using NAudio.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ceenth.Model
{
    public class MidiManager : IDisposable
    {
        private MidiIn _midiIn;

        // Events that the ViewModel can subscribe to
        public event EventHandler<MidiNoteEventArgs> NoteOnReceived;
        public event EventHandler<MidiNoteEventArgs> NoteOffReceived;

        /// <summary>
        /// Gets a list of all available MIDI input devices.
        /// </summary>
        /// <returns>A dictionary where the key is the device ID and the value is the product name.</returns>
        public static Dictionary<int, string> GetInputDevices()
        {
            var devices = new Dictionary<int, string>();
            for (int deviceId = 0; deviceId < MidiIn.NumberOfDevices; deviceId++)
            {
                devices.Add(deviceId, MidiIn.DeviceInfo(deviceId).ProductName);
            }
            return devices;
        }

        /// <summary>
        /// Starts listening for MIDI messages on a specific device.
        /// </summary>
        /// <param name="deviceId">The ID of the MIDI device to connect to.</param>
        public void StartListening(int deviceId)
        {
            // Stop listening to any previous device
            StopListening();

            if (deviceId >= 0 && deviceId < MidiIn.NumberOfDevices)
            {
                _midiIn = new MidiIn(deviceId);
                // Subscribe to the MessageReceived event from NAudio
                _midiIn.MessageReceived += MidiIn_MessageReceived;
                _midiIn.Start();
            }
        }

        /// <summary>
        /// Stops listening for MIDI messages.
        /// </summary>
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
