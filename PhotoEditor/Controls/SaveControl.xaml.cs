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


            Stopwatch sw = new Stopwatch();
            sw.Start();
            byte[] compressImage = ShannonFano.Compress(downsapledImage.ImageData);
            sw.Stop();

            Console.WriteLine("Sannon Fano:"+sw.ElapsedMilliseconds + "ms");

            //loadAndSaveHelper.SaveCustomImage(compressImage);
            //loadAndSaveHelper.SaveCustomImage(downsapledImage.ImageData);
            //viewLogic.MainImage.Image = downsapledImage.Image;
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


