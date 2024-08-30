using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Gpt4All.Bindings
{
    public unsafe partial struct llmodel_prompt_context
    {
        [NativeTypeName("int32_t *")]
        public int* tokens;

        [NativeTypeName("size_t")]
        public nuint tokens_size;

        [NativeTypeName("int32_t")]
        public int n_past;

        [NativeTypeName("int32_t")]
        public int n_ctx;

        [NativeTypeName("int32_t")]
        public int n_predict;

        [NativeTypeName("int32_t")]
        public int top_k;

        public float top_p;

        public float min_p;

        public float temp;

        [NativeTypeName("int32_t")]
        public int n_batch;

        public float repeat_penalty;

        [NativeTypeName("int32_t")]
        public int repeat_last_n;

        public float context_erase;
    }

    public partial struct llmodel_gpu_device
    {
        [NativeTypeName("const char *")]
        public string backend;

        public int index;

        public int type;

        [NativeTypeName("size_t")]
        public nuint heapSize;

        [NativeTypeName("const char *")]
        public string name;

        [NativeTypeName("const char *")]
        public string vendor;
    }

    [SuppressMessage("Globalization", "CA2101:Especificar marshaling para argumentos de cadeias de caracteres P/Invoke")]
    internal static unsafe partial class NativeMethods
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool LlmodelResponseCallback(int token_id, string response);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool LlmodelPromptCallback(int token_id);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("llmodel_model")]
        [Obsolete("llmodel_model_create is obsolete")]
        public static extern IntPtr llmodel_model_create([NativeTypeName("const char *")] IntPtr model_path);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("llmodel_model")]
        public static extern IntPtr llmodel_model_create2(
            [NativeTypeName("const char *")][MarshalAs(UnmanagedType.LPUTF8Str)] string model_path,
            [NativeTypeName("const char *")][MarshalAs(UnmanagedType.LPUTF8Str)] string backend,
            out IntPtr error);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void llmodel_model_destroy([NativeTypeName("llmodel_model")] IntPtr model);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern nuint llmodel_required_mem(
            [NativeTypeName("llmodel_model")] IntPtr model,
            [NativeTypeName("const char *")] string model_path,
            int n_ctx,
            int ngl);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern bool llmodel_loadModel(
            [NativeTypeName("llmodel_model")] IntPtr model,
            [NativeTypeName("const char *")][MarshalAs(UnmanagedType.LPUTF8Str)] string model_path,
            [NativeTypeName("int32_t")] int n_ctx,
            [NativeTypeName("int32_t")] int ngl);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("bool"), MarshalAs(UnmanagedType.I1)]
        public static extern bool llmodel_isModelLoaded([NativeTypeName("llmodel_model")] IntPtr model);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uint64_t")]
        public static extern ulong llmodel_get_state_size([NativeTypeName("llmodel_model")] IntPtr model);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uint64_t")]
        public static extern ulong llmodel_save_state_data(
            [NativeTypeName("llmodel_model")] IntPtr model,
            [NativeTypeName("uint8_t *")] byte* dest);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("uint64_t")]
        public static extern ulong llmodel_restore_state_data(
            [NativeTypeName("llmodel_model")] IntPtr model,
            [NativeTypeName("const uint8_t *")] byte* src);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl)]
        public static extern void llmodel_prompt(
            [NativeTypeName("llmodel_model")] IntPtr model,
            [NativeTypeName("const char *")] string prompt,
            [NativeTypeName("const char *")] string prompt_template,
            [NativeTypeName("llmodel_prompt_callback")] LlmodelPromptCallback prompt_callback,
            [NativeTypeName("llmodel_response_callback")] LlmodelResponseCallback response_callback,
            [NativeTypeName("bool")] bool allow_context_shift,
            ref llmodel_prompt_context ctx,
            [NativeTypeName("bool")] bool special,
            [NativeTypeName("const char *")] IntPtr fake_reply);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern float* llmodel_embed(
            [NativeTypeName("llmodel_model")] IntPtr model,
            [NativeTypeName("const char **")] IntPtr* texts,
            [NativeTypeName("size_t *")] nuint* embedding_size,
            [NativeTypeName("const char *")] IntPtr prefix,
            int dimensionality,
            [NativeTypeName("size_t *")] nuint* token_count,
            [NativeTypeName("bool")] byte do_mean,
            [NativeTypeName("bool")] byte atlas,
            [NativeTypeName("llmodel_emb_cancel_callback")] delegate* unmanaged[Cdecl]<uint*, uint, IntPtr, byte> cancel_cb,
            [NativeTypeName("const char **")] IntPtr* error);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void llmodel_free_embedding(float* ptr);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void llmodel_setThreadCount(
            [NativeTypeName("llmodel_model")] IntPtr model,
            [NativeTypeName("int32_t")] int n_threads);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("int32_t")]
        public static extern int llmodel_threadCount([NativeTypeName("llmodel_model")] IntPtr model);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void llmodel_set_implementation_search_path([NativeTypeName("const char *")] IntPtr path);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("const char *")]
        public static extern IntPtr llmodel_get_implementation_search_path();

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("struct llmodel_gpu_device *")]
        public static extern IntPtr llmodel_available_gpu_devices(
            [NativeTypeName("size_t")] nuint memoryRequired,
            ref int num_devices);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte llmodel_gpu_init_gpu_device_by_string(
            [NativeTypeName("llmodel_model")] IntPtr model,
            [NativeTypeName("size_t")] nuint memoryRequired,
            [NativeTypeName("const char *")] IntPtr device);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern byte llmodel_gpu_init_gpu_device_by_struct(
            [NativeTypeName("llmodel_model")] IntPtr model,
            [NativeTypeName("const llmodel_gpu_device *")] IntPtr device);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("bool")]
        public static extern bool llmodel_gpu_init_gpu_device_by_int(
            [NativeTypeName("llmodel_model")] IntPtr model,
            int device);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("const char *")]
        public static extern IntPtr llmodel_model_backend_name([NativeTypeName("llmodel_model")] IntPtr model);

        [DllImport("libllmodel", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: NativeTypeName("const char *")]
        public static extern IntPtr llmodel_model_gpu_device_name([NativeTypeName("llmodel_model")] IntPtr model);
    }
}
