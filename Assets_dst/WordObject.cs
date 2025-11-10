using UnityEngine;
using TMPro; // <-- Penting untuk Text

public class WordObject : MonoBehaviour
{
    public TextMeshProUGUI textDisplay;
    public float moveSpeed = 10f; // Kecepatan gerak

    public bool isBossWord = false;

    private string wordToType;
    private int typeIndex = 0;
    
    // Kita butuh referensi ke WordManager untuk Game Over
    private WordManager wordManager;

    // Tambahkan fungsi Start untuk mencari Manager
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

    // Fungsi untuk mengatur kata
    public void SetWord(string word)
    {
        wordToType = word;
        textDisplay.text = word;
        typeIndex = 0;
    }

    // Fungsi untuk menggerakkan zombie ke kiri
    void Update()
    {
        if (!isBossWord)
        {
        // Bergerak ke kiri (Vector3.left)
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
    }

    // Fungsi untuk deteksi tabrakan
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isBossWord)
        {
            if (other.CompareTag("Penembak"))
            {
                if (wordManager != null)
                {
                    wordManager.KurangiNyawa();
                }
                Destroy(gameObject);
            }
        }
    }

    // --- FUNGSI UNTUK MENGETIK ---

    public char GetNextLetter()
    {
        return wordToType[typeIndex];
    }

    public void TypeLetter(char letter)
    {
        if (letter == wordToType[typeIndex])
        {
            typeIndex++;
            // Beri warna hijau
            textDisplay.text = $"<color=green>{wordToType.Substring(0, typeIndex)}</color>{wordToType.Substring(typeIndex)}";
        }
        else
        {
            // Reset jika salah ketik
            textDisplay.text = wordToType;
            typeIndex = 0;
        }
    }

    public bool IsComplete()
    {
        return typeIndex >= wordToType.Length;
    }
}