﻿using PhotoEditor.HistoryProvider.Commands;
using PhotoEditor.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using PhotoEditor.ImageOperations;
using System.Threading;

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
            viewLogic.MainImage.Image = tempImage;
        }

        private void ButtonOkClicked(object sender, RoutedEventArgs e)
        {
            HistoryProvider.Commands.ICommand gammaComand = new GammaFilterCommand(originalImage, slider.Value);
            viewLogic.AddCommand(gammaComand);

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
            GammaFilter.GammaFilterUnsafeWithCopy(originalImage, tempImage, slider.Value);
        }
        private void ButtonPlusClicked(object sender, RoutedEventArgs e)
        {
            slider.Value += 0.05;
            GammaFilter.GammaFilterUnsafeWithCopy(originalImage, tempImage, slider.Value);
        }
        private void ButtonMinusClicked(object sender, RoutedEventArgs e)
        {
            slider.Value -= 0.05;
            GammaFilter.GammaFilterUnsafeWithCopy(originalImage, tempImage, slider.Value);
        }

        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
