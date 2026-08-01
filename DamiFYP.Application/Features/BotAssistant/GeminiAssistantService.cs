using DamiFYP.Domain.Models;
using Google.GenAI;
using Google.GenAI.Types;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchemaType = Google.GenAI.Types.Type;

namespace DamiFYP.Application.Features.BotAssistant;

// Calls Gemini directly through the Google.GenAI SDK and runs the
// "model asks for a tool -> we execute it server-side -> feed the result
// back" loop by hand. This keeps the set of callable tools explicit and
// whitelisted (right now: just check_blood_type_availability, which is
// backed by the plain CheckBloodTypeAvailabilityQuery from step 6) instead
// of ever letting the model run arbitrary queries.
public sealed class GeminiAssistantService : IAssistantService
{
    private const int MaxToolIterations = 4;
    private const string CheckBloodTypeAvailabilityToolName = "check_blood_type_availability";

    private readonly IMediator _mediator;
    private readonly GeminiOptions _options;
    private readonly AssistantRateLimiter _rateLimiter;
    private readonly ILogger<GeminiAssistantService> _logger;

    public GeminiAssistantService(IMediator mediator, IOptions<GeminiOptions> options,
        AssistantRateLimiter rateLimiter, ILogger<GeminiAssistantService> logger)
    {
        _mediator = mediator;
        _options = options.Value;
        _rateLimiter = rateLimiter;
        _logger = logger;
    }

    public async Task<string> GetReplyAsync(IReadOnlyList<BotMessage> history, string userMessage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Gemini:ApiKey is not configured.");
        }

        using var client = new Client(apiKey: _options.ApiKey);

        var contents = new List<Content>();
        foreach (var message in history)
        {
            contents.Add(new Content
            {
                Role = message.Role == BotMessageRole.User ? "user" : "model",
                Parts = new List<Part> { new Part { Text = message.Content } }
            });
        }

        contents.Add(new Content
        {
            Role = "user",
            Parts = new List<Part> { new Part { Text = userMessage } }
        });

        var config = new GenerateContentConfig
        {
            SystemInstruction = new Content
            {
                Parts = new List<Part> { new Part { Text = AssistantSystemPrompt.Text } }
            },
            Tools = new List<Tool>
            {
                new Tool
                {
                    FunctionDeclarations = new List<FunctionDeclaration> { BuildCheckBloodTypeAvailabilityDeclaration() }
                }
            }
        };

        for (var iteration = 0; iteration < MaxToolIterations; iteration++)
        {
            // Every loop pass is a real call against the shared Gemini API
            // key's quota, whether it's the first turn or a tool follow-up.
            if (!_rateLimiter.TryAcquire())
            {
                throw new AssistantRateLimitedException();
            }

            var response = await client.Models.GenerateContentAsync(
                model: _options.Model,
                contents: contents,
                config: config,
                cancellationToken: cancellationToken);

            var functionCalls = response.FunctionCalls;
            if (functionCalls == null || functionCalls.Count == 0)
            {
                return string.IsNullOrWhiteSpace(response.Text)
                    ? "Sorry, I wasn't able to come up with a reply."
                    : response.Text;
            }

            var modelTurn = response.Candidates?.FirstOrDefault()?.Content;
            if (modelTurn != null)
            {
                contents.Add(modelTurn);
            }

            foreach (var call in functionCalls)
            {
                var resultText = await ExecuteToolAsync(call, cancellationToken);
                contents.Add(new Content
                {
                    Role = "user",
                    Parts = new List<Part>
                    {
                        new Part
                        {
                            FunctionResponse = new FunctionResponse
                            {
                                Id = call.Id,
                                Name = call.Name,
                                Response = new Dictionary<string, object> { ["result"] = resultText }
                            }
                        }
                    }
                });
            }
        }

        _logger.LogWarning("Assistant hit the max tool-call iteration limit without a final answer");
        return "Sorry, I'm having trouble finishing that request right now — could you try rephrasing it?";
    }

    private async Task<string> ExecuteToolAsync(FunctionCall call, CancellationToken cancellationToken)
    {
        try
        {
            if (call.Name == CheckBloodTypeAvailabilityToolName)
            {
                return await CheckBloodTypeAvailabilityAsync(call.Args, cancellationToken);
            }

            _logger.LogWarning("Assistant requested unknown tool {ToolName}", call.Name);
            return "That tool isn't available.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Assistant tool {ToolName} failed", call.Name);
            return "That lookup failed — tell the user you're unable to check right now.";
        }
    }

    private async Task<string> CheckBloodTypeAvailabilityAsync(Dictionary<string, object>? args,
        CancellationToken cancellationToken)
    {
        var rawValue = args != null && args.TryGetValue("bloodType", out var value) ? value?.ToString() : null;
        if (string.IsNullOrWhiteSpace(rawValue) || !Enum.TryParse<BloodTypeName>(rawValue, out var bloodType))
        {
            return "Unknown or missing blood type argument.";
        }

        var result = await _mediator.Send(new CheckBloodTypeAvailabilityQuery { BloodTypeName = bloodType },
            cancellationToken);

        return $"Blood type {result.BloodTypeName}: {result.UnmatchedDonationPostCount} unmatched donor post(s) " +
               $"totaling {result.TotalPledgedQuantity} pledged unit(s); {result.AvailableDonorCount} registered " +
               "donor(s) of this type are currently marked available.";
    }

    private static FunctionDeclaration BuildCheckBloodTypeAvailabilityDeclaration() => new()
    {
        Name = CheckBloodTypeAvailabilityToolName,
        Description = "Looks up how many unmatched donor posts, pledged units, and available donors exist in " +
                      "Dami right now for a given blood type. Always call this instead of guessing.",
        Parameters = new Schema
        {
            Type = SchemaType.Object,
            Properties = new Dictionary<string, Schema>
            {
                ["bloodType"] = new Schema
                {
                    Type = SchemaType.String,
                    Description = "The blood type to check.",
                    Enum = Enum.GetNames(typeof(BloodTypeName)).ToList()
                }
            },
            Required = new List<string> { "bloodType" }
        }
    };
}
