using System;
using System.Collections.Generic;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters
{
    [Serializable]
    class GaussFilter : ImageFilter
    {
        public int size { get; set; }
        public float sigma { get; set; }
        public float[,] convolutionMatrix { get; set; }

        public ISet<ColorChannelEnum> colorChannelsToFilter { get; set; }

        public GaussFilter(int size, float sigma, ISet<ColorChannelEnum> colorChannelsToFilter)
        {
            this.size = size;
            this.sigma = sigma;
            this.colorChannelsToFilter = colorChannelsToFilter;

            convolutionMatrix = FilterBankUtil.generateNormalizedGaussConvolutionMatrix(sigma, size);
        }

        public virtual ImageDescription filter(ImageDescription inputImage)
        {
            return ImageDescriptionUtil.mirroredMarginConvolution(inputImage, colorChannelsToFilter, convolutionMatrix);
        }
    }
}
