using System.ComponentModel;

namespace PhotoEditor.DataModel
{
    public class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

       
        public void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
