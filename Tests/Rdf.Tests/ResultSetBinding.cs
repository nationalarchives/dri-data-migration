using System.Text.Json.Serialization;

namespace Rdf.Tests;

internal class ResultSetBinding
{
    internal ResultSetBinding() { }

    internal ResultSetBinding(string[] vars, (string name, string value)[][] bindings)
    {
        head = new() { vars = vars };
        results = new()
        {
            bindings = bindings.Select(section => section.Select(b => new KeyValuePair<string, Binding>(b.name, new Binding() { value = b.value }))
            .ToDictionary()).ToArray()
        };
    }

    internal ResultSetBinding(string[] vars, (string name, string type, string value)[][] bindings)
    {
        head = new() { vars = vars };
        results = new()
        {
            bindings = bindings.Select(section => section.Select(b => new KeyValuePair<string, Binding>(b.name, new Binding() { type = b.type, value = b.value }))
            .ToDictionary()).ToArray()
        };
    }

    public Head head { get; set; }
    public Results results { get; set; }

    public class Head
    {
        public string[] vars { get; set; }
    }

    public class Results
    {
        public Dictionary<string, Binding>[] bindings { get; set; }
    }

    public class Binding
    {
        public string type { get; set; } = "literal";
        public string value { get; set; }
    }

}
