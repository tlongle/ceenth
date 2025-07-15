> [!WARNING]  
> THIS PROJECT IS NOT FINISHED! The current version of the project, was made JUST to be presented in my final class, there WILL be changes to the tech stack to make it portable, and the code has to be cleaned up. Use at your own risk and headaches!

![Ceenth Logo](https://i.imgur.com/6CL8BDM.png)




A basic synthesizer made using C# & WPF for a final school project for the Programming II class.

The focus of this project is to apply basic knowledge of the MVVM (Model-View-Viewmodel) architecture, also applying basic CRUD (Create, Remove, Update, Delete) operations.

The polyphonic audio engine was made using the Naudio library, making it possible to do basic effects, filters, and things of the sort.

## Demo

![Demo](https://i.imgur.com/kbO61R7.gif)


## Features

- Polyphonic engine, which supports chording
- Basic low-pass filter (Filter Cutoff + Q)
- Attack and Release sliders
- Basic effects for sound manipulation (Tremolo & Vibrato)
- Preset support (comes with four factory presets)
- Waveform preview via a basic Oscilloscope

### **To implement:**

- MIDI support
- More effects (like Reverb, completing the ADSR suite, etc...)
- Design to be changed
- Database engine to be changed



## About the warning...

**Sigh..** the database was made using SQL Server,  and the ADO.NET Entity Framework was used to pass the data onto the client... I know... It's ancient technology and it is VERY deprecated. The project itself, was even made using **.NET Framework**, instead of .NET Core...

These are, unfortunately, requirements for this final project. Since we used these technologies in our classes, it has to use this tech stack. For now.

> [!IMPORTANT]  
> The project will continue, and I will remake the entire database part to make it just SQLite based on local databases. I will add more features to this, but, for now, I'm not accepting any contributions. Sorry!
