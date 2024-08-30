using Gpt4All;
using Gpt4All.Enums;

var modelFactory = new Gpt4AllModelFactory();
if (args.Length < 2)
{
    Console.WriteLine($"Usage: Gpt4All.Samples <model-path> <prompt>");
    return;
}

var modelPath = args[0];
var prompt = args[1];

using var model = modelFactory.LoadModel(modelPath, EBackend.Auto);

var result = await model.GetStreamingPredictionAsync(
    prompt,
    PredictRequestOptions.Defaults);

var predictions = result.GetPredictionStreamingAsync();
await foreach (var token in predictions)
{
    Console.Write(token);
}
