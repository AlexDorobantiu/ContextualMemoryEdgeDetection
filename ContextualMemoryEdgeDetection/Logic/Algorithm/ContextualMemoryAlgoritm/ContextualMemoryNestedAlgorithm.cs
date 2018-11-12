using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm
{
    [Serializable]
    class ContextualMemoryNestedAlgorithm : EdgeDetectionAlgorithm
    {
        private readonly List<ContextualMemoryNestedAlgorithmLayer> layers = new List<ContextualMemoryNestedAlgorithmLayer>();

        private ImageBlender imageBlender;

        public void addLayer(ContextualMemoryNestedAlgorithmLayer layer)
        {
            layers.Add(layer);
        }

        public List<ContextualMemoryNestedAlgorithmLayer> getLayers()
        {
            return layers;
        }

        public ImageBlender getImageBlender()
        {
            return imageBlender;
        }

        public void setImageBlender(ImageBlender imageBlender)
        {
            this.imageBlender = imageBlender;
        }

        public void save(Stream stream)
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(stream, this);
        }

        public float train(ImageDescription inputImage, ImageDescription inputImageGroundTruth)
        {
            throw new NotImplementedException();
        }

        public ImageDescription test(ImageDescription inputImage)
        {            
            List<ImageDescription> computedImages = computeImageForLayers(inputImage, layers.Count);
            ImageDescription outputImage = imageBlender.blendImages(computedImages);
            return outputImage;
        }

        public List<ImageDescription> computeImageForLayers(ImageDescription inputImage, int numberOfLayersToCompute)
        {
            List<ImageDescription> computedImages = new List<ImageDescription>(numberOfLayersToCompute);
            for (int i = 0; i < numberOfLayersToCompute; i++)
            {
                EdgeDetectionAlgorithm algorithm = layers[i].algorithm;
                int layerResizeFactor = layers[i].resizeFactor;

                ImageDescription newInputImage = null;

                ResizeFilter resizeGrayscale = new ResizeFilter(inputImage.sizeX / layerResizeFactor, inputImage.sizeY / layerResizeFactor, ImageDescriptionUtil.grayscaleChannel);
                ResizeFilter resizeColor = new ResizeFilter(inputImage.sizeX / layerResizeFactor, inputImage.sizeY / layerResizeFactor, ImageDescriptionUtil.colorChannels);

                if (layerResizeFactor == 1)
                {
                    newInputImage = inputImage;
                }
                else
                {
                    newInputImage = resizeColor.filter(inputImage);
                }
                if (i > 0)
                {
                    ImageDescription resizedComputed = resizeGrayscale.filter(computedImages[i - 1]);
                    newInputImage.setColorChannel(ColorChannelEnum.Layer, resizedComputed.gray);
                }

                ImageDescription layerOutputImage = algorithm.test(newInputImage);
                computedImages.Add(layerOutputImage);
            }

            return computedImages;
        }
    }
}
