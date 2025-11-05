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
    [CreateAssetMenu(menuName = "Procedural Generation Method/Cellular Automata")]
    public class CellularAutomata : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 5;
        [SerializeField] private Vector2 _minSize = new Vector2(6, 8);
        [SerializeField] private Vector2 _maxSize = new Vector2(12, 20);
        [SerializeField] private List<RectInt> _allRooms = new();

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            // Waiting between steps to see the result.
            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }

        
    }
}
