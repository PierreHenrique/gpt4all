using Gpt4All.Bindings;
using Gpt4All.Enums;

namespace Gpt4All;

public interface IGpt4AllModelFactory
{
    List<llmodel_gpu_device> GetAllGpus(nuint memoryRequired);
    IGpt4AllModel LoadModel(string modelPath);
    IGpt4AllModel LoadModel(string modelPath, EBackend backend);
}
