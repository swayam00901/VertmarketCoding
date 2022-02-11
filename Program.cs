using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Diagnostics;

namespace MagazineStore
{
    class Program
    {

        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            var url = configuration.GetSection("APISettings").GetValue<String>("baseURL");
            var tokenLoc = configuration.GetSection("APISettings").GetValue<String>("tokenPath");
            var categoriesPath = configuration.GetSection("APISettings").GetValue<String>("categoriesPath");
            var specificCategoryPath = configuration.GetSection("APISettings").GetValue<String>("specificCategoryPath");
            var subscribersPath = configuration.GetSection("APISettings").GetValue<String>("subscribersPath");
            var answerPath = configuration.GetSection("APISettings").GetValue<String>("answerPath");

            do
            {
                using (var httpClient = new HttpClient() { BaseAddress = new Uri(url) })
                {
                    Console.WriteLine("----------------PROCESS STARTED------------------");

                    var token = APICaller<RespFromToken>(httpClient, tokenLoc, HttpMethod.Get);
                    var subscriptions = APICaller<MagSubscribeResp>(httpClient, subscribersPath.Replace("{token}", token.Token), HttpMethod.Get);
                    var categories = APICaller<CategoryResponse>(httpClient, categoriesPath.Replace("{token}", token.Token), HttpMethod.Get);
                    //Make the  api calls in parallel to improve the performance
                    var magazinesInCategory = categories?.Data?.AsParallel()
                    .Select(c => APICaller<MagazineCategoryResponse>(httpClient, specificCategoryPath.Replace("{category}", c)
                    .Replace("{token}", token.Token), HttpMethod.Get)).ToList();
                    //Flattens all the magazines from each category
                    var magazines = magazinesInCategory?.SelectMany(m => m.data);

                    var matchingSub = subscriptions?.Data?.Select(s => new { Sub = s, Magazines = s?.magazineIds?.Select(mid => magazines.FirstOrDefault(m => m.Id == mid)) })
                        .Where(item => categories.Data.TrueForAll(cat => item.Magazines.Any(m => m.Category == cat))).Select(item => item.Sub).ToList();


                    foreach (var item in matchingSub)
                    {
                        Console.WriteLine("______________________________________________________________");
                        Console.WriteLine("Magazine Info ");
                        Console.Write("ID : ");
                        Console.WriteLine(item.id);
                        Console.Write("FirstName : ");
                        Console.WriteLine(item.firstName);
                        Console.Write("LastName : ");
                        Console.WriteLine(item.lastName);
                        Console.Write("Magaine Ids : ");
                        Console.WriteLine(string.Join(", ", item?.magazineIds));
                        Console.WriteLine();
                        Console.WriteLine("______________________________________________________________");
                    }
                    
                    StringContent stringContent = new StringContent(Newtonsoft.Json.JsonConvert.
                    SerializeObject(new MagazineReqBody { Subscribers = matchingSub.Select(x => x.id).ToList() }), Encoding.UTF8, "application/json");


                    var result = APICaller<MagazineResp>(httpClient, answerPath.Replace("{token}", token.Token), HttpMethod.Post, stringContent);
                    if (result.Data.AnswerCorrect)
                    {
                        Console.WriteLine($"TIME TAKEN : {result?.Data?.TotalTime}");
                    }
                    else
                    {
                        Console.WriteLine($"Wrong output.");

                    }
                    Console.WriteLine("End of Process.");
                }

                Console.WriteLine("Please enter any key to rerun or esc to quit.");

            } while ((Console.ReadKey().Key != ConsoleKey.Escape));

            Console.WriteLine("------------------PROCESS ENDS--------------------------");
        }
        /// <summary>
        /// Hellper method to execute an API call and return the result in JSON format.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpClient">HTTPClient to use to make the calls.</param>
        /// <param name="requestURL">Request URL for the resourse</param>
        /// <param name="method">HTTP Method used</param>
        /// <param name="httpContent">Content of Request Body </param>
        /// <returns></returns>
        static T APICaller<T>(HttpClient httpClient, string requestURL, HttpMethod method, HttpContent httpContent = null)
        {
            Debug.WriteLine($"Calling  Endpoint: {requestURL}");
            if (method == HttpMethod.Get)
            {
                var response = httpClient.GetAsync(requestURL).Result;
                response.EnsureSuccessStatusCode();
                var resultText = response.Content.ReadAsStringAsync().Result;
                Debug.WriteLine($"Response text : {resultText}");
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(resultText);
            }
            else if (method == HttpMethod.Post)
            {

                var response = httpClient.PostAsync(requestURL, httpContent).Result;
                response.EnsureSuccessStatusCode();
                var resultText = response.Content.ReadAsStringAsync().Result;

                Debug.WriteLine($"Response text : {resultText}");
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(resultText);


            }
            return default(T);

        }

    }

}
