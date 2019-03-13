using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleAuthFunctions.Extensions
{
    public class BlogExtensions
    {
        public string GetMarkup(string content, JArray markups)
        {
            try
            {
                var type = (int)markups[0]["type"];

                var markup = string.Empty;
                var markupLength = (int)markups[0]["end"] - (int)markups[0]["start"];
                var markupText = content.Substring((int)markups[0]["start"], markupLength);

                switch ((int)markups[0]["type"])
                {
                    case 1:
                        markup = $"<strong>{markupText}</strong>";
                        break;
                    case 2:
                        markup = $"<i>{markupText}</i>";
                        break;
                }

                var preMarkup = content.Substring(0, (int)markups[0]["start"]);
                var postMarkup = content.Substring((int)markups[0]["end"]);

                return $"{preMarkup}{markup}{postMarkup}";
            }
            catch
            {
                return content;
            }
        }

        public static async Task<List<string>> GetBlogIdsAsync()
        {
            var blogUrl = Environment.GetEnvironmentVariable("BlogUrl", EnvironmentVariableTarget.Process);
            List<string> lists = new List<string>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(blogUrl);
                var result = await client.GetAsync("");
                string resultContent = await result.Content.ReadAsStringAsync();

                var rawContent = resultContent.Substring(resultContent.IndexOf("</x>") + 4);

                JObject content = JObject.Parse(rawContent);

                var posts = content["payload"]["posts"];

                foreach (var post in posts)
                {
                    lists.Add((string)post["id"]);
                }

                return lists;
            }
        }
    }
}
