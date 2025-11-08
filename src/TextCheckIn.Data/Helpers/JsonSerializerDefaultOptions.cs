using System.Text.Json;

namespace TextCheckIn.Data.Helpers;

public static class JsonSerializerDefaultOptions
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);
}