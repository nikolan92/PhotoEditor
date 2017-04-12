using PhotoEditor.DataModel;
using PhotoEditor.ImageOperations;
using PhotoEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    /// Interaction logic for PixelateImageControl.xaml
    /// </summary>
    public partial class PixelateImageControl : UserControl
    {
        private ViewLogic viewLogic;
        private WriteableBitmap originalImage;
        public PixelateImageControl(ViewLogic viewLogic)
        {
            InitializeComponent();
            this.viewLogic = viewLogic;
            //Clone MainImage image.
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
        private void SliderCompleted(object sender, DragCompletedEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            PixelateImage.PixelateImageUnsafeWithCopy(originalImage, viewLogic.MainImage.Image, (int)slider.Value);
            sw.Stop();
            Console.WriteLine("Pixelate image:" + sw.ElapsedMilliseconds + "ms");
        }
        private void WindowMouseLeftButtonDownClicked(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).DragMove();
        }
    }
}
