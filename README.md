![Ceenth Logo](https://i.imgur.com/6CL8BDM.png)



A basic **standalone** synthesizer app, with a basic effects suite and waveform preview.

*Now updated to not use the shitty .NET Framework and ADO.NET Entity Framework!*

The initial scope of this project was to be used as a final school project for the **Programming II** class, but ever since then, it has been updated with loads of goodies and a more up-to-date design language.

The focus of this project is to apply basic knowledge of the **MVVM (Model-View-Viewmodel)** architecture, also applying basic **CRUD (Create, Remove, Update, Delete)** operations.

The polyphonic audio engine was made using the [NAudio](https://github.com/naudio/NAudio) library, making it possible to do basic effects, filters, and more.

## Demo

![Demo](https://i.imgur.com/kbO61R7.gif)


## Features

- Up-to-date design language, made using **XAML**
- Polyphonic engine, which supports chording
- Basic low-pass filter (Filter cutoff + Q - which is at what Hz the filter applies to)
- ADSR suite *(only attack and release for now)*
- Basic effects for sound manipulation *(tremolo, vibrato, and more to be added soon)*
- Preset support *(comes with four factory presets)*
- Waveform preview via a basic **Oscilloscope**


## To-do

- *MIDI support - can be added via NAudio library*
- *Finish effects suite - add a basic Reverb, and other effects*
