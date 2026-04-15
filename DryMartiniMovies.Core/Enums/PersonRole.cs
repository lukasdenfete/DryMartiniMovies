using System.Text.Json.Serialization;

namespace DryMartiniMovies.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PersonRole { Director, Actor }