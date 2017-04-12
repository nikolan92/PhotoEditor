using PhotoEditor.HistoryProvider;
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
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        HistoryHelper historyHelper;
        int historyBufferSize = -1;
        public SettingsControl(HistoryHelper historyHelper)
        {
            InitializeComponent();
            this.historyHelper = historyHelper;
            textBoxMaxHistoryBuffer.Text = historyHelper.StackSize().ToString();
        }
        private void WindowMouseLeftButtonDownClicked(object sender, MouseButtonEventArgs e)
        {
            Window.GetWindow(this).DragMove();
        }
        private void ButtonOkClicked(object sender, RoutedEventArgs e)
        {
            if (!historyBufferSize.Equals(-1))
                historyHelper.ReseizeStackSize(historyBufferSize);

            Window.GetWindow(this).Close();
        }
        private void ButtonCancelClicked(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void TextBoxMaxHistoryChanged(object sender, TextChangedEventArgs e)
        {

            if (Int32.TryParse(textBoxMaxHistoryBuffer.Text, out historyBufferSize))
            {
                if (historyBufferSize < 3)
                    OkButtonStatus(false);
                else if (historyBufferSize > 30)
                    OkButtonStatus(false);
                else
                    OkButtonStatus(true);
            }
            else
            {
                OkButtonStatus(false);
            }
        }
        private void OkButtonStatus(bool enable)
        {
            if (buttonOk == null)
                return;

            if (enable)
            {
                buttonOk.Content = "OK";
                buttonOk.Foreground = App.Current.Resources["FontColor"] as SolidColorBrush;
                buttonOk.IsEnabled = true;
            }
            else
            {
                buttonOk.Content = "Stack can be between 3 and 30.";
                buttonOk.IsEnabled = false;
                buttonOk.Foreground = Brushes.Red;
            }
        }
    }
}
