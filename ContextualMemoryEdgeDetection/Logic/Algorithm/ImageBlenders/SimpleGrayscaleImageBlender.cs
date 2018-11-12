using System;
using System.Collections.Generic;
using System.Linq;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageBlenders
{
    [Serializable]
    public class SimpleGrayscaleImageBlender : ImageBlender
    {
        public float train(List<ImageDescription> inputImages, ImageDescription inputImageGroundTruth)
        {
            return 0;
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

            float blendFactor = 1.0f / images.Count();
            byte[,] outputGray = new byte[newSizeY, newSizeX];

            for (int i = 0; i < newSizeY; i++)
            {
                for (int j = 0; j < newSizeX; j++)
                {
                    float sum = 0;
                    for (int imageIndex = 0; imageIndex < images.Count; imageIndex++)
                    {
                        sum += imagesToBlend[imageIndex].gray[i, j];
                    }
                    sum *= blendFactor;
                    outputGray[i, j] = (byte)(sum + 0.5f);
                }
            }
            output.gray = outputGray;
            return output;
        }
    }
}
