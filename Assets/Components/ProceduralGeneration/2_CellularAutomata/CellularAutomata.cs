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

public class CATiles
{
    public Vector2 _coords = new();
    public bool _change; //become grass

    public CATiles(Vector2 coords, bool change) {  _coords = coords; _change = change;}
}

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Cellular Automata")]
    public class CellularAutomata : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [Range(0f, 1f)]
        [SerializeField] private float _noiseDensity = 0.5f;
        [Range(0, 8)]
        [SerializeField] private int _nbVoisins = 4;
        [SerializeField] private List<CATiles> _allCells = new();
        [SerializeField] bool isMapTiny;
        [SerializeField] bool ConwaysGameOfLife = false;

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            isMapTiny = Grid.Width * Grid.Lenght < 250 * 250 + 1;
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();
            if (isMapTiny)
            {
                CreateNoiseLittleMap();
            } else
            {
                CreateNoise();
            }
                    
            Debug.Log("Noise created.");

            // Waiting between steps to see the result.
            //await UniTask.NextFrame(cancellationToken);
            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);


            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                if (ConwaysGameOfLife)
                {
                    ChangeCellCGOL();
                } else
                {
                    ChangeCell();
                }
                
                Debug.Log($"Step {i + 1} complete.");

                // Waiting between steps to see the result.
                //await UniTask.NextFrame(cancellationToken);
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }
        }

        private void CreateNoise()
        {
            for (int i = 0; i < Grid.Width; i++)
            {
                for (int j = 0; j < Grid.Lenght; j++)
                {
                    if (RandomService.Chance(_noiseDensity))
                    {
                        if (Grid.TryGetCellByCoordinates(i, j, out var chosenCell, out var chosenCell2))
                        {
                            AddTileToCell(chosenCell, GRASS_TILE_NAME, false, true);
                            AddTileToCell(chosenCell2, WATER_TILE_NAME, false, false);
                        }
                    }
                    else
                    {
                        if (Grid.TryGetCellByCoordinates(i, j, out var chosenCell, out var chosenCell2))
                        {
                            AddTileToCell(chosenCell, WATER_TILE_NAME, false, true);
                            AddTileToCell(chosenCell2, GRASS_TILE_NAME, false, false);
                        }
                    }
                }
            }
        }

        private void CreateNoiseLittleMap()
        {
            for (int i = 0; i < Grid.Width; i++)
            {
                for (int j = 0; j < Grid.Lenght; j++)
                {
                    if (RandomService.Chance(_noiseDensity))
                    {
                        if (Grid.TryGetCellByCoordinates(i, j, out var chosenCell, out var _))
                        {
                            AddTileToCell(chosenCell, GRASS_TILE_NAME, true);
                        }
                    }
                    else
                    {
                        if (Grid.TryGetCellByCoordinates(i, j, out var chosenCell, out var _))
                        {
                            AddTileToCell(chosenCell, WATER_TILE_NAME, true);
                        }
                    }
                }
            }
        }

        private void ChangeCellCGOL()
        {
            _allCells.Clear();
            for (int i = 0; i < Grid.Width; i++)
            {
                for (int j = 0; j < Grid.Lenght; j++)
                {
                    checkSurrondingCGOL(i, j);
                }
            }
            UpdateCells(isMapTiny);
        }

        private void ChangeCell()
        {
            _allCells.Clear();
            for (int i = 0; i < Grid.Width; i++)
            {
                for (int j = 0; j < Grid.Lenght; j++)
                {
                    checkSurronding(i, j);
                }
            }
            UpdateCells(isMapTiny);
        }

        private void UpdateCells(bool isMapTiny)
        {
            if (isMapTiny)
            {
                foreach (CATiles tiles in _allCells)
                {
                    if (Grid.TryGetCellByCoordinates((int)tiles._coords.x, (int)tiles._coords.y, out var chosenCell, out var _))
                    {
                        if (tiles._change == true)
                        {
                            AddTileToCell(chosenCell, GRASS_TILE_NAME, true);
                        }
                        else
                        {
                            AddTileToCell(chosenCell, WATER_TILE_NAME, true);
                        }
                    }
                }
            } else
            {
                foreach (CATiles tiles in _allCells)
                {
                    if (Grid.TryGetCellByCoordinates((int)tiles._coords.x, (int)tiles._coords.y, out var topCell, out var bottomCell))
                    {
                        var bottomTuple = bottomCell._object;
                        var topTuple = topCell._object;

                        bottomCell._object = topTuple;
                        topCell._object = bottomTuple;

                        var parentTransform = GridGenerator.transform;
                        Vector3 bottomTargetPos = bottomCell.GetCenterPosition(new Vector3(parentTransform.position.x, parentTransform.position.y - 10, parentTransform.position.z));
                        Vector3 topTargetPos = topCell.GetCenterPosition(parentTransform.position);

                        bottomTuple.Item2.MoveTo(topTargetPos);
                        topTuple.Item2.MoveTo(bottomTargetPos);

                        bottomTuple.Item1.SetGridData(topCell, Grid);
                        topTuple.Item1.SetGridData(bottomCell, Grid);
                    }
                }
            }
        }

        private void checkSurronding(int i, int j)
        {
            int grassTiles = 0;
            for (int a = 0; a < 3; a++)
            {
                for (int b = 0; b < 3; b++)
                {
                    if (!(a == 1 && b == 1))
                    {
                        if (Grid.TryGetCellByCoordinates(i - 1 + a, j - 1 + b, out var topCell, out var _))
                        {
                            if (topCell.GridObject.Template.Name == GRASS_TILE_NAME) 
                            { 
                                grassTiles++; 
                            }
                        }
                    }
                }
            }
                
            if (Grid.TryGetCellByCoordinates(i, j, out var topCellBis, out var _))
            {
                if (topCellBis.GridObject.Template.Name == GRASS_TILE_NAME)
                {
                    if (grassTiles < _nbVoisins)
                    {
                        _allCells.Add(new CATiles(new Vector2(i, j), false));
                    }
                }
                else
                {
                    if (grassTiles >= _nbVoisins)
                    {
                        _allCells.Add(new CATiles(new Vector2(i, j), true));
                    }
                }
            }
        }

        private void checkSurrondingCGOL(int i, int j)
        {
            int grassTiles = 0;
            for (int a = 0; a < 3; a++)
            {
                for (int b = 0; b < 3; b++)
                {
                    if (!(a == 1 && b == 1))
                    {
                        if (Grid.TryGetCellByCoordinates(i - 1 + a, j - 1 + b, out var topCell, out var _))
                        {
                            if (topCell.GridObject.Template.Name == GRASS_TILE_NAME) 
                            { 
                                grassTiles++; 
                            }
                        }
                    }
                }
            }
                
            if (Grid.TryGetCellByCoordinates(i, j, out var topCellBis, out var _))
            {
                if (topCellBis.GridObject.Template.Name == GRASS_TILE_NAME)
                {
                    if (grassTiles < 2 || grassTiles > 3)
                    {
                        _allCells.Add(new CATiles(new Vector2(i, j), false));
                    }
                }
                else
                {
                    if (grassTiles == 3)
                    {
                        _allCells.Add(new CATiles(new Vector2(i, j), true));
                    }
                }
            }
        }
    }
}