using UnityEngine; // EZ KRITIKUSAN FONTOS A Debug, FindObjectOfType stb. miatt!
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
public class FigureClick : MonoBehaviour, IPointerDownHandler
{
    private Figure figure;
    private GameManager gameManager; // Mentett változó a gyorsításhoz

    private void Start()
    {
        figure = GetComponent<Figure>();
        // Egyszer keressük meg a GameManager-t a Start-ban:
        gameManager = FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError("FigureClick nem találja a GameManager-t a jelenetben!");
        }
        Debug.LogError("Elindul?");
    }

    public void OnPointerDown(PointerEventData EventData)
    {
        Debug.Log("Kattintás ÉRZÉKELVE a figurán."); // Ez a sor teszteli a Collider/Rigidbody beállítást

        // Eltávolítottuk a korábbi hibás "private GameManager gm" sort!
        // A fent mentett gameManager változót használjuk:
        if (gameManager == null) return; // Védősor, ha a Start nem talált semmit

        if (!gameManager.awaitingFigureSelect)
        {
            Debug.Log("Most nem választhatsz figurát!");
            return;
        }

        Debug.Log("Kiválasztott figura: " + figure.name);
        gameManager.HandlePlayerMove(figure);
    }
}