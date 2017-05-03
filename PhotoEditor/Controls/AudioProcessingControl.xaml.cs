using Microsoft.Win32;
using PhotoEditor.DataModel;
using PhotoEditor.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
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

namespace PhotoEditor.Controls
{
    /// <summary>
    /// Interaction logic for AudioProcessingControl.xaml
    /// </summary>
    public partial class AudioProcessingControl : UserControl
    {
        SoundPlayer soundPlayer;
        MemoryStream soundStream;
        public ObservableCollection<string> WavItems { get; set; }

        public AudioModel AudioFile { get; set; }
        public AudioProcessingControl()
        {
            InitializeComponent();
            DataContext = this;

            AudioFile = new AudioModel() { LastLoadedFile = "N/A" };
            WavItems = new ObservableCollection<string>();     
        }

        private void WindowMouseLeftButtonDownClicked(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).DragMove();
        }

        private void ButtonCloseClicked(object sender, RoutedEventArgs e)
        {
            if (soundPlayer != null)
            {
                soundPlayer.Dispose();
            }
            Window.GetWindow(this).Close();
        }
        private void ButtonPlayClicked(object sender, RoutedEventArgs e)
        {
            if (soundPlayer != null)
            {
                try
                {
                    soundPlayer.Play();
                }
                catch (InvalidOperationException exception) { MessageBox.Show(exception.ToString()); }
            }
        }
        private void ButtonStopClicked(object sender, RoutedEventArgs e)
        {
            if(soundPlayer != null)
                soundPlayer.Stop();
        }
        private void ButtonOpenClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio files(*.wav)|*.wav";
            if (openFileDialog.ShowDialog().Equals(true))
            {
                //Release old data.
                if (soundPlayer != null)
                    soundPlayer.Dispose();

                soundStream = new MemoryStream(System.IO.File.ReadAllBytes(openFileDialog.FileName));
                soundPlayer = new SoundPlayer(soundStream);

                AudioFile.LastLoadedFile = openFileDialog.FileName;
            }
        }
        private void ButtonSaveClicked(object sender, RoutedEventArgs e)
        {
            
        }
        private void ButtonAddClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio files(*.wav)|*.wav";
            if (openFileDialog.ShowDialog().Equals(true))
            {
                WavItems.Add(openFileDialog.FileName);
            }
        }

        private void ButtonRemoveClicked(object sender, RoutedEventArgs e)
        {
            if (listViewItems.SelectedIndex != -1)
                WavItems.RemoveAt(listViewItems.SelectedIndex);
        }

        private void ButtonMergeClicked(object sender, RoutedEventArgs e)
        {
            if (WavItems.Count == 0)
                return;
            //Merge all files.
            WavFile[] wavFiles = new WavFile[WavItems.Count];

            //Load all files.
            for (int i = 0; i < WavItems.Count; i++)
            {
                wavFiles[i] = new WavFile(System.IO.File.ReadAllBytes(WavItems[i]));
            }

            byte[] newWavFile;

            try
            {
                newWavFile = WavFile.Concat(wavFiles);
            }
            catch (InvalidOperationException exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Audio files(*.wav)|*.wav";
            if (saveFileDialog.ShowDialog().Equals(true))
            {
                System.IO.File.WriteAllBytes(saveFileDialog.FileName, newWavFile);
            }
        }
        private void ButtonSmoothClicked(object sender, RoutedEventArgs e)
        {
            if (soundStream != null)
            {
                WavFile smoothWav = new WavFile(soundStream.ToArray());
                //Smooth evry sample.
                smoothWav.SmoothWav(50);

                soundStream.Dispose();
                soundPlayer.Dispose();

                soundStream = new MemoryStream(smoothWav.WavData);

                soundPlayer.Stream = soundStream;
            }
        }
        public class AudioModel : ObservableObject
        {
            private string lastLoadedFile;

            public string LastLoadedFile
            {
                get { return lastLoadedFile; }
                set { lastLoadedFile = value; OnPropertyChanged("LastLoadedFile"); }
            }

        }


    }
}
