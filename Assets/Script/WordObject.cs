using UnityEngine;
using TMPro; // <-- Penting untuk Text

public class WordObject : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public float moveSpeed = 10f;

    public int laneIndex;
    private string wordToType;
    private int typeIndex = 0;
    private WordManager wordManager;

    void Start()
    {
        // Cari objek GameManager di scene
        wordManager = FindObjectOfType<WordManager>();
        
        // Peringatan jika GameManager tidak ketemu
        if (wordManager == null)
        {
            Debug.LogError("GameManager tidak ditemukan di scene!");
        }
    }

    public void SetWord(string word)
    {
        wordToType = word;
        textDisplay.text = word;
        typeIndex = 0;
    }

    void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Penembak"))
        {
            if (wordManager != null)
            {
                wordManager.ZombieReachedEnd(this, other.gameObject);
            }
            Destroy(gameObject); 
        }
    }


    public char GetNextLetter()
    {
        return wordToType[typeIndex];
    }

    public void TypeLetter(char letter)
    {
        if (letter == wordToType[typeIndex])
        {
            typeIndex++;
            textDisplay.text = $"<color=green>{wordToType.Substring(0, typeIndex)}</color>{wordToType.Substring(typeIndex)}";
        }
        else
        {
            textDisplay.text = wordToType;
            typeIndex = 0;
        }
    }

    public bool IsComplete()
    {
        return typeIndex >= wordToType.Length;
    }
}