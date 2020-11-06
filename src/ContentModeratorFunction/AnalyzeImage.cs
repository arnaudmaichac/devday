using ContentModeratorFunction.Models;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ContentModeratorFunction
{
    public class AnalyzeImage
    {
        private const string SearchTag = "beer";

        private static readonly string ApiRoot = $"https://{Environment.GetEnvironmentVariable("AssetsLocation")}.api.cognitive.microsoft.com";
        private static readonly string ApiUri = $"{ApiRoot}/contentmoderator/moderate/v1.0/ProcessText/Screen?language=eng";
        private static readonly string ApiKey = Environment.GetEnvironmentVariable("MicrosoftVisionApiKey");

        private static readonly List<VisualFeatureTypes?> VisualFeatures = new List<VisualFeatureTypes?> { VisualFeatureTypes.Description };

        private readonly HttpClient httpClient;

        public AnalyzeImage(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory.CreateClient();
        }

        /// Function entry point. Review image and text and set inputDocument.isApproved.
        [FunctionName("ReviewImageAndText")]
        public async Task ReviewImageAndText(
            [QueueTrigger("%queueName%")] ReviewRequestItem queueInput,
            [Blob("input-images/{BlobName}", FileAccess.Read)] Stream image,
            [Table("%tableName%", "Reviews", "{DocumentId}")] BeerReview inputDocument,
            [Table("%tableName%")] CloudTable table)
        {
            bool passesText = await PassesTextModeratorAsync(inputDocument);

            (bool isBottleOfBeer, string caption) = await PassesImageModerationAsync(image);
            inputDocument.IsApproved = isBottleOfBeer && passesText;
            inputDocument.Caption = caption;

            TableOperation operation = TableOperation.Replace(inputDocument);
            table.Execute(operation);
        }

        private async Task<(bool, string)> PassesImageModerationAsync(Stream image)
        {
            var client = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(ApiKey),
                httpClient,
                false)
            {
                Endpoint = ApiRoot
            };

            var result = await client.AnalyzeImageInStreamAsync(image, VisualFeatures);

            string message = result?.Description?.Captions.FirstOrDefault()?.Text;
            bool isBottleOfBeer = result.Description.Tags.Contains(SearchTag) || message.Contains("beer");

            return (isBottleOfBeer, message);
        }

        private async Task<bool> PassesTextModeratorAsync(BeerReview document)
        {
            if (string.IsNullOrWhiteSpace(document.ReviewText))
            {
                return true;
            }

            string content = document.ReviewText;
            StringContent stringContent = new StringContent(content);
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("ContentModerationApiKey"));
            var response = await httpClient.PostAsync(ApiUri, stringContent);

            response.EnsureSuccessStatusCode();

            JObject data = JObject.Parse(await response.Content.ReadAsStringAsync());
            JToken token = data["Terms"];

            //If we have Terms in result it failed the moderation (Terms will have the bad terms)
            return !token.HasValues;
        }
    }
}