namespace Gpt4All;

public record PredictRequestOptions
{
    public nuint TokensSize { get; init; } = 0;

    public int PastConversationTokensNum { get; init; } = 0;

    public int ContextSize { get; init; } = 0;

    public int TokensToPredict { get; init; } = 4096;

    public int TopK { get; init; } = 40;

    public float TopP { get; init; } = 0.9f;

    public float MinP { get; init; } = 0.0f;

    public float Temperature { get; init; } = 0.1f;

    public int Batches { get; init; } = 8;

    public float RepeatPenalty { get; init; } = 1.2f;

    public int RepeatLastN { get; init; } = 10;

    public float ContextErase { get; init; } = 0.75f;

    public static readonly PredictRequestOptions Defaults = new();
}
