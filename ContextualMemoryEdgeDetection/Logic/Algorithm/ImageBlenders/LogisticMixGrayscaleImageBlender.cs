#define useBinaryFeedback
//#define useEntropyLoss

using System;
using System.Collections.Generic;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;
using ContextualMemoryEdgeDetection.Logic.Algorithm.Utils;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageBlenders
{
    [Serializable]
    public class LogisticMixGrayscaleImageBlender : ImageBlender
    {
        int numberOfInputImages;

        float learningConstant;

        float[] weights;

        float[] groundTruthProbabilityCache;

        float[] stretchedPixelValueCache;

        public LogisticMixGrayscaleImageBlender(int numberOfInputImages, float learningConstant = 0.0004f)
        {
            this.numberOfInputImages = numberOfInputImages;
            this.learningConstant = learningConstant;

            weights = new float[numberOfInputImages];

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
            stretchedPixelValueCache = new float[256];
            stretchedPixelValueCache[0] = -LogisticHelper.squashAbsoluteMaximumValue;
            stretchedPixelValueCache[255] = LogisticHelper.squashAbsoluteMaximumValue;
            for (int i = 1; i < 255; i++)
            {
                stretchedPixelValueCache[i] = LogisticHelper.stretch(i / 255.0f);
            }
        }

        public float train(List<ImageDescription> inputImages, ImageDescription inputImageGroundTruth)
        {
            int newSizeX, newSizeY;
            List<ImageDescription> imagesToBlend;
            ImageDescriptionUtil.makeAllImagesSameSize(inputImages, out newSizeX, out newSizeY, out imagesToBlend);

            float entropyLoss = 0;
            for (int i = 0; i < newSizeY; i++)
            {
                for (int j = 0; j < newSizeX; j++)
                {
                    float stretchedProbability = computePerPixelStretchedProbability(imagesToBlend, i, j);
                    float probability = LogisticHelper.squash(stretchedProbability);
                    if (probability < LogisticHelper.probabilityMinValue)
                    {
                        probability = LogisticHelper.probabilityMinValue;
                    }
                    else
                    {
                        if (probability > LogisticHelper.probabilityMaxValue)
                        {
                            probability = LogisticHelper.probabilityMaxValue;
                        }
                    }
                    float groundTruthProbability = groundTruthProbabilityCache[inputImageGroundTruth.gray[i, j]];
                    entropyLoss += LogisticHelper.computeEntropyLoss(probability, groundTruthProbability);

#if useEntropyLoss
                    float loss = groundTruthProbability - probability;
#else
                    float loss = (groundTruthProbability - probability) * probability * (1 - probability);
#endif
                    for (int imageIndex = 0; imageIndex < imagesToBlend.Count; imageIndex++)
                    {
                        byte pixelValue = imagesToBlend[imageIndex].gray[i, j];
                        weights[imageIndex] += learningConstant * stretchedPixelValueCache[pixelValue] * loss;
                    }
                }
            }
            return entropyLoss;
        }

        private float computePerPixelStretchedProbability(List<ImageDescription> imagesToBlend, int i, int j)
        {
            float sum = 0;
            for (int imageIndex = 0; imageIndex < imagesToBlend.Count; imageIndex++)
            {
                byte pixelValue = imagesToBlend[imageIndex].gray[i, j];
                float stretchedProbability = stretchedPixelValueCache[pixelValue];
                sum += stretchedProbability * weights[imageIndex];
            }
            if (sum > LogisticHelper.squashAbsoluteMaximumValue)
            {
                sum = LogisticHelper.squashAbsoluteMaximumValue;
            }
            if (sum < -LogisticHelper.squashAbsoluteMaximumValue)
            {
                sum = -LogisticHelper.squashAbsoluteMaximumValue;
            }
            return sum;
        }

        public ImageDescription blendImages(List<ImageDescription> images)
        {
            int newSizeX, newSizeY;
            List<ImageDescription> imagesToBlend;
            ImageDescriptionUtil.makeAllImagesSameSize(images, out newSizeX, out newSizeY, out imagesToBlend);

            ImageDescription output = new ImageDescription();
            output.sizeX = newSizeX;
            output.sizeY = newSizeY;
            output.grayscale = true;

            byte[,] outputGray = new byte[newSizeY, newSizeX];

            for (int i = 0; i < newSizeY; i++)
            {
                for (int j = 0; j < newSizeX; j++)
                {
                    float stretchedProbability = computePerPixelStretchedProbability(imagesToBlend, i, j);
                    float probability = LogisticHelper.squash(stretchedProbability);
                    if (probability < LogisticHelper.probabilityMinValue)
                    {
                        probability = LogisticHelper.probabilityMinValue;
                    }
                    else
                    {
                        if (probability > LogisticHelper.probabilityMaxValue)
                        {
                            probability = LogisticHelper.probabilityMaxValue;
                        }
                    }
                    outputGray[i, j] = (byte)(probability * 255.0f + 0.5f);
                }
            }
            output.gray = outputGray;
            return output;
        }
    }
}
