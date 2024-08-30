using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Gpt4All.Bindings;
using Gpt4All.Enums;
using Gpt4All.LibraryLoader;
using Microsoft.Extensions.Logging;

namespace Gpt4All;

public class Gpt4AllModelFactory : IGpt4AllModelFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;
    private static bool bypassLoading;
    private static string? libraryPath;

    private static readonly Lazy<LoadResult> libraryLoaded = new(() => NativeLibraryLoader.LoadNativeLibrary(libraryPath, bypassLoading), true);

    private readonly EBackend[] _cudas = new[] { EBackend.Amd, EBackend.Gpu, EBackend.Cuda };

    public Gpt4AllModelFactory(string? libraryPath = default, bool bypassLoading = true, ILoggerFactory? loggerFactory = null)
    {
        _loggerFactory = loggerFactory ?? LoggerFactory.Create(builder => builder.AddConsole());
        _logger = _loggerFactory.CreateLogger<Gpt4AllModelFactory>();

        Gpt4AllModelFactory.libraryPath = libraryPath;
        Gpt4AllModelFactory.bypassLoading = bypassLoading;

        if (!libraryLoaded.Value.IsSuccess)
        {
            throw new Exception($"Failed to load native gpt4all library. Error: {libraryLoaded.Value.ErrorMessage}");
        }
    }

    private Gpt4All CreateModel(string modelPath, EBackend backend)
    {
        _logger.LogInformation("Creating model path={ModelPath} with backend={Backend}", modelPath, backend);
        IntPtr error;
        var handle = NativeMethods.llmodel_model_create2(modelPath, backend.ToString().ToLower(), out error);

        if (error != IntPtr.Zero)
        {
            throw new Exception(Marshal.PtrToStringAnsi(error));
        }

        _logger.LogInformation("Model created handle=0x{ModelHandle:X8}", handle);

        if (_cudas.Contains(backend))
        {
            var requiredMemory = NativeMethods.llmodel_required_mem(handle, modelPath, 4096, 0);

            if (requiredMemory <= 0)
                _logger.LogWarning("Unable to get model required memory");

            var allGpuDevices = GetAllGpus(requiredMemory);
            _logger.LogWarning("Inform GPU device index:");

            if (int.TryParse(Console.ReadLine(), out var index))
            {
                if (!NativeMethods.llmodel_gpu_init_gpu_device_by_int(handle, index))
                    _logger.LogError("Unable to init gpu device by index {index}", index);
            }
        }

        _logger.LogInformation("Model loading started");
        var loadedSuccessfully = NativeMethods.llmodel_loadModel(handle, modelPath, 2048, 100);
        _logger.LogInformation("Model loading completed success={ModelLoadSuccess}", loadedSuccessfully);
        if (!loadedSuccessfully)
        {
            throw new Exception($"Failed to load model: '{modelPath}'");
        }

        var logger = _loggerFactory.CreateLogger<LLModel>();
        var underlyingModel = LLModel.Create(handle, logger: logger);

        Debug.Assert(underlyingModel.IsLoaded());

        return new Gpt4All(underlyingModel, logger: logger);
    }

    [SuppressMessage("Performance", "CA1822:Marcar membros como estáticos")]
    public List<llmodel_gpu_device> GetAllGpus(nuint memoryRequired)
    {
        var numDevices = 0;
        var availableGpuDevicesPtr = NativeMethods.llmodel_available_gpu_devices(memoryRequired, ref numDevices);

        if (availableGpuDevicesPtr == IntPtr.Zero)
        {
            _logger.LogWarning("Unable to retrieve available GPU devices");
            return new List<llmodel_gpu_device>();
        }

        var devices = new llmodel_gpu_device[numDevices];
        var sizeOfDeviceStruct = Marshal.SizeOf(typeof(llmodel_gpu_device));

        for (var i = 0; i < numDevices; i++)
        {
            var currentDevicePtr = new IntPtr(availableGpuDevicesPtr.ToInt64() + i * sizeOfDeviceStruct);
            var device = devices[i] = Marshal.PtrToStructure<llmodel_gpu_device>(currentDevicePtr);
            _logger.LogInformation("[{Index}] Backend={Backend} Gpu={name}", device.index, device.backend, device.name);
        }

        return [.. devices];
    }

    public IGpt4AllModel LoadModel(string modelPath) => LoadModel(modelPath, EBackend.Auto);
    public IGpt4AllModel LoadModel(string modelPath, EBackend backend) => CreateModel(modelPath, backend);
}
