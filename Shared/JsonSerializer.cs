using System.Text.Json;
using Confluent.Kafka;

namespace Shared;

public sealed class JsonSerializer<T> : ISerializer<T>, IDeserializer<T>
{
    public byte[] Serialize(T data, SerializationContext context) => JsonSerializer.SerializeToUtf8Bytes(data);

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) => 
        JsonSerializer.Deserialize<T>(data) ?? throw new InvalidOperationException();
}