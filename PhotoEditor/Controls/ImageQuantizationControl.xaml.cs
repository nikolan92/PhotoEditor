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
    /// Interaction logic for ImageQuantizationControl.xaml
    /// </summary>
    public partial class ImageQuantizationControl : UserControl
    {
        private ViewLogic viewLogic;
        public ImageQuantizationControl(ViewLogic viewLogic)
        {
            InitializeComponent();
            this.viewLogic = viewLogic;
        }

        private async void ButtonOkClicked(object sender, RoutedEventArgs e)
        {
            viewLogic.AddImageReference(viewLogic.MainImage);

            ImageQuantization iq = new ImageQuantization(viewLogic.MainImage.Image);

            buttonOk.Visibility = Visibility.Collapsed;
            buttonCancel.Visibility = Visibility.Collapsed;
            labelWorking.Visibility = Visibility.Visible;

            if(radioButtonMyAlgorithm.IsChecked.Equals(true))
                viewLogic.MainImage.Image = await iq.MyAlgorithmAsync(int.Parse(textBoxMaxColors.Text));
            else
                viewLogic.MainImage.Image = await iq.MedianCutAsync(int.Parse(textBoxMaxColors.Text));

            viewLogic.RefreshView();

            Window.GetWindow(this).Close();
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
