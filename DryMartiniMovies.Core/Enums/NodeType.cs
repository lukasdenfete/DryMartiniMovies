using System.Text.Json.Serialization;

namespace DryMartiniMovies.Core.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))] public enum NodeType { Movie, Director, Actor }