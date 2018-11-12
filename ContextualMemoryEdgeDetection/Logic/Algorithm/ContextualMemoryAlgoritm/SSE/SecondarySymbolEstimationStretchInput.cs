using System;
using System.Collections.Generic;
using System.Text;
using ContextualMemoryEdgeDetection.Logic.Algorithm.Utils;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm.SSE
{
    [Serializable]
    class SecondarySymbolEstimationStretchInput
    {
        int numberOfInterpolationIntervals;

        float learningRate;

        float[,] estimators;

        float intervalWeightFactor;

        public SecondarySymbolEstimationStretchInput(int numberOfContexts, int numberOfInterpolationIntervals, float learningRate)
        {
            this.numberOfInterpolationIntervals = numberOfInterpolationIntervals;
            this.learningRate = learningRate;
            estimators = new float[numberOfContexts, numberOfInterpolationIntervals + 1];
            for (int i = 0; i < numberOfContexts; i++)
            {
                for (int j = 0; j <= numberOfInterpolationIntervals; j++)
                {
                    float p = (float)j / numberOfInterpolationIntervals;
                    estimators[i, j] = LogisticHelper.squash((2 * LogisticHelper.squashAbsoluteMaximumValue) * p - LogisticHelper.squashAbsoluteMaximumValue);
                }
            }

            intervalWeightFactor = numberOfInterpolationIntervals / (2 * LogisticHelper.squashAbsoluteMaximumValue);
        }

        private void computeIntervals(float probability, out int intervalLowIndex, out int intervalHighIndex, out float intervalWeight)
        {
            intervalWeight = intervalWeightFactor * ((LogisticHelper.stretch(probability) + LogisticHelper.squashAbsoluteMaximumValue));
            intervalLowIndex = (int)(intervalWeight);

            if (intervalLowIndex >= numberOfInterpolationIntervals)
            {
                intervalLowIndex = numberOfInterpolationIntervals - 1;
                intervalWeight = 1;
            }
            else
            {
                if (intervalLowIndex <= 0)
                {
                    intervalLowIndex = 0;
                    intervalWeight = 0;
                }
                else
                {
                    intervalWeight -= intervalLowIndex;
                }
            }
            intervalHighIndex = intervalLowIndex + 1;
        }

        public float getEstimation(int contextIndex, float probability)
        {
            int intervalLowIndex;
            int intervalHighIndex;
            float intervalWeight;
            computeIntervals(probability, out intervalLowIndex, out intervalHighIndex, out intervalWeight);
            float intervalLow = estimators[contextIndex, intervalLowIndex];
            float intervalHigh = estimators[contextIndex, intervalHighIndex];

            return (1f - intervalWeight) * intervalLow + intervalWeight * intervalHigh;
        }

        public void update(int contextIndex, float inputProbability, float outcomeProbability)
        {
            int intervalLowIndex;
            int intervalHighIndex;
            float intervalWeight;
            computeIntervals(inputProbability, out intervalLowIndex, out intervalHighIndex, out intervalWeight);
            float intervalLow = estimators[contextIndex, intervalLowIndex];
            float intervalHigh = estimators[contextIndex, intervalHighIndex];

            estimators[contextIndex, intervalLowIndex] = intervalLow - (intervalLow - outcomeProbability) * learningRate * (1f - intervalWeight);
            estimators[contextIndex, intervalHighIndex] = intervalHigh - (intervalHigh - outcomeProbability) * learningRate * intervalWeight;
        }
    }
}
