using CodeFex.NetCore.Json;
using CodeFex.NetCore.Json.Dynamic;
using CodeFex.NetCore.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CodeFex.NetCore.Config
{
    public interface IStagedSettings
    {
        T ReadSection<T>(string path) where T : class;
        T ReadSection<T>(string @namespace, string configName) where T : class;
        T ReadSection<T>() where T : class;
        T ReadSection<N, T>() where N : class where T : class;
    }

    public class StagedSettings : IStagedSettings
    {
        protected const string ConfigSource = "[config:source]";
        protected const string ConfigStage = "[config:stage]";

        protected JsonData RootConfig;
        protected JsonSerializerOptions JsonSerializerOptions;

        public string StageName { get; protected set; }

        public StagedSettings(string path, JsonSerializerOptions jsonSerializerOptions = null)
        {
            JsonSerializerOptions = jsonSerializerOptions ?? JsonSerializationDefaults.JsonGenericDefaults;

            var configs = TryLoadRecursive(path);

            foreach (var config in configs)
            {
                // "config:stage"
                var stageConfig = config[ConfigStage] as JsonData;
                if (stageConfig != null)
                {
                    // consume "config:stage"
                    config.Remove(ConfigStage);

                    var stageName = stageConfig[Environment.MachineName];
                    if (stageName != null)
                    {
                        StageName = stageName.ToString();
                    }
                }

                if (config.Count > 0)
                {
                    if (RootConfig == null)
                    {
                        RootConfig = new JsonData().AsDictionary<JsonData>();
                    }

                    RootConfig.CopyFrom(config);
                }
            }
        }

        public StagedSettings(JsonSerializerOptions jsonSerializerOptions = null) : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("{0}.json", "config")), jsonSerializerOptions)
        {
        }

        protected IEnumerable<JsonData> TryLoadSources(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (File.Exists(path))
            {
                var jsonData = JsonSource.ReadFromFile(path, JsonSerializerOptions);

                return jsonData != null && jsonData.Count > 0 ? new[] { jsonData } : null;
            }

            List<JsonData> result = null;

            if (Directory.Exists(path))
            {
                foreach (var configFile in Directory.GetFiles(path, "*.json"))
                {
                    var jsonData = JsonSource.ReadFromFile(configFile, JsonSerializerOptions);
                    if (jsonData != null && jsonData.Count > 0)
                    {
                        if (result == null)
                        {
                            result = new List<JsonData>();
                        }
                        result.Add(jsonData);
                    }
                }
            }

            return result;
        }

        protected IEnumerable<JsonData> TryLoadRecursive(string path, List<JsonData> result = null)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (result == null)
            {
                result = new List<JsonData>();
            }

            var configs = TryLoadSources(path);
            if (configs != null)
            {
                foreach (var configPart in configs)
                {
                    var configSource = configPart[ConfigSource] as JsonData;
                    if (configSource != null)
                    {
                        // consume "config:source"
                        configPart.Remove(ConfigSource);

                        var configPath = configSource["path"];
                        if (configPath != null)
                        {
                            TryLoadRecursive(configPath.ToString(), result);
                        }
                    }

                    // add to result if not empty
                    if (configPart.Count > 0)
                    {
                        result.Add(configPart);
                    }
                }
            }

            return result;
        }

        public T ReadSection<T>(string path) where T : class
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            object result = null;

            if (RootConfig != null)
            {
                // try with stagename config
                if (StageName != null)
                {
                    result = RootConfig[string.Concat(StageName, ":", path)];
                }

                // try without stagename config
                if (result == null)
                {
                    result = RootConfig[path];
                }

                if (result is JsonData)
                {
                    return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(result, JsonSerializerOptions), JsonSerializerOptions);
                }
            }

            return result as T;
        }

        public T ReadSection<T>(string @namespace, string configName) where T : class
        {
            if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));
            if (configName == null) throw new ArgumentNullException(nameof(configName));

            return ReadSection<T>(string.Concat(@namespace, ":", configName));
        }

        public T ReadSection<N, T>() where N : class where T : class
        {
            return ReadSection<T>(typeof(N).FullName, typeof(T).Name);
        }

        public T ReadSection<T>() where T : class
        {
            return ReadSection<T>(typeof(T).FullName);
        }
    }
}
