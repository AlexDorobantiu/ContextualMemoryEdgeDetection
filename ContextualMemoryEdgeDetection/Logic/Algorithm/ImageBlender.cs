using System.Collections.Generic;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm
{
    public interface ImageBlender
    {
        float train(List<ImageDescription> inputImages, ImageDescription inputImageGroundTruth);

        ImageDescription blendImages(List<ImageDescription> images);
    }
}
