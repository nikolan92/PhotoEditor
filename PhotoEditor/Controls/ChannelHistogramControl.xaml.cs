using PhotoEditor.DataModel;
using PhotoEditor.ImageOperations;
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

namespace PhotoEditor.Controls
{
    /// <summary>
    /// Interaction logic for ChannelHistogramControl.xaml
    /// </summary>
    public partial class ChannelHistogramControl : UserControl
    {
        private ViewLogic viewLogic;
        private WriteableBitmap originalImage;

        public ChannelHistogramControl(ViewLogic viewLogic)
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
            ChannelHistogramFilter.ChannelHistogramUnsafeWithCopy(originalImage, viewLogic.MainImage.Image, (int)slider.LowerValue, (int)slider.UpperValue);
            viewLogic.RefreshView();
        }
    }
}
