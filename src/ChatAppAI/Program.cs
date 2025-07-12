using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OpenAI;

namespace ChatAppAI;

class Program
{
    const string _key = "";
    const string _model = "gpt-3.5-turbo";
    public static void Main(string[] args)
    {
        ImplementationWithGptLearn().GetAwaiter().GetResult();
        ImplementationWithLearnMicrosoft().GetAwaiter().GetResult();
    }

    // references: https://learn.microsoft.com/en-us/dotnet/ai/quickstarts/build-chat-app?pivots=openai
    private static async Task ImplementationWithLearnMicrosoft()
    {
        // Create the IChatClient
        var chatClient = new OpenAIClient(_key).GetChatClient(_model).AsIChatClient();

        // Start the conversation with context for the AI model
        List<ChatMessage> chatHistory =
            [
                new ChatMessage(ChatRole.System, """
                    You are a friendly hiking enthusiast who helps people discover fun hikes in their area.
                    You introduce yourself when first saying hello.
                    When helping people out, you always ask them for this information
                    to inform the hiking recommendation you provide:

                    1. The location where they would like to hike
                    2. What hiking intensity they are looking for

                    You will then provide three suggestions for nearby hikes that vary in length
                    after you get that information. You will also share an interesting fact about
                    the local nature on the hikes when making a recommendation. At the end of your
                    response, ask if there is anything else you can help with.
                """)
            ];


        while (true)
        {
            // Get user prompt and add to chat history
            Console.WriteLine("Your prompt:");
            chatHistory.Add(new ChatMessage(ChatRole.User, ""));

            // Stream the AI response and add to chat history
            Console.WriteLine("AI Response:");
            var responseBuilder = new StringBuilder();

            await Task.Run(async () =>
            {
                await foreach (ChatResponseUpdate item in chatClient.GetStreamingResponseAsync(chatHistory))
                {
                    Console.Write(item.Text);
                    responseBuilder.Append(item.Text);
                }

                chatHistory.Add(new ChatMessage(ChatRole.Assistant, responseBuilder.ToString()));
                Console.WriteLine();
            });
        }
    }

    private static async Task ImplementationWithGptLearn()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _key);

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = "Olá, tudo bem?" }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine("Resposta da OpenAI:");
        Console.WriteLine(responseString);
    }

}
