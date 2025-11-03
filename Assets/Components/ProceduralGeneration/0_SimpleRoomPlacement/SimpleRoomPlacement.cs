using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [SerializeField] private Vector2 _minSize = new Vector2(12,16);
        [SerializeField] private Vector2 _maxSize = new Vector2(16,24);
        [SerializeField] private List<RectInt> _allRooms = new();
        
        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            int roomPlaced = 0;
            _allRooms.Clear();

            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                if (roomPlaced < _maxRooms)
                {
                    RectInt roomRect = new((int)RandomService.Range(0, Grid.Width - _maxSize.x), (int)RandomService.Range(0, Grid.Lenght - _maxSize.y), (int)RandomService.Range(_minSize.x, _maxSize.x), (int)RandomService.Range(_minSize.y, _maxSize.y));
                    //RectInt roomRect = new RectInt(10,10,10,10);
                    Debug.Log("x : " + roomRect.x + " y : " + roomRect.y + " width : " + roomRect.width + " height : " + roomRect.height);
                    if (CanPlaceRoom(roomRect, 1))
                    {
                        roomPlaced++;
                        _allRooms.Add(roomRect);
                        Debug.Log(roomPlaced);
                        for (int j = roomRect.xMin; j < roomRect.xMax; j++)
                        {
                            for (int k = roomRect.yMin; k < roomRect.yMax; k++)
                            {
                                Debug.Log("x : " + j + " y : " + k);
                                if (!Grid.TryGetCellByCoordinates(j, k, out var chosenCell))
                                {
                                    Debug.LogError($"Unable to get cell on coordinates : ({j}, {k})");
                                    continue;
                                }
                                Debug.Log("Room placée");
                                AddTileToCell(chosenCell, ROOM_TILE_NAME, true);
                            }
                        }
                    }
                } else
                {
                    break;
                }
                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }

            for (int i = 0; i < _maxSteps; i++)
            {


                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                for (int j = 0; j < _allRooms.Count - 1; j++)
                {
                    RectInt room1 = _allRooms[j];
                    RectInt room2 = _allRooms[j+1];

                    for (int x = room1.x; x < room2.x; x++)
                    {
                        if (!Grid.TryGetCellByCoordinates(x, room1.y, out var chosenCell))
                        {
                            Debug.LogError($"Unable to get cell on coordinates : ({x}, {room1.y})");
                            continue;
                        }
                        Debug.Log($"Chemin placé : ({x}, {room1.y})");
                        AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                    }

                    for (int y = room1.y; y < room2.y; y++)
                    {
                        if (!Grid.TryGetCellByCoordinates(room2.x, y, out var chosenCell))
                        {
                            Debug.LogError($"Unable to get cell on coordinates : ({room2.x}, {y})");
                            continue;
                        }
                        Debug.Log($"Chemin placé : ({room2.x}, {y})");
                        AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                    }
                }
                
                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }
            // Final ground building.
            BuildGround();
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
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }
                    
                    AddTileToCell(chosenCell, GRASS_TILE_NAME, false);
                }
            }
        }
    }
}