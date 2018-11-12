using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters
{
    [Serializable]
    class MedianFilter : ImageFilter
    {
        private ISet<ColorChannelEnum> colorChannelsToFilter { get; set; }

        private int size;

        private byte[] medianValueArray;

        private int halfSize;

        private int medianPosition;

        public MedianFilter(int vicinitySize, ISet<ColorChannelEnum> colorChannelsToFilter)
        {
            size = 2 * vicinitySize + 1;
            halfSize = size / 2;
            medianPosition = (size * size) / 2;
            this.colorChannelsToFilter = colorChannelsToFilter;
            medianValueArray = new byte[size * size];
        }
        public ImageDescription filter(ImageDescription inputImage)
        {
            ImageDescription outputImage = new ImageDescription();
            outputImage.sizeX = inputImage.sizeX;
            outputImage.sizeY = inputImage.sizeY;
            if (colorChannelsToFilter.Count == 1 && colorChannelsToFilter.Contains(ColorChannelEnum.Gray))
            {
                outputImage.grayscale = true;
            }

            foreach (ColorChannelEnum channelEnum in colorChannelsToFilter)
            {
                byte[,] inputChannel = inputImage.getColorChannel(channelEnum);
                byte[,] outputChannel = new byte[outputImage.sizeY, outputImage.sizeX];
                outputImage.setColorChannel(channelEnum, outputChannel);

                for (int y = 0; y < inputImage.sizeY; y++)
                {
                    for (int x = 0; x < inputImage.sizeX; x++)
                    {
                        int index = 0;
                        for (int i = -halfSize; i <= halfSize; i++)
                        {
                            for (int j = -halfSize; j <= halfSize; j++)
                            {
                                medianValueArray[index++] = inputChannel[ImageDescriptionUtil.outsideMirroredPosition(y + i, inputImage.sizeY), ImageDescriptionUtil.outsideMirroredPosition(x + j, inputImage.sizeX)];
                            }
                        }
                        Array.Sort(medianValueArray);
                        outputChannel[y, x] = medianValueArray[medianPosition];
                    }
                }
            }

            return outputImage;
        }
    }
}
