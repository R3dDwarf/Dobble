using Unity.Netcode;
using Unity.Collections;
using System;

[Serializable]
public struct CardData : INetworkSerializable, IEquatable<CardData>
{
    public FixedList64Bytes<int> symbols;

    public CardData(int s1, int s2, int s3, int s4, int s5, int s6, int s7, int s8)
    {
        symbols = new FixedList64Bytes<int>
        {
            s1,
            s2,
            s3,
            s4,
            s5,
            s6,
            s7,
            s8
        };
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int count = symbols.Length;
        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            symbols.Clear();
            for (int i = 0; i < count; i++)
            {
                int value = 0;
                serializer.SerializeValue(ref value);
                symbols.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                int value = symbols[i];
                serializer.SerializeValue(ref value);
            }
        }
    }

    public bool Equals(CardData other)
    {
        return symbols.Equals(other.symbols);
    }
    

    public override bool Equals(object obj)
    {
        return obj is CardData other && Equals(other);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        for (int i = 0; i < symbols.Length; i++)
        {
            hash = hash * 31 + symbols[i].GetHashCode();
        }
        return hash;
    }
}
