using System.Text.Json;
using System.Text.Json.Serialization;
using ErrorOr;

namespace Cobranca.Cidada.Infrastructure.Serialization;

public sealed class ErrorOrJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert.IsGenericType
           && typeToConvert.GetGenericTypeDefinition() == typeof(ErrorOr<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(ErrorOrJsonConverter<>).MakeGenericType(valueType);

        return (JsonConverter)Activator.CreateInstance(converterType, nonPublic: true)!;
    }

    private sealed class ErrorOrJsonConverter<TValue> : JsonConverter<ErrorOr<TValue>>
    {
        public override ErrorOr<TValue> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            if (!TryGetProperty(root, "isError", out var isErrorProperty))
                throw new JsonException("O valor armazenado não representa um ErrorOr válido.");

            if (isErrorProperty.GetBoolean())
            {
                if (!TryGetProperty(root, "errors", out var errorsProperty))
                    throw new JsonException("Um ErrorOr de erro deve conter a propriedade errors.");

                if (errorsProperty.ValueKind != JsonValueKind.Array || errorsProperty.GetArrayLength() == 0)
                    throw new JsonException("Um ErrorOr de erro deve conter ao menos um erro.");

                var errors = errorsProperty.EnumerateArray().Select(ReadError).ToList();
                return ErrorOr<TValue>.From(errors);
            }

            if (!TryGetProperty(root, "value", out var valueProperty))
                throw new JsonException("Um ErrorOr de sucesso deve conter a propriedade value.");

            var value = valueProperty.Deserialize<TValue>(options);
            return value!;
        }

        public override void Write(
            Utf8JsonWriter writer,
            ErrorOr<TValue> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteBoolean("isError", value.IsError);

            if (value.IsError)
            {
                writer.WritePropertyName("errors");
                writer.WriteStartArray();
                foreach (var error in value.Errors)
                    WriteError(writer, error, options);
                writer.WriteEndArray();
            }
            else
            {
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, value.Value, options);
            }

            writer.WriteEndObject();
        }

        private static Error ReadError(JsonElement element)
        {
            if (!TryGetProperty(element, "numericType", out var numericTypeProperty)
                || !TryGetProperty(element, "code", out var codeProperty)
                || !TryGetProperty(element, "description", out var descriptionProperty))
            {
                throw new JsonException("O valor armazenado contém um erro inválido.");
            }

            Dictionary<string, object>? metadata = null;
            if (TryGetProperty(element, "metadata", out var metadataProperty)
                && metadataProperty.ValueKind == JsonValueKind.Object)
            {
                metadata = metadataProperty
                    .EnumerateObject()
                    .ToDictionary(property => property.Name, property => (object)property.Value.Clone());
            }

            return Error.Custom(
                numericTypeProperty.GetInt32(),
                codeProperty.GetString() ?? string.Empty,
                descriptionProperty.GetString() ?? string.Empty,
                metadata);
        }

        private static void WriteError(Utf8JsonWriter writer, Error error, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("numericType", error.NumericType);
            writer.WriteString("code", error.Code);
            writer.WriteString("description", error.Description);
            writer.WritePropertyName("metadata");
            JsonSerializer.Serialize(writer, error.Metadata, options);
            writer.WriteEndObject();
        }

        private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }
    }
}
