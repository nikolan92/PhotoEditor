using PhotoEditor.Controls;
using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using PhotoEditor.ImageOperations;
using PhotoEditor.Utility;
using PhotoEditor.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace PhotoEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool chanellViewIsOpen = false;
        private bool histogramViewIsOpen = false;
        private ViewLogic viewLogic;
        private HistoryHelper histortHelper;
        private LoadAndSaveHelper loadAndSaveHelper;
        private enum ViewModel
        {
            ViewWithChanell,ViewWithoutChanell,ViewWithHistogram
        }
        public MainWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            loadAndSaveHelper = new LoadAndSaveHelper();
        }

        private async void ButtonOpenClicked(object sender, RoutedEventArgs e)
        {
            WriteableBitmap image = await loadAndSaveHelper.LoadImageAsync();
            if (image != null)
            {
                histortHelper = new HistoryHelper(10);
                if (!chanellViewIsOpen)
                {
                    viewLogic = new ViewWithoutChanell(image, histortHelper, loadAndSaveHelper.LastUsedFileName);
                }
                else if (histogramViewIsOpen)
                {
                    viewLogic = new ViewWithHistogram(image, histortHelper, loadAndSaveHelper.LastUsedFileName);
                }
                else
                {
                    viewLogic = new ViewWithChanell(image, histortHelper, loadAndSaveHelper.LastUsedFileName);
                }
                //GC.Collect(); // just for test
                DataContext = viewLogic;
                UndoRedoButtonStatusChanged();
            }
        }
        private void ButtonInvertColorsClicked(object sender, RoutedEventArgs e)
        {
            //If image is not loaded return TODO:Disable all buttons and enable them when tha acction is posible 
            if (viewLogic == null)
                return;

            viewLogic.AddImageReference(new ImageModel(viewLogic.MainImage.Image.Clone()));
            InvertFilter.InvertFilterUnsafe(viewLogic.MainImage.Image);
            viewLogic.RefreshView();
            UndoRedoButtonStatusChanged();
        }
        private void ButtonSaveClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic == null)
                return;
            loadAndSaveHelper.SaveImage(viewLogic.MainImage.Image);
        }
        private void ButtonShowHideChanellViewClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic == null)
                return;

            if (!chanellViewIsOpen)
            {
                ChangeView(ViewModel.ViewWithChanell);
                Storyboard sb = App.Current.Resources["sbChanellViewShow"] as Storyboard;
                sb.Begin(chanellView);
                chanellViewIsOpen = true;
            }
            else
            {
                Storyboard sb = App.Current.Resources["sbChanellViewHide"] as Storyboard;
                //Event when storyBoard finish animation      
                sb.Completed += (o, s) => {
                    ChangeView(ViewModel.ViewWithoutChanell);

                    redChanellImageView.Visibility = Visibility.Visible;
                    greenChanellImageView.Visibility = Visibility.Visible;
                    blueChanellImageView.Visibility = Visibility.Visible;
                    redHistogramView.Visibility = Visibility.Collapsed;
                    greenHistogramView.Visibility = Visibility.Collapsed;
                    blueHistogramView.Visibility = Visibility.Collapsed;
                };
                sb.Begin(chanellView);
                chanellViewIsOpen = false;
            }
        }
        private void ButtonUndoClicked(object sender, RoutedEventArgs e)
        {
            viewLogic.Undo();
            UndoRedoButtonStatusChanged();
        }
        private void ButtonRedoClicked(object sender, RoutedEventArgs e)
        {
            viewLogic.Redo();
            UndoRedoButtonStatusChanged();
        }
        private void ButtonSharpnessClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic != null)
            {
                Window gammaControl = new Window
                {
                    Content = new SharpenControl(viewLogic)
                };
                gammaControl.WindowStyle = WindowStyle.None;
                gammaControl.AllowsTransparency = true;
                gammaControl.Width = 300;
                gammaControl.Height = 155;
                gammaControl.Left = Left + 100;
                gammaControl.Top = Top + 100;
                gammaControl.ShowDialog();

                UndoRedoButtonStatusChanged();
            }
        }
        private void ButtonGammaClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic != null )
            {
                Window gammaControl = new Window
                {
                    Content = new GammaControl(viewLogic)
                };
                gammaControl.WindowStyle = WindowStyle.None;
                gammaControl.AllowsTransparency = true;
                gammaControl.Width = 300;
                gammaControl.Height = 135;
                gammaControl.Left = Left + 100;
                gammaControl.Top = Top + 100;
                gammaControl.ShowDialog();

                UndoRedoButtonStatusChanged();
            }
        }
        private void ButtonImageQuantizationClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic != null)
            {
                Window imageQuantization = new Window
                {
                    Content = new ImageQuantizationControl(viewLogic)
                };
                imageQuantization.WindowStyle = WindowStyle.None;
                imageQuantization.AllowsTransparency = true;
                imageQuantization.Width = 300;
                imageQuantization.Height = 140;
                imageQuantization.Left = Left + 100;
                imageQuantization.Top = Top + 100;
                imageQuantization.ShowDialog();

                UndoRedoButtonStatusChanged();
            }
        }
        private void ButtonShowHideHistogramViewClicked(object sender, RoutedEventArgs e)
        {
            if (!histogramViewIsOpen)
            {
                redChanellImageView.Visibility = Visibility.Collapsed;
                greenChanellImageView.Visibility = Visibility.Collapsed;
                blueChanellImageView.Visibility = Visibility.Collapsed;

                redHistogramView.Visibility = Visibility.Visible;
                greenHistogramView.Visibility = Visibility.Visible;
                blueHistogramView.Visibility = Visibility.Visible;

                ChangeView(ViewModel.ViewWithHistogram);
                histogramViewIsOpen = true;
            }
            else
            {
                redHistogramView.Visibility = Visibility.Collapsed;
                greenHistogramView.Visibility = Visibility.Collapsed;
                blueHistogramView.Visibility = Visibility.Collapsed;

                redChanellImageView.Visibility = Visibility.Visible;
                greenChanellImageView.Visibility = Visibility.Visible;
                blueChanellImageView.Visibility = Visibility.Visible;

                ChangeView(ViewModel.ViewWithChanell);
                histogramViewIsOpen = false;
            }
        }
        private void ButtonPixelateImageClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic != null)
            {
                Window pixelateImageControl = new Window
                {
                    Content = new PixelateImageControl(viewLogic)
                };
                pixelateImageControl.WindowStyle = WindowStyle.None;
                pixelateImageControl.AllowsTransparency = true;
                pixelateImageControl.Width = 300;
                pixelateImageControl.Height = 150;
                pixelateImageControl.Left = Left + 100;
                pixelateImageControl.Top = Top + 100;
                pixelateImageControl.ShowDialog();

                UndoRedoButtonStatusChanged();
            }
        }
        private void ButtonSettingsClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic != null)
            {
                Window settingsControl = new Window
                {
                    Content = new SettingsControl(histortHelper)
                };
                settingsControl.WindowStyle = WindowStyle.None;
                settingsControl.AllowsTransparency = true;
                settingsControl.Width = 300;
                settingsControl.Height = 90;
                settingsControl.Left = Left + 100;
                settingsControl.Top = Top + 100;
                settingsControl.ShowDialog();

                UndoRedoButtonStatusChanged();
            }
        }
        private void ButtonEdgeEnhanceClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic == null)
                return;
            WriteableBitmap tmp = viewLogic.MainImage.Image.Clone();
            viewLogic.AddImageReference(new ImageModel(tmp));
            EdgeEnhenceFilter.EdgeEnhenceUnsafeWithCopy(tmp, viewLogic.MainImage.Image, 100);
            viewLogic.RefreshView();
            UndoRedoButtonStatusChanged();
        }
        private void ButtonChannelHistogramClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic != null)
            {
                Window channelHistogramControl = new Window
                {
                    Content = new ChannelHistogramControl(viewLogic)
                };
                channelHistogramControl.WindowStyle = WindowStyle.None;
                channelHistogramControl.AllowsTransparency = true;
                channelHistogramControl.Width = 300;
                channelHistogramControl.Height = 135;
                channelHistogramControl.Left = Left + 100;
                channelHistogramControl.Top = Top + 100;
                channelHistogramControl.ShowDialog();

                UndoRedoButtonStatusChanged();
            }
        }
        private void ButtonAdvancedSaveClicked(object sender, RoutedEventArgs e)
        {
            if (viewLogic != null)
            {
                Window saveControl = new Window
                {
                    Content = new SaveControl(viewLogic, loadAndSaveHelper)
                };
                saveControl.WindowStyle = WindowStyle.None;
                saveControl.AllowsTransparency = true;
                saveControl.Width = 300;
                saveControl.Height = 192;
                saveControl.Left = Left + 100;
                saveControl.Top = Top + 100;
                saveControl.ShowDialog();
            }
        }

        private void UndoRedoButtonStatusChanged()
        {
            if (histortHelper.CanUndo())
                btnUndo.IsEnabled = true;
            else
                btnUndo.IsEnabled = false;

            if (histortHelper.CanRedo())
                btnRedo.IsEnabled = true;
            else
                btnRedo.IsEnabled = false;
        }
        private void ChangeView(ViewModel view)
        {
            switch (view)
            {
                case ViewModel.ViewWithChanell:
                    viewLogic = new ViewWithChanell(viewLogic.MainImage.Image, histortHelper, loadAndSaveHelper.LastUsedFileName);
                    DataContext = viewLogic;
                    break;
                case ViewModel.ViewWithoutChanell:
                    viewLogic = new ViewWithoutChanell(viewLogic.MainImage.Image, histortHelper, loadAndSaveHelper.LastUsedFileName);
                    DataContext = viewLogic;
                    break;
                case ViewModel.ViewWithHistogram:
                    viewLogic = new ViewWithHistogram(viewLogic.MainImage.Image, histortHelper, loadAndSaveHelper.LastUsedFileName);
                    DataContext = viewLogic;
                    break;
            }
        }
    }
}
