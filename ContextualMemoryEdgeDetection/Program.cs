using System;
using System.Collections.Generic;
using System.IO;
using ContextualMemoryEdgeDetection.Benchmark;
using ContextualMemoryEdgeDetection.Logic.Algorithm;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm;
using ContextualMemoryEdgeDetection.Logic.Algorithm.FileHandling;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageBlenders;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters;
using ContextualMemoryEdgeDetection.Logic.AlgorithmProcessor;
using ContextualMemoryEdgeDetection.Logic.Configuration;

namespace ContextualMemoryEdgeDetection
{
    class Program
    {
        const string localBenchmarkPath = @"..\benchmark berkeley\images\";
        
        static void Main(string[] args)
        {
            Console.BufferHeight = short.MaxValue - 1;

            GeneralConfiguration.maximumNumberOfThreads = 8;

            trainTestAndSaveNestedAlgorithm();
            //testNestedAlgorithmOnFile();

            //trainAndSaveModel(exportedModel, relativeImageOutputFolder, numberOfRays, rayLength, memoryBits, numberOfPasses, useRotations);
            //loadAndTestModel(exportedModel, relativeImageOutputFolder);

            // cross entropy validation
            //justValidateImages("folder");

            // testing utilities
            //testFiltering();

            GC.Collect();
            Console.ReadLine();
            Console.ReadLine();
        }


        private static void testAlgorithmOnFile(string algorithmToTest, string filename)
        {
            Console.WriteLine("Started loading " + algorithmToTest);
            EdgeDetectionAlgorithm edgeDetectionAlgorithm = EdgeDetectionAlgorithmUtil.loadAlgorithmFromCompressedFile(algorithmToTest);
            Console.WriteLine("Loaded algorithm. Testing.");
            ImageDescription inputImage = ImageFileHandler.loadFromPath(filename);
            ImageDescription outputImage = edgeDetectionAlgorithm.test(inputImage);
            ImageFileHandler.saveToPath(outputImage, "test", ".png");
            Console.WriteLine("Saved");
        }

        public static void loadAndTestModel(string inputModelFilename, string relativeOutputPath)
        {
            EdgeDetectionAlgorithm algorithm = EdgeDetectionAlgorithmUtil.loadAlgorithmFromCompressedFile(inputModelFilename);
            BerkeleyEdgeDetectionBenchmark benchmark = new BerkeleyEdgeDetectionBenchmark(localBenchmarkPath, Path.Combine(localBenchmarkPath, relativeOutputPath));
            EdgeDetectionProcessor processor = new EdgeDetectionProcessor(benchmark, 0, true);

            Console.WriteLine("Testing started.");
            processor.test(algorithm);
            Console.WriteLine("Testing finished.");

            Console.WriteLine("Validation started.");
            processor.validate();
            Console.WriteLine("Validation finished.");
        }

        private static void trainTestAndSaveNestedAlgorithm()
        {
            string relativeImageOutputFolder = "cm m20 collisions 4layers thr1 1 2 4 8 canny kirsh nms newderivative (gauss) square l16 squaremix";
            string outputModelFilename = relativeImageOutputFolder + ".alg";
            int numberOfPasses = 1;
            int? trainingSetSizeLimit = null;
            bool useRotations = false;
            bool testOnTrainingFiles = false;

            ISet<ColorChannelEnum> colorChannels = new HashSet<ColorChannelEnum> { ColorChannelEnum.Red, ColorChannelEnum.Green, ColorChannelEnum.Blue, ColorChannelEnum.Canny, ColorChannelEnum.Kirsch };
            ISet<ColorChannelEnum> colorAndComputedChannels = new HashSet<ColorChannelEnum> { ColorChannelEnum.Red, ColorChannelEnum.Green, ColorChannelEnum.Blue, ColorChannelEnum.Canny, ColorChannelEnum.Kirsch, ColorChannelEnum.Layer };
            
            ContextualMemoryNestedAlgorithm nestedAlgorithm = new ContextualMemoryNestedAlgorithm();

            ContextualMemoryNestedAlgorithmLayer layer0 = new ContextualMemoryNestedAlgorithmLayer();
            layer0.colorChannels = colorChannels;
            layer0.longestContextLength = 16;
            layer0.numberOfRays = 16;
            layer0.tableSizeBits = 20;
            layer0.resizeFactor = 1;
            layer0.outputResults = true;
            nestedAlgorithm.addLayer(layer0);

            ContextualMemoryNestedAlgorithmLayer layer1 = new ContextualMemoryNestedAlgorithmLayer();
            layer1.colorChannels = colorAndComputedChannels;
            layer1.longestContextLength = 16;
            layer1.numberOfRays = 16;
            layer1.tableSizeBits = 20;
            layer1.resizeFactor = 2;
            layer1.outputResults = true;
            nestedAlgorithm.addLayer(layer1);

            ContextualMemoryNestedAlgorithmLayer layer2 = new ContextualMemoryNestedAlgorithmLayer();
            layer2.colorChannels = colorAndComputedChannels;
            layer2.longestContextLength = 16;
            layer2.numberOfRays = 16;
            layer2.tableSizeBits = 20;
            layer2.resizeFactor = 4;
            layer2.outputResults = true;
            nestedAlgorithm.addLayer(layer2);

            ContextualMemoryNestedAlgorithmLayer layer3 = new ContextualMemoryNestedAlgorithmLayer();
            layer3.colorChannels = colorAndComputedChannels;
            layer3.longestContextLength = 16;
            layer3.numberOfRays = 16;
            layer3.tableSizeBits = 19;
            layer3.resizeFactor = 8;
            layer3.outputResults = true;
            nestedAlgorithm.addLayer(layer3);

            //nestedAlgorithm.setImageBlender(new SimpleGrayscaleImageBlender());
            nestedAlgorithm.setImageBlender(new LogisticMixGrayscaleImageBlender(nestedAlgorithm.getLayers().Count));

            BerkeleyEdgeDetectionBenchmark benchmark = new BerkeleyEdgeDetectionBenchmark(localBenchmarkPath, Path.Combine(localBenchmarkPath, relativeImageOutputFolder), useRotations, false, trainingSetSizeLimit);
            EdgeDetectionProcessor processor = new EdgeDetectionProcessor(benchmark, numberOfPasses, testOnTrainingFiles);

            Console.WriteLine("Training started.");
            processor.trainNestedAlgorithm(nestedAlgorithm);
            Console.WriteLine("Training finished.");

            EdgeDetectionAlgorithmUtil.saveToCompressedFile(nestedAlgorithm, outputModelFilename);

            nestedAlgorithm = null;

            EdgeDetectionAlgorithm algorithm = EdgeDetectionAlgorithmUtil.loadAlgorithmFromCompressedFile(outputModelFilename);         

            Console.WriteLine("Testing started.");
            processor.testNestedAlgorithm((ContextualMemoryNestedAlgorithm)algorithm);
            Console.WriteLine("Testing finished.");
        }

        public static void trainAndSaveModel(string outputModelFilename, string relativeOutputPath,
            int numberOfRays = 16, int rayLength = 10, int memoryBits = 24, int numberOfPasses = 1, bool useRotations = false)
        {
            ISet<ColorChannelEnum> colorChannels = new HashSet<ColorChannelEnum> { ColorChannelEnum.Red, ColorChannelEnum.Green, ColorChannelEnum.Blue };
            ISet<ColorChannelEnum> colorChannelsAndComputed = new HashSet<ColorChannelEnum> { ColorChannelEnum.Red, ColorChannelEnum.Green, ColorChannelEnum.Blue, ColorChannelEnum.Canny, ColorChannelEnum.Kirsch };

            ImageFilterChain filterChain = new ImageFilterChain();
            filterChain.addFilter(new CannyAppenderFilter());
            filterChain.addFilter(new KirschAppenderFilter());
            filterChain.addFilter(new GaussFilterExcludeComputed(5, 1.4f, colorChannels));
            //filterChain.addFilter(new MedianFilter(1, colorChannels));

            ContextualMemoryEdgeDetectionAlgorithm algorithm = new ContextualMemoryEdgeDetectionAlgorithm(colorChannelsAndComputed, rayLength, memoryBits, numberOfRays);
            //ContextualMemoryEdgeDetectionAlgorithmNoCollisions algorithm = new ContextualMemoryEdgeDetectionAlgorithmNoCollisions(colorChannelsAndComputed, rayLength, memoryBits, numberOfRays);
            //ContextualMemoryEdgeDetectionAlgorithmNoCollisionsBiasReplace algorithm = 
            //    new ContextualMemoryEdgeDetectionAlgorithmNoCollisionsBiasReplace(colorChannelsAndComputed, rayLength, memoryBits, numberOfRays);
            algorithm.inputImageFilterChain = filterChain;

            BerkeleyEdgeDetectionBenchmark benchmark = new BerkeleyEdgeDetectionBenchmark(localBenchmarkPath, Path.Combine(localBenchmarkPath, relativeOutputPath), useRotations, false);
            EdgeDetectionProcessor processor = new EdgeDetectionProcessor(benchmark, numberOfPasses, true);

            Console.WriteLine("Training started.");
            processor.train(algorithm);
            Console.WriteLine("Training finished.");

            EdgeDetectionAlgorithmUtil.saveToCompressedFile(algorithm, outputModelFilename);

            EdgeDetectionAlgorithm algorithmToTest = EdgeDetectionAlgorithmUtil.loadAlgorithmFromCompressedFile(outputModelFilename);

            Console.WriteLine("Testing started.");
            processor.test(algorithmToTest);
            Console.WriteLine("Testing finished.");

            Console.WriteLine("Validation started.");
            processor.validate();
            Console.WriteLine("Validation finished.");            
        }

        public static void justValidateImages(string relativeOutputPath)
        {
            BerkeleyEdgeDetectionBenchmark benchmark = new BerkeleyEdgeDetectionBenchmark(localBenchmarkPath, Path.Combine(localBenchmarkPath, relativeOutputPath), false, false);
            EdgeDetectionProcessor processor = new EdgeDetectionProcessor(benchmark, 1, false);
            Console.WriteLine("Validation started.");
            processor.validate();
            Console.WriteLine("Validation finished.");
        }

        private static void testBlending(string filePath1, string filePath2, string outputPath)
        {
            ImageBlender blender = new SimpleGrayscaleImageBlender();
            List<ImageDescription> images = new List<ImageDescription>();
            images.Add(ImageFileHandler.loadFromPath(filePath1));
            images.Add(ImageFileHandler.loadFromPath(filePath2));
            ImageDescription result = blender.blendImages(images);
            ImageFileHandler.saveToPath(result, outputPath, ".png");
        }

        private static void testFiltering(string filePath, string outputPath)
        {
            ImageDescription inputImage = ImageFileHandler.loadFromPath(filePath);
            //ImageFilter filter = new CannyAppenderFilter();
            //ImageFilter filter = new KirschAppenderFilter();
            ImageFilter filter = new SobelAppenderFilter();
            ImageDescription outputImage = filter.filter(inputImage);
            GaussFilter gaussFilter = new GaussFilter(5, 1.4f, new HashSet<ColorChannelEnum> { ColorChannelEnum.Red, ColorChannelEnum.Green, ColorChannelEnum.Blue });
            outputImage = gaussFilter.filter(outputImage);
            ImageFileHandler.saveToPath(outputImage, outputPath, ".png");

            //ImageDescription newOutputImage = new ImageDescription();
            //newOutputImage.sizeX = outputImage.sizeX;
            //newOutputImage.sizeY = outputImage.sizeY;
            //newOutputImage.grayscale = true;
            //newOutputImage.setColorChannel(ColorChannelEnum.Gray, outputImage.getColorChannel(ColorChannelEnum.Canny));
            //ImageFileHandler.saveToPath(newOutputImage, "test2", ".png");
        }

    }
}
