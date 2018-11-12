using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ImageProcessing.Filters
{
    public static class FilterBankUtil
    {
        public static float[,] generateNormalizedGaussConvolutionMatrix(float sigma, int size)
        {
            float[,] gaussConvolutionMatrix = new float[size, size];

            float coef1 = (float)(1 / (2 * Math.PI * sigma * sigma));
            float coef2 = -1 / (2 * sigma * sigma);
            int min = size / 2;
            int max = size / 2 + size % 2;

            float sum = 0;
            for (int y = -min; y < max; y++)
            {
                for (int x = -min; x < max; x++)
                {
                    sum += gaussConvolutionMatrix[y + min, x + min] = coef1 * (float)Math.Exp(coef2 * (x * x + y * y));
                }
            }

            // normalize
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    gaussConvolutionMatrix[y, x] /= sum;
                }
            }
            return gaussConvolutionMatrix;
        }

        public static float[,] sobelX = new float[3, 3] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
        public static float[,] sobelY = new float[3, 3] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };

        public static float[,] normalizedSobelX = new float[3, 3] { { 1 / 4.0f, 2 / 4.0f, 1 / 4.0f }, { 0, 0, 0 }, { -1 / 4.0f, -2 / 4.0f, -1 / 4.0f } };
        public static float[,] normalizedSobelY = new float[3, 3] { { -1 / 4.0f, 0, 1 / 4.0f }, { -2 / 4.0f, 0, 2 / 4.0f }, { -1 / 4.0f, 0, 1 / 4.0f } };

        public static List<float[,]> kirschTemplates = new List<float[,]>
        {
           new float[,] {{ -3, -3, 5 }, { -3, 0, 5 }, { -3, -3, 5 } },
           new float[,] {{ -3, 5, 5 }, { -3, 0, 5 }, { -3, -3, -3 } },
           new float[,] {{ 5, 5, 5 }, { -3, 0, -3 }, { -3, -3, -3 } },
           new float[,] {{ 5, 5, -3 }, { 5, 0, -3 }, { -3, -3, -3 } },
           new float[,] {{ 5, -3, -3 }, { 5, 0, -3 }, { 5, -3, -3 } },
           new float[,] {{ -3, -3, -3 }, { 5, 0, -3 }, { 5, 5, -3 } },
           new float[,] {{ -3, -3, -3 }, { -3, 0, -3 }, { 5, 5, 5 } },
           new float[,] {{ -3, -3, -3 }, { -3, 0, 5 }, { -3, 5, 5 } }
        };

        public static List<float[,]> normalizedKirschTemplates = new List<float[,]>
        {
           new float[,] {{ -3 / 15.0f, -3 / 15.0f, 5 / 15.0f }, { -3 / 15.0f, 0, 5 / 15.0f }, { -3 / 15.0f, -3 / 15.0f, 5 / 15.0f } }, // -
           new float[,] {{ -3 / 15.0f, 5 / 15.0f, 5 / 15.0f }, { -3 / 15.0f, 0, 5 / 15.0f }, { -3 / 15.0f, -3 / 15.0f, -3 / 15.0f } }, // /
           new float[,] {{ 5 / 15.0f, 5 / 15.0f, 5 / 15.0f }, { -3 / 15.0f, 0, -3 / 15.0f }, { -3 / 15.0f, -3 / 15.0f, -3 / 15.0f } }, // |
           new float[,] {{ 5 / 15.0f, 5 / 15.0f, -3 / 15.0f }, { 5 / 15.0f, 0, -3 / 15.0f }, { -3 / 15.0f, -3 / 15.0f, -3 / 15.0f } }, // \
           new float[,] {{ 5 / 15.0f, -3 / 15.0f, -3 / 15.0f }, { 5 / 15.0f, 0, -3 / 15.0f }, { 5 / 15.0f, -3 / 15.0f, -3 / 15.0f } }, // -
           new float[,] {{ -3 / 15.0f, -3 / 15.0f, -3 / 15.0f }, { 5 / 15.0f, 0, -3 / 15.0f }, { 5 / 15.0f, 5 / 15.0f, -3 / 15.0f } }, // /
           new float[,] {{ -3 / 15.0f, -3 / 15.0f, -3 / 15.0f }, { -3 / 15.0f, 0, -3 / 15.0f }, { 5 / 15.0f, 5 / 15.0f, 5 / 15.0f } }, // |
           new float[,] {{ -3 / 15.0f, -3 / 15.0f, -3 / 15.0f }, { -3 / 15.0f, 0, 5 / 15.0f }, { -3 / 15.0f, 5 / 15.0f, 5 / 15.0f } }  // \
        };
    }
}
