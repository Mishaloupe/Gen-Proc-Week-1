using UnityEngine;

namespace VTools.Grid
{
    public static class GridObjectFactory
    {
        /// <summary>
        /// Spawn a Grid Object with all required data setup. 
        /// </summary>
        /// <returns>The GridObjectController mono behaviour that represents the view of the object.</returns>
        public static GridObjectController SpawnFrom(GridObjectTemplate template, Transform parent /*= null*/, bool isTop, int rotation = 0, 
            Vector3? scale = null)
        {
            var finalScale = scale ?? Vector3.one;
            GridObjectController view;

            // 1. Instantiate controller from prefab
            if (!isTop)
            {
                view = UnityEngine.Object.Instantiate(template.View, new Vector3(parent.position.x, parent.position.y - 10, parent.position.z), parent.rotation);
            } else
            {
                view = UnityEngine.Object.Instantiate(template.View, parent);
            }

            // 2. Create the data model
            GridObject gridObject = template.CreateInstance();

            // 3. Inject into a controller and finalize the view
            view.Initialize(gridObject);
            view.ApplyTransform(rotation, finalScale);
            view.Rotate(rotation);
            
            return view;
        }

        /// <summary>
        /// Spawn a Grid Object with all required data setup. Add the object to the grid at the correct position.
        /// </summary>
        /// <returns>The GridObjectController mono behaviour that represents the view of the object.</returns>
        public static GridObjectController SpawnOnGridFrom(GridObjectTemplate template, Cell cell, Grid grid,
            Transform parent /*= null*/, bool isTop, int rotation = 0, Vector3? scale = null)
        {
            var view = SpawnFrom(template, parent, isTop, rotation, scale);

            view.AddToGrid(cell, grid, parent, isTop);
            cell.AddObject(view);

            return view;
        }
    }
}