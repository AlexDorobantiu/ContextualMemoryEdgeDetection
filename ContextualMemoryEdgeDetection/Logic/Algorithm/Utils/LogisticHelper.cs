using System;
using System.Collections.Generic;
using System.Text;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.Utils
{
    public static class LogisticHelper
    {
        public const int numberOfLengthLogValues = 65536;
        public const int stretchTableNumberOfCuantizationValues = 16535;//4095;//400000;//
        public const int squashTableNumberOfCuantizationValues = 16535;
        public const float squashAbsoluteMaximumValue = 10f;
        public const float probabilityMaxValue = 0.99999f;
        public const float probabilityMinValue = 0.00001f;
        public const float squashFactor = squashTableNumberOfCuantizationValues / (2 * squashAbsoluteMaximumValue);

        public static float[] stretchedProbabilitiesTable;
        public static float[] squashedProbabilitiesTable;
        public static float[] lengthLogarithmTable;

        static LogisticHelper()
        {
            stretchedProbabilitiesTable = new float[stretchTableNumberOfCuantizationValues + 1];
            squashedProbabilitiesTable = new float[squashTableNumberOfCuantizationValues + 1];

            stretchedProbabilitiesTable[stretchTableNumberOfCuantizationValues] = squashAbsoluteMaximumValue;
            for (int i = 0; i < stretchTableNumberOfCuantizationValues; i++)
            {
                float stretchedProbability = slowStretch((i + 0.5f) / stretchTableNumberOfCuantizationValues);
                if (stretchedProbability > squashAbsoluteMaximumValue)
                {
                    stretchedProbability = squashAbsoluteMaximumValue;
                }
                else
                {
                    if (stretchedProbability < -squashAbsoluteMaximumValue)
                    {
                        stretchedProbability = -squashAbsoluteMaximumValue;
                    }
                }
                stretchedProbabilitiesTable[i] = stretchedProbability;
            }
            squashedProbabilitiesTable[squashTableNumberOfCuantizationValues] = probabilityMaxValue;
            for (int i = 0; i < squashTableNumberOfCuantizationValues; i++)
            {
                float probability = slowSquash(2 * squashAbsoluteMaximumValue * ((i + 0.5f) / squashTableNumberOfCuantizationValues) - squashAbsoluteMaximumValue);
                if (probability > probabilityMaxValue)
                {
                    probability = probabilityMaxValue;
                }
                else
                {
                    if (probability < probabilityMinValue)
                    {
                        probability = probabilityMinValue;
                    }
                }
                squashedProbabilitiesTable[i] = probability;
            }
            lengthLogarithmTable = new float[numberOfLengthLogValues];
            lengthLogarithmTable[0] = 0; // just a convention to have lengths from 1
            for (int i = 1; i < numberOfLengthLogValues; i++)
            {
                lengthLogarithmTable[i] = (float)Math.Log(i, 2);
            }
        }

        public static float stretch(float p)
        {
            return stretchedProbabilitiesTable[(int)(p * stretchTableNumberOfCuantizationValues)];
        }

        public static float squash(float p)
        {
            int index = (int)((p + squashAbsoluteMaximumValue) * squashFactor);
            if (index > squashTableNumberOfCuantizationValues)
            {
                index = squashTableNumberOfCuantizationValues;
            }
            else
            {
                if (index < 0)
                {
                    index = 0;
                }
            }
            return squashedProbabilitiesTable[index];
        }

        public static float slowStretch(float p)
        {
            return (float)Math.Log(p / (1 - p));
        }

        public static float slowSquash(float p)
        {
            return (float)(1 / (1 + Math.Exp(-p)));
        }

        public static float computeEntropyLoss(float estimatedProbability, float actualProbability)
        {
            return -(float)(actualProbability * Math.Log(estimatedProbability) + (1 - actualProbability) * Math.Log(1 - estimatedProbability));
        }
    }
}
