using PhotoEditor.ViewModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PhotoEditor.ImageOperations;
using System.Windows;
using PhotoEditor.DataModel;
using System.Diagnostics;
using System;

namespace PhotoEditor.Controls
{
    /// <summary>
    /// Interaction logic for SharpnessControl.xaml
    /// </summary>
    public partial class SharpenControl : UserControl
    {
        private ViewLogic viewLogic;
        private WriteableBitmap originalImage;

        public SharpenControl(ViewLogic viewLogic)
        {
            InitializeComponent();
            this.viewLogic = viewLogic;
            //Clone MainImage image 
            originalImage = viewLogic.MainImage.Image.Clone();
        }
        private void ButtonOkClicked(object sender, RoutedEventArgs e)
        {
            viewLogic.AddImageReference(new ImageModel(originalImage));
            viewLogic.RefreshView();
            Window.GetWindow(this).Close();
        }

        private void ButtonCancelClicked(object sender, RoutedEventArgs e)
        {
            //Restore reference
            viewLogic.MainImage.Image = originalImage;
            Window.GetWindow(this).Close();
        }
        private void WindowMouseLeftButtonDownClicked(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).DragMove();
        }
        private void SliderNCompleted(object sender, DragCompletedEventArgs e)
        {


            Stopwatch sw = new Stopwatch();

            sw.Start();
            if (radioBtn3x3.IsChecked.Equals(true))
            {
                int n = (19 - (int)slider.Value);
                SharpenFilter.SharpenFilter3x3UnsafeWithCopy(originalImage, viewLogic.MainImage.Image, n);
            }
            else if (radioBtn5x5.IsChecked.Equals(true))
            {
                int n = 35 - (int)(2*slider.Value);
                SharpenFilter.SharpenFilter5x5UnsafeWithCopy(originalImage, viewLogic.MainImage.Image, n);
            }
            sw.Stop();
            Console.WriteLine("Sharpens filter:" + sw.ElapsedMilliseconds + "ms");
        }
    }
}
