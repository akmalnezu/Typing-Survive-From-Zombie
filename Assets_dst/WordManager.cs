using System.Collections.Generic;
using UnityEngine;
using TMPro; // Untuk UI
using System.IO;
using System;
using UnityEngine.SceneManagement;
public class WordManager : MonoBehaviour
{
    [Header("Referensi Objek")]
    public GameObject wordPrefab;      // Nanti diisi ZombieContoh.prefab
    public Transform wordCanvas;       // Nanti diisi Canvas
    public List<Transform> penembakLanes;
    public GameObject tombolUlang;

    [Header("Daftar Kata")]
    public string namaFileKata = "Indonesia";
    private List<string> wordList = new List<string>();
    private List<WordObject> activeWords = new List<WordObject>();

    [Header("Pengaturan Bos")]
    public bool isBossLevel = false;
    public float bossTimer = 30.0f;
    private WordObject currentBossWordObject;
    private WordObject currentWord;
    [Header("Status Player")]
    public int nyawa = 3;
    public int skor = 0;
    public int level = 1;
    public int skorNaikLevel = 50;

    [Header("Pengaturan Spawn")]
    private float spawnRate = 3.0f;
    private float spawnDelay = 2.0f;

    void Start()
    {
        MuatDaftarKataDariFile("Indonesia", false);
        InvokeRepeating("SpawnZombie", spawnDelay, spawnRate);
        Time.timeScale = 1f;
    }

    void MuatDaftarKataDariFile(string namaFile, bool tambahkan)
    {
        if (!tambahkan)
        {
            wordList.Clear();
        }
        // 1. Muat TextAsset dari folder "Resources"
        TextAsset textAsset = Resources.Load<TextAsset>(namaFile);

        if (textAsset == null)
        {
            Debug.LogError("Error: Tidak bisa menemukan file kata di 'Resources/" + namaFile + ".txt'");
            // Jika file tidak ada, tambahkan kata darurat agar game tidak error
            wordList.Add("FILE");
            wordList.Add("HILANG");
            return;
        }

        // 2. Baca seluruh teks dan pisahkan berdasarkan baris baru
        string[] kataArray = textAsset.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // 3. Masukkan ke dalam wordList
        foreach (string kata in kataArray)
        {
            // .Trim() untuk hapus spasi, .ToUpper() untuk pastikan huruf besar
            wordList.Add(kata.Trim());
        }

        Debug.Log("Berhasil memuat " + wordList.Count + " kata dari " + namaFile);
    }

    public void  TambahSkor(int jumlah)
    {
        skor += jumlah;

        if (skor >= skorNaikLevel)
        {
            NaikLevel();
        }
    }

    public void KurangiNyawa()
    {
        nyawa--;
        Debug.Log("Nyawa tersisa: " + nyawa);

        // if (textNyawa != null)
        // {
        //     textNyawa.text = "Nyawa: " + nyawa;
        // }

        if (nyawa <= 0)
        {
            GameOver();
        }
    }

    void NaikLevel()
    {
        level++;
        skor = 0;

        Debug.Log("Selamat, Naik Ke Level " + level);
        CancelInvoke("SpawnZombie");

        if (level == 2)
        {
            skorNaikLevel = 100;
            spawnRate = 2.5f;
            MuatDaftarKataDariFile("Inggris", false);
        }
        else if (level == 3)
        {
            skorNaikLevel = 30;
            spawnRate = 2.0f;

            MuatDaftarKataDariFile("Indonesia", false);
            MuatDaftarKataDariFile("Inggris", true);
        }

        else if (level == 4)
        {
            Debug.Log("Persiapkan Diri, Boss Muncul!!");
            isBossLevel = true;
            foreach (WordObject word in activeWords)
            {
                if (word != null) Destroy(word.gameObject);
            }
            activeWords.Clear();
            if (currentWord != null) currentWord = null;
            MulaiLevelBos();
        }
        if (!isBossLevel && level <= 3)
        {
            InvokeRepeating("SpawnZombie", 1f, spawnRate);
        }
    }

    void MenangGame()
    {
        Debug.Log("Selamat! Kamu Menang!!");
        Time.timeScale = 0f;
        isBossLevel = false;

        if (currentBossWordObject != null)
        {
            Destroy(currentBossWordObject.gameObject);
            currentBossWordObject = null;
        }
    }

    public void SpawnZombie()
    {
        string randomWord = wordList[UnityEngine.Random.Range(0, wordList.Count)];

        GameObject wordObj = Instantiate(wordPrefab, wordCanvas);

        // Atur posisi Y acak, tapi X tetap di kanan
        int lajurAcak = UnityEngine.Random.Range(0, penembakLanes.Count);
        Transform lajurTerpilih = penembakLanes[lajurAcak];

        float spawnY = lajurTerpilih.localPosition.y;
        wordObj.transform.localPosition = new Vector3(500f, spawnY, 0);
        // Ambil komponen scriptnya dan atur teksnya
        WordObject wordDisplay = wordObj.GetComponent<WordObject>();
        wordDisplay.SetWord(randomWord);

        activeWords.Add(wordDisplay);
    }

    void MulaiLevelBos()
    {
        bossTimer = 30.0f;
        string bossWordString = "Kamu Pikir Kamu Bisa Mengalahkanku Semudah Itu Hahaha";
        GameObject bossObj = Instantiate(wordPrefab, wordCanvas);
        bossObj.transform.localPosition = new Vector3(0, 0, 0);
        currentBossWordObject = bossObj.GetComponent<WordObject>();
        currentBossWordObject.SetWord(bossWordString);
        currentBossWordObject.isBossWord = true;
    }

    void Update()
    {
        // Cek kita di mode mana
        if (isBossLevel)
        {
            // Panggil logika untuk bos
            HandleBossLevelUpdate();
        }
        else
        {
            // Panggil logika untuk level biasa
            HandleNormalLevelUpdate();
        }
    }

    void HandleBossLevelUpdate()
    {
        // 1. Hitung mundur timer
        if (bossTimer > 0)
        {
            bossTimer -= Time.deltaTime;
            // Nanti perbarui UI Timer:
            // if (textTimer != null) textTimer.text = bossTimer.ToString("F1");

            // 2. Cek input ketikan
            if (Input.inputString.Length > 0)
            {
                char typedChar = Input.inputString[0]; // Case-sensitive
                
                if (currentBossWordObject != null)
                {
                    currentBossWordObject.TypeLetter(typedChar);

                    // 3. Cek Kemenangan
                    if (currentBossWordObject.IsComplete())
                    {
                        MenangGame();
                    }
                }
            }
        }
        else
        {
            // 4. Cek Kekalahan (Waktu Habis)
            // Waktu habis DAN kata belum selesai
            if (currentBossWordObject != null && !currentBossWordObject.IsComplete())
            {
                Debug.Log("WAKTU HABIS! KALAH!");
                // if (textTimer != null) textTimer.text = "0.0";
                
                // Hancurkan objek bos sebelum game over
                Destroy(currentBossWordObject.gameObject);
                currentBossWordObject = null;
                
                GameOver(); // Panggil game over
            }
        }
    }

    void HandleNormalLevelUpdate()
    {
        if (Input.inputString.Length > 0)
        {
            char typedChar = Input.inputString[0]; // Case-sensitive

            if (currentWord == null)
            {
                foreach (WordObject word in activeWords)
                {
                    if (word != null && word.GetNextLetter() == typedChar)
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
                    TambahSkor(10);
                    activeWords.Remove(currentWord);
                    Destroy(currentWord.gameObject);
                    currentWord = null;
                }
            }
        }
    }

    public void GameOver()
    {
        Debug.Log("GAME OVER!");
        Time.timeScale = 0f;

        if (tombolUlang != null)
        {
            tombolUlang.SetActive(true);
        }
    }
    
    public void UlangiGame()
    {
        Time.timeScale = 1f;

        Scene sceneSekarang = SceneManager.GetActiveScene();
        SceneManager.LoadScene(sceneSekarang.name);
    }
}