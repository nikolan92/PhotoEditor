using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PhotoEditor.ImageOperations;

namespace PhotoEditor.HistoryProvider.Commands
{
    public class GammaFilterCommand : ICommand
    {
        private WriteableBitmap backUpImage;
        private double gammaValue;
        /// <summary>
        /// Gamma Filter Comand constructor.
        /// </summary>
        /// <param name="imageSource">Image for processing.</param>
        /// <param name="gammaValue">Gamma value between 0.05-7.0</param>
        public GammaFilterCommand(WriteableBitmap imageSource ,double gammaValue)
        {
            backUpImage = imageSource;
            this.gammaValue = gammaValue;
        }
        /// <summary>
        /// Execute will clone imageSource and save inside backUpImage for future use, perform gamma filter on imageSourceAndDestination
        /// </summary>
        /// <param name="imageSourceAndDestination">Image for processing.</param>
        public void Execute(WriteableBitmap imageSourceAndDestination)
        {
            backUpImage = imageSourceAndDestination.Clone();
            GammaFilter.GammaFilterUnsafe(imageSourceAndDestination, gammaValue);
        }
        /// <summary>
        /// UnExecute will return backUpImage reference.
        /// </summary>
        /// <param name="imageData">Not used inside this function.</param>
        /// <returns></returns>
        public WriteableBitmap UnExecute(WriteableBitmap imageData)
        {
            return backUpImage;
        }
    }
}
