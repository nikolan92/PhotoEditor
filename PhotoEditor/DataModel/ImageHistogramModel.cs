using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PhotoEditor.DataModel
{
    public class ImageHistogramModel : ObservableObject
    {

        private PointCollection redHistogram;

        public PointCollection RedHistogram
        {
            get { return redHistogram; }
            set
            {
                redHistogram = value;
                OnPropertyChanged("RedHistogram");
            }
        }
        private PointCollection greenHistogram;

        public PointCollection GreenHistogram
        {
            get { return greenHistogram; }
            set
            {
                greenHistogram = value;
                OnPropertyChanged("GreenHistogram");
            }
        }
        private PointCollection blueHistogram;

        public PointCollection BlueHistogram
        {
            get { return blueHistogram; }
            set
            {
                blueHistogram = value;
                OnPropertyChanged("BlueHistogram");
            }
        }

    }
}
