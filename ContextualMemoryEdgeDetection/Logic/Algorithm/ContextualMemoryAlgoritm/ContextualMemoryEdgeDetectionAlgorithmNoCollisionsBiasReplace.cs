#define useBinaryFeedback
//#define useParallelTraining
//#define useEntropyLoss

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm.ContextMap;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm.Contexts;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm.SSE;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing;
using ContextualMemoryEdgeDetection.Logic.Algorithm.Utils;
using ContextualMemoryEdgeDetection.Logic.Configuration;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm
{
    [Serializable]
    class ContextualMemoryEdgeDetectionAlgorithmNoCollisionsBiasReplace : EdgeDetectionAlgorithm
    {
        int longestContextLength;
        int tableSizeBits;
        int numberOfRays;

        const int bucketSizeBits = 2;
        const float globalErrorWeight = 1.25f; // 1.15f; //1;// 0.5f;//
        const float localErrorWeight = 1.25f / 2; // globalErrorWeight / 2.0f; //0.5f;// 0.25f;//

        const int floatPartBits = 10;
        const int floatUnit = 1 << floatPartBits;
        const float invertedFloatUnit = 1.0f / floatUnit;

        const float confidenceSquashFactor = 2.0f;
        const float tableValueSquashFactor = 1.0f / floatUnit;

        int numberOfContexts;
        AbstractContextualMemoryContextComputer contextComputer;

        [NonSerialized]
        int[,] computedIndexes;

        BiasContextMapShort[] contextTableMap;
        int[] tableSizeMasks;

        float outputProbabilitySquashFactor;

        SecondarySymbolEstimationStretchInput sse;
        //SecondarySymbolEstimationStretchInput sse1;

        float[] groundTruthProbabilityCache;

        [NonSerialized]
        ImageDescription currentInputImage;

        public ImageFilterChain inputImageFilterChain { get; set; }

        public ContextualMemoryEdgeDetectionAlgorithmNoCollisionsBiasReplace(ISet<ColorChannelEnum> colorChannels, int longestContextLength = 5, int tableSizeBits = 22, int numberOfRays = 8)
        {
            this.longestContextLength = longestContextLength;
            this.tableSizeBits = tableSizeBits;
            this.numberOfRays = numberOfRays;

            contextComputer = new RaysContextualMemoryContextComputer(longestContextLength, numberOfRays, tableSizeBits, colorChannels);
            List<int> tableBitSizes = contextComputer.getTableBitSizes();
            numberOfContexts = tableBitSizes.Count;

            contextTableMap = new BiasContextMapShort[numberOfContexts];
            for (int i = 0; i < numberOfContexts; i++)
            {
                contextTableMap[i] = new BiasContextMapShort(tableBitSizes[i] - bucketSizeBits, 1 << bucketSizeBits);
            }
            tableSizeMasks = new int[numberOfContexts];
            for (int i = 0; i < numberOfContexts; i++)
            {
                tableSizeMasks[i] = (1 << tableBitSizes[i]) - 1;
            }

            outputProbabilitySquashFactor = confidenceSquashFactor / floatUnit;

            sse = new SecondarySymbolEstimationStretchInput(256, 32, 0.001f);
            //sse1 = new SecondarySymbolEstimationStretchInput(65536, 32, 0.001f);

            groundTruthProbabilityCache = new float[256];
#if !useBinaryFeedback
            groundTruthProbabilityCache[0] = LogisticHelper.probabilityMinValue;
            groundTruthProbabilityCache[255] = LogisticHelper.probabilityMaxValue;
            for (int i = 1; i < 255; i++)
            {
                groundTruthProbabilityCache[i] = i / 255.0f;
            }
#else
            groundTruthProbabilityCache[0] = 0;
            for (int i = 1; i < 256; i++)
            {
                groundTruthProbabilityCache[i] = 1.0f;
            }
#endif
        }

        private void computeIndexes()
        {
            int numberOfIndexes = currentInputImage.sizeX * currentInputImage.sizeY;
            if (computedIndexes == null || computedIndexes.GetLength(0) < numberOfIndexes)
            {
                computedIndexes = new int[numberOfIndexes, numberOfContexts];
            }
            //for (int i = 0; i < numberOfIndexes; i++)
            Parallel.For(0, numberOfIndexes, new ParallelOptions { MaxDegreeOfParallelism = GeneralConfiguration.maximumNumberOfThreads }, (i) =>
            {
                int positionX = i % currentInputImage.sizeX;
                int positionY = i / currentInputImage.sizeX;
                contextComputer.computeIndexes(currentInputImage, positionX, positionY, i, computedIndexes);
            }
            );
        }

        private void computeProbabilityForPosition(int positionX, int positionY, int computedIndex, out float probability, out float feebackProbability, out float outputProbability)
        {
            int sum = 0;
            int numberOfHits = 0;
            for (int i = 0; i < numberOfContexts; i++)
            {
                short? contextValue = contextTableMap[i].getContextValue(computedIndexes[computedIndex, i]);
                if (contextValue != null)
                {
                    sum += contextValue.Value;
                    numberOfHits++;
                }
            }

            probability = LogisticHelper.squash(sum * outputProbabilitySquashFactor / ((numberOfContexts + numberOfHits + 1) >> 1));
            int sseContext = currentInputImage.gray[positionY, positionX];
            feebackProbability = sse.getEstimation(sseContext, probability);
            //int sse1Context = NumberUtils.hash3(currentInputImage.r[positionY, positionX], currentInputImage.g[positionY, positionX], currentInputImage.b[positionY, positionX]) & 65535;
            //outputProbability = sse1.getEstimation(sse1Context, feebackProbability);
            outputProbability = feebackProbability;
        }

        private void updateProbability(int positionX, int positionY, float groundTruthProbability, float probability, float feebackProbability, int computedIndex)
        {
            int sseContext = currentInputImage.gray[positionY, positionX];
            sse.update(sseContext, probability, groundTruthProbability);
            //sse1.update(groundTruthProbability);

#if useEntropyLoss
            float globalError = (feebackProbability - groundTruthProbability) * globalErrorWeight; // entropy loss
#else
            float globalError = (feebackProbability - groundTruthProbability) * feebackProbability * (1 - feebackProbability) * globalErrorWeight; // square loss
#endif
            for (int i = 0; i < numberOfContexts; i++)
            {
                int tableValue;
                short? contextValue = contextTableMap[i].getContextValue(computedIndexes[computedIndex, i]);
                if (contextValue != null)
                {
                    tableValue = contextValue.Value;
                }
                else
                {
                    tableValue = 0;
                }
#if useEntropyLoss
                float localError = (LogisticHelper.squash(tableValue * tableValueSquashFactor) - groundTruthProbability) * localErrorWeight; // entropy loss
#else
                float localProbability = LogisticHelper.squash(tableValue * tableValueSquashFactor);
                float localError = (localProbability - groundTruthProbability) * localProbability * (1 - localProbability) * localErrorWeight; // square loss
#endif
                tableValue = tableValue - (int)(floatUnit * (globalError + localError) + 0.5f);
                if (tableValue > short.MaxValue)
                {
                    tableValue = short.MaxValue;
                }
                else
                {
                    if (tableValue < short.MinValue)
                    {
                        tableValue = short.MinValue;
                    }
                }
                contextTableMap[i].updateLastContextValue((short)tableValue);
            }
        }

        private void setInputImageToContexts(ImageDescription inputImage)
        {
            currentInputImage = inputImage;
            if (inputImageFilterChain != null)
            {
                currentInputImage = inputImageFilterChain.applyFiltering(inputImage);
            }
            currentInputImage.computeGrayscale();
        }

        public float train(ImageDescription inputImage, ImageDescription inputImageGroundTruth)
        {
            float entropyLoss = 0;
            setInputImageToContexts(inputImage);

            computeIndexes();

            //for (int positionY = 0; positionY < inputImage.sizeY; positionY++)
            //{
            //    for (int positionX = 0; positionX < inputImage.sizeX; positionX++)
            //    {

            int numberOfIndexes = currentInputImage.sizeX * currentInputImage.sizeY;
#if !useParallelTraining
            for (int computedIndex = 0; computedIndex < numberOfIndexes; computedIndex++)
#else
            Parallel.For(0, numberOfIndexes, new ParallelOptions { MaxDegreeOfParallelism = GeneralConfiguration.maximumNumberOfThreads }, (computedIndex) =>
#endif
            {
                int positionX = computedIndex % currentInputImage.sizeX;
                int positionY = computedIndex / currentInputImage.sizeX;
                float probability;
                float feebackProbability;
                float outputProbability;
                computeProbabilityForPosition(positionX, positionY, computedIndex, out probability, out feebackProbability, out outputProbability);
                float groundTruthProbability = groundTruthProbabilityCache[inputImageGroundTruth.gray[positionY, positionX]];
                entropyLoss += LogisticHelper.computeEntropyLoss(outputProbability, groundTruthProbability);
                updateProbability(positionX, positionY, groundTruthProbability, probability, feebackProbability, computedIndex);
            }
#if useParallelTraining
            );
#endif
            //    }
            //}
            return entropyLoss;
        }

        public ImageDescription test(ImageDescription inputImage)
        {
            setInputImageToContexts(inputImage);
            ImageDescription outputImage = ImageDescriptionUtil.createGrayscaleImageWithSameSize(inputImage);

            computeIndexes();

            //for (int positionY = 0; positionY < inputImage.sizeY; positionY++)
            //{
            //    for (int positionX = 0; positionX < inputImage.sizeX; positionX++)
            //    {

            int numberOfIndexes = currentInputImage.sizeX * currentInputImage.sizeY;
            //for (int computedIndex = 0; computedIndex < numberOfIndexes; computedIndex++)  
            Parallel.For(0, numberOfIndexes, new ParallelOptions { MaxDegreeOfParallelism = GeneralConfiguration.maximumNumberOfThreads }, (computedIndex) =>
            {
                int positionX = computedIndex % currentInputImage.sizeX;
                int positionY = computedIndex / currentInputImage.sizeX;
                float probability;
                float feebackProbability;
                float outputProbability;
                computeProbabilityForPosition(positionX, positionY, computedIndex, out probability, out feebackProbability, out outputProbability);
                outputImage.gray[positionY, positionX] = (byte)(outputProbability * 255 + 0.5f);
            }
            );
            //    }
            //}

            return outputImage;
        }

        public void save(Stream stream)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, this);
        }
    }
}
