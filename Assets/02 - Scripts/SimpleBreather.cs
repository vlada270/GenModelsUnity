using UnityEngine;

public class SimpleBreather : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int resolution = 20; // 20x20x20 = 8000 objects (Keep it lower for GameObjects)
    [SerializeField] private float spacing = 1.0f;
    [SerializeField] private GameObject cubePrefab; // Drag a Cube Prefab here
    [SerializeField] private ComputeShader computeShader;
    private int _kernelHandler;

    [Header("Performance")]
    [SerializeField] private bool useGPU = true;
    [Range(1, 1000)][SerializeField] private int mathComplexity = 500; // Artificially increase calculation load

    // Data containers
    private GameObject[] _cubes;
    private Vector3[] _positions;
    private float[] _scaleResults; // Array to receive data from GPU
    private int _count;

    // GPU Buffers
    private ComputeBuffer _positionBuffer;
    private ComputeBuffer _resultBuffer;

    void Start()
    {
        _count = resolution * resolution * resolution;
        _cubes = new GameObject[_count];
        _positions = new Vector3[_count];
        _scaleResults = new float[_count];

        int index = 0;
        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    Vector3 pos = new Vector3(
                        x * spacing,
                        y * spacing,
                        z * spacing);

                    _positions[index] = pos;
                    _cubes[index] = Instantiate(cubePrefab, pos, Quaternion.identity, transform);
                    index++;
                }
            }
        }

        // Init GPU Buffers
        _kernelHandler = computeShader.FindKernel("CSMain");

        _positionBuffer = new ComputeBuffer(_count, sizeof(float) * 3);
        _resultBuffer = new ComputeBuffer(_count, sizeof(float));

        _positionBuffer.SetData(_positions);

        // Pass constant buffer to shader (Positions never change in this demo)
        computeShader.SetBuffer(_kernelHandler, "Positions", _positionBuffer);
        computeShader.SetBuffer(_kernelHandler, "ResultScales", _resultBuffer);
        computeShader.SetInt("Resolution", resolution);
    }

    void Update()
    {
        float time = Time.time;

        if (useGPU)
        {
            // --- GPU Mode ---

            // Set constants
            computeShader.SetFloat("Time", time);
            computeShader.SetInt("Complexity", mathComplexity);

            // Calculate on GPU
            int threadGroupX = Mathf.CeilToInt(_count / 64f); // 64 = numthreads x
            computeShader.Dispatch(_kernelHandler, threadGroupX, 1, 1);

            // Read back to CPU (The expensive part!)
            _resultBuffer.GetData(_scaleResults);

            // Apply to GameObjects
            for (int i = 0; i < _count; i++)
            {
                float s = _scaleResults[i];
                _cubes[i].transform.localScale = Vector3.one * (0.3f + s); // add a base size
            }
        }
        else
        {
            // --- CPU Mode ---
            for (int i = 0; i < _count; i++)
            {
                float dist = Vector3.Distance(_positions[i], Vector3.zero);
                float v = Mathf.Sin(time + dist);

                for (int j = 0; j < mathComplexity; j++)
                {
                    v = Mathf.Sin(v + dist * 0.1f);
                }

                float s = v * 0.5f + 0.5f;
                _cubes[i].transform.localScale = Vector3.one * (0.3f + s);
            }
        }
    }

    void OnDestroy()
    {
        if (_positionBuffer != null) _positionBuffer.Release();
        if (_resultBuffer != null) _resultBuffer.Release();
    }
}
