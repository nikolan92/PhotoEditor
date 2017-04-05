using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoEditor.DataModel;
using System.Windows.Media.Imaging;
using PhotoEditor.HistoryProvider;
using System.Windows.Media;
using System.Windows;
using PhotoEditor.ImageOperations;

namespace PhotoEditor.ViewModel
{
    class ViewWithHistogram : ViewLogic
    {
        public ImageHistogramModel ImageHistogram { get; private set; }
        public ViewWithHistogram(WriteableBitmap image, HistoryHelper histortHelper, string fileName)
        {
            HistortHelper = histortHelper;
            MainImage = new ImageModel(image);
            ImageInfo = new ImageInfoModel(fileName, image.PixelWidth.ToString(), image.PixelHeight.ToString(), image.Format.BitsPerPixel.ToString());

            ImageHistogram = new ImageHistogramModel();

            DoImageHistogram(MainImage.Image);
        }

        public override void Undo()
        {
            HistortHelper.Undo(MainImage);
            RefreshView();
        }

        public override void Redo()
        {
            HistortHelper.Redo(MainImage);
            RefreshView();
        }

        public override void RefreshView()
        {
            DoImageHistogram(MainImage.Image);
        }
        private void DoImageHistogram(WriteableBitmap image)
        {
            ImageHistogram ih = new ImageHistogram(image);

            ImageHistogram.RedHistogram = ConvertToPointCollection(ih.RedHistogram, ih.RedMax);
            ImageHistogram.GreenHistogram = ConvertToPointCollection(ih.GreenHistogram, ih.GreenMax);
            ImageHistogram.BlueHistogram = ConvertToPointCollection(ih.BlueHistogram, ih.BlueMax);
        }
        private PointCollection ConvertToPointCollection(int[] values,int maxValue)
        {
            //values = SmoothHistogram(values);

            PointCollection points = new PointCollection();
            // first point (lower-left corner)
            points.Add(new Point(0, maxValue));
            // middle points
            for (int i = 0; i < values.Length; i++)
            {
                points.Add(new Point(i, maxValue - values[i]));
            }
            // last point (lower-right corner)
            points.Add(new Point(values.Length - 1, maxValue));

            return points;
        }
    }
}
