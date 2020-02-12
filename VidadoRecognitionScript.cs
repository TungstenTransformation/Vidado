using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Kofax.AscentCapture.NetScripting;
using Kofax.Capture.CaptureModule.InteropServices;
using System.Text.Json;

namespace KofaxVidadoRead
{
    public class Vidado__Response
    {
        public float confidence {get; set;}
        public string value { get; set; }
        public string filename { get; set; }
        public string request_id { get; set; }
    }
    public class Vidado__READ : RecognitionScript
    {
        public Vidado__READ() : base()
        {
            this.BatchLoading += Vidado__READ_BatchLoading;
            this.BatchUnloading += Vidado__READ_BatchUnloading;
        }


        void Vidado__READ_BatchLoading(object sender, ref bool ImageFileRequired)
        {
            this.RecognitionPostProcessing += Vidado__READ_RecognitionPostProcessing;
            ImageFileRequired = true;
        }


        void Vidado__READ_BatchUnloading(object sender)
        {
            this.BatchLoading -= Vidado__READ_BatchLoading;
            this.RecognitionPostProcessing -= Vidado__READ_RecognitionPostProcessing;
            this.BatchUnloading -= Vidado__READ_BatchUnloading;

        }


        void Vidado__READ_RecognitionPostProcessing(object sender, PostRecognitionEventArgs e)
        {
            //Get VIDADO READ API token from https://api.vidado.ai/portal
            var apiToken = "";
            var api = "https://api.vidado.ai/read/text";
            
            
            var imageData = File.ReadAllBytes(e.ImangeFile);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", apiToken);
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(new MemoryStream(imageData)), "image", "image.tiff");

                    var result = client.PostAsync(new Uri(api), content).Result;

                    try
                    {
                        result.EnsureSuccessStatusCode();
                        //var responseData = result.Content.ReadAsStringAsync();
                        Task<string> responseData = result.Content.ReadAsStringAsync();
                        var res = responseData.Result;
                        var digitization_response = JsonSerializer.Deserialize<Vidado__Response>(res);
                        e.value = digitization_response.value;
                        e.confidence = Convert.ToInt32(digitization_response.confidence * 100); 

                    }
                    catch (HttpRequestException)
                    {
                        throw;
                    }

                }
            }
        }
    }
}
