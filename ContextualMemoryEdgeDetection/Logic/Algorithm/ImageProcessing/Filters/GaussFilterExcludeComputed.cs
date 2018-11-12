using System;
using System.Collections.Generic;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters
{
    [Serializable]
    class GaussFilterExcludeComputed : GaussFilter
    {
        public GaussFilterExcludeComputed(int size, float sigma, ISet<ColorChannelEnum> colorChannelsToFilter) : base(size, sigma, colorChannelsToFilter)
        {
            this.colorChannelsToFilter = new HashSet<ColorChannelEnum>(colorChannelsToFilter);
            this.colorChannelsToFilter.Remove(ColorChannelEnum.Canny);
            this.colorChannelsToFilter.Remove(ColorChannelEnum.Layer);
            this.colorChannelsToFilter.Remove(ColorChannelEnum.Kirsch);
            this.colorChannelsToFilter.Remove(ColorChannelEnum.Sobel);
        }
    }
}
