using System;
using System.Collections.Generic;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing
{
    [Serializable]
    class ImageFilterChain
    {
        private List<ImageFilter> filters = new List<ImageFilter>();

        public ImageFilterChain()
        {
        }

        public ImageFilterChain(params ImageFilter[] filters)
        {
            if (filters != null)
            {
                this.filters.AddRange(filters);
            }
        }

        public ImageFilterChain addFilter(ImageFilter filter)
        {
            this.filters.Add(filter);
            return this;
        }

        public ImageDescription applyFiltering(ImageDescription inputImage)
        {
            ImageDescription outputImage = inputImage;
            foreach (ImageFilter filter in filters)
            {
                outputImage = filter.filter(outputImage);
            }
            return outputImage;
        }
    }
}
