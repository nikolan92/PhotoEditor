using PhotoEditor.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PhotoEditor.ImageOperations;
using PhotoEditor.DataModel;

namespace PhotoEditor.Controls
{
    /// <summary>
    /// Interaction logic for GammaControl.xaml
    /// </summary>
    public partial class GammaControl : UserControl
    {
        private ViewLogic viewLogic;
        private WriteableBitmap originalImage;

        public GammaControl(ViewLogic viewLogic)
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
        private void SliderCompleted(object sender, DragCompletedEventArgs e)
        {
            GammaFilter.GammaFilterUnsafeWithCopy(originalImage, viewLogic.MainImage.Image, slider.Value);
        }
        private void ButtonPlusClicked(object sender, RoutedEventArgs e)
        {
            slider.Value += 0.05;
            GammaFilter.GammaFilterUnsafeWithCopy(originalImage, viewLogic.MainImage.Image, slider.Value);
        }
        private void ButtonMinusClicked(object sender, RoutedEventArgs e)
        {
            slider.Value -= 0.05;
            GammaFilter.GammaFilterUnsafeWithCopy(originalImage, viewLogic.MainImage.Image, slider.Value);
        }
    }
}
