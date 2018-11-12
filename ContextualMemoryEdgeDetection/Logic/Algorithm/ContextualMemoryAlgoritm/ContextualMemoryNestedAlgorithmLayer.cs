using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm
{
    [Serializable]
    class ContextualMemoryNestedAlgorithmLayer
    {
        public int longestContextLength;

        public int tableSizeBits;

        public int numberOfRays;

        public ISet<ColorChannelEnum> colorChannels;

        public int resizeFactor;

        public bool outputResults;

        public EdgeDetectionAlgorithm algorithm;

        public void initialize()
        {
            ContextualMemoryEdgeDetectionAlgorithm algorithm = new ContextualMemoryEdgeDetectionAlgorithm(colorChannels, longestContextLength, tableSizeBits, numberOfRays);
            //ContextualMemoryEdgeDetectionAlgorithmNoCollisions algorithm = new ContextualMemoryEdgeDetectionAlgorithmNoCollisions(colorChannels, longestContextLength, tableSizeBits, numberOfRays);
            //ContextualMemoryEdgeDetectionAlgorithmNoCollisionsBiasReplace algorithm = 
            //    new ContextualMemoryEdgeDetectionAlgorithmNoCollisionsBiasReplace(colorChannels, longestContextLength, tableSizeBits, numberOfRays);
            
            ImageFilterChain filterChain = new ImageFilterChain();
            filterChain.addFilter(new CannyAppenderFilter());
            //filterChain.addFilter(new SobelAppenderFilter());
            filterChain.addFilter(new KirschAppenderFilter(1.4f, true, 32, 0));
            //filterChain.addFilter(new GaussFilter(5, 1.4f, new HashSet<ColorChannelEnum> { ColorChannelEnum.Gray }));
            filterChain.addFilter(new GaussFilterExcludeComputed(5, 1.4f, colorChannels));
            //filterChain.addFilter(new GaussFilter(5, 1.4f, colorChannels));
            algorithm.inputImageFilterChain = filterChain;

            this.algorithm = algorithm;
        }

        public void save(Stream stream)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, this);
        }
    }
}
