using Unity.Netcode;
using Unity.Collections;
using System;

[Serializable]
public struct CardData : INetworkSerializable, IEquatable<CardData>
{
    public FixedList64Bytes<int> symbols;
    public FixedList64Bytes<float> scales;
    public FixedList64Bytes<int> rotations;
    public int size;

    // Nové pole
    public int cardId;
    public ulong ownerId;
    public bool isClaimed;

    public CardData(int size, int cardId)
    {
        this.size = size;
        this.symbols = new FixedList64Bytes<int>();
        this.rotations = new FixedList64Bytes<int>();
        this.scales = new FixedList64Bytes<float>();

        // Inicializace nových polí
        this.cardId = cardId;
        this.ownerId = 0;
        this.isClaimed = false;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serializace symbols
        int symbolsCount = symbols.Length;
        serializer.SerializeValue(ref symbolsCount);
        if (serializer.IsReader)
        {
            symbols.Clear();
            for (int i = 0; i < symbolsCount; i++)
            {
                int value = 0;
                serializer.SerializeValue(ref value);
                symbols.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < symbolsCount; i++)
            {
                int value = symbols[i];
                serializer.SerializeValue(ref value);
            }
        }

        // Serializace scales
        int scalesCount = scales.Length;
        serializer.SerializeValue(ref scalesCount);
        if (serializer.IsReader)
        {
            scales.Clear();
            for (int i = 0; i < scalesCount; i++)
            {
                float value = 0;
                serializer.SerializeValue(ref value);
                scales.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < scalesCount; i++)
            {
                float value = scales[i];
                serializer.SerializeValue(ref value);
            }
        }

        // Serializace rotations
        int rotationsCount = rotations.Length;
        serializer.SerializeValue(ref rotationsCount);
        if (serializer.IsReader)
        {
            rotations.Clear();
            for (int i = 0; i < rotationsCount; i++)
            {
                int value = 0;
                serializer.SerializeValue(ref value);
                rotations.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < rotationsCount; i++)
            {
                int value = rotations[i];
                serializer.SerializeValue(ref value);
            }
        }

        // Serializace ostatních polí
        serializer.SerializeValue(ref size);
        serializer.SerializeValue(ref cardId);
        serializer.SerializeValue(ref ownerId);
        serializer.SerializeValue(ref isClaimed);
    }


    public bool Equals(CardData other)
    {
        return symbols.Equals(other.symbols) &&
               size == other.size &&
               cardId == other.cardId &&
               ownerId == other.ownerId &&
               isClaimed == other.isClaimed;
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

        hash = hash * 31 + size.GetHashCode();
        hash = hash * 31 + cardId.GetHashCode();
        hash = hash * 31 + ownerId.GetHashCode();
        hash = hash * 31 + isClaimed.GetHashCode();

        return hash;
    }
}
