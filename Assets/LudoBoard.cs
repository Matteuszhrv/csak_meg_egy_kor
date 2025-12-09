using UnityEngine;



public class LudoBoard : MonoBehaviour

{

    // This script holds the lists of all field positions so figures know where to walk.



    [Header("Main Track")]

    // Drag your 52 main fields here (Field_00 to Field_51)

    public Transform[] mainTrack;



    [Header("Victory Paths (Homes)")]

    public Transform[] redHome;    // 5 fields

    public Transform[] blueHome;   // 5 fields

    public Transform[] greenHome;  // 5 fields

    public Transform[] yellowHome; // 5 fields



    [Header("Spawn Points (Bases)")]

    public Transform[] redBase;    // 4 fields

    public Transform[] blueBase;   // 4 fields

    public Transform[] greenBase;  // 4 fields

    public Transform[] yellowBase; // 4 fields



    [Header("Finish")]

    public Transform redFinish;

    public Transform blueFinish;

    public Transform greenFinish;

    public Transform yellowFinish;

    public Transform GetSpawnForColor(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Blue: return blueBase[0];
            case Figure.TeamColor.Red: return redBase[0];
            case Figure.TeamColor.Green: return greenBase[0];
            case Figure.TeamColor.Yellow: return yellowBase[0];
        }
        return null;
    }
    public Transform GetExactSpawn(Figure fig)
    {
        switch (fig.teamColor)
        {
            case Figure.TeamColor.Blue:
                return blueBase[fig.spawnIndex];
            case Figure.TeamColor.Red:
                return redBase[fig.spawnIndex];
            case Figure.TeamColor.Green:
                return greenBase[fig.spawnIndex];
            case Figure.TeamColor.Yellow:
                return yellowBase[fig.spawnIndex];
        }
        return null;
    }


}