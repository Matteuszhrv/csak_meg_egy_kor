using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public LudoBoard board;

    public Figure activeFigure;
    public int lastRoll = 0;
    public bool awaitingFigureSelect = false;

    public Figure.TeamColor currentTurn = Figure.TeamColor.Blue;

    [ContextMenu("TEST: Roll Dice Only")]
    public void TestRollOnly()
    {
        lastRoll = RollDice();
        Debug.Log("🎲 " + currentTurn + " dobott: " + lastRoll);

        // AUTO PASS – ha nincs mozgatható bábu
        if (!HasMovableFigure(currentTurn, lastRoll))
        {
            Debug.Log("⛔ Nincs léphető bábu → PASS!");
            AdvanceTurn();
            return;
        }

        awaitingFigureSelect = true;
        Debug.Log("Most kattints egy figurára (" + currentTurn + ")!");
    }

    public void HandlePlayerMove(Figure selectedFigure)
    {
        if (!awaitingFigureSelect)
        {
            Debug.Log("❌ Előbb dobni kell!");
            return;
        }

        if (selectedFigure.teamColor != currentTurn)
        {
            Debug.Log("❌ Nem a te színed van soron! Most: " + currentTurn);
            return;
        }

        // Ha ezzel a bábúval nem lehet lépni
        if (!CanFigureMove(selectedFigure, lastRoll))
        {
            // Van más, amivel lehetne?
            if (HasMovableFigure(currentTurn, lastRoll))
            {
                Debug.Log("❌ Ezzel a bábuval nem tudsz lépni! Válassz másikat.");
                // ❗ SEMMIT sem nullázunk, marad a dobás
                return;
            }
            else
            {
                Debug.Log("⛔ Senkivel sem lehet lépni → PASS!");
                awaitingFigureSelect = false;
                lastRoll = 0;
                AdvanceTurn();
                return;
            }
        }

        // ✅ Itt már biztos, hogy érvényes a lépés
        activeFigure = selectedFigure;

        HandlePlayerTurn();

        awaitingFigureSelect = false;
        activeFigure = null;
        lastRoll = 0;

        AdvanceTurn();
    }

    private bool CanFigureMove(Figure fig, int dice)
    {
        if (fig == null || fig.currentField == null) return false;

        // 1) Spawn-ról csak 6-tal lehet kimenni
        if (fig.currentField.type == FieldMarker.FieldType.Spawn)
        {
            return dice == 6;
        }

        // 2) Homes (home path) belső mozgás ellenőrzése
        if (fig.currentField.type == FieldMarker.FieldType.Homes)
        {
            Transform[] homePath = GetHomePath(fig.teamColor);
            if (homePath == null) return false;

            int currentIndex = Array.IndexOf(homePath, fig.currentField.transform);
            if (currentIndex == -1) return false;

            int destinationIndex = currentIndex + dice;

            if (destinationIndex == homePath.Length) return true;    // pont a finish
            if (destinationIndex < homePath.Length) return true;     // sima home mező
            return false; // túldobás → nem léphet
        }

        // 3) Main track mozgás szimuláció (kilépés a home path-ra is)
        int currentIndexMain = Array.IndexOf(board.mainTrack, fig.currentField.transform);
        if (currentIndexMain == -1)
        {
            // Nem a main track-en — biztonsági okokból false
            return false;
        }

        int startIndex = GetStartIndex(fig.teamColor);
        if (startIndex == -1) return false;

        int N = board.mainTrack.Length;
        int stepsDone = (currentIndexMain - startIndex + N) % N;
        int newTotal = stepsDone + dice;

        if (newTotal < N) return true; // marad a main track-en

        // Belép a home path-ra vagy pont a finish-re — ellenőrizzük, hogy nem túldob-e
        Transform[] home = GetHomePath(fig.teamColor);
        if (home == null) return false;

        int remainingInHome = newTotal - (N - 1); // fontos: a GameManager korábbi logikája N = mainTrack.Length-1 használta
                                                  // Az eredeti HandleEnterHomePathFromTotal-ban N = board.mainTrack.Length - 1 volt.
                                                  // Itt azért számolunk hasonlóan: ha newTotal == N-1 -> belép home[0].

        // Szigorúan követjük az ottani logikát:
        int NforHome = board.mainTrack.Length - 1;
        remainingInHome = newTotal - NforHome;

        if (newTotal == NforHome) return true; // pontosan home[0]
        if (remainingInHome < 0) return true;   // bár elvileg nem fordul elő
        if (remainingInHome == home.Length) return true; // pont finish
        if (remainingInHome < home.Length) return true;  // beléphető home mező
        return false; // túldobás
    }



    public int RollDice() => UnityEngine.Random.Range(1, 7);

    // -------------------------------------------------------------------------------------
    //                           FŐ LÉPÉS LOGIKA
    // -------------------------------------------------------------------------------------

    public void HandlePlayerTurn()
    {
        int steps = lastRoll;

        if (activeFigure.currentField.type == FieldMarker.FieldType.Spawn)
        {
            if (steps != 6)
            {
                Debug.Log("❌ 6 kell a kilépéshez!");
                return;
            }

            Transform baseField = GetFirstBaseField(activeFigure.teamColor);
            FieldMarker destinationField = baseField.GetComponent<FieldMarker>();
            activeFigure.MoveToField(destinationField);

            return;
        }

        if (activeFigure.currentField.type == FieldMarker.FieldType.Homes)
        {
            HandleHomePathMove(steps);
            return;
        }

        HandleMainTrackMove(steps);
    }

    // -------------------------------------------------------------------------------------
    //                      MAIN TRACK MOZGÁS
    // -------------------------------------------------------------------------------------

    private void HandleMainTrackMove(int steps)
    {
        int currentIndex = Array.IndexOf(board.mainTrack, activeFigure.currentField.transform);
        if (currentIndex == -1)
        {
            Debug.LogError("Figure is on an unrecognized field.");
            return;
        }

        int startIndex = GetStartIndex(activeFigure.teamColor);
        if (startIndex == -1) return;

        int N = board.mainTrack.Length - 2;
        int stepsDone = 0;
        if (currentIndex - startIndex < 0) {
            stepsDone = (currentIndex - startIndex + N + 2) % (N+2);
        }
        else {
            stepsDone = (currentIndex - startIndex + N ) % (N);
        }
        
        int newTotal = stepsDone + steps;

        if (newTotal < N+1)
        {
            int newIndex = (startIndex + newTotal) % (N+2);
            FieldMarker destinationField = board.mainTrack[newIndex].GetComponent<FieldMarker>();
            MoveFigurePreserveZ(destinationField);
            return;
        }

        HandleEnterHomePathFromTotal(newTotal);
    }

    // -------------------------------------------------------------------------------------
    //                           HOME PATH LOGIKA
    // -------------------------------------------------------------------------------------

    private void HandleEnterHomePathFromTotal(int newTotal)
    {
        Transform[] homePath = GetHomePath(activeFigure.teamColor);
        if (homePath == null) return;

        int N = board.mainTrack.Length - 1;

        if (newTotal == N)
        {
            MoveFigurePreserveZ(homePath[0].GetComponent<FieldMarker>());
            return;
        }

        int remainingInHome = newTotal - N;

        if (remainingInHome == homePath.Length)
        {
            Transform finishT = GetFinishField(activeFigure.teamColor);
            MoveFigurePreserveZ(finishT.GetComponent<FieldMarker>());
            return;
        }

        if (remainingInHome > homePath.Length)
        {
            Debug.Log("Túldobás → nem lép.");
            return;
        }

        MoveFigurePreserveZ(homePath[remainingInHome].GetComponent<FieldMarker>());
    }

    private void HandleHomePathMove(int steps)
    {
        Transform[] homePath = GetHomePath(activeFigure.teamColor);
        if (homePath == null) return;

        int currentIndex = Array.IndexOf(homePath, activeFigure.currentField.transform);
        if (currentIndex == -1) return;

        int destinationIndex = currentIndex + steps;

        if (destinationIndex == homePath.Length)
        {
            Transform finishT = GetFinishField(activeFigure.teamColor);
            MoveFigurePreserveZ(finishT.GetComponent<FieldMarker>());
            return;
        }

        if (destinationIndex > homePath.Length)
        {
            Debug.Log("Túldobás → nem lép.");
            return;
        }

        MoveFigurePreserveZ(homePath[destinationIndex].GetComponent<FieldMarker>());
    }

    // -------------------------------------------------------------------------------------
    //                         AUTOPASS LOGIKA
    // -------------------------------------------------------------------------------------

    private bool HasMovableFigure(Figure.TeamColor color, int dice)
    {
        Figure[] allFigures = FindObjectsOfType<Figure>();

        foreach (var fig in allFigures)
        {
            if (fig.teamColor != color) continue;

            if (CanFigureMove(fig, dice))
            {
                return true;
            }
            
            
        }

        return false;
    }

    // -------------------------------------------------------------------------------------
    //                         SEGÉDFÜGGVÉNYEK
    // -------------------------------------------------------------------------------------

    private void MoveFigurePreserveZ(FieldMarker destination)
    {
        Vector3 pos = destination.transform.position;
        pos.z = activeFigure.transform.position.z;
        activeFigure.transform.position = pos;

        activeFigure.MoveToField(destination);

    }

    private int GetStartIndex(Figure.TeamColor color)
    {
        Transform t = GetFirstBaseField(color);
        if (t == null) return -1;
        return Array.IndexOf(board.mainTrack, t);
    }

    private Transform GetFirstBaseField(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Yellow: return board.mainTrack[39];
            case Figure.TeamColor.Blue: return board.mainTrack[0];
            case Figure.TeamColor.Red: return board.mainTrack[13];
            case Figure.TeamColor.Green: return board.mainTrack[26];
        }
        return null;
    }

    private Transform[] GetHomePath(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Yellow: return board.yellowHome;
            case Figure.TeamColor.Blue: return board.blueHome;
            case Figure.TeamColor.Red: return board.redHome;
            case Figure.TeamColor.Green: return board.greenHome;
        }
        return null;
    }

    private Transform GetFinishField(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Yellow: return board.yellowFinish;
            case Figure.TeamColor.Blue: return board.blueFinish;
            case Figure.TeamColor.Red: return board.redFinish;
            case Figure.TeamColor.Green: return board.greenFinish;
        }
        return null;
    }

    private void AdvanceTurn()
    {
        switch (currentTurn)
        {
            case Figure.TeamColor.Blue:
                currentTurn = Figure.TeamColor.Red;
                break;
            case Figure.TeamColor.Red:
                currentTurn = Figure.TeamColor.Green;
                break;
            case Figure.TeamColor.Green:
                currentTurn = Figure.TeamColor.Yellow;
                break;
            case Figure.TeamColor.Yellow:
                currentTurn = Figure.TeamColor.Blue;
                break;
        }

        Debug.Log("🔄 Következő játékos: " + currentTurn);
    }
}
