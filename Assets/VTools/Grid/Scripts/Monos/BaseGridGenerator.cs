using UnityEngine;

namespace VTools.Grid
{
    public class BaseGridGenerator : MonoBehaviour
    {
        [Header("Grid Parameters")]
        [SerializeField] private int _gridXValue = 64;
        [SerializeField] private int _gridYValue = 64;
        [SerializeField] private float _cellSize = 1;
        [SerializeField] private Vector3 _startPosition = new(0, 0);
        
        public Grid Grid { get; private set; }
        
        protected virtual void Start()
        {
            GenerateGrid();
        }
        
        public virtual void GenerateGrid()
        {
            if (Grid != null)
            {
                ClearGrid();
            }

            Grid = new Grid(_gridXValue, _gridYValue, _cellSize, _startPosition, false);
        }
        
        public void AddGridObjectToCell(Cell cell, GridObjectTemplate template, bool overrideExistingObjects, bool isTop)
        {
            if (overrideExistingObjects && cell.ContainObject)
            {
                cell.ClearGridObject();
            }

            if (cell.ContainObject && !overrideExistingObjects)
            {
                return;
            }

            if (cell.ContainObject && cell.GridObject.Template.Name == template.Name)
            {
                return;
            }
            
            GridObjectFactory.SpawnOnGridFrom(template, cell, Grid, transform, isTop);
        }

        public void ClearGrid()
        {
            if (Grid.Width * Grid.Lenght > 250 * 250 || Grid.Cells2[Grid.Width * Grid.Lenght - 1] != null)
            {
                for (int i = 0; i < Grid.Cells.Count; i++)
                {
                    Grid.Cells[i].ClearGridObject();
                    Grid.Cells2[i].ClearGridObject();
                }
            } else
            {
                foreach (var cell in Grid.Cells)
                {
                    cell.ClearGridObject();
                }
            }

            Grid = null;
        }
    }
}