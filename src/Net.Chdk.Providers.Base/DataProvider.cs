using Microsoft.Extensions.Logging;
using Net.Chdk.Json;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Net.Chdk.Providers
{
    public abstract class DataProvider<TData>
    {
        #region Fields

        private ILogger Logger { get; }

        #endregion

        #region Constructor

        protected DataProvider(ILogger logger)
        {
            Logger = logger;

            data = new Lazy<TData>(GetData);
        }

        #endregion

        #region Serializer

        private static readonly Lazy<JsonSerializer> serializer = new Lazy<JsonSerializer>(GetSerializer);

        private static JsonSerializer Serializer => serializer.Value;

        private static JsonSerializer GetSerializer()
        {
            return JsonSerializer.CreateDefault(Settings);
        }

        private static JsonSerializerSettings Settings => new JsonSerializerSettings
        {
            Converters = new[] { new HexStringJsonConverter() }
        };

        #endregion

        #region Data

        private readonly Lazy<TData> data;

        protected TData Data => data.Value;

        private TData GetData()
        {
            var filePath = GetFilePath();
            using (var reader = File.OpenText(filePath))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var data = Serializer.Deserialize<TData>(jsonReader);
                if (LogLevel < LogLevel.None)
                    Logger.Log(LogLevel, default(EventId), data, null, GetFormat);
                return data;
            }
        }

        private string GetFormat(TData data, Exception ex)
        {
            return string.Format(Format, JsonConvert.SerializeObject(data));
        }

        protected abstract string GetFilePath();

        protected virtual LogLevel LogLevel => LogLevel.None;

        protected virtual string Format => null;

        #endregion
    }
}
