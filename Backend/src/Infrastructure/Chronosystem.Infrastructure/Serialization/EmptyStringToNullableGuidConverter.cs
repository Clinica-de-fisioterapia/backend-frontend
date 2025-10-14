// ======================================================================================
// ARQUIVO: EmptyStringToNullableGuidConverter.cs
// CAMADA: Infrastructure / Serialization
// OBJETIVO: Permite desserializar valores de GUID nulos ou vazios ("") corretamente.
// ======================================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chronosystem.Infrastructure.Serialization
{
    /// <summary>
    /// Converte strings vazias ("") para null em propriedades Guid?.
    /// Evita erro de desserialização no model binding.
    /// </summary>
    public class EmptyStringToNullableGuidConverter : JsonConverter<Guid?>
    {
        public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value))
                    return null;

                if (Guid.TryParse(value, out var guid))
                    return guid;

                throw new JsonException($"Invalid Guid format: {value}");
            }

            if (reader.TokenType == JsonTokenType.Null)
                return null;

            throw new JsonException($"Unexpected token {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteStringValue(value.Value.ToString());
            else
                writer.WriteNullValue();
        }
    }
}
