> [!WARNING]  
> The comments are almost **ALL** in Portuguese, I'm working on translating to English, but for now, since I don't have any contributors, it will stay like this. Thanks!

<a id="readme-top"></a>

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/tlongle/ceenth">
    <img src="https://i.imgur.com/6CL8BDM.png" alt="Ceenth Logo" width="250" height="250">
  </a>
    <br />
    <br />

  <p align="center">
    A standalone software synthesizer for Windows built with .NET 8 and WPF/XAML.  
    Real-time waveform generation, polyphony, oscilloscope preview, filters, effects, and presets.
    <br />
    <br />
    <a href="https://github.com/tlongle/ceenth/releases">Download Release</a>
    &middot;
    <a href="https://github.com/tlongle/ceenth/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://github.com/tlongle/ceenth/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>



<!-- ABOUT THE PROJECT -->
## About The Project

Ceenth is a lightweight synthesizer that generates sound in real time.  
It features a polyphonic engine, oscilloscope waveform preview, a basic low-pass filter with resonance, AxxR envelope (Attack & Release currently), effects like tremolo and vibrato, and preset storage via SQLite.

Originally built as a final project for **Programming II**, it has since evolved into a modern, maintainable .NET 8 app using MVVM and WPF.


<!-- GETTING STARTED -->
## Getting Started

Follow these steps to run Ceenth locally.

### Prerequisites

- Windows 10/11 (for now) 
- .NET 8 **Desktop Runtime** (for running) and **SDK** (for building)  
  Download: https://dotnet.microsoft.com/download/dotnet/8.0


### Installation

1. Clone the repository
   ```sh
   git clone https://github.com/tlongle/ceenth.git
   cd ceenth
   ```
2. Restore dependencies
   ```sh
   dotnet restore
   ```
3. Build and run (Release)
   ```sh
   dotnet run --project src/Ceenth -c Release
   ```

> If your project path differs, adjust the `--project` path accordingly.

## Usage

- Play chords with the polyphonic engine  
- Tweak oscillator, filter cutoff, and resonance (Q)  
- Shape sound with ADSR (A/R for now)  
- Add tremolo and vibrato  
- Save and load presets (SQLite)  
- Visualize output with the oscilloscope

_For packaged builds, see Releases: https://github.com/tlongle/ceenth/releases_

## Roadmap

- [ ] MIDI input support (via NAudio)
- [ ] Effects: Reverb, Chorus, Delay
- [ ] Full ADSR (add Decay & Sustain)

See the [open issues](https://github.com/tlongle/ceenth/issues) for the full list of planned features and known issues.

<!-- CONTRIBUTING -->
## Contributing

Contributions are welcome!
