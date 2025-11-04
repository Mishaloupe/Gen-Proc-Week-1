using Cysharp.Threading.Tasks;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/BSP")]
    public class BSP : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 5;
        private List<Node> _listNodes = new();
        private List<RectInt> _listRooms = new();
        [SerializeField] private List<RectInt> _listRectNodes; //debug
        [SerializeField] private Vector2 _minSize = new Vector2(6, 8);
        [SerializeField] private Vector2 _maxSize = new Vector2(12, 20);

        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            //variables
            int it = 0;
            
            _listNodes.Clear();
            _listRectNodes.Clear();
            Node root = new Node(new RectInt(0, 0, Grid.Width, Grid.Lenght));
            _listNodes.Add(root);
            _listRectNodes.Add(root.rect);

            while (it < _maxSteps/* || _listNodes.Count < (_maxRooms*2)-1*/)
            {
                it++;
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                foreach( Node node in _listNodes.ToList())
                {
                    if (node.child1 == null && node.child2 == null/* && _listNodes.Count < (_maxRooms * 2) - 1*/)
                    {
                        Cut(node);
                    }
                }

                //Cut();

                //logique



                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }

            foreach (Node node in _listNodes.ToList())
            {
                if (node.child1 == null || node.child2 == null/* && _listNodes.Count < (_maxRooms * 2) - 1*/)
                {
                    BuildRoom(node);
                }
            }

            BuildCorridors(cancellationToken);
            BuildGround();
        }

        private void BuildRoom(Node node)
        {
            int newRoomX = (int)RandomService.Range(node.rect.x, node.rect.x + node.rect.width - _minSize.x);
            int newRoomY = (int)RandomService.Range(node.rect.y, node.rect.y + node.rect.height - _minSize.y);
            RectInt newRoom = new(newRoomX, newRoomY, (int)RandomService.Range(_minSize.x, node.rect.width - (newRoomX - node.rect.x)), (int)RandomService.Range(_minSize.y, node.rect.height - (newRoomY - node.rect.y)));
            if (CanPlaceRoom(newRoom, 2))
            {
                _listRooms.Add(newRoom);
                for (int j = newRoom.xMin; j < newRoom.xMax; j++)
                {
                    for (int k = newRoom.yMin; k < newRoom.yMax; k++)
                    {
                        if (!Grid.TryGetCellByCoordinates(j, k, out var chosenCell))
                        {
                            continue;
                        }
                        AddTileToCell(chosenCell, ROOM_TILE_NAME, true);
                    }
                }
            }
            _listRooms.Add(newRoom);
        }

        private void Cut(Node parent)
        {
            bool dir = RandomService.Chance(0.5f);

            if (dir == true && parent.rect.width > _minSize.x * 2)
            {
                int w = RandomService.Range((int)_minSize.x, parent.rect.width - (int)_minSize.x);
                Node c1 = new Node(new RectInt(parent.rect.x, parent.rect.y, w, parent.rect.height));
                Node c2 = new Node(new RectInt(parent.rect.x + w, parent.rect.y, parent.rect.width - w, parent.rect.height));
                parent.child1 = c1;
                parent.child2 = c2;
                _listNodes.Add(c1);
                _listNodes.Add(c2);
                _listRectNodes.Add(c1.rect);
                _listRectNodes.Add(c2.rect);
            }
            else if (parent.rect.height > _minSize.y * 2) 
            {
                int h = RandomService.Range((int)_minSize.y, parent.rect.height - (int)_minSize.y);
                Node c1 = new Node(new RectInt(parent.rect.x, parent.rect.y, parent.rect.width, h));
                Node c2 = new Node(new RectInt(parent.rect.x, parent.rect.y + h, parent.rect.width, parent.rect.height - h));
                parent.child1 = c1;
                parent.child2 = c2;
                _listNodes.Add(c1);
                _listNodes.Add(c2);
                _listRectNodes.Add(c1.rect);
                _listRectNodes.Add(c2.rect);
            } 
            else
            {
                Debug.Log("pas la place");
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

        private async UniTask BuildCorridors(CancellationToken cancellationToken)
        {
            // Check for cancellation
            cancellationToken.ThrowIfCancellationRequested();

            for (int j = 0; j < _listRooms.Count - 1; j++)
            {
                RectInt room1 = _listRooms[j];
                RectInt room2 = _listRooms[j + 1];



                BuildHorizontalCorridor(room1, room2);
                BuildVerticalCorridor(room1, room2);

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }
        }

        public bool IsPositionInRoom(Vector2Int position, out int roomIndex)
        {
            for (int i = 0; i < _listRooms.Count; i++)
            {
                if (_listRooms[i].Contains(position))
                {
                    roomIndex = i;
                    return true;
                }
            }
            roomIndex = -1;
            return false;
        }
        private void BuildHorizontalCorridor(RectInt room1, RectInt room2)
        {

            int x1 = room1.x + (room1.width / 2);
            int y1 = room1.y + (room1.height / 2);
            int x2 = room2.x + (room2.width / 2);
            int y2 = room2.y + (room2.height / 2);

            bool hasMoved = false;
            int offset = 0;

            for (int x = Mathf.Min(x1, x2); x <= Mathf.Max(x1, x2); x++)
            {
                int baseY = y1;
                if (Grid.TryGetCellByCoordinates(x, baseY, out var chosenCell)/* && !chosenCell.ContainObject*/)
                {
                    Vector2Int position = new(x, baseY);
                    if (IsPositionInRoom(position, out int roomIndex))
                    {
                        if (_listRooms[roomIndex] == room1 || _listRooms[roomIndex] == room2)
                        {
                            AddTileToCell(chosenCell, ROCK_TILE_NAME, true);
                        }
                        else
                        {
                            while (IsPositionInRoom(position, out int roomIndexTest))
                            {
                                baseY++;
                                position.y = baseY;
                                offset = baseY;
                                if (!hasMoved)
                                {
                                    if (Grid.TryGetCellByCoordinates(x - 1, baseY, out var decalage))
                                    {
                                        AddTileToCell(decalage, SAND_TILE_NAME, true);
                                    }
                                }
                            }
                            hasMoved = true;
                            if (Grid.TryGetCellByCoordinates(x, baseY, out chosenCell))
                            {
                                AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                            }
                            //AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                        }
                    }
                    else
                    {
                        if (hasMoved)
                        {
                            while (offset >= baseY)
                            {
                                if (Grid.TryGetCellByCoordinates(x, offset, out chosenCell))
                                {
                                    AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                                }
                                offset--;
                            }
                            hasMoved = false;
                        }
                        else
                        {
                            AddTileToCell(chosenCell, CORRIDOR_TILE_NAME, true);
                        }
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

            bool hasMoved = false;
            int offset = 0;

            for (int y = Mathf.Min(y1, y2); y <= Mathf.Max(y1, y2); y++)
            {
                int baseX = x2;
                if (Grid.TryGetCellByCoordinates(baseX, y, out var chosenCell)/* && !chosenCell.ContainObject*/)
                {
                    Vector2Int position = new(baseX, y);
                    if (IsPositionInRoom(position, out int roomIndex))
                    {
                        if (_listRooms[roomIndex] == room1 || _listRooms[roomIndex] == room2)
                        {
                            AddTileToCell(chosenCell, ROCK_TILE_NAME, true);
                        }
                        else
                        {
                            while (IsPositionInRoom(position, out int roomIndexTest))
                            {
                                baseX++;
                                position.x = baseX;
                                offset = baseX;
                                if (!hasMoved)
                                {
                                    if (Grid.TryGetCellByCoordinates(baseX, y - 1, out var decalage))
                                    {
                                        AddTileToCell(decalage, SAND_TILE_NAME, true);
                                    }
                                }
                            }
                            hasMoved = true;
                            if (Grid.TryGetCellByCoordinates(baseX, y, out chosenCell))
                            {
                                AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                            }
                            //AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                        }
                    }
                    else
                    {
                        if (hasMoved)
                        {
                            while (offset >= baseX)
                            {
                                if (Grid.TryGetCellByCoordinates(offset, y, out chosenCell))
                                {
                                    AddTileToCell(chosenCell, SAND_TILE_NAME, true);
                                }
                                offset--;
                            }
                            hasMoved = false;
                        }
                        else
                        {
                            AddTileToCell(chosenCell, CORRIDOR_TILE_NAME, true);
                        }
                    }
                }
            }
        }


    }
}
