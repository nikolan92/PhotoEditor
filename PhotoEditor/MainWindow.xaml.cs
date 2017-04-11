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
        //private Stopwatch sw = new Stopwatch();
        public MainWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            loadAndSaveHelper = new LoadAndSaveHelper();
        }

        private void ButtonOpenClicked(object sender, RoutedEventArgs e)
        {
            WriteableBitmap image = loadAndSaveHelper.LoadImage();
            if(image!= null)
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
                Window gammaControl = new Window
                {
                    Content = new ImageQuantizationControl(viewLogic)
                };
                gammaControl.WindowStyle = WindowStyle.None;
                gammaControl.AllowsTransparency = true;
                gammaControl.Width = 300;
                gammaControl.Height = 140;
                gammaControl.Left = Left + 100;
                gammaControl.Top = Top + 100;
                gammaControl.ShowDialog();

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
    }
}
