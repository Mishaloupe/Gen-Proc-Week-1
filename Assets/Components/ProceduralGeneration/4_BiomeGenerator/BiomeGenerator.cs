using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using VTools.Grid;

namespace Components.ProceduralGeneration.BiomeGeneration
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Biome Generator")]
    public class BiomeGenerator : ProceduralGenerationMethod
    {
        // === Trois bruits ===
        private FastNoiseLite _temperatureNoise = new();
        private FastNoiseLite _moistureNoise = new();
        private FastNoiseLite _heightNoise = new();

        [Header("Général")]
        [Range(0, 2)][SerializeField] private float _amplitude = 1f;

        [Header("Température")]
        [SerializeField] private int _SeedTemp = 1234;
        [SerializeField] private float _tempFrequency = 0.02f;
        [SerializeField] private FastNoiseLite.FractalType _tempFractalType = FastNoiseLite.FractalType.FBm;
        [SerializeField] private int _tempOctaves = 3;

        [Header("Humidité")]
        [SerializeField] private int _SeedMoist = 4512;
        [SerializeField] private float _moistureFrequency = 0.05f;
        [SerializeField] private FastNoiseLite.CellularDistanceFunction _moistureDistance = FastNoiseLite.CellularDistanceFunction.Hybrid;
        [SerializeField] private FastNoiseLite.CellularReturnType _moistureReturn = FastNoiseLite.CellularReturnType.CellValue;
        [SerializeField, Range(0, 1)] private float _moistureJitter = 0.7f;

        [Header("Hauteur")]
        [SerializeField] private int _SeedHeight = 9436;
        [SerializeField] private float _heightFrequency = 0.1f;
        [SerializeField] private FastNoiseLite.FractalType _heightFractal = FastNoiseLite.FractalType.FBm;
        [SerializeField] private int _heightOctaves = 4;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            CreateNoise();
            BuildMap();

            await UniTask.NextFrame(cancellationToken);
        }

        private void CreateNoise()
        {
            // === TEMPÉRATURE ===
            _temperatureNoise.SetSeed(_SeedTemp);
            _temperatureNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _temperatureNoise.SetFrequency(_tempFrequency);
            _temperatureNoise.SetFractalType(_tempFractalType);
            _temperatureNoise.SetFractalOctaves(_tempOctaves);
            _temperatureNoise.SetFractalGain(0.5f);
            _temperatureNoise.SetFractalLacunarity(2f);

            // === HUMIDITÉ ===
            _moistureNoise.SetSeed(_SeedMoist);
            _moistureNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
            _moistureNoise.SetFrequency(_moistureFrequency);
            _moistureNoise.SetCellularDistanceFunction(_moistureDistance);
            _moistureNoise.SetCellularReturnType(_moistureReturn);
            _moistureNoise.SetCellularJitter(_moistureJitter);

            // === HAUTEUR ===
            _heightNoise.SetSeed(_SeedHeight);
            _heightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            _heightNoise.SetFrequency(_heightFrequency);
            _heightNoise.SetFractalType(_heightFractal);
            _heightNoise.SetFractalOctaves(_heightOctaves);
            _heightNoise.SetFractalGain(0.5f);
        }

        private void BuildMap()
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var cell, out _))
                        continue;

                    float temp = _temperatureNoise.GetNoise(x, z);
                    float moist = _moistureNoise.GetNoise(x, z);
                    float height = _heightNoise.GetNoise(x, z);

                    string biomeTile = GetBiomeTile(temp, moist, height);
                    AddTileToCell(cell, biomeTile, false);
                }
            }
        }

        private string GetBiomeTile(float temp, float moist, float height)
        {
            if (height > 0.6f) return "SNOW_TILE";
            if (temp > 0.4f && moist < -0.2f) return "DESERT_TILE";
            if (temp > 0.4f && moist > 0.2f) return "JUNGLE_TILE";
            if (temp < -0.4f && moist > 0.2f) return "TAIGA_TILE";
            if (temp < -0.4f && moist < -0.2f) return "TUNDRA_TILE";
            if (moist < -0.3f) return "SAVANNA_TILE";

            return "PLAINS_TILE";
        }

#if UNITY_EDITOR
        // ==================== DEBUG VISUEL ====================

        [SerializeField] private int _debugSize = 128;
        [System.NonSerialized] private Texture2D _temperatureMap;
        [System.NonSerialized] private Texture2D _moistureMap;
        [System.NonSerialized] private Texture2D _heightMap;
        [System.NonSerialized] private Texture2D _biomeMap;

        public void GenerateDebugMapsEditor()
        {
            CreateNoise();

            _temperatureMap = GenerateNoiseTexture(_temperatureNoise, Color.blue, Color.red);
            _moistureMap = GenerateNoiseTexture(_moistureNoise, Color.cyan, Color.mediumPurple);
            _heightMap = GenerateNoiseTexture(_heightNoise, Color.black, Color.white);
            _biomeMap = GenerateBiomeTexture();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            Debug.Log("Debug maps generated!");
        }

        private Texture2D GenerateNoiseTexture(FastNoiseLite noise, Color minColor, Color maxColor)
        {
            Texture2D tex = new Texture2D(_debugSize, _debugSize);
            for (int x = 0; x < _debugSize; x++)
            {
                for (int y = 0; y < _debugSize; y++)
                {
                    float val = (noise.GetNoise(x, y) + 1f) / 2f;
                    tex.SetPixel(x, y, Color.Lerp(minColor, maxColor, val));
                }
            }
            tex.Apply();
            return tex;
        }

        private Texture2D GenerateBiomeTexture()
        {
            Texture2D tex = new Texture2D(_debugSize, _debugSize);
            for (int x = 0; x < _debugSize; x++)
            {
                for (int y = 0; y < _debugSize; y++)
                {
                    float temp = _temperatureNoise.GetNoise(x, y);
                    float moist = _moistureNoise.GetNoise(x, y);
                    float height = _heightNoise.GetNoise(x, y);

                    string biome = GetBiomeTile(temp, moist, height);
                    Color c = GetBiomeColor(biome);
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return tex;
        }

        private Color GetBiomeColor(string biome)
        {
            switch (biome)
            {
                case "DESERT_TILE": return new Color(0.86f, 0.8f, 0.42f);
                case "JUNGLE_TILE": return new Color(0.0f, 0.4f, 0.1f);
                case "PLAINS_TILE": return new Color(0.2f, 0.7f, 0.2f);
                case "SAVANNA_TILE": return new Color(0.55f, 0.35f, 0.1f);
                case "SNOW_TILE": return Color.white;
                case "TAIGA_TILE": return new Color(0.5f, 0.6f, 0.3f);
                case "TUNDRA_TILE": return new Color(0.6f, 0.5f, 0.8f);
                default: return Color.magenta;
            }
        }
        public Texture2D TemperatureMap => _temperatureMap;
        public Texture2D MoistureMap => _moistureMap;
        public Texture2D HeightMap => _heightMap;
        public Texture2D BiomeMap => _biomeMap;
#endif

    }
}
