using PhotoEditor.HistoryProvider.Commands;
using PhotoEditor.ViewModel;
using System;
using System.Collections.Generic;
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
using PhotoEditor.HistoryProvider;
using PhotoEditor.ImageOperations;

namespace PhotoEditor.Controls
{
    /// <summary>
    /// Interaction logic for GammaControl.xaml
    /// </summary>
    public partial class GammaControl : UserControl
    {
        private ViewLogic viewLogic;
        private WriteableBitmap originalImage;
        private WriteableBitmap tempImage;
        public GammaControl(ViewLogic viewLogic)
        {
            InitializeComponent();
            this.viewLogic = viewLogic;
            //Back Up reference
            originalImage = viewLogic.MainImage.Image;
            //Clone original image 
            tempImage = originalImage.Clone();
        }

        private void ButtonGammaOkClicked(object sender, RoutedEventArgs e)
        {
            HistoryProvider.Commands.ICommand gammaComand = new GammaFilterComand(originalImage, sliderGamma.Value);
            viewLogic.AddCommand(gammaComand);

            Window.GetWindow(this).Close();
        }

        private void ButtonGammaCancelClicked(object sender, RoutedEventArgs e)
        {
            //Restore reference
            viewLogic.MainImage.Image = originalImage;

            Window.GetWindow(this).Close();
        }

        private void WindowMouseLeftButtonDownClicked(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).DragMove();
        }
        private void SliderGammaCompleted(object sender, DragCompletedEventArgs e)
        {
            GammaFilter.GammaFilterUnsafeWithCopy(originalImage, tempImage, sliderGamma.Value);
            viewLogic.MainImage.Image = tempImage;
        }

        private void ButtonPlusClicked(object sender, RoutedEventArgs e)
        {
            sliderGamma.Value += 0.05;
            GammaFilter.GammaFilterUnsafeWithCopy(originalImage, tempImage, sliderGamma.Value);
            viewLogic.MainImage.Image = tempImage;
        }
        private void ButtonMinusClicked(object sender, RoutedEventArgs e)
        {
            sliderGamma.Value -= 0.05;
            GammaFilter.GammaFilterUnsafeWithCopy(originalImage, tempImage, sliderGamma.Value);
            viewLogic.MainImage.Image = tempImage;
        }
    }
}
