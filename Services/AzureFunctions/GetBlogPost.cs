using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SimpleAuthFunctions.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleAuthFunctions
{
    public class GetBlogPost
    {
        [FunctionName("GetBlogPost")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "GetBlogPost/{postId}")] HttpRequest req,
            ILogger log, string postId)
        {
            var _blogExtensions = new BlogExtensions();

            var postBody = string.Empty;

            var mediumKeys = new Dictionary<int, string>(){
                {1,"p"},
                {3, "h1"},
                {4,"img"},
                {6,"blockquote"},
                {9,"li"}
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"https://medium.com/p/{postId}?format=json");
                var result = await client.GetAsync("");
                string resultContent = await result.Content.ReadAsStringAsync();

                var rawContent = resultContent.Substring(resultContent.IndexOf("</x>") + 4);

                JObject content = JObject.Parse(rawContent);

                var paragraphs = content["payload"]["value"]["content"]["bodyModel"]["paragraphs"];

                foreach (var paragraph in paragraphs)
                {
                    var tag = getMediumTag(mediumKeys, (int)paragraph["type"]);

                    var text = _blogExtensions.GetMarkup((string)paragraph["text"], (JArray)paragraph["markups"]);

                    if (tag == "li")
                    {
                        if (postBody.EndsWith("</li>"))
                        {
                            postBody += $"<li>{text}</li>";
                        }
                        else
                        {
                            postBody += $"<ul><li>{text}</li>";
                        }
                    }
                    else
                    {
                        if (postBody.EndsWith("</li>"))
                        {
                            postBody += "</ul>";
                        }

                        if (tag == "img")
                        {
                            var imageUrl = "https://cdn-images-1.medium.com/max/800/" + paragraph["metadata"]["id"];
                            postBody += $"<{tag} src='{imageUrl}'></{tag}>";
                        }
                        else
                        {
                            postBody += $"<{tag}>{text}</{tag}>";
                        }
                    }
                }
            }

            return postBody != null
                ? (ActionResult)new OkObjectResult(postBody)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        public static string getMediumTag (Dictionary<int, string> mediumKeys, int index)
        {
            var tag = "p";

            try
            {
                tag = mediumKeys[index];
            }
            catch {}

            return tag;
        }
    }
}
