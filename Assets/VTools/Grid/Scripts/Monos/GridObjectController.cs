using UnityEngine;
using VTools.Utility;

namespace VTools.Grid
{
    public class GridObjectController : MonoBehaviour
    {
        public GridObject GridObject { get; private set; }
        
        public void Initialize(GridObject gridObject)
        {
            GridObject = gridObject;
        }
        
        public void ApplyTransform(float localRotation, Vector3 scale)
        {
            transform.localScale = scale;
            transform.localRotation = Quaternion.Euler(0, localRotation, 0);
        }
        
        public void MoveTo(Vector3 position)
        {
            transform.position = position;
        }
        
        public void Rotate(int angle)
        {
            angle = angle.NormalizeAngle();
            transform.localRotation = Quaternion.Euler(0, angle, 0);
            GridObject.Rotate(angle);
        }

        public void AddToGrid(Cell cell, Grid grid, Transform parent, bool isTop)
        {
            GridObject.SetGridData(cell, grid);
            if (!isTop)
            {
                MoveTo(cell.GetCenterPosition(new Vector3(parent.position.x, parent.position.y - 10, parent.position.z)));
            } else
            {
                MoveTo(cell.GetCenterPosition(parent.position));
            }
        }
    }
}