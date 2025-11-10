using System.Collections.Generic;
using UnityEngine;
using TMPro; // Untuk UI

public class WordManager : MonoBehaviour
{
    [Header("Referensi Objek")]
    public GameObject wordPrefab;      // Nanti diisi ZombieContoh.prefab
    public Transform wordCanvas;       // Nanti diisi Canvas
    public List<Transform> penembakLanes;

    [Header("Daftar Kata")]
    public List<string> wordList; 

    // Ganti List ini untuk menyimpan skrip Zombie (WordObject)
    private List<WordObject> activeWords = new List<WordObject>(); 
    private WordObject currentWord; 

    void Start()
    {
        wordList.Add("HANTU");
        wordList.Add("ZOMBIE");
        wordList.Add("SERAM");
        wordList.Add("MATI");
        wordList.Add("LARI");

        // Ganti nama fungsi yang dipanggil
        InvokeRepeating("SpawnZombie", 2f, 3f); // Spawn tiap 3 detik
        
        // Pastikan game berjalan (jika TADI di-pause)
        Time.timeScale = 1f; 
    }

    // Ganti nama fungsi AddWord menjadi SpawnZombie
    public void SpawnZombie()
    {
        string randomWord = wordList[Random.Range(0, wordList.Count)];
        
        GameObject wordObj = Instantiate(wordPrefab, wordCanvas);

        // Atur posisi Y acak, tapi X tetap di kanan
        int lajurAcak = Random.Range(0, penembakLanes.Count);
        Transform lajurTerpilih = penembakLanes[lajurAcak];

        float spawnY = lajurTerpilih.localPosition.y;
        wordObj.transform.localPosition = new Vector3(500f, spawnY, 0);
        // Ambil komponen scriptnya dan atur teksnya
        WordObject wordDisplay = wordObj.GetComponent<WordObject>();
        wordDisplay.SetWord(randomWord);
        
        activeWords.Add(wordDisplay);
    }

    // Fungsi Update (logika pengetikan)
    void Update()
    {
        if (Input.inputString.Length > 0)
        {
            // Paksa jadi huruf besar agar cocok
            char typedChar = char.ToUpper(Input.inputString[0]);

            if (currentWord == null)
            {
                // Cari kata yang cocok (Ganti 'Zombie' ke 'WordObject')
                foreach (WordObject word in activeWords) 
                {
                    if (word.GetNextLetter() == typedChar)
                    {
                        currentWord = word; 
                        currentWord.TypeLetter(typedChar); 
                        break;
                    }
                }
            }
            else
            {
                currentWord.TypeLetter(typedChar); 

                if (currentWord.IsComplete())
                {
                    activeWords.Remove(currentWord); 
                    Destroy(currentWord.gameObject); // Hancurkan zombie
                    currentWord = null; 
                }
            }
        }
    }
    
    // --- INI FUNGSI YANG HILANG ---
    public void GameOver()
    {
        Debug.Log("GAME OVER!");
        // Hentikan semua pergerakan game
        Time.timeScale = 0f; 
        
        // Nanti bisa tambahkan UI "Game Over" di sini
    }
}