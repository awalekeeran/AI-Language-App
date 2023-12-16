﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Azure;
using Azure.AI.TextAnalytics;

namespace Analyze_Text_App
{
    public class Program
    {
        protected Program() { }

        static void Main(string[] args)
        {
            try
            {
                var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

                // Get config settings from AppSettings
                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .AddJsonFile($"appsettings.json", true, true)
                    .AddJsonFile($"appsettings.development.json", true, true)
                    .AddEnvironmentVariables();

                IConfigurationRoot configuration = builder.Build();

                string aiSvcEndpoint = configuration["AIServicesEndpoint"];
                string aiSvcKey = configuration["AIServicesKey"];

                // Create client using endpoint and key
                AzureKeyCredential credentials = new AzureKeyCredential(aiSvcKey);
                Uri endpoint = new Uri(aiSvcEndpoint);
                TextAnalyticsClient aiClient = new TextAnalyticsClient(endpoint, credentials);

                // Analyze each text file in the reviews folder
                var folderPath = Path.GetFullPath("./reviews");
                DirectoryInfo folder = new DirectoryInfo(folderPath);
                foreach (var file in folder.GetFiles("*.txt"))
                {
                    // Read the file contents
                    Console.WriteLine("\n-------------\n" + file.Name);
                    StreamReader sr = file.OpenText();
                    var text = sr.ReadToEnd();
                    sr.Close();
                    Console.WriteLine("\n" + text);

                    // Get language
                    DetectedLanguage detectedLanguage = aiClient.DetectLanguage(text);
                    System.Console.WriteLine($"\nLanguage: {detectedLanguage.Name}");

                    // Get sentiment
                    DocumentSentiment sentimentAnalysis = aiClient.AnalyzeSentiment(text);
                    System.Console.WriteLine($"\nSentiment: {sentimentAnalysis.Sentiment}");

                    // Get key phrases
                    KeyPhraseCollection phrases = aiClient.ExtractKeyPhrases(text);
                    if (phrases.Count > 0)
                    {
                        System.Console.WriteLine("\nKey Phrases:");
                        foreach (string phrase in phrases)
                        {
                            System.Console.WriteLine($"\t{phrase}");
                        }
                    }

                    // Get entities
                    CategorizedEntityCollection entities = aiClient.RecognizeEntities(text);
                    if (entities.Count > 0)
                    {
                        System.Console.WriteLine("\nEntities:");
                        foreach (CategorizedEntity entity in entities)
                        {
                            System.Console.WriteLine($"\t{entity.Text} ({entity.Category})");
                        }
                    }

                    // Get linked entities
                    LinkedEntityCollection linkedEntities = aiClient.RecognizeLinkedEntities(text);
                    if (linkedEntities.Count > 0)
                    {
                        System.Console.WriteLine("\nLinks: ");
                        foreach (LinkedEntity linkedEntity in linkedEntities)
                        {
                            System.Console.WriteLine($"\t{linkedEntity.Name} ({linkedEntity.Url})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
