using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ContextualMemoryEdgeDetection.Benchmark;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm
{
    public interface EdgeDetectionAlgorithm
    {
        // returns the loss for the training image
        float train(ImageDescription inputImage, ImageDescription inputImageGroundTruth);

        ImageDescription test(ImageDescription inputImage);

        void save(Stream stream);
    }
}
