using PhotoEditor.Utility;
using PhotoEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace PhotoEditor.Controls
{
    /// <summary>
    /// Interaction logic for SaveControl.xaml
    /// </summary>
    public partial class SaveControl : UserControl
    {
        private ViewLogic viewLogic;
        private LoadAndSaveHelper loadAndSaveHelper;
        
        public SaveControl(ViewLogic viewLogic, LoadAndSaveHelper loadAndSaveHelper)
        {
            InitializeComponent();
            this.viewLogic = viewLogic;
            this.loadAndSaveHelper = loadAndSaveHelper;
        }

        private void ButtonOkClicked(object sender, RoutedEventArgs e)
        {
            DownsampledImage downsapledImage;
            if (radioButtonRed.IsChecked.Equals(true))
                downsapledImage = new DownsampledImage(viewLogic.MainImage.Image, DownsampledImage.SpareChannel.Red);
            else if (radioButtonGreen.IsChecked.Equals(true))
                downsapledImage = new DownsampledImage(viewLogic.MainImage.Image, DownsampledImage.SpareChannel.Green);
            else
                downsapledImage = new DownsampledImage(viewLogic.MainImage.Image, DownsampledImage.SpareChannel.Blue);



            //Progress update.
            //Progress<string> progress = new Progress<string>((value) => { labelProgress.Content = value; });

            ShannonFano shannonFano = new ShannonFano();

            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte[] compressImage = shannonFano.Compress(downsapledImage.ImageData);
            sw.Stop();
            labelProgress.Content = "Compress finish in: " + sw.ElapsedMilliseconds +"ms.";

            byte[] data = shannonFano.Decompress(compressImage);
            downsapledImage = new DownsampledImage(shannonFano.Decompress(compressImage));
            viewLogic.MainImage.Image = downsapledImage.Image;

            //loadAndSaveHelper.SaveCustomImage(compressImage);
            //loadAndSaveHelper.SaveCustomImage(downsapledImage.ImageData);
            //Window.GetWindow(this).Close();
        }

        private void ButtonCancelClicked(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
        private void WindowMouseLeftButtonDownClicked(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).DragMove();
        }
    }
}


