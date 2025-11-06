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
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 5;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            FastNoiseLite Noise = new(RandomService.Seed);

            // Waiting between steps to see the result.
            await UniTask.NextFrame(cancellationToken);

            BuildGround();
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
                    if (true)
                    {

                    }
                    AddTileToCell(chosenCell, GRASS_TILE_NAME, false, true);
                }
            }
        }

        private void BuildGround()
        {

            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell, out var _))
                    {
                        continue;
                    }
                    AddTileToCell(chosenCell, GRASS_TILE_NAME, false, true);
                }
            }
        }
    }
}
