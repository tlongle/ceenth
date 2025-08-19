using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using ceenth.Viewmodel;

namespace ceenth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewmodel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as MainViewmodel;
            var dbPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ceenth.db");
            System.Windows.MessageBox.Show($"Database at: {dbPath}");
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is System.IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // We prevent the key from being processed again if it's already down
            if (!e.IsRepeat)
            {
                _viewModel?.HandleKeyDown(e.Key);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            _viewModel?.HandleKeyUp(e.Key);
        }

        // --- EVENT HANDLERS FOR MOUSE CLICKS ---

        private void Key_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get the Rectangle that was clicked and its ViewModel
            if (sender is Rectangle key && key.DataContext is PianoKeyViewmodel keyViewModel)
            {
                // If the key is not already pressed, execute the NoteOn command
                if (!keyViewModel.IsPressed)
                {
                    keyViewModel.IsPressed = true; // Manually set IsPressed for visual feedback
                    keyViewModel.NoteOnCommand.Execute(null);
                }
            }
        }

        private void Key_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            // Get the Rectangle that was released and its ViewModel
            if (sender is Rectangle key && key.DataContext is PianoKeyViewmodel keyViewModel)
            {
                // If the key is pressed, execute the NoteOff command
                if (keyViewModel.IsPressed)
                {
                    keyViewModel.IsPressed = false;
                    keyViewModel.NoteOffCommand.Execute(null);
                }
            }
        }

        private void Key_MouseLeave(object sender, MouseEventArgs e)
        {
            // This handles the case where the user clicks down and drags the mouse off the key
            if (sender is Rectangle key && key.DataContext is PianoKeyViewmodel keyViewModel)
            {
                // If the key is pressed, execute the NoteOff command
                if (keyViewModel.IsPressed)
                {
                    keyViewModel.IsPressed = false;
                    keyViewModel.NoteOffCommand.Execute(null);
                }
            }
        }


    }
}
