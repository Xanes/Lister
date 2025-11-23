using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.OpenAI.Models
{
    public record OpenAiApiResponse(
    string Id,
    string Object,
    long CreatedAt,
    string Status,
    bool Background,
    object? Error,
    object? IncompleteDetails,
    List<Instruction> Instructions,
    int? MaxOutputTokens,
    int? MaxToolCalls,
    string Model,
    List<OpenAiMessage> Output,
    bool ParallelToolCalls,
    string? PreviousResponseId,
    Prompt Prompt,
    string? PromptCacheKey,
    Reasoning? Reasoning,
    string? SafetyIdentifier,
    string ServiceTier,
    bool Store,
    double Temperature,
    TextFormat Text,
    string ToolChoice,
    List<object> Tools,
    double TopLogprobs,
    double TopP,
    string Truncation,
    TokenUsage Usage,
    object? User,
    Dictionary<string, object> Metadata
);
}
