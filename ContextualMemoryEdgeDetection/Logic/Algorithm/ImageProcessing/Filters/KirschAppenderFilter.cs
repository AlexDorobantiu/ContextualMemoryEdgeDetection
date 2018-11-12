using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters
{
    [Serializable]
    class KirschAppenderFilter : ImageFilter
    {
        private float sigma;
        private int thresholdHigh;
        private int thresholdLow;
        private bool applyNms;

        // delta for eight directions
        private static int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        private static int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

        public KirschAppenderFilter(float sigma = 3f, bool applyNms = false, int thresholdHigh = 16, int thresholdLow = 0)
        {
            this.sigma = sigma;
            this.thresholdHigh = thresholdHigh;
            this.thresholdLow = thresholdLow;
            this.applyNms = applyNms;
        }

        public virtual ImageDescription filter(ImageDescription inputImage)
        {
            inputImage.computeGrayscale();

            int imageSizeX = inputImage.sizeX;
            int imageSizeY = inputImage.sizeY;
            byte[,] inputGray = inputImage.gray;
            byte[,] outputGray = new byte[imageSizeY, imageSizeX];

            float[,] gaussConvolutionMatrix = FilterBankUtil.generateNormalizedGaussConvolutionMatrix(sigma, 7);
            float[,] gaussResult = ImageDescriptionUtil.mirroredMarginConvolution(inputGray, gaussConvolutionMatrix);

            List<float[,]> templates = FilterBankUtil.normalizedKirschTemplates;
            List<float[,]> results = new List<float[,]>(templates.Count);
            foreach (float[,] template in templates)
            {
                results.Add(ImageDescriptionUtil.mirroredMarginConvolution(gaussResult, template));
            }

            float[,] amplitudeResult = new float[imageSizeY, imageSizeX];
            int[,] anglesResult = new int[imageSizeY, imageSizeX];
            for (int i = 0; i < imageSizeY; i++)
            {
                for (int j = 0; j < imageSizeX; j++)
                {
                    int direction = 0;
                    float maxValue = 0;
                    for (int templateIndex = 0; templateIndex < templates.Count; templateIndex++)
                    {
                        float value = results[templateIndex][i, j];
                        if (value > maxValue)
                        {
                            maxValue = value;
                            direction = templateIndex;
                        }
                    }
                    amplitudeResult[i, j] = maxValue;
                    anglesResult[i, j] = direction;
                }
            }

            if (!applyNms)
            {
                for (var i = 0; i < imageSizeY; i++)
                {
                    for (var j = 0; j < imageSizeX; j++)
                    {
                        if (amplitudeResult[i, j] < 255)
                        {
                            outputGray[i, j] = (byte)(amplitudeResult[i, j] + 0.5f);
                        }
                        else
                        {
                            outputGray[i, j] = 255;
                        }
                    }
                }
            }
            else
            {
                float[,] nmsResult = new float[imageSizeY, imageSizeX];
                for (int i = 0; i < imageSizeY; i++)
                {
                    for (int j = 0; j < imageSizeX; j++)
                    {
                        int angle = anglesResult[i, j];
                        if (angle == 2 || angle == 6)
                        {
                            if ((i == 0 || amplitudeResult[i, j] >= amplitudeResult[i - 1, j]) &&
                                (i == imageSizeY - 1 || amplitudeResult[i, j] > amplitudeResult[i + 1, j]))
                            {
                                nmsResult[i, j] = amplitudeResult[i, j];
                            }
                        }
                        else
                        {
                            if (angle == 1 || angle == 5)
                            {
                                if ((i == 0 || j == imageSizeX - 1 || amplitudeResult[i, j] >= amplitudeResult[i - 1, j + 1]) &&
                                    (i == imageSizeY - 1 || j == 0 || amplitudeResult[i, j] > amplitudeResult[i + 1, j - 1]))
                                {
                                    nmsResult[i, j] = amplitudeResult[i, j];
                                }
                            }
                            else
                            {
                                if (angle == 3 || angle == 7)
                                {
                                    if ((i == 0 || j == 0 || amplitudeResult[i, j] >= amplitudeResult[i - 1, j - 1]) &&
                                        (i == imageSizeY - 1 || j == imageSizeX - 1 || amplitudeResult[i, j] > amplitudeResult[i + 1, j + 1]))
                                    {
                                        nmsResult[i, j] = amplitudeResult[i, j];
                                    }
                                }
                                else
                                {
                                    if ((j == 0 || amplitudeResult[i, j] >= amplitudeResult[i, j - 1]) &&
                                        (j == imageSizeX - 1 || amplitudeResult[i, j] > amplitudeResult[i, j + 1]))
                                    {
                                        nmsResult[i, j] = amplitudeResult[i, j];
                                    }
                                }
                            }
                        }
                    }
                }

                float[,] hysteresisResult = new float[imageSizeY, imageSizeX];
                bool[,] retainedPositions = applyHysteresisThreshold(nmsResult, imageSizeX, imageSizeY);

                for (var i = 0; i < imageSizeY; i++)
                {
                    for (var j = 0; j < imageSizeX; j++)
                    {
                        if (retainedPositions[i, j])
                        {
                            hysteresisResult[i, j] = nmsResult[i, j];
                        }
                    }
                }

                for (var i = 0; i < imageSizeY; i++)
                {
                    for (var j = 0; j < imageSizeX; j++)
                    {
                        if (hysteresisResult[i, j] < 255)
                        {
                            outputGray[i, j] = (byte)(hysteresisResult[i, j] + 0.5f);
                        }
                        else
                        {
                            outputGray[i, j] = 255;
                        }
                    }
                }
            }

            ImageDescription outputImage = new ImageDescription();
            outputImage.sizeX = imageSizeX;
            outputImage.sizeY = imageSizeY;
            foreach (ColorChannelEnum colorChannel in Enum.GetValues(typeof(ColorChannelEnum)))
            {
                outputImage.setColorChannel(colorChannel, inputImage.getColorChannel(colorChannel));
            }
            outputImage.setColorChannel(ColorChannelEnum.Kirsch, outputGray);

            return outputImage;
        }

        private bool[,] applyHysteresisThreshold(float[,] nmsResult, int sizeX, int sizeY)
        {
            bool[,] retained = new bool[sizeY, sizeX];
            bool[,] visited = new bool[sizeY, sizeX];
            Queue<Position2d> positionsQueue = new Queue<Position2d>();
            Position2d currentPosition;
            Position2d nextPosition;
            for (var i = 0; i < sizeY; i++)
            {
                for (var j = 0; j < sizeX; j++)
                {
                    if (nmsResult[i, j] >= thresholdHigh)
                    {
                        retained[i, j] = true;

                        currentPosition.x = j;
                        currentPosition.y = i;
                        positionsQueue.Enqueue(currentPosition);

                        while (positionsQueue.Count > 0)
                        {
                            currentPosition = positionsQueue.Dequeue();
                            for (int deltaIndex = 0; deltaIndex < 8; deltaIndex++)
                            {
                                nextPosition.x = currentPosition.x + dx[deltaIndex];
                                nextPosition.y = currentPosition.y + dy[deltaIndex];
                                if (nextPosition.y >= 0 && nextPosition.x >= 0 && nextPosition.y < sizeY && nextPosition.x < sizeX)
                                {
                                    if (!visited[nextPosition.y, nextPosition.x] &&
                                        (nmsResult[nextPosition.y, nextPosition.x] >= thresholdLow && nmsResult[nextPosition.y, nextPosition.x] < thresholdHigh))
                                    {
                                        visited[nextPosition.y, nextPosition.x] = true;
                                        retained[nextPosition.y, nextPosition.x] = true;
                                        positionsQueue.Enqueue(nextPosition);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return retained;
        }

        private struct Position2d
        {
            public int x;
            public int y;
        }
    }
}
