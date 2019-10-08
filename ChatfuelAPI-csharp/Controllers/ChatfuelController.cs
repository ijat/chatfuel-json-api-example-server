using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace ChatfuelAPI_csharp.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ChatfuelController : ControllerBase
    {

        private IMemoryCache _cache;
        

        public ChatfuelController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }
        
        [HttpGet]
        public async Task<string> Get()
        {
            string first_name;
            if (!_cache.TryGetValue("first_name", out first_name))
                return SendText("Please send your name first :)");
            return SendText($"Your name is {first_name}");
        }

        [HttpPost]
        public async Task<string> Post()
        {
            var json = await GetJsonFromRequestBody();
            string first_name = json["first name"];
            _cache.Set("first_name", first_name);
            return SendText($"Your name has been saved as {_cache.Get("first_name")}");
        }

        [HttpPost]
        [Route("image")]
        public async Task<string> Image()
        {
            var json = await GetJsonFromRequestBody();
            string first_name = json["first name"];
            
            string assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "assets");
            string imageFilePath = assetsPath + "/pp.jpg";
            using (MagickImage image = new MagickImage())
            {
                image.Read(imageFilePath);
                new Drawables()
                    // Draw text on the image
                    .FontPointSize(150)
                    .Font("Lato")
                    .StrokeColor(new MagickColor("white"))
                    .FillColor(MagickColors.Black)
                    .TextAlignment(TextAlignment.Center)
                    .Text(1000, 500, "Hi~ "+ first_name)
                    .Draw(image);
                image.Write(assetsPath + "/pp2.jpg");
            }
            
            return SendImage($"https://{Request.Host.Host}/file/pp2.jpg?" + get_unique_string(4));
        }
        
        private async Task<dynamic> GetJsonFromRequestBody()
        {
            string jsonString;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                jsonString = await reader.ReadToEndAsync();
            }

            dynamic json = JsonConvert.DeserializeObject(jsonString);
            return json;
        }
        
        private string SendText(string text)
        {
            return "{\"messages\":[{\"text\":\"" + text + "\"}]}";
        }

        private string SendImage(string imagePath)
        {
            return "{\"messages\":[{\"attachment\":{\"type\":\"image\",\"payload\":{\"url\":\"" + imagePath + "\"}}}]}";
        }
        
        string get_unique_string(int string_length) {
            using(var rng = new RNGCryptoServiceProvider()) {
                var bit_count = (string_length * 6);
                var byte_count = ((bit_count + 7) / 8); // rounded up
                var bytes = new byte[byte_count];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}