using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenLakes.BusinessLayer
{
    public class JsonService:IJsonService
    {
        /// <summary>
        /// Method expects valid JSON input and return the flatten JSON
        /// </summary>
        /// <param name="json">Expects Valid JSON</param>
        /// <returns>Returns Flatten JSON</returns>
        public JToken Flatten(string json)
        {
            JToken input = JToken.Parse(json);
            var result = new JArray();
            foreach (var obj in GetFlattenedObjects(input))
            {
                result.Add(obj);
            }
            return result;
        }

        /// <summary>
        /// Method used to call recursively to get the flattaned object
        /// </summary>
        /// <param name="token">Expects Valid JToken</param>
        /// <param name="OtherProperties">Expects IEnumerable props</param>
        /// <returns>Return flattened objects recursively  </returns>
        private IEnumerable<JToken> GetFlattenedObjects(JToken token, IEnumerable<JProperty> OtherProperties = null)
        {
            //if token is JObject
            if (token is JObject obj)
            {
                var children = obj.Children<JProperty>().GroupBy(prop => prop.Value?.Type == JTokenType.Array).ToDictionary(gr => gr.Key);
                if (children.TryGetValue(false, out var directProps))
                    OtherProperties = OtherProperties?.Concat(directProps) ?? directProps; //NB, no checks if any sub collection contains duplicate prop names

                //Look for childrens in the childcollection
                if (children.TryGetValue(true, out var ChildCollections))
                {
                    foreach (var childObj in ChildCollections.SelectMany(childColl => childColl.Values()).SelectMany(childColl => GetFlattenedObjects(childColl, OtherProperties)))
                        yield return childObj;
                }
                else //no (more) child properties, return an object
                {
                    var res = new JObject();
                    if (OtherProperties != null)
                        foreach (var prop in OtherProperties)
                            res.Add(prop);
                    yield return res;
                }
            }
            //if token is Jarray
            else if (token is JArray arr)
            {
                foreach (var co in token.Children().SelectMany(c => GetFlattenedObjects(c, OtherProperties)))
                    yield return co;
            }
            else
            {
                throw new NotImplementedException(token.GetType().Name);
            }
        }
    }
}

