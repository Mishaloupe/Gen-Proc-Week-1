using Cysharp.Threading.Tasks;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Noise Generator")]
    public class NoiseGenerator : ProceduralGenerationMethod
    {
        private FastNoiseLite _noise = new();

        [Header("Général")]
        [SerializeField] private FastNoiseLite.NoiseType _noiseType = FastNoiseLite.NoiseType.OpenSimplex2;
        [Range(0.01f, 0.1f)]
        [SerializeField] private float _frequency = 0.01f;
        [Range(0, 2)]
        [SerializeField] private float _amplitude = 1f;

        [Header("Fractal")]
        [SerializeField] private FastNoiseLite.FractalType _fractalType = FastNoiseLite.FractalType.None;
        [Range(1, 20)]
        [SerializeField] private int _fractalOctaves = 3;
        [Range(0, 5)]
        [SerializeField] private float _lacunarity = 2f;
        [Range(0, 3)]
        [SerializeField] private float _gain = 0.5f;
        [Range(-20, 20)]
        [SerializeField] private float _wStrength = 0f;
        [Range(-20, 20)]
        [SerializeField] private float _ppStrength = 2f;

        [Header("Cellular")]
        [SerializeField] private FastNoiseLite.CellularDistanceFunction _cellularDistanceFunction = FastNoiseLite.CellularDistanceFunction.EuclideanSq;
        [SerializeField] private FastNoiseLite.CellularReturnType _cellularReturnType = FastNoiseLite.CellularReturnType.Distance;
        [Range(-2, 2)]
        [SerializeField] private float _jitter = 1f;

        [Header("Height")]
        [Range(-1, 1)]
        [SerializeField] private float _water = -0.6f;
        [Range(-1, 1)]
        [SerializeField] private float _sand = -0.3f;
        [Range(-1, 1)]
        [SerializeField] private float _room = 0f;
        [Range(-1, 1)]
        [SerializeField] private float _corridor = 0.3f;
        [Range(-1, 1)]
        [SerializeField] private float _grass = 0.6f;
        [Range(-1, 1)]
        [SerializeField] private float _rock = 1f;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Check for cancellation²
            cancellationToken.ThrowIfCancellationRequested();

            _noise.SetSeed(RandomService.Seed); //7797 pas mal ouais ca marche
            _noise.SetNoiseType(_noiseType);
            _noise.SetFrequency(_frequency);

            _noise.SetFractalType(_fractalType);
            _noise.SetFractalOctaves(_fractalOctaves);
            _noise.SetFractalLacunarity(_lacunarity);
            _noise.SetFractalGain(_gain);
            _noise.SetFractalWeightedStrength(_wStrength);
            _noise.SetFractalPingPongStrength(_ppStrength);

            _noise.SetCellularDistanceFunction(_cellularDistanceFunction);
            _noise.SetCellularReturnType(_cellularReturnType);
            _noise.SetCellularJitter(_jitter);

            BuildMap();

            // Waiting between steps to see the result.
            await UniTask.NextFrame(cancellationToken);
        }

        public void BuildMap()
        {
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell, out var _))
                    {
                        continue;
                    }
                    float noiseValue = GetNoiseData(_noise, x, z);
                    if (noiseValue <= _water)
                    {
                        AddTileToCell(chosenCell, WATER_TILE_NAME, false);
                    } else if (noiseValue <= _sand)
                    {
                        AddTileToCell(chosenCell, SAND_TILE_NAME, false);
                    } else if (noiseValue <= _room)
                    {
                        AddTileToCell(chosenCell, ROOM_TILE_NAME, false);
                    } else if (noiseValue <= _corridor)
                    {
                        AddTileToCell(chosenCell, CORRIDOR_TILE_NAME, false);
                    }else if (noiseValue <= _grass)
                    {
                        AddTileToCell(chosenCell, GRASS_TILE_NAME, false);
                    }else if (noiseValue <= _rock)
                    {
                        AddTileToCell(chosenCell, ROCK_TILE_NAME, false);
                    }
                }
            }
        }

        public float GetNoiseData(FastNoiseLite noise, int x, int z)
        {
            float NoiseAtCoords = noise.GetNoise(x, z) * _amplitude;
            return Mathf.Clamp(NoiseAtCoords, -1, 1);
        }

        public float Get01NoiseData(FastNoiseLite noise, int x, int z)
        {
            return (GetNoiseData(noise, x, z) + 1f) / 2f;
        }
    }
}
