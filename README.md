<a id="readme-top"></a>

<!-- PROJECT SHIELDS -->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![License][license-shield]][license-url]



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



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

[![Ceenth Demo][product-screenshot]](https://github.com/tlongle/ceenth)

Ceenth is a lightweight synthesizer that generates sound in real time — no samples, just pure waveforms.  
It features a polyphonic engine (chords), oscilloscope waveform preview, a basic low-pass filter with resonance, ADSR envelope (Attack & Release currently), effects like tremolo and vibrato, and preset storage via SQLite.

Originally built as a final project for **Programming II**, it has since evolved into a modern, maintainable .NET 8 app using MVVM and WPF.


### Built With

* [![DotNet][DotNet-badge]][DotNet-url]
* [![WPF][WPF-badge]][WPF-url]
* [![NAudio][NAudio-badge]][NAudio-url]
* [![SQLite][SQLite-badge]][SQLite-url]
* [![Windows][Windows-badge]][Windows-url]

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

Follow these steps to run Ceenth locally or publish a lightweight framework-dependent build.

### Prerequisites

- Windows 10/11  
- .NET 8 **Desktop Runtime** (for running) and **SDK** (for building)  
  Download: https://dotnet.microsoft.com/download/dotnet/8.0

<p align="right">(<a href="#readme-top">back to top</a>)</p>



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

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE -->
## Usage

- Play chords with the polyphonic engine  
- Tweak oscillator, filter cutoff, and resonance (Q)  
- Shape sound with ADSR (A/R for now)  
- Add tremolo and vibrato  
- Save and load presets (SQLite)  
- Visualize output with the oscilloscope

_For packaged builds, see Releases: https://github.com/tlongle/ceenth/releases_

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- ROADMAP -->
## Roadmap

- [ ] MIDI input support (via NAudio)
- [ ] Effects: Reverb, Chorus, Delay
- [ ] Full ADSR (add Decay & Sustain)

See the [open issues](https://github.com/tlongle/ceenth/issues) for the full list of planned features and known issues.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are welcome!

1. Fork the repository  
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)  
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)  
4. Push to the branch (`git push origin feature/AmazingFeature`)  
5. Open a Pull Request

### Top contributors

<a href="https://github.com/tlongle/ceenth/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=tlongle/ceenth" alt="contrib.rocks image" />
</a>

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- LICENSE -->
## License

Distributed under the license specified in `LICENSE`. See the file for details.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Project maintainer: https://github.com/tlongle  
Project Link: https://github.com/tlongle/ceenth

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

- [NAudio](https://github.com/naudio/NAudio)
- [SQLite](https://www.sqlite.org/)
- Everyone filing issues and PRs

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
[contributors-shield]: https://img.shields.io/github/contributors/tlongle/ceenth.svg?style=for-the-badge
[contributors-url]: https://github.com/tlongle/ceenth/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/tlongle/ceenth.svg?style=for-the-badge
[forks-url]: https://github.com/tlongle/ceenth/network/members
[stars-shield]: https://img.shields.io/github/stars/tlongle/ceenth.svg?style=for-the-badge
[stars-url]: https://github.com/tlongle/ceenth/stargazers
[issues-shield]: https://img.shields.io/github/issues/tlongle/ceenth.svg?style=for-the-badge
[issues-url]: https://github.com/tlongle/ceenth/issues
[license-shield]: https://img.shields.io/github/license/tlongle/ceenth.svg?style=for-the-badge
[license-url]: https://github.com/tlongle/ceenth/blob/main/LICENSE

[product-screenshot]: https://i.imgur.com/1Q4OPib.gif

[DotNet-badge]: https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white
[DotNet-url]: https://dotnet.microsoft.com/

[WPF-badge]: https://img.shields.io/badge/WPF-XAML-0C7BD6?style=for-the-badge&logo=windows&logoColor=white
[WPF-url]: https://learn.microsoft.com/en-us/dotnet/desktop/wpf/

[NAudio-badge]: https://img.shields.io/badge/NAudio-Audio-6DA252?style=for-the-badge
[NAudio-url]: https://github.com/naudio/NAudio

[SQLite-badge]: https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white
[SQLite-url]: https://www.sqlite.org/

[Windows-badge]: https://img.shields.io/badge/Windows-10%2F11-0078D6?style=for-the-badge&logo=windows&logoColor=white
[Windows-url]: https://www.microsoft.com/windows
