using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 5;
        [SerializeField] private Vector2 _minSize = new Vector2(6, 8);
        [SerializeField] private Vector2 _maxSize = new Vector2(12, 20);
        [SerializeField] private List<RectInt> _allRooms = new();

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            _allRooms.Clear();

            await BuildRooms(cancellationToken);
            await BuildCorridors(cancellationToken);
            BuildGround();
        }

        private async UniTask BuildRooms(CancellationToken cancellationToken)
        {
            int roomPlaced = 0;

            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                if (roomPlaced < _maxRooms)
                {
                    RectInt roomRect = new((int)RandomService.Range(0, Grid.Width - _maxSize.x), (int)RandomService.Range(0, Grid.Lenght - _maxSize.y), (int)RandomService.Range(_minSize.x, _maxSize.x), (int)RandomService.Range(_minSize.y, _maxSize.y));
                    if (CanPlaceRoom(roomRect, 2))
                    {
                        roomPlaced++;
                        _allRooms.Add(roomRect);
                        for (int j = roomRect.xMin; j < roomRect.xMax; j++)
                        {
                            for (int k = roomRect.yMin; k < roomRect.yMax; k++)
                            {
                                if (!Grid.TryGetCellByCoordinates(j, k, out var chosenCell))
                                {
                                    continue;
                                }
                                AddTileToCell(chosenCell, ROOM_TILE_NAME, true);
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }
        }

        private async UniTask BuildCorridors(CancellationToken cancellationToken)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            for (int j = 0; j < _allRooms.Count - 1; j++)
            {
                RectInt room1 = _allRooms[j];
                RectInt room2 = _allRooms[j + 1];

                int x1 = room1.x + (room1.width / 2);
                int y1 = room1.y + (room1.height / 2);
                int x2 = room2.x + (room2.width / 2);
                int y2 = room2.y + (room2.height / 2);

                BuildHorizontalCorridor(x1, y1, x2, y2);
                BuildVerticalCorridor(x1, y1, x2, y2);

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }
        }

        private void BuildHorizontalCorridor(int x1, int y1, int x2, int y2)
        {
            for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
            {
                if (Grid.TryGetCellByCoordinates(x, y1, out var chosenCell)/* && !chosenCell.ContainObject*/)
                {
                    AddTileToCell(chosenCell, CORRIDOR_TILE_NAME, true);
                }
            }
        }

        private void BuildVerticalCorridor(int x1, int y1, int x2, int y2)
        {
            for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
            {
                if (Grid.TryGetCellByCoordinates(x2, y, out var chosenCell)/* && !chosenCell.ContainObject*/)
                {
                    AddTileToCell(chosenCell, CORRIDOR_TILE_NAME, true);
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
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        continue;
                    }
                    AddTileToCell(chosenCell, GRASS_TILE_NAME, false);
                }
            }
        }
    }
}
