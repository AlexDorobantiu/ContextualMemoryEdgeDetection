#define useQuantizedDerivative

using System;
using System.Collections.Generic;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.ImageHandling;
using ContextualMemoryEdgeDetection.Logic.Algorithm.Utils;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm.Contexts
{
    [Serializable]
    public class RaysContextualMemoryContextComputer : AbstractContextualMemoryContextComputer
    {
        private static ISet<ColorChannelEnum> trueColorChannels = new HashSet<ColorChannelEnum>()
        {
            ColorChannelEnum.Red, ColorChannelEnum.Green, ColorChannelEnum.Blue, ColorChannelEnum.Gray
        };

        private int longestRay;
        private int numberOfRays;
        private int maxTableSizeBits;
        private ISet<ColorChannelEnum> selectedColorChannels;

        private RelativePixelInformation[][] relativePixelInformationsForRay;
        private int[][] contextLenghtsForRay;

        const int noBitsMasked = 255;
        const int oneBitMasked = 254;
        const int twoBitsMasked = 252;
        const int threeBitsMasked = 248;

        const int longestContextLength = 33;
        static byte[] byteMaskValues;

        int numberOfContexts;

        List<int> tableBitSizes = new List<int>();

        static RaysContextualMemoryContextComputer()
        {
            byteMaskValues = new byte[longestContextLength];
            // hard quantize
            //byteMaskValues[0] = noBitsMasked;
            //byteMaskValues[1] = oneBitMasked;
            //byteMaskValues[2] = twoBitsMasked;
            //for (int i = 3; i < longestContextLength; i++)
            //{
            //    byteMaskValues[i] = threeBitsMasked;
            //}

            // softer quantize
            //byteMaskValues[0] = noBitsMasked;
            //byteMaskValues[1] = noBitsMasked;
            //byteMaskValues[2] = oneBitMasked;
            //for (int i = 3; i < longestContextLength; i++)
            //{
            //    byteMaskValues[i] = oneBitMasked;
            //}

            // quantize all elements
            for (int i = 0; i < longestContextLength; i++)
            {
                byteMaskValues[i] = twoBitsMasked;
                //byteMaskValues[i] = oneBitMasked;
            }
        }

        public RaysContextualMemoryContextComputer(int longestRay, int numberOfRays, int maxTableSizeBits, ISet<ColorChannelEnum> selectedColorChannels)
        {
            this.longestRay = longestRay;
            this.numberOfRays = numberOfRays;
            this.maxTableSizeBits = maxTableSizeBits;
            this.selectedColorChannels = selectedColorChannels;

            initializeContexts();
        }

        private void initializeContexts()
        {
            List<int> oneChannelTableSizes = new List<int>();

            relativePixelInformationsForRay = new RelativePixelInformation[numberOfRays][];
            contextLenghtsForRay = new int[numberOfRays][];

            RelativePixelInformation relativePixelInformation;
            // current pixel context
            List<RelativePixelInformation> relativePixelsList = new List<RelativePixelInformation>();
            // to avoid duplicates
            List<List<RelativePixelInformation>> relativePixelsLists = new List<List<RelativePixelInformation>>();

            // pixel rays around current pixel
            double[] angles = new double[numberOfRays];
            double baseAngle = 0;
            double angleIncrement = (2 * Math.PI) / numberOfRays;
            for (int i = 0; i < numberOfRays; i++)
            {
                angles[i] = baseAngle;
                baseAngle += angleIncrement;
            }
            for (int i = 0; i < numberOfRays; i++)
            {
                List<int> contextLenghts = new List<int>();
                relativePixelsList = new List<RelativePixelInformation>();
                for (int l = 0; l <= longestRay; l++)
                {
                    double x = l;
                    double y = 0;
                    NumberUtils.rotate2dPointAroundOrigin(ref x, ref y, angles[i]);
                    relativePixelInformation.deltaX = (int)Math.Round(x);
                    relativePixelInformation.deltaY = -(int)Math.Round(y);
                    if (!relativePixelsList.Contains(relativePixelInformation))
                    {
                        relativePixelsList.Add(relativePixelInformation);
                    }

                    List<RelativePixelInformation> duplicateCheckList = new List<RelativePixelInformation>(relativePixelsList);
                    if (l != 0 && l != 1 && l != 2 && l != 4 && l != 8 && l != 16 && l != 32)
                    //if (l > 8 && (l != 16 || l != 32))
                    {
                        continue;
                    }
                    // avoid duplicate contexts
                    if (ListUtils.listNotContained(duplicateCheckList, relativePixelsLists))
                    {
                        int numberOfElements = duplicateCheckList.Count;
                        contextLenghts.Add(numberOfElements);
                        int tableSizeBits = maxTableSizeBits;
                        if (numberOfElements == 1)
                        {
                            tableSizeBits = 8; // one pixel
                        }
                        else
                        {
                            if (numberOfElements == 2)
                            {
                                tableSizeBits = 16; // two pixels
                            }
                            else
                            {
                                if (tableSizeBits == 3 && maxTableSizeBits >= 24)
                                {
                                    tableSizeBits = 24; // three pixels
                                }
                            }
                        }
                        oneChannelTableSizes.Add(tableSizeBits);
#if useQuantizedDerivative
                        //oneChannelTableSizes.Add(tableSizeBits); // added for the derivative part
#endif
                        relativePixelsLists.Add(duplicateCheckList);
                    }
                }
                relativePixelInformationsForRay[i] = relativePixelsList.ToArray();
                contextLenghtsForRay[i] = contextLenghts.ToArray();
            }
            numberOfContexts = 0;
            foreach (ColorChannelEnum colorChannel in selectedColorChannels)
            {
                numberOfContexts += oneChannelTableSizes.Count;
#if useQuantizedDerivative
                if (trueColorChannels.Contains(colorChannel))
                {
                    numberOfContexts += oneChannelTableSizes.Count;
                    foreach (int tableSize in oneChannelTableSizes)
                    {
                        tableBitSizes.Add(tableSize);
                        tableBitSizes.Add(tableSize); // added for the derivative part
                    }
                }
                else
                {
                    tableBitSizes.AddRange(oneChannelTableSizes);
                }
#else
                // for each channel add a repetition
                tableBitSizes.AddRange(oneChannelTableSizes);
#endif
            }
        }

        public override void computeIndexes(ImageDescription inputImage, int positionX, int positionY, int computedIndex, int[,] computedIndexes)
        {
            byte[] byteValues = new byte[longestRay + 1];
            int[] contextHashesForRay = new int[longestRay + 1];

#if useQuantizedDerivative
            byte[] byteDerivativeValues = new byte[longestRay + 1];
            int[] contextDerivativeHashesForRay = new int[longestRay + 1];
#endif

            int currentIndex = 0;
            foreach (ColorChannelEnum selectedColorChannel in selectedColorChannels)
            {
                byte[,] colorChannel = inputImage.getColorChannel(selectedColorChannel);
                for (int ray = 0; ray < numberOfRays; ray++)
                {
                    RelativePixelInformation[] relativePixelInformation = relativePixelInformationsForRay[ray];
                    for (int i = 0; i < relativePixelInformation.Length; i++)
                    {
                        byteValues[i] = ImageDescriptionUtil.getPixelMirrored(colorChannel, positionX + relativePixelInformation[i].deltaX, positionY + relativePixelInformation[i].deltaY);
#if useQuantizedDerivative
                        byteDerivativeValues[i] = byteValues[i];
#endif
                    }
#if useQuantizedDerivative
                    if (trueColorChannels.Contains(selectedColorChannel))
                    {
                        for (int i = relativePixelInformation.Length - 1; i > 0; i--)
                        {
                            byteDerivativeValues[i] = (byte)((byteDerivativeValues[i] - byteDerivativeValues[i - 1] + 256) & 255);
                        }
                    }
#endif
                    NumberUtils.fnvOneAtATimeHash(byteValues, relativePixelInformation.Length, contextHashesForRay);
                    //NumberUtils.fnvOneAtATimeWithMaskHash(byteValues, byteMaskValues, relativePixelInformation.Length, contextHashesForRay);                    
                    //NumberUtils.jenkinsOneAtATimeHash(byteValues, relativePixelInformation.Length, contextHashesForRay);

#if useQuantizedDerivative
                    if (trueColorChannels.Contains(selectedColorChannel))
                    {
                        NumberUtils.fnvOneAtATimeWithMaskHash(byteDerivativeValues, byteMaskValues, relativePixelInformation.Length, contextDerivativeHashesForRay);
                    }
#endif
                    for (int i = 0; i < contextLenghtsForRay[ray].Length; i++)
                    {
                        int contextLength = contextLenghtsForRay[ray][i];
                        int contextIndex;
#if useQuantizedDerivative
                        int contextDerivativeIndex;
#endif
                        if (contextLength == 1)
                        {
                            contextIndex = byteValues[0];
#if useQuantizedDerivative
                            contextDerivativeIndex = byteDerivativeValues[0];
#endif
                        }
                        else
                        {
                            if (contextLength == 2)
                            {
                                contextIndex = (byteValues[0] << 8) + byteValues[1];
#if useQuantizedDerivative
                                contextDerivativeIndex = (byteDerivativeValues[0] << 8) + byteDerivativeValues[1];
#endif
                            }
                            else
                            {
                                if (contextLength == 3 && maxTableSizeBits >= 24)
                                {
                                    contextIndex = (byteValues[0] << 16) + (byteValues[1] << 8) + byteValues[2];
#if useQuantizedDerivative
                                    contextDerivativeIndex = (byteDerivativeValues[0] << 16) + (byteDerivativeValues[1] << 8) + byteDerivativeValues[2];
#endif
                                }
                                else
                                {
                                    contextIndex = contextHashesForRay[contextLength - 1];
#if useQuantizedDerivative
                                    contextDerivativeIndex = contextDerivativeHashesForRay[contextLength - 1];
#endif
                                }
                            }
                        }
                        computedIndexes[computedIndex, currentIndex] = contextIndex;
                        currentIndex += 1;
#if useQuantizedDerivative
                        if (trueColorChannels.Contains(selectedColorChannel))
                        {
                            computedIndexes[computedIndex, currentIndex] = contextDerivativeIndex;
                            currentIndex += 1;
                        }
#endif
                    }
                }
            }
        }

        public override List<int> getTableBitSizes()
        {
            return tableBitSizes;
        }

    }
}
