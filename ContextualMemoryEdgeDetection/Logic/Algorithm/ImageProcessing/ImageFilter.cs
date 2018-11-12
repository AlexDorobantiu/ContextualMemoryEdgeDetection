using System;
using System.Collections.Generic;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing
{
    public interface ImageFilter
    {
        ImageDescription filter(ImageDescription inputImage);
    }
}
