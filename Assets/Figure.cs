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
        // ✅ ÜTÉS + eltolás logika
        if (newField.HasOccupants())
        {
            foreach (Figure other in newField.occupants.ToArray())
            {
                if (other.teamColor != this.teamColor)
                {
                    // Más szín → leütés
                    other.SendBackToSpawn();
                }
                else
                {
                    // Saját szín → eltolás 5 unitra
                    OffsetSameColorFigures(newField);
                }
            }
        }

        if (currentField != null)
            currentField.RemoveFigure(this);

        currentField = newField;
        currentField.AddFigure(this);

        Vector3 pos = currentField.transform.position;
        pos.z = transform.position.z;   // megtartjuk a bábu Z-t
        transform.position = pos;

    }

    private void OffsetSameColorFigures(FieldMarker field)
    {
        float offset = 5f;

        for (int i = 0; i < field.occupants.Count; i++)
        {
            Figure fig = field.occupants[i];
            Vector3 pos = field.transform.position;

            if (i % 2 == 0)
                pos.x -= offset;
            else
                pos.x += offset;

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


}

