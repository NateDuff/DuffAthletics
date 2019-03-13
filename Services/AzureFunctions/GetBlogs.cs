using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using SimpleAuthFunctions.Extensions;

namespace SimpleAuthFunctions
{
    public static class GetBlogs
    {
        [FunctionName("GetBlogs")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var _blogExtensions = new BlogExtensions();
            var blogUrl = Environment.GetEnvironmentVariable("BlogUrl", EnvironmentVariableTarget.Process);
            var mediumKeys = new Dictionary<int, string>(){
                {1,"p"},
                {3, "h1"},
                {4,"img"},
                {6,"blockquote"},
                {9,"li"}
            };
            var lists = new Dictionary<string, string>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri($"{blogUrl}/latest?format=json");
                var result = await client.GetAsync("");
                string resultContent = await result.Content.ReadAsStringAsync();

                var rawContent = resultContent.Substring(resultContent.IndexOf("</x>") + 4);

                JObject content = JObject.Parse(rawContent);

                var posts = content["payload"]["posts"];

                foreach (var post in posts)
                {
                    var title = post["title"];
                    var previewImage = post["virtuals"]["previewImage"]["imageId"];
                    previewImage = $"https://cdn-images-1.medium.com/fit/t/640/192/{previewImage}";
                    var paragraphs = post["previewContent"]["bodyModel"]["paragraphs"];
                    var previewBody = string.Empty;

                    foreach (var paragraph in paragraphs)
                    {
                        var tag = getMediumTag(mediumKeys, (int)paragraph["type"]);

                        var text = _blogExtensions.GetMarkup((string)paragraph["text"], (JArray)paragraph["markups"]);

                        if (tag == "li")
                        {
                            if (previewBody.EndsWith("</li>")) {
                                previewBody += $"<li>{text}</li>";
                            }
                            else {
                                previewBody += $"<ul><li>{text}</li>";
                            }
                        }
                        else
                        {
                            if (previewBody.EndsWith("</li>")) {
                                previewBody += "</ul>";
                            }

                            if (tag != "img")
                            {
                                previewBody += $"<{tag}>{text}</{tag}>";
                            }
                            //else
                            //{
                            //    var imageUrl = "https://cdn-images-1.medium.com/max/800/" + paragraph["metadata"]["id"];
                            //    postBody += $"<{tag} src='{imageUrl}'></{tag}>";
                            //}
                        }
                    }

                    var blogPost = $"{{\"title\": \"{title}\", \"previewImage\": \"{previewImage}\", \"previewBody\": \"{previewBody}\"}}";

                    lists.Add((string)post["id"], blogPost);
                }
            }

            var output = string.Empty;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>));

            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, lists);
                output += Encoding.Default.GetString(ms.ToArray());
            }

            return output != null
                ? (ActionResult)new OkObjectResult(output)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        public static string getMediumTag(Dictionary<int, string> mediumKeys, int index)
        {
            var tag = "p";

            try
            {
                tag = mediumKeys[index];
            }
            catch { }

            return tag;
        }
    }
}
