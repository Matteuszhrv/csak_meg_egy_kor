using UnityEngine;
using System;
using TMPro;

public class GameManager : MonoBehaviour
{
    public LudoBoard board;

    public TMP_Text DiceText;
    public TMP_Text TurnText;
    public TMP_Text WinnerText;

    public Figure activeFigure;
    public int lastRoll = 0;
    public bool awaitingFigureSelect = false;

    public Figure.TeamColor currentTurn = Figure.TeamColor.Blue;

    // Játék vége jelző
    private bool gameOver = false;

    [ContextMenu("TEST: Roll Dice Only")]
    public void TestRollOnly()
    {
        if (gameOver) return;

        lastRoll = RollDice();
        Debug.Log($"🎲 {currentTurn} dobott: {lastRoll}");
        DiceText.text = $"🎲 {lastRoll}";
        TurnText.text = $"Most lép: {currentTurn}";

        if (!HasMovableFigure(currentTurn, lastRoll))
        {
            Debug.Log("⛔ Nincs léphető bábu → PASS!");
            AdvanceTurn();
            return;
        }

        awaitingFigureSelect = true;
        Debug.Log($"Most kattints egy figurára ({currentTurn})!");
    }

    public void HandlePlayerMove(Figure selectedFigure)
    {
        if (gameOver) return;

        if (!awaitingFigureSelect)
        {
            Debug.Log("❌ Előbb dobni kell!");
            return;
        }

        if (selectedFigure.teamColor != currentTurn)
        {
            Debug.Log($"❌ Nem a te színed van soron! Most: {currentTurn}");
            return;
        }

        if (!CanFigureMove(selectedFigure, lastRoll))
        {
            if (HasMovableFigure(currentTurn, lastRoll))
            {
                Debug.Log("❌ Ezzel a bábuval nem tudsz lépni! Válassz másikat.");
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

        activeFigure = selectedFigure;
        HandlePlayerTurn();

        awaitingFigureSelect = false;
        activeFigure = null;
        lastRoll = 0;

        if (!gameOver)
            AdvanceTurn();
    }

    private bool CanFigureMove(Figure fig, int dice)
    {
        if (fig == null || fig.currentField == null) return false;

        // Spawn-ról csak 6-tal lehet kimenni
        if (fig.currentField.type == FieldMarker.FieldType.Spawn)
            return dice == 6;

        // Home path
        if (fig.currentField.type == FieldMarker.FieldType.Homes)
        {
            Transform[] homePath = GetHomePath(fig.teamColor);
            int currentIndex = Array.IndexOf(homePath, fig.currentField.transform);
            int destIndex = currentIndex + dice;
            return destIndex <= homePath.Length;
        }

        // Main track
        int currentIndexMain = Array.IndexOf(board.mainTrack, fig.currentField.transform);
        if (currentIndexMain == -1) return false;

        int startIndex = GetStartIndex(fig.teamColor);
        int N = board.mainTrack.Length;
        int stepsDone = (currentIndexMain - startIndex + N) % N;
        int newTotal = stepsDone + dice;

        Transform[] home = GetHomePath(fig.teamColor);
        if (home == null) return false;

        int remainingInHome = newTotal - (N - 1);

        if (newTotal < N) return true;
        if (remainingInHome <= home.Length) return true;
        return false;
    }

    public int RollDice() => UnityEngine.Random.Range(1, 7);

    // --------------------- LÉPÉS LOGIKA ---------------------

    public void HandlePlayerTurn()
    {
        if (gameOver) return;

        int steps = lastRoll;

        if (activeFigure.currentField.type == FieldMarker.FieldType.Spawn)
        {
            if (steps != 6)
            {
                Debug.Log("❌ 6 kell a kilépéshez!");
                return;
            }

            Transform baseField = GetFirstBaseField(activeFigure.teamColor);
            activeFigure.MoveToField(baseField.GetComponent<FieldMarker>());
            return;
        }

        if (activeFigure.currentField.type == FieldMarker.FieldType.Homes)
        {
            HandleHomePathMove(steps);
            return;
        }

        HandleMainTrackMove(steps);
    }

    private void HandleMainTrackMove(int steps)
    {
        int currentIndex = Array.IndexOf(board.mainTrack, activeFigure.currentField.transform);
        int startIndex = GetStartIndex(activeFigure.teamColor);
        int N = board.mainTrack.Length;

        int stepsDone = (currentIndex - startIndex + N) % N;
        int newTotal = stepsDone + steps;

        if (newTotal < N)
        {
            int newIndex = (startIndex + newTotal) % N;
            activeFigure.MoveToField(board.mainTrack[newIndex].GetComponent<FieldMarker>());
            return;
        }

        HandleEnterHomePathFromTotal(newTotal);
    }

    private void HandleEnterHomePathFromTotal(int newTotal)
    {
        Transform[] homePath = GetHomePath(activeFigure.teamColor);
        int N = board.mainTrack.Length - 1;
        int remaining = newTotal - N;

        if (remaining == homePath.Length)
        {
            Transform finishT = GetFinishField(activeFigure.teamColor);
            activeFigure.MoveToField(finishT.GetComponent<FieldMarker>());
            CheckWinner(activeFigure.teamColor);
            return;
        }

        if (remaining > homePath.Length)
        {
            Debug.Log("Túldobás → nem lép.");
            return;
        }

        activeFigure.MoveToField(homePath[remaining].GetComponent<FieldMarker>());
    }

    private void HandleHomePathMove(int steps)
    {
        Transform[] homePath = GetHomePath(activeFigure.teamColor);
        int currentIndex = Array.IndexOf(homePath, activeFigure.currentField.transform);
        int destIndex = currentIndex + steps;

        if (destIndex == homePath.Length)
        {
            Transform finishT = GetFinishField(activeFigure.teamColor);
            activeFigure.MoveToField(finishT.GetComponent<FieldMarker>());
            CheckWinner(activeFigure.teamColor);
            return;
        }

        if (destIndex > homePath.Length) return;

        activeFigure.MoveToField(homePath[destIndex].GetComponent<FieldMarker>());
    }

    // --------------------- AUTOPASS ---------------------

    private bool HasMovableFigure(Figure.TeamColor color, int dice)
    {
        Figure[] allFigures = FindObjectsOfType<Figure>();
        foreach (var fig in allFigures)
        {
            if (fig.teamColor != color) continue;
            if (CanFigureMove(fig, dice)) return true;
        }
        return false;
    }

    // --------------------- SEGÉDFÜGGVÉNYEK ---------------------

    private int GetStartIndex(Figure.TeamColor color)
    {
        Transform t = GetFirstBaseField(color);
        return t != null ? Array.IndexOf(board.mainTrack, t) : -1;
    }

    private Transform GetFirstBaseField(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Blue: return board.mainTrack[0];
            case Figure.TeamColor.Red: return board.mainTrack[13];
            case Figure.TeamColor.Green: return board.mainTrack[26];
            case Figure.TeamColor.Yellow: return board.mainTrack[39];
        }
        return null;
    }

    private Transform[] GetHomePath(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Blue: return board.blueHome;
            case Figure.TeamColor.Red: return board.redHome;
            case Figure.TeamColor.Green: return board.greenHome;
            case Figure.TeamColor.Yellow: return board.yellowHome;
        }
        return null;
    }

    private Transform GetFinishField(Figure.TeamColor color)
    {
        switch (color)
        {
            case Figure.TeamColor.Blue: return board.blueFinish;
            case Figure.TeamColor.Red: return board.redFinish;
            case Figure.TeamColor.Green: return board.greenFinish;
            case Figure.TeamColor.Yellow: return board.yellowFinish;
        }
        return null;
    }

    private void AdvanceTurn()
    {
        if (gameOver) return;

        switch (currentTurn)
        {
            case Figure.TeamColor.Blue: currentTurn = Figure.TeamColor.Red; break;
            case Figure.TeamColor.Red: currentTurn = Figure.TeamColor.Green; break;
            case Figure.TeamColor.Green: currentTurn = Figure.TeamColor.Yellow; break;
            case Figure.TeamColor.Yellow: currentTurn = Figure.TeamColor.Blue; break;
        }

        TurnText.text = $"Most lép: {currentTurn}";
        Debug.Log($"🔄 Következő játékos: {currentTurn}");
    }

    private void CheckWinner(Figure.TeamColor color)
    {
        // Ellenőrizzük, hogy legalább 2 bábu a finish mezőn
        int count = 0;
        foreach (var fig in FindObjectsOfType<Figure>())
        {
            if (fig.teamColor == color && fig.currentField.type == FieldMarker.FieldType.Finish)
                count++;
        }

        if (count >= 2)
        {
            gameOver = true;
            WinnerText.text = $"Győztes: {color}";
            Debug.Log($"🏆 Győztes: {color}");
        }
    }
}
