using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using ContextualMemoryEdgeDetection.Logic.Algorithm.Utils;

namespace ContextualMemoryEdgeDetection.Logic.Algorithm.ContextualMemoryAlgoritm.ContextMap
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BiasContextMapShortTableEntry
    {
        public sbyte match;
        public short value;

        public BiasContextMapShortTableEntry(sbyte match, short value)
        {
            this.match = match;
            this.value = value;
        }
    }

    [Serializable]
    class BiasContextMapShort : ISerializable
    {
        private int numberOfBucketsBits;
        private int numberOfBuckets;
        private int numberOfBucketsMask;
        private int bucketSize;

        [NonSerialized]
        private BiasContextMapShortTableEntry[,] valuesTable;

        private sbyte match;
        private int bucketIndex;
        private int indexInBucket;

        public BiasContextMapShort(int numberOfBucketsBits, int bucketSize)
        {
            this.numberOfBucketsBits = numberOfBucketsBits;
            this.bucketSize = bucketSize;

            numberOfBuckets = 1 << numberOfBucketsBits;
            numberOfBucketsMask = numberOfBuckets - 1;

            valuesTable = new BiasContextMapShortTableEntry[numberOfBuckets, bucketSize];
        }

        public short? getContextValue(int context)
        {
            bucketIndex = context & numberOfBucketsMask;
            match = (sbyte)(context >> numberOfBucketsBits);
            for (int i = 0; i < bucketSize; i++)
            {
                if (valuesTable[bucketIndex, i].match == match)
                {
                    indexInBucket = i;
                    return valuesTable[bucketIndex, i].value;
                }
            }
            indexInBucket = -1;
            return null;
        }

        public void updateLastContextValue(short newValue)
        {
            if (indexInBucket < 0)
            {
                short min = short.MaxValue;
                for (int i = 0; i < bucketSize; i++)
                {
                    short currentValue = valuesTable[bucketIndex, i].value;
                    currentValue ^= (short)(currentValue >> 15); // fast approx abs
                    if (currentValue < min)
                    {
                        min = currentValue;
                        indexInBucket = i;
                    }
                }
                valuesTable[bucketIndex, indexInBucket].match = match;
            }
            valuesTable[bucketIndex, indexInBucket].value = newValue;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("numberOfBucketsBits", numberOfBucketsBits);
            info.AddValue("numberOfBuckets", numberOfBuckets);
            info.AddValue("numberOfBucketsMask", numberOfBucketsMask);
            info.AddValue("bucketSize", bucketSize);

            byte[] rawData = new byte[numberOfBuckets * bucketSize * (sizeof(sbyte) + sizeof(short))];
            int position = 0;
            for (int i = 0; i < numberOfBuckets; i++)
            {
                for (int j = 0; j < bucketSize; j++)
                {
                    rawData[position] = (byte)valuesTable[i, j].match;
                    position += sizeof(sbyte);
                    byte[] value = BitConverter.GetBytes(valuesTable[i, j].value);
                    Array.Copy(value, 0, rawData, position, sizeof(short));
                    position += sizeof(short);
                }
            }
            info.AddValue("valuesTable", rawData);
        }

        public BiasContextMapShort(SerializationInfo info, StreamingContext context)
        {
            numberOfBucketsBits = info.GetInt32("numberOfBucketsBits");
            numberOfBuckets = info.GetInt32("numberOfBuckets");
            numberOfBucketsMask = info.GetInt32("numberOfBucketsMask");
            bucketSize = info.GetInt32("bucketSize");

            byte[] rawData = (byte[])info.GetValue("valuesTable", typeof(byte[]));
            valuesTable = new BiasContextMapShortTableEntry[numberOfBuckets, bucketSize];

            int position = 0;
            for (int i = 0; i < numberOfBuckets; i++)
            {
                for (int j = 0; j < bucketSize; j++)
                {
                    sbyte match = (sbyte)rawData[position];
                    position += sizeof(sbyte);
                    short value = BitConverter.ToInt16(rawData, position);
                    position += sizeof(short);
                    valuesTable[i, j] = new BiasContextMapShortTableEntry(match, value);
                }
            }
        }
    }
}
