using System;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public struct IntArrayWrapper : INetworkSerializable, IEquatable<IntArrayWrapper>
{
    private const int MaxElements = 8; 
    public FixedList32Bytes<int> Indices;

    public bool Equals(IntArrayWrapper other)
    {
        if (Indices.Length != other.Indices.Length)
            return false;

        for (int i = 0; i < Indices.Length; i++)
        {
            if (Indices[i] != other.Indices[i])
                return false;
        }
        return true;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serialize the length first to know how many elements to read/write
        int length = Indices.Length;
        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            Indices.Clear(); // Clear the list if reading
            for (int i = 0; i < length; i++)
            {
                int value = 0;
                serializer.SerializeValue(ref value);
                Indices.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < length; i++)
            {
                int value = Indices[i];
                serializer.SerializeValue(ref value);
            }
        }
    }
}
