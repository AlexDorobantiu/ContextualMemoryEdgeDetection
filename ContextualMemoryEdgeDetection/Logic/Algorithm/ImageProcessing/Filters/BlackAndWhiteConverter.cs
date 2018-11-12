using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters
{
    [Serializable]
    class BlackAndWhiteConverter : ImageFilter
    {
        int threshold;

        public BlackAndWhiteConverter(int threshold)
        {
            this.threshold = threshold;
        }

        public virtual ImageDescription filter(ImageDescription inputImage)
        {
            inputImage.computeGrayscale();

            ImageDescription outputImage = new ImageDescription();
            outputImage.sizeX = inputImage.sizeX;
            outputImage.sizeY = inputImage.sizeY;
            outputImage.grayscale = true;
            byte[,] inputGray = inputImage.gray;
            byte[,] outputGray = new byte[inputImage.sizeY, inputImage.sizeX];
            outputImage.gray = outputGray;

            for (int i = 0; i < inputImage.sizeY; i++)
            {
                for (int j = 0; j < inputImage.sizeX; j++)
                {
                    if (inputGray[i, j] >= threshold)
                    {
                        outputGray[i, j] = 255;
                    }
                    else
                    {
                        outputGray[i, j] = 0;
                    }
                }
            }
            return outputImage;
        }
    }
}
