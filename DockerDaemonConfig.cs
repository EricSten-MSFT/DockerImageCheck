using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DockerImageCheck
{
    class DockerDaemonConfig
    {
        [Newtonsoft.Json.JsonProperty("graph", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Graph { get; set; }

        [Newtonsoft.Json.JsonProperty("icc", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool Icc { get; set; }

        [Newtonsoft.Json.JsonProperty("userland-proxy", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string UserlandProxy { get; set; }

        [Newtonsoft.Json.JsonProperty("max-concurrent-downloads", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int MaxConcurrentDownloads { get; set; }

        [Newtonsoft.Json.JsonProperty("storage-driver", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string StorageDriver { get; set; }

        [Newtonsoft.Json.JsonProperty("bridge", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string bridge { get; set; }

        [Newtonsoft.Json.JsonProperty("insecure-registries", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string[] InsecureRegistries { get; set; }
    }
}
