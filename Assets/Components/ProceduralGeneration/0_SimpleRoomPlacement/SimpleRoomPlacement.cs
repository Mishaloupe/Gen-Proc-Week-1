using Cysharp.Threading.Tasks;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
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

                

                BuildHorizontalCorridor(room1, room2);
                BuildVerticalCorridor(room1, room2);

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }
        }

        private void BuildHorizontalCorridor(RectInt room1, RectInt room2)
        {

            int x1 = room1.x + (room1.width / 2);
            int y1 = room1.y + (room1.height / 2);
            int x2 = room2.x + (room2.width / 2);
            int y2 = room2.y + (room2.height / 2);

            for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
            {
                int baseY = y1;
                if (Grid.TryGetCellByCoordinates(x, baseY, out var chosenCell)/* && !chosenCell.ContainObject*/)
                {
                    Vector2Int position = new(x, baseY);
                    if (IsPositionInRoom(position, out int roomIndex))
                    {
                        if (_allRooms[roomIndex] == room1 || _allRooms[roomIndex] == room2)
                        {
                            AddTileToCell(chosenCell, ROCK_TILE_NAME, true);
                        }
                        else
                        {
                            while (IsPositionInRoom(position, out int roomIndexTest))
                            {
                                baseY++;
                                position.y = baseY;
                            }
                            if (Grid.TryGetCellByCoordinates(x, baseY, out chosenCell))
                            {
                                AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                            }
                            //AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                        }
                    }
                    else
                    {
                        AddTileToCell(chosenCell, CORRIDOR_TILE_NAME, true);
                    }
                }
            }
        }

        private void BuildVerticalCorridor(RectInt room1, RectInt room2)
        {

            int x1 = room1.x + (room1.width / 2);
            int y1 = room1.y + (room1.height / 2);
            int x2 = room2.x + (room2.width / 2);
            int y2 = room2.y + (room2.height / 2);

            for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
            {
                int baseX = x2;
                if (Grid.TryGetCellByCoordinates(baseX, y, out var chosenCell)/* && !chosenCell.ContainObject*/)
                {
                    Vector2Int position = new(baseX, y);
                    if (IsPositionInRoom(position, out int roomIndex))
                    {
                        if (_allRooms[roomIndex] == room1 ||  _allRooms[roomIndex] == room2)
                        {
                            AddTileToCell(chosenCell, ROCK_TILE_NAME, true);
                        } else
                        {
                            while (IsPositionInRoom(position, out int roomIndexTest))
                            {
                                baseX++;
                                position.x = baseX;
                            }
                            if (Grid.TryGetCellByCoordinates(baseX, y, out chosenCell)) {
                                AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                            }
                            //AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                        }
                    }
                    else
                    {
                        AddTileToCell(chosenCell, CORRIDOR_TILE_NAME, true);
                    }
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

        public bool IsPositionInRoom(Vector2Int position, out int roomIndex)
        {
            for (int i = 0; i < _allRooms.Count; i++)
            {
                if (_allRooms[i].Contains(position))
                {
                    roomIndex = i;
                    return true;
                }
            }
            roomIndex = -1;
            return false;
        }
    }
}
