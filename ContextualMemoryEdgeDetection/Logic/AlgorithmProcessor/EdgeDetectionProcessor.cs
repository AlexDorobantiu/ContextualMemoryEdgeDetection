using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ContextualMemoryEdgeDetection.Benchmark;
using ContextualMemoryEdgeDetection.Logic.Algorithm;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm;
using ContextualMemoryEdgeDetection.Logic.Algorithm.FileHandling;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters;
using ContextualMemoryEdgeDetection.Logic.Algorithm.Utils;

namespace ContextualMemoryEdgeDetection.Logic.AlgorithmProcessor
{
    class EdgeDetectionProcessor
    {
        private const string outputFileExtension = ".png";

        private const string trainingFilesTestOutput = "training";

        int numberOfTrainingSetPasses;

        bool testOnTrainingFiles;

        EdgeDetectionBenchmark benchmark;

        public EdgeDetectionProcessor(EdgeDetectionBenchmark benchmark, int numberOfTrainingSetPasses = 3, bool testOnTrainingFiles = false)
        {
            this.benchmark = benchmark;
            this.numberOfTrainingSetPasses = numberOfTrainingSetPasses;
            this.testOnTrainingFiles = testOnTrainingFiles;
        }

        public void train(EdgeDetectionAlgorithm algorithm)
        {
            BlackAndWhiteConverter blackAndWhiteConverter = new BlackAndWhiteConverter(1);

            DateTime trainingStart = DateTime.Now;
            float totalLoss = 0;
            List<String> fileList = new List<string>(benchmark.getTrainingFilesPathList());

            int totalNumberOfFiles = numberOfTrainingSetPasses * fileList.Count;
            int totalIndex = 0;

            for (int pass = 0; pass < numberOfTrainingSetPasses; pass++)
            {
                ListUtils.Shuffle(fileList);
                int index = 1;
                float totalPassLoss = 0;
                DateTime trainingPassStart = DateTime.Now;
                foreach (string trainingFileName in fileList)
                {
                    DateTime start = DateTime.Now;

                    Console.WriteLine("Pass: " + (pass + 1) + "/" + numberOfTrainingSetPasses + ", " + index + "/" + fileList.Count + " Training file: " + Path.GetFileName(trainingFileName));
                    ImageDescription inputImage = ImageFileHandler.loadFromPath(trainingFileName);
                    ImageDescription inputImageGroundTruth = ImageFileHandler.loadFromPath(benchmark.getTrainingFileGroundTruth(trainingFileName));
                    inputImageGroundTruth.computeGrayscale();
                    inputImageGroundTruth = blackAndWhiteConverter.filter(inputImageGroundTruth);
                    float loss = algorithm.train(inputImage, inputImageGroundTruth);
                    totalLoss += loss;
                    totalPassLoss += loss;
                    index++;
                    totalIndex++;

                    double timeElapsed = (DateTime.Now - start).TotalSeconds;
                    double timeElapsedSoFar = (DateTime.Now - trainingStart).TotalSeconds;
                    double estimatedTime = (timeElapsedSoFar / totalIndex) * (totalNumberOfFiles - totalIndex);
                    Console.WriteLine("Loss: " + loss.ToString("0.00") + " Time: " + timeElapsed.ToString("0.00") + "s Time elapsed: "
                        + timeElapsedSoFar.ToString("0.00") + "s ETA: " + estimatedTime.ToString("0.00") + "s");
                }
                double tariningPassTimeElapsed = (DateTime.Now - trainingPassStart).TotalSeconds;
                Console.WriteLine("Pass took " + tariningPassTimeElapsed.ToString("0.00") + " sec. Pass loss: " + totalPassLoss.ToString("0.00")
                + " Avg loss: " + (totalPassLoss / (fileList.Count)).ToString("0.00"));
            }
            double totalTimeElapsed = (DateTime.Now - trainingStart).TotalSeconds;
            Console.WriteLine("Training took " + totalTimeElapsed.ToString("0.00") + " sec. Total loss: " + totalLoss.ToString("0.00")
                + " Avg loss: " + (totalLoss / (totalNumberOfFiles)).ToString("0.00"));
        }

        public void trainWithBaseAlgorithm(EdgeDetectionAlgorithm algorithm, EdgeDetectionAlgorithm baseAlgorithm, int resizeFactor)
        {
            DateTime trainingStart = DateTime.Now;
            float totalLoss = 0;
            List<String> fileList = new List<string>(benchmark.getTrainingFilesPathList());

            int totalNumberOfFiles = numberOfTrainingSetPasses * fileList.Count;
            int totalIndex = 0;

            for (int pass = 0; pass < numberOfTrainingSetPasses; pass++)
            {
                ListUtils.Shuffle(fileList);
                int index = 1;
                float totalPassLoss = 0;
                DateTime trainingPassStart = DateTime.Now;
                foreach (string trainingFileName in fileList)
                {
                    DateTime start = DateTime.Now;

                    Console.WriteLine("Pass: " + (pass + 1) + "/" + numberOfTrainingSetPasses + ", " + index + "/" + fileList.Count + " Training file: " + Path.GetFileName(trainingFileName));
                    ImageDescription inputImage = ImageFileHandler.loadFromPath(trainingFileName);
                    ImageDescription computedImage = baseAlgorithm.test(inputImage);

                    ResizeFilter resizeColor = new ResizeFilter(inputImage.sizeX / resizeFactor, inputImage.sizeY / resizeFactor, ImageDescriptionUtil.colorChannels);
                    ImageDescription newInputImage = resizeColor.filter(inputImage);

                    ImageDescription inputImageGroundTruth = ImageFileHandler.loadFromPath(benchmark.getTrainingFileGroundTruth(trainingFileName));
                    inputImageGroundTruth.computeGrayscale();
                    ResizeFilter resizeGrayscale = new ResizeFilter(inputImage.sizeX / resizeFactor, inputImage.sizeY / resizeFactor, ImageDescriptionUtil.grayscaleChannel);
                    ImageDescription newInputImageGroundTruth = resizeGrayscale.filter(inputImageGroundTruth);

                    ImageDescription resizedComputed = resizeGrayscale.filter(computedImage);
                    newInputImage.setColorChannel(ColorChannelEnum.Layer, resizedComputed.gray);

                    float loss = algorithm.train(newInputImage, newInputImageGroundTruth);
                    totalLoss += loss;
                    totalPassLoss += loss;
                    index++;
                    totalIndex++;

                    double timeElapsed = (DateTime.Now - start).TotalSeconds;
                    double timeElapsedSoFar = (DateTime.Now - trainingStart).TotalSeconds;
                    double estimatedTime = (timeElapsedSoFar / totalIndex) * (totalNumberOfFiles - totalIndex);
                    Console.WriteLine("Loss: " + loss.ToString("0.00") + " Time: " + timeElapsed.ToString("0.00") + "s Time elapsed: "
                        + timeElapsedSoFar.ToString("0.00") + "s ETA: " + estimatedTime.ToString("0.00") + "s");
                }
                double tariningPassTimeElapsed = (DateTime.Now - trainingPassStart).TotalSeconds;
                Console.WriteLine("Pass took " + tariningPassTimeElapsed.ToString("0.00") + " sec. Pass loss: " + totalPassLoss.ToString("0.00")
                + " Avg loss: " + (totalPassLoss / (fileList.Count)).ToString("0.00"));
            }
            double totalTimeElapsed = (DateTime.Now - trainingStart).TotalSeconds;
            Console.WriteLine("Training took " + totalTimeElapsed.ToString("0.00") + " sec. Total loss: " + totalLoss.ToString("0.00")
                + " Avg loss: " + (totalLoss / (totalNumberOfFiles)).ToString("0.00"));
        }
        public void trainNestedAlgorithm(ContextualMemoryNestedAlgorithm nestedAlgorithm)
        {
            BlackAndWhiteConverter blackAndWhiteConverter = new BlackAndWhiteConverter(1);
            //BlackAndWhiteConverter blackAndWhiteConverter = new BlackAndWhiteConverter(63);

            List<String> fileList = new List<string>(benchmark.getTrainingFilesPathList());

            List<ContextualMemoryNestedAlgorithmLayer> layers = nestedAlgorithm.getLayers();
            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                ContextualMemoryNestedAlgorithmLayer layer = layers[layerIndex];
                layer.initialize();
                Console.WriteLine("Layer: " + (layerIndex + 1) + "/" + layers.Count);

                EdgeDetectionAlgorithm algorithm = layer.algorithm;

                DateTime trainingStart = DateTime.Now;
                float totalLoss = 0;
                int totalNumberOfFiles = numberOfTrainingSetPasses * fileList.Count;
                int totalIndex = 0;
                for (int pass = 0; pass < numberOfTrainingSetPasses; pass++)
                {
                    ListUtils.Shuffle(fileList);
                    int index = 1;
                    float totalPassLoss = 0;
                    DateTime trainingPassStart = DateTime.Now;
                    foreach (string trainingFileName in fileList)
                    {
                        DateTime start = DateTime.Now;
                        Console.WriteLine("Pass: " + (pass + 1) + "/" + numberOfTrainingSetPasses + ", " + index + "/" + fileList.Count + " Training file: " + Path.GetFileName(trainingFileName));

                        ImageDescription inputImage = ImageFileHandler.loadFromPath(trainingFileName);
                        int layerResizeFactor = layer.resizeFactor;

                        ImageDescription computedImage = null;
                        if (layerIndex > 0)
                        {
                            List<ImageDescription> computedImages = nestedAlgorithm.computeImageForLayers(inputImage, layerIndex);
                            computedImage = computedImages[layerIndex - 1];
                        }

                        ImageDescription inputImageGroundTruth = ImageFileHandler.loadFromPath(benchmark.getTrainingFileGroundTruth(trainingFileName));
                        inputImageGroundTruth = blackAndWhiteConverter.filter(inputImageGroundTruth);

                        ImageDescription newInputImage = null;
                        ImageDescription newInputImageGroundTruth = null;

                        ResizeFilter resizeGrayscale = new ResizeFilter(inputImage.sizeX / layerResizeFactor, inputImage.sizeY / layerResizeFactor, ImageDescriptionUtil.grayscaleChannel);
                        ResizeFilter resizeColor = new ResizeFilter(inputImage.sizeX / layerResizeFactor, inputImage.sizeY / layerResizeFactor, ImageDescriptionUtil.colorChannels);

                        if (layerResizeFactor == 1)
                        {
                            newInputImage = inputImage;
                            newInputImageGroundTruth = inputImageGroundTruth;
                        }
                        else
                        {
                            newInputImage = resizeColor.filter(inputImage);
                            newInputImageGroundTruth = resizeGrayscale.filter(inputImageGroundTruth);
                        }
                        if (layerIndex > 0)
                        {
                            ImageDescription resizedComputed = resizeGrayscale.filter(computedImage);
                            newInputImage.setColorChannel(ColorChannelEnum.Layer, resizedComputed.gray);
                        }

                        float loss = algorithm.train(newInputImage, newInputImageGroundTruth);

                        totalLoss += loss;
                        totalPassLoss += loss;
                        index++;
                        totalIndex++;

                        double timeElapsed = (DateTime.Now - start).TotalSeconds;
                        double timeElapsedSoFar = (DateTime.Now - trainingStart).TotalSeconds;
                        double estimatedTime = (timeElapsedSoFar / totalIndex) * (totalNumberOfFiles - totalIndex);
                        Console.WriteLine("Loss: " + loss.ToString("0.00") + " Time: " + timeElapsed.ToString("0.00") + "s Time elapsed: "
                            + timeElapsedSoFar.ToString("0.00") + "s ETA: " + estimatedTime.ToString("0.00") + "s");
                    }
                    double tariningPassTimeElapsed = (DateTime.Now - trainingPassStart).TotalSeconds;
                    Console.WriteLine("Pass took " + tariningPassTimeElapsed.ToString("0.00") + " sec. Pass loss: " + totalPassLoss.ToString("0.00")
                    + " Avg loss: " + (totalPassLoss / (fileList.Count)).ToString("0.00"));
                }
                double totalTimeElapsed = (DateTime.Now - trainingStart).TotalSeconds;
                Console.WriteLine("Training took " + totalTimeElapsed.ToString("0.00") + " sec. Total loss: " + totalLoss.ToString("0.00")
                    + " Avg loss: " + (totalLoss / (totalNumberOfFiles)).ToString("0.00"));
            }

            Console.WriteLine("Training blender");

            DateTime blenderTrainingStart = DateTime.Now;
            float blenderTotalLoss = 0;
            int blenderTotalNumberOfFiles =/* numberOfTrainingSetPasses * */fileList.Count;
            int blenderTotalIndex = 0;
            ImageBlender blender = nestedAlgorithm.getImageBlender();
            //for (int pass = 0; pass < numberOfTrainingSetPasses; pass++)
            {
                ListUtils.Shuffle(fileList);
                int index = 1;
                float totalPassLoss = 0;
                DateTime trainingPassStart = DateTime.Now;
                foreach (string trainingFileName in fileList)
                {
                    DateTime start = DateTime.Now;
                    //Console.Write("Pass: " + (pass + 1) + "/" + numberOfTrainingSetPasses + ", ");
                    Console.WriteLine(index + "/" + fileList.Count + " Training file: " + Path.GetFileName(trainingFileName));

                    ImageDescription inputImage = ImageFileHandler.loadFromPath(trainingFileName);
                    List<ImageDescription> computedImages = nestedAlgorithm.computeImageForLayers(inputImage, layers.Count);

                    ImageDescription inputImageGroundTruth = ImageFileHandler.loadFromPath(benchmark.getTrainingFileGroundTruth(trainingFileName));
                    inputImageGroundTruth = blackAndWhiteConverter.filter(inputImageGroundTruth);

                    float blenderLoss = blender.train(computedImages, inputImageGroundTruth);

                    blenderTotalLoss += blenderLoss;
                    totalPassLoss += blenderLoss;
                    index++;
                    blenderTotalIndex++;

                    double timeElapsed = (DateTime.Now - start).TotalSeconds;
                    double timeElapsedSoFar = (DateTime.Now - blenderTrainingStart).TotalSeconds;
                    double estimatedTime = (timeElapsedSoFar / blenderTotalIndex) * (blenderTotalNumberOfFiles - blenderTotalIndex);
                    Console.WriteLine("Loss: " + blenderLoss.ToString("0.00") + " Time: " + timeElapsed.ToString("0.00") + "s Time elapsed: "
                        + timeElapsedSoFar.ToString("0.00") + "s ETA: " + estimatedTime.ToString("0.00") + "s");
                }
                //double tariningPassTimeElapsed = (DateTime.Now - trainingPassStart).TotalSeconds;
                //Console.WriteLine("Pass took " + tariningPassTimeElapsed.ToString("0.00") + " sec. Pass loss: " + totalPassLoss.ToString("0.00")
                //+ " Avg loss: " + (totalPassLoss / (fileList.Count)).ToString("0.00"));
            }
            double blenderTotalTimeElapsed = (DateTime.Now - blenderTrainingStart).TotalSeconds;
            Console.WriteLine("Training took " + blenderTotalTimeElapsed.ToString("0.00") + " sec. Total loss: " + blenderTotalLoss.ToString("0.00")
                + " Avg loss: " + (blenderTotalLoss / (blenderTotalNumberOfFiles)).ToString("0.00"));
        }

        public void test(EdgeDetectionAlgorithm algorithm)
        {
            DateTime testingStart = DateTime.Now;
            List<String> fileList = benchmark.getTestFilesPathList();
            int index = 1;

            string outputDirectory = null;
            foreach (string testFileName in fileList)
            {
                DateTime start = DateTime.Now;
                outputDirectory = Path.GetDirectoryName(benchmark.getTestFileOutputPathWithoutExtension(testFileName));
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                Console.WriteLine(index + "/" + fileList.Count + " Testing file: " + Path.GetFileName(testFileName));
                ImageDescription inputImage = ImageFileHandler.loadFromPath(testFileName);
                ImageDescription outputImage = algorithm.test(inputImage);
                ImageFileHandler.saveToPath(outputImage, benchmark.getTestFileOutputPathWithoutExtension(testFileName), outputFileExtension);


                double timeElapsed = (DateTime.Now - start).TotalSeconds;
                double timeElapsedSoFar = (DateTime.Now - testingStart).TotalSeconds;
                double estimatedTime = (timeElapsedSoFar / index) * (fileList.Count - index);
                Console.WriteLine(timeElapsed.ToString("0.00") + "s Time elapsed: "
                        + timeElapsedSoFar.ToString("0.00") + "s ETA: " + estimatedTime.ToString("0.00") + "s");
                index++;
            }
            double totalTimeElapsed = (DateTime.Now - testingStart).TotalSeconds;
            Console.WriteLine("Testing took " + totalTimeElapsed.ToString("0.00") + " sec.");


            if (testOnTrainingFiles)
            {
                Console.WriteLine("Testing on training files");
                testingStart = DateTime.Now;
                index = 0;

                // we have the outputDirectory from test, else, relative to the exe
                outputDirectory = Path.Combine(outputDirectory, trainingFilesTestOutput);
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                fileList = new List<string>(benchmark.getTrainingFilesPathList());
                foreach (string trainingFileName in fileList)
                {
                    DateTime start = DateTime.Now;
                    string outputPath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(trainingFileName));
                    Console.WriteLine(index + "/" + fileList.Count + " Testing file: " + Path.GetFileName(trainingFileName));
                    ImageDescription inputImage = ImageFileHandler.loadFromPath(trainingFileName);
                    ImageDescription outputImage = algorithm.test(inputImage);
                    ImageFileHandler.saveToPath(outputImage, outputPath, outputFileExtension);
                    index++;

                    double timeElapsed = (DateTime.Now - start).TotalSeconds;
                    Console.WriteLine(timeElapsed.ToString("0.00") + " seconds");
                }
                totalTimeElapsed = (DateTime.Now - testingStart).TotalSeconds;
                Console.WriteLine("Testing on training files took " + totalTimeElapsed.ToString("0.00") + " sec.");
            }
        }

        public void testNestedAlgorithm(ContextualMemoryNestedAlgorithm nestedAlgorithm)
        {
            DateTime testingStart = DateTime.Now;
            List<String> fileList = benchmark.getTestFilesPathList();
            int index = 1;

            string outputDirectory = null;
            foreach (string testFileName in fileList)
            {
                DateTime start = DateTime.Now;
                outputDirectory = Path.GetDirectoryName(benchmark.getTestFileOutputPathWithoutExtension(testFileName));
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                Console.WriteLine(index + "/" + fileList.Count + " Testing file: " + Path.GetFileName(testFileName));


                ImageDescription inputImage = ImageFileHandler.loadFromPath(testFileName);
                List<ContextualMemoryNestedAlgorithmLayer> layers = nestedAlgorithm.getLayers();

                List<ImageDescription> computedImages = nestedAlgorithm.computeImageForLayers(inputImage, layers.Count);
                for (int i = 0; i < layers.Count; i++)
                {
                    if (layers[i].outputResults)
                    {
                        ImageFileHandler.saveToPath(computedImages[i], benchmark.getTestFileOutputPathWithoutExtension(testFileName) + "_layer" + i, outputFileExtension);
                    }
                }
                ImageDescription outputImage = nestedAlgorithm.getImageBlender().blendImages(computedImages);
                ImageFileHandler.saveToPath(outputImage, benchmark.getTestFileOutputPathWithoutExtension(testFileName), outputFileExtension);

                double timeElapsed = (DateTime.Now - start).TotalSeconds;
                double timeElapsedSoFar = (DateTime.Now - testingStart).TotalSeconds;
                double estimatedTime = (timeElapsedSoFar / index) * (fileList.Count - index);
                Console.WriteLine(timeElapsed.ToString("0.00") + "s Time elapsed: "
                        + timeElapsedSoFar.ToString("0.00") + "s ETA: " + estimatedTime.ToString("0.00") + "s");
                index++;
            }
            double totalTimeElapsed = (DateTime.Now - testingStart).TotalSeconds;
            Console.WriteLine("Testing took " + totalTimeElapsed.ToString("0.00") + " sec.");

            if (testOnTrainingFiles)
            {
                Console.WriteLine("Testing on training files");
                testingStart = DateTime.Now;
                index = 0;

                // we have the outputDirectory from test, else, relative to the exe
                outputDirectory = Path.Combine(outputDirectory, trainingFilesTestOutput);
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                fileList = new List<string>(benchmark.getTrainingFilesPathList());
                foreach (string trainingFileName in fileList)
                {
                    DateTime start = DateTime.Now;
                    string outputPath = Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(trainingFileName));
                    Console.WriteLine((index + 1) + "/" + fileList.Count + " Testing file: " + Path.GetFileName(trainingFileName));
                    ImageDescription inputImage = ImageFileHandler.loadFromPath(trainingFileName);

                    List<ContextualMemoryNestedAlgorithmLayer> layers = nestedAlgorithm.getLayers();
                    List<ImageDescription> computedImages = nestedAlgorithm.computeImageForLayers(inputImage, layers.Count);
                    for (int i = 0; i < layers.Count; i++)
                    {
                        if (layers[i].outputResults)
                        {
                            ImageFileHandler.saveToPath(computedImages[i], outputPath + "_layer" + i, outputFileExtension);
                        }
                    }
                    ImageDescription outputImage = nestedAlgorithm.getImageBlender().blendImages(computedImages);

                    ImageFileHandler.saveToPath(outputImage, outputPath, outputFileExtension);
                    index++;

                    double timeElapsed = (DateTime.Now - start).TotalSeconds;
                    Console.WriteLine(timeElapsed.ToString("0.00") + " seconds");
                }
                totalTimeElapsed = (DateTime.Now - testingStart).TotalSeconds;
                Console.WriteLine("Testing on training files took " + totalTimeElapsed.ToString("0.00") + " sec.");
            }
        }

        public void validate()
        {
            DateTime validateStart = DateTime.Now;

            List<String> fileList = benchmark.getTestFilesPathList();

            float totalCrossEntropy = 0;
            foreach (string testFilePath in fileList)
            {
                DateTime start = DateTime.Now;
                string outputFilePath = Path.ChangeExtension(benchmark.getTestFileOutputPathWithoutExtension(testFilePath), outputFileExtension);
                string groundTruthPath = benchmark.getTestingFileGroundTruth(testFilePath);

                ImageDescription outputImage = ImageFileHandler.loadFromPath(outputFilePath);
                ImageDescription groundTruthImage = ImageFileHandler.loadFromPath(groundTruthPath);

                byte[,] outputGray = outputImage.getColorChannel(ColorChannelEnum.Gray);
                byte[,] groundTruthGray = groundTruthImage.getColorChannel(ColorChannelEnum.Gray);
                // might be a bug in GDI
                if (outputGray == null)
                {
                    outputImage.computeGrayscale();
                    outputGray = outputImage.getColorChannel(ColorChannelEnum.Gray);
                }
                if (groundTruthGray == null)
                {
                    groundTruthImage.computeGrayscale();
                    groundTruthGray = groundTruthImage.getColorChannel(ColorChannelEnum.Gray);
                }

                float crossEntropy = 0;
                for (int i = 0; i < outputGray.GetLength(0); i++)
                {
                    for (int j = 0; j < outputGray.GetLength(1); j++)
                    {
                        byte output = outputGray[i, j];
                        byte groundTruth = groundTruthGray[i, j];

                        float groundTruthProbability;
                        float outputProbability;
                        if (groundTruth != 0)
                        {
                            groundTruthProbability = 1.0f;
                        }
                        else
                        {
                            groundTruthProbability = 0;
                        }
                        //groundTruthProbability = groundTruth / 255.0f;


                        if (output == 0)
                        {
                            outputProbability = 1 / 255.0f;
                        }
                        else
                        {
                            if (output == 255)
                            {
                                outputProbability = 254 / 255.0f;
                            }
                            else
                            {
                                outputProbability = output / 255.0f;
                            }
                        }
                        float loss = LogisticHelper.computeEntropyLoss(outputProbability, groundTruthProbability);
                        crossEntropy += loss;
                    }
                }

                totalCrossEntropy += crossEntropy;
                Console.WriteLine(testFilePath);
                Console.WriteLine("Cross entropy: " + crossEntropy.ToString("0.00"));
            }
            Console.WriteLine("Total cross entropy: " + totalCrossEntropy.ToString("0.00"));
            double totalTimeElapsed = (DateTime.Now - validateStart).TotalSeconds;
            Console.WriteLine("Validation took " + totalTimeElapsed.ToString("0.00") + " sec.");
        }
    }
}
