﻿using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ocelot.Multiplexer;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Ocelot.Middleware.Multiplexer
{
    public class AbpApiDefinitionAggregator : IDefinedAggregator
    {
        public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
        {
            return await MapAbpApiDefinitionAggregateContentAsync(responses);
        }

        protected virtual async Task<DownstreamResponse> MapAbpApiDefinitionAggregateContentAsync(List<HttpContext> responses)
        {
            JObject responseObject = null;
            JsonMergeSettings mergeSetting = new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union,
                PropertyNameComparison = System.StringComparison.CurrentCultureIgnoreCase
            };
            foreach (var httpResponse in responses)
            {
                var content = await httpResponse.Items.DownstreamResponse().Content.ReadAsStringAsync();
                var contentObject = JsonConvert.DeserializeObject(content);
                if (responseObject == null)
                {
                    responseObject = JObject.FromObject(contentObject);
                }
                else
                {
                    responseObject.Merge(contentObject, mergeSetting);
                }
            }
            var stringContent = new StringContent(responseObject.ToString())
            {
                Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
            };
            stringContent.Headers.Add("_AbpErrorFormat", "true");
            return new DownstreamResponse(stringContent, HttpStatusCode.OK,
                new List<KeyValuePair<string, IEnumerable<string>>>(), "OK");
        }

    }
}
