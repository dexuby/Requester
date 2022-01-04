using System;
using System.Net;
using System.Threading.Tasks;
using RequesterLib;

namespace RequesterTestApplication
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var requester = new Requester.Builder()
                .Build();

            var test = await requester.GetJsonElementAsync("https://random-data-api.com/api/users/random_user");
            Console.WriteLine(test);
        }
    }
}