using PhotoEditor.ViewModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PhotoEditor.ImageOperations;
using System.Windows;
using PhotoEditor.DataModel;

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
            int n = (19 - (int)slider.Value);
            SharpenFilter.SharpenFilterUnsafeWithCopy(originalImage, viewLogic.MainImage.Image, n);
        }

    }
}
