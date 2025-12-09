using UnityEngine;
using System.Linq;

public class Figure : MonoBehaviour
{
    public enum TeamColor { Red, Blue, Green, Yellow }
    [Range(0, 3)]
    public int spawnIndex;

    public TeamColor teamColor;
    public LudoBoard board;
    public FieldMarker currentField;

    public void MoveToField(FieldMarker newField)
    {
        if (board.gameOver) return; // Ha vége a játéknak, ne lépjen

        bool isProtected = newField.type == FieldMarker.FieldType.Protected;
        bool isBase = newField.type == FieldMarker.FieldType.Spawn;

        bool doOffset = false;

        int fieldIndex = System.Array.IndexOf(board.mainTrack, newField.transform);

        if (newField.HasOccupants())
        {
            foreach (Figure other in newField.occupants.ToArray())
            {
                if (other.teamColor != this.teamColor)
                {
                    // Protected mező → nem lehet ütni
                    if (isProtected)
                    {
                        doOffset = true;
                        continue;
                    }

                    // Ha ez a mező az ő saját bázisa → nem üthető
                    if (isBase && fieldIndex == GetBaseIndexForColor(other.teamColor))
                    {
                        doOffset = true;
                        continue;
                    }

                    // ✅ Egyébként: leütés
                    other.SendBackToSpawn();
                }
                else
                {
                    // Saját szín → eltolás
                    doOffset = true;
                }
            }
        }

        // Figurka áthelyezése
        if (currentField != null)
            currentField.RemoveFigure(this);

        currentField = newField;
        currentField.AddFigure(this);

        // Állítsuk a pozíciót
        Vector3 pos = currentField.transform.position;
        pos.z = transform.position.z;
        transform.position = pos;

        // Ha szükséges, eltoljuk az azonos színű bábu(ka)t
        if (doOffset)
            OffsetSameColorFigures(newField);

        // Ellenőrizzük a győzelmet
        CheckForWinCondition();
    }

    private int GetBaseIndexForColor(TeamColor color)
    {
        switch (color)
        {
            case TeamColor.Blue: return 0;
            case TeamColor.Red: return 13;
            case TeamColor.Green: return 26;
            case TeamColor.Yellow: return 39;
        }
        return -1;
    }

    private void OffsetSameColorFigures(FieldMarker field)
    {
        float offset = 5f;
        int count = field.occupants.Count;

        for (int i = 0; i < count; i++)
        {
            Figure fig = field.occupants[i];
            Vector3 basePos = field.transform.position;
            Vector3 pos = basePos;

            if (count == 1)
            {
                pos = basePos;
            }
            else if (count == 2)
            {
                pos.x += (i == 0) ? -offset : offset;
            }
            else if (count == 3)
            {
                if (i == 0) pos.x -= offset;
                if (i == 1) pos = basePos;
                if (i == 2) pos.x += offset;
            }

            fig.transform.position = pos;
        }
    }

    public void SendBackToSpawn()
    {
        Transform spawnT = board.GetExactSpawn(this);
        FieldMarker spawnField = spawnT.GetComponent<FieldMarker>();

        if (currentField != null)
            currentField.RemoveFigure(this);

        currentField = spawnField;
        currentField.AddFigure(this);

        Vector3 pos = spawnField.transform.position;
        pos.z = transform.position.z;
        transform.position = pos;
    }

    private void CheckForWinCondition()
    {
        Transform finishT = board.GetFinishField(teamColor);
        FieldMarker finishField = finishT.GetComponent<FieldMarker>();

        int sameColorCount = finishField.occupants
            .Count(f => f.teamColor == teamColor);

        if (sameColorCount >= 2 && !board.gameOver)
        {
            board.DeclareWinner(teamColor);
        }
    }
}
