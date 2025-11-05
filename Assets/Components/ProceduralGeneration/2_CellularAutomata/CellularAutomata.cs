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
    public bool _change;

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

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            CreateNoise();

            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                ChangeCell();

                // Waiting between steps to see the result.
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
                        if (Grid.TryGetCellByCoordinates(i, j, out var chosenCell))
                        {
                            AddTileToCell(chosenCell, GRASS_TILE_NAME, false);
                        }
                    }
                    else
                    {
                        if (Grid.TryGetCellByCoordinates(i, j, out var chosenCell))
                        {
                            AddTileToCell(chosenCell, WATER_TILE_NAME, false);
                        }
                    }
                }
            }
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

            foreach (CATiles tiles in _allCells)
            {
                if (Grid.TryGetCellByCoordinates((int)tiles._coords.x, (int)tiles._coords.y, out var chosenCell))
                {
                    if (tiles._change == true) { AddTileToCell(chosenCell, GRASS_TILE_NAME, true); }
                    else { AddTileToCell(chosenCell, WATER_TILE_NAME, true); }
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
                        if (Grid.TryGetCellByCoordinates(i - 1 + a, j - 1 + b, out var chosenCell1))
                        {
                            if (chosenCell1.GridObject.Template.Name == GRASS_TILE_NAME) { grassTiles++; }
                        }
                    }
                }
            }
                
            if (Grid.TryGetCellByCoordinates(i, j, out var chosenCell))
            {
                if (chosenCell.GridObject.Template.Name == GRASS_TILE_NAME)
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
    }
}




//    if (Grid.TryGetCellByCoordinates(i - 1 + a, j - 1, out var chosenCell1))
//    {
//        if (chosenCell1.GridObject.Template.Name == GRASS_TILE_NAME) { grassTiles++; }
//    }
//}
//for (int a = 0; a < 3; a++)
//{
//    if (Grid.TryGetCellByCoordinates(i - 1 + a, j + 1, out var chosenCell2))
//    {
//        if (chosenCell2.GridObject.Template.Name == GRASS_TILE_NAME) { grassTiles++; }
//    }
//}
//if (Grid.TryGetCellByCoordinates(i + 1, j, out var chosenCell3))
//{
//    if (chosenCell3.GridObject.Template.Name == GRASS_TILE_NAME) { grassTiles++; }
//}
//if (Grid.TryGetCellByCoordinates(i - 1, j, out var chosenCell4))
//{
//    if (chosenCell4.GridObject.Template.Name == GRASS_TILE_NAME) { grassTiles++; }
//}