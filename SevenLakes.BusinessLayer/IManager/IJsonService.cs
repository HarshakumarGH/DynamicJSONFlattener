using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenLakes.BusinessLayer
{
    public interface IJsonService
    {
        public JToken Flatten(string json);
    }
}
