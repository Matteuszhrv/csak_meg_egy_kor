using UnityEngine;

public class LudoBoard : MonoBehaviour
{
    [Header("Main Track (52 mező)")]
    public Transform[] mainTrack;

    [Header("Victory Paths (Homes)")]
    public Transform[] redHome;    // 5 mező
    public Transform[] blueHome;   // 5 mező
    public Transform[] greenHome;  // 5 mező
    public Transform[] yellowHome; // 5 mező

    [Header("Spawn Points (Bases)")]
    public Transform[] redBase;    // 4 mező
    public Transform[] blueBase;   // 4 mező
    public Transform[] greenBase;  // 4 mező
    public Transform[] yellowBase; // 4 mező

    [Header("Finish")]
    public Transform redFinish;
    public Transform blueFinish;
    public Transform greenFinish;
    public Transform yellowFinish;

    [Header("Game Status")]
    public bool gameOver = false;
    public Figure.TeamColor? winner = null;

    // ---------------- Segédfüggvények ----------------

    // Visszaadja a színhez tartozó finish mezőt
    public Transform GetFinishField(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Blue: return blueFinish;
            case Figure.TeamColor.Red: return redFinish;
            case Figure.TeamColor.Green: return greenFinish;
            case Figure.TeamColor.Yellow: return yellowFinish;
        }
        return null;
    }

    // Visszaadja a színhez tartozó home path mezőket
    public Transform[] GetHomePath(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Blue: return blueHome;
            case Figure.TeamColor.Red: return redHome;
            case Figure.TeamColor.Green: return greenHome;
            case Figure.TeamColor.Yellow: return yellowHome;
        }
        return null;
    }

    // Visszaadja az adott figura spawn mezőjét
    public Transform GetExactSpawn(Figure figure)
    {
        switch (figure.teamColor)
        {
            case Figure.TeamColor.Blue: return blueBase[figure.spawnIndex];
            case Figure.TeamColor.Red: return redBase[figure.spawnIndex];
            case Figure.TeamColor.Green: return greenBase[figure.spawnIndex];
            case Figure.TeamColor.Yellow: return yellowBase[figure.spawnIndex];
        }
        return null;
    }

    // Visszaadja a szín első main track mezőjét (a bázistól indulás indexe)
    public Transform GetFirstBaseField(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Blue: return mainTrack[0];
            case Figure.TeamColor.Red: return mainTrack[13];
            case Figure.TeamColor.Green: return mainTrack[26];
            case Figure.TeamColor.Yellow: return mainTrack[39];
        }
        return null;
    }

    // ---------------- Győztes hirdetés ----------------
    public void DeclareWinner(Figure.TeamColor color)
    {
        if (gameOver) return;

        gameOver = true;
        winner = color;
        Debug.Log("🏆 GYŐZTES: " + color + " !");
    }
}
