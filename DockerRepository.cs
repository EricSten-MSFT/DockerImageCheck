using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DockerImageCheck
{
    class DockerRepository
    {
        [Newtonsoft.Json.JsonProperty("Repositories", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Dictionary<string, Dictionary<string, string>> Repositories { get; set; }
    }
}
