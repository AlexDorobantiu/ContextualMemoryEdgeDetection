using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters
{
    [Serializable]
    class SobelAppenderFilter : ImageFilter
    {
        private float sigma;

        public SobelAppenderFilter(float sigma = 1.0f)
        {
            this.sigma = sigma;
        }
        public virtual ImageDescription filter(ImageDescription inputImage)
        {
            inputImage.computeGrayscale();

            int imageSizeX = inputImage.sizeX;
            int imageSizeY = inputImage.sizeY;
            byte[,] inputGray = inputImage.gray;
            byte[,] outputGray = new byte[imageSizeY, imageSizeX];

            // 1. Gauss
            float[,] gaussConvolutionMatrix = FilterBankUtil.generateNormalizedGaussConvolutionMatrix(sigma, 5);
            float[,] gaussResult = ImageDescriptionUtil.mirroredMarginConvolution(inputGray, gaussConvolutionMatrix);

            // 2. Gradient
            float[,] dx = ImageDescriptionUtil.mirroredMarginConvolution(gaussResult, FilterBankUtil.normalizedSobelX);
            float[,] dy = ImageDescriptionUtil.mirroredMarginConvolution(gaussResult, FilterBankUtil.normalizedSobelY);

            // 3. Gradient Amplitude
            float[,] amplitudeResult = new float[imageSizeY, imageSizeX];
            for (int i = 0; i < imageSizeY; i++)
            {
                for (int j = 0; j < imageSizeX; j++)
                {
                    amplitudeResult[i, j] = (float)Math.Sqrt(dx[i, j] * dx[i, j] + dy[i, j] * dy[i, j]);
                }
            }

            for (var i = 0; i < imageSizeY; i++)
            {
                for (var j = 0; j < imageSizeX; j++)
                {
                    if (amplitudeResult[i, j] < 255)
                    {
                        outputGray[i, j] = (byte)(amplitudeResult[i, j] + 0.5f);
                    }
                    else
                    {
                        outputGray[i, j] = 255;
                    }
                }
            }

            ImageDescription outputImage = new ImageDescription();
            outputImage.sizeX = imageSizeX;
            outputImage.sizeY = imageSizeY;
            foreach (ColorChannelEnum colorChannel in Enum.GetValues(typeof(ColorChannelEnum)))
            {
                outputImage.setColorChannel(colorChannel, inputImage.getColorChannel(colorChannel));
            }
            outputImage.setColorChannel(ColorChannelEnum.Sobel, outputGray);

            return outputImage;
        }
    }
}
