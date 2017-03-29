using PhotoEditor.Controls;
using PhotoEditor.DataModel;
using PhotoEditor.HistoryProvider;
using PhotoEditor.ImageOperations;
using PhotoEditor.Utility;
using PhotoEditor.ViewModel;
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
        private ViewLogic viewLogic;
        private HistoryHelper histortHelper;
        private LoadAndSaveHelper loadAndSaveHelper;
        
        private Stopwatch sw = new Stopwatch();
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
                if(!chanellViewIsOpen)
                    viewLogic = new ViewWithoutChanell(image, histortHelper,loadAndSaveHelper.LastUsedFileName);
                else
                    viewLogic = new ViewWithChanell(image, histortHelper, loadAndSaveHelper.LastUsedFileName);
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
                ChangeView();
                Storyboard sb = App.Current.Resources["sbChanellViewShow"] as Storyboard;
                sb.Begin(chanellView);     
            }
            else
            {
                Storyboard sb = App.Current.Resources["sbChanellViewHide"] as Storyboard;
                //Event when storyBoard finish animation      
                sb.Completed += (o,s)=> ChangeView();
                sb.Begin(chanellView);
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
        private void ChangeView()
        {
            if (chanellViewIsOpen)
            {
                viewLogic = new ViewWithoutChanell(viewLogic.MainImage.Image, histortHelper, loadAndSaveHelper.LastUsedFileName);
                DataContext = viewLogic;
                chanellViewIsOpen = false;
            }
            else
            {
                viewLogic = new ViewWithChanell(viewLogic.MainImage.Image, histortHelper, loadAndSaveHelper.LastUsedFileName);
                DataContext = viewLogic;
                chanellViewIsOpen = true;
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
                gammaControl.Height = 140;
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
    }
}
