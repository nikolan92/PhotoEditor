using Microsoft.Win32;
using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using PhotoEditor.HistoryProvider.Commands;
using PhotoEditor.ImageOperations;
using PhotoEditor.Utility;
using PhotoEditor.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;
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
        private IUserControl viewModel;
        private HistoryHelper histortHelper;
        private LoadAndSaveHelper loadAndSaveHelper;

        private Stopwatch sw = new Stopwatch();
        public MainWindow()
        {
            InitializeComponent();
            loadAndSaveHelper = new LoadAndSaveHelper(this);
        }

        private void ButtonOpenClicked(object sender, RoutedEventArgs e)
        {
            WriteableBitmap image = loadAndSaveHelper.LoadImage();
            if(image!= null)
            {
                histortHelper = new HistoryHelper(10);
                viewModel = new ViewWithoutChanellView(image, histortHelper);
                DataContext = viewModel;
            }
        }
        private void ButtonInvertColorsClicked(object sender, RoutedEventArgs e)
        {
            //If image is not loaded return TODO:Disable all buttons and enable them when tha acction is posible 
            if (viewModel == null)
                return;

            //stopWatch
            sw.Start();
            viewModel.InvertFilter();
            sw.Stop();
            
            Console.WriteLine("InvertFilter---------------:"+sw.ElapsedMilliseconds + "ms");
            //stopWatch
            sw.Reset();
            UndoRedoButtonStatusChanged();
            
        }
        private void ButtonSaveClicked(object sender, RoutedEventArgs e)
        {
            
        }
        private void ButtonShowHideChanellViewClicked(object sender, RoutedEventArgs e)
        {
            if (viewModel == null)
                return;

            if (!chanellViewIsOpen)
            {
                ChangeView();
                Storyboard sb = App.Current.Resources["sbChanellViewShow"] as Storyboard;
                sb.Begin(chanellView);
                 
            }
            else
            {
                Storyboard sb = App.Current.Resources["sbChanellViewHide"] as Storyboard;
                sb.Begin(chanellView);
                ChangeView();
            }
        }
        private void ButtonUndoClicked(object sender, RoutedEventArgs e)
        {
            viewModel.Undo();
            UndoRedoButtonStatusChanged();
        }
        private void ButtonRedoClicked(object sender, RoutedEventArgs e)
        {
            viewModel.Redo();
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
        private void ChangeView()
        {
            if (chanellViewIsOpen)
            {
                viewModel = new ViewWithoutChanellView(viewModel.GetMainImage(),histortHelper);
                DataContext = viewModel;
                chanellViewIsOpen = false;
            }
            else
            {
                viewModel = new ViewWithChanellView(viewModel.GetMainImage(), histortHelper);
                DataContext = viewModel;
                chanellViewIsOpen = true;
            }
        }
    }
}
