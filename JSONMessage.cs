using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GolgedarEngine
{
    public class JSONMessage<T>
    {
        private readonly T data;
        private readonly string json;
        private readonly string typeName;

        public JSONMessage(T data)
        {
            this.data = data;
            typeName = data.GetType().Name;
            json = JsonSerializer.Serialize(this).ToString().Replace(Environment.NewLine, " ");
        }

        public override string ToString() => json;

        public T Data => data;
        [JsonIgnore]
        public string JSON => json;
        public string TypeName => typeName;
    }
}
