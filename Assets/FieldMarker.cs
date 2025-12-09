using UnityEngine;

using System.Collections.Generic;

public class FieldMarker : MonoBehaviour

{

    public enum FieldType { Normal, Base, Protected, Lucky, Spawn, Homes, Finish }

    public FieldType type = FieldType.Normal;



    // A List can hold 0, 1, 2, or 100 figures.

    public List<Figure> occupants = new List<Figure>();



    // Helper function to add a figure

    public void AddFigure(Figure figure)

    {

        occupants.Add(figure);

    }

    // Helper function to remove a figure

    public void RemoveFigure(Figure figure)

    {

        if (occupants.Contains(figure))

        {

            occupants.Remove(figure);

        }

    }

    // Quick check to see if anyone is here

    public bool HasOccupants()

    {

        return occupants.Count > 0;

    }

    private void OnDrawGizmos()

    {

        // 1. Set the color (Red with 50% transparency so you can see the board under it)

        Gizmos.color = new Color(1, 0, 0, 0.5f);


        // 2. Draw a Cube (Square) instead of a Sphere

        // The Vector3(0.5f, 0.5f, 0.1f) sets the Size: Width, Height, Thickness

        Gizmos.DrawCube(transform.position, new Vector3(13f, 13f, 0.1f));

        // Optional: Draw a wire outline to make it pop

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.1f));

    }

}