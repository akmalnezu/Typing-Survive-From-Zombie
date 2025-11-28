using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting; // Untuk UI
using System.IO;
using UnityEngine.SceneManagement; // <-- TAMBAHKAN BARIS INI
using UnityEngine.UI; // <-- TAMBAHKAN BARIS INI (untuk Button)
public class WordManager : MonoBehaviour
{
    [Header("Referensi Objek")]
    public GameObject wordPrefab;      // Nanti diisi ZombieContoh.prefab
    public Transform wordCanvas;       // Nanti diisi Canvas
    public List<Transform> penembakLanes;
    public GameObject gameOverPanel;
    public GameObject pauseBoardPanel;
    private bool isPaused = false;
    private bool isGameOver = false;

    [Header("Objek Nyawa")]
    public List<GameObject> penembakObjects;

    [Header("Refrensi UI Bos")]
    public GameObject bossPanel;
    public TextMeshProUGUI bossWordText;
    public TextMeshProUGUI bossTimerText;
    public GameObject bossZombiePrefab;

    [Header("Pengaturan Bos")]
    [TextArea(5, 10)]
    public string bossWord = "Ini Adalah Contoh Teks Bos Yang Sangat Panjang Dan Harus Diketik Dengan Cepat Tanpa Ada Kesalahan Sedikitpun Dalam Waktu Tiga Puluh Detik";
    public float bossTimeLimit = 30f;
    private bool isBossActive = false;
    private int bossTypeIndex = 0; // Melacak sudah berapa huruf bos yang diketik
    private float currentBossTime = 0f;
    private BossMovement currentBossVisual;

    [Header("Refrensi UI")]
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI LevelText;

    [Header("Status Game")]
    public int score = 0;
    public int level = 1;
    [SerializeField] private int scoreToNextLevel = 100;
    [SerializeField] private float currentSpawnRate = 3f;

    [Header("Efek Tembakan")]
    public GameObject bulletPrefab;
    public GameObject scopePrefab;

    [Header("Daftar Kata")]
    private List<string> wordList_ID = new List<string>();
    private List<string> wordList_EN = new List<string>();
    private List<string> currentActiveWordList = new List<string>(); 
    private List<WordObject> activeWords = new List<WordObject>();
    private WordObject currentWord;


    void Start()
    {
        LoadWordLists();
        currentActiveWordList.AddRange(wordList_ID);
        currentActiveWordList.AddRange(wordList_EN);

        StartSpawning();
        Time.timeScale = 1f;

        if (bossPanel != null)
            bossPanel.SetActive(false);

        UpdateUI();
    }

    void LoadWordLists()
    {
        TextAsset textFile_ID = Resources.Load<TextAsset>("wordList_ID");
        if (textFile_ID != null)
        {
            // Baca semua teks dan pisah berdasarkan baris
            string[] words = textFile_ID.text.Split(
                new[] { "\r\n", "\r", "\n" }, // Pemisah baris (Windows, Mac, Linux)
                System.StringSplitOptions.RemoveEmptyEntries // Abaikan baris kosong
            );
            
            foreach (string word in words)
            {
                wordList_ID.Add(word.Trim()); // Trim() untuk hapus spasi ekstra
            }
            Debug.Log($"Berhasil memuat {wordList_ID.Count} kata Bahasa Indonesia.");
        }
        else
        {
            Debug.LogError("File 'wordList_ID.txt' tidak ditemukan di folder 'Resources'!");
        }

        // 2. Muat file Bahasa Inggris (wordList_EN.txt)
        TextAsset textFile_EN = Resources.Load<TextAsset>("wordList_EN");
        if (textFile_EN != null)
        {
            string[] words = textFile_EN.text.Split(
                new[] { "\r\n", "\r", "\n" },
                System.StringSplitOptions.RemoveEmptyEntries
            );
            
            foreach (string word in words)
            {
                wordList_EN.Add(word.Trim());
            }
            Debug.Log($"Berhasil memuat {wordList_EN.Count} kata Bahasa Inggris.");
        }
        else
        {
            Debug.LogError("File 'wordList_EN.txt' tidak ditemukan di folder 'Resources'!");
        }
    }

    public void SpawnZombie()
    {
        if (currentActiveWordList.Count == 0)
        {
            Debug.LogError("Dafta kata (currentActiveWordList) kosong! Tidak bisa spawn zombie.");
            return;
        }

        string randomWord = currentActiveWordList[Random.Range(0, currentActiveWordList.Count)];
        GameObject wordObj = Instantiate(wordPrefab, wordCanvas.transform);

        int lajurAcak = Random.Range(0, penembakLanes.Count);
        Transform lajurTerpilih = penembakLanes[lajurAcak];

        float spawnY = lajurTerpilih.position.y;
        wordObj.transform.position = new Vector3(25f, spawnY, 0);
        WordObject wordDisplay = wordObj.GetComponent<WordObject>();
        wordDisplay.SetWord(randomWord);

        wordDisplay.laneIndex = lajurAcak;
        activeWords.Add(wordDisplay);
    }

    void Update()
    {
        if (isGameOver)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (isPaused)
        {
            return;
        }
        
        if (isBossActive)
        {
            HandleBossTyping();
            UpdateBossTimer();
        }
        else
        {
            HandleNormalTyping();
        }
    }

    void HandleNormalTyping()
    {
        if (Input.inputString.Length > 0)
        {
            char typedChar = Input.inputString[0];
            if (currentWord == null)
            {
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
                    WordObject wordToDestroy = currentWord;
                    currentWord = null;
                    AddScore(10);
                    activeWords.Remove(wordToDestroy);
                    TriggerShootEffect(wordToDestroy);
                }
            }
        }
    }

    void TriggerShootEffect(WordObject targetZombie)
    {
        GameObject shooter = FindClosestLivingShooter(targetZombie.transform);

        if (scopePrefab != null)
        {
            GameObject scope = Instantiate(scopePrefab);
            scope.transform.SetParent(targetZombie.transform);
            scope.transform.localPosition = Vector3.zero;
        }
        if (bulletPrefab != null && shooter != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, shooter.transform.position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetTarget(targetZombie.transform);
            }

        }
    }

    GameObject FindClosestLivingShooter(Transform zombie)
    {
        GameObject closestShooter = null;
        float minDistance = float.MaxValue;

        foreach (GameObject shooter in penembakObjects)
        {
            float distance = Mathf.Abs(shooter.transform.position.y - zombie.position.y);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestShooter = shooter;
            }
        }
        return closestShooter;
    }
    
    void HandleBossTyping()
    {
        if (Input.inputString.Length > 0)
        {
            char typedChar = Input.inputString[0];
            if (typedChar == bossWord[bossTypeIndex])
            {
                bossTypeIndex++;

                string display = $"<color=green>{bossWord.Substring(0, bossTypeIndex)}</color>{bossWord.Substring(bossTypeIndex)}";
                bossWordText.text = display;

                if (bossTypeIndex >= bossWord.Length)
                {
                    BossDefeated();
                }
            }
        }
    }

    void UpdateBossTimer()
    {
        currentBossTime -= Time.deltaTime;
        bossTimerText.text = $"Waktu: {currentBossTime:F1}s"; 

        if (currentBossTime <= 0f)
        {
            isBossActive = false; 
            bossTimerText.text = "Waktu Habis!";
            Debug.Log("KALAH MELAWAN BOS");

            if (currentBossVisual != null)
            {
                Destroy(currentBossVisual.gameObject);
            }
            GameOver(); 
        }    }

    void StartBossBattle()
    {
        isBossActive = true;
        CancelInvoke("SpawnZombie");

        foreach (WordObject word in activeWords)
        {
            if (word != null) Destroy(word.gameObject);
        }
        activeWords.Clear();
        currentWord = null;

        Transform middleLane = penembakLanes[1];

        float spawnX = 1000f;
        float spawnY = 0f;
        float targetX = middleLane.localPosition.x;

        Vector3 targetPosition = new Vector3(targetX, spawnY, 0);

        GameObject bossObj = Instantiate(bossZombiePrefab, wordCanvas);
        bossObj.transform.SetAsFirstSibling();
        bossObj.transform.localPosition = new Vector3(spawnX, spawnY, 500f);

        currentBossVisual = bossObj.GetComponent<BossMovement>();

        float distance = spawnX - targetX;
        float requiredSpeed = distance / bossTimeLimit;

        if (currentBossVisual != null)
        {
            currentBossVisual.StartMoving(targetPosition, requiredSpeed);
        }

        bossTypeIndex = 0;
        currentBossTime = bossTimeLimit;
        bossWordText.text = bossWord;

        if (bossPanel != null)
            bossPanel.SetActive(true);

    }
    
    void BossDefeated()
    {
        isBossActive = false;

        if (bossPanel != null)
            bossPanel.SetActive(false);
            
        if (currentBossVisual != null)
        {
            Destroy(currentBossVisual.gameObject);
        }

        Debug.Log("BOS DIKALAHKAN!");
        
        AddScore(500); 

        levelUp();    }

    void AddScore(int amount)
    {
        score += amount;
        if (score >= scoreToNextLevel)
        {
            levelUp();
        }
        UpdateUI();
    }

    void levelUp()
    {
        level++;
        score -= scoreToNextLevel;
        Debug.Log("Naik Level, Selamat datand di level " + level);

        CancelInvoke("SpawnZombie");

        currentActiveWordList.Clear();
        if (level == 2)
        {
            currentActiveWordList.AddRange(wordList_EN);
            currentSpawnRate = 2f;
            StartSpawning();
        }
        else if (level == 3)
        {
            currentActiveWordList.AddRange(wordList_ID);
            currentSpawnRate = 1.5f;
            StartSpawning();
        }
        else if (level == 4)
        {
            StartBossBattle();
        }
        else
        {
            currentActiveWordList.AddRange(wordList_ID);
            currentActiveWordList.AddRange(wordList_EN);
            currentSpawnRate = 1f;
            StartSpawning();
        }
        UpdateUI();
    }
    
    void StartSpawning()
    {
        CancelInvoke("SpawnZombie");
        InvokeRepeating("SpawnZombie", 1f, currentSpawnRate);
    }

    void UpdateUI()
    {
        if (livesText != null)
        {
            livesText.text = "Nyawa: " + penembakObjects.Count;
        }
        if (ScoreText != null)
        {
            ScoreText.text = "Skor: " + score;
        }
        if (LevelText != null)
        {
            LevelText.text = "Level: " + level;
        }
    }

    public void ZombieReachedEnd(WordObject zombie, GameObject penembakYangKena)
    {
        if (activeWords.Contains(zombie))
        {
            activeWords.Remove(zombie);
        }
        if (currentWord == zombie)
        {
            currentWord = null;
        }
        if (penembakObjects.Contains(penembakYangKena))
        {
            penembakObjects.Remove(penembakYangKena);
            SpriteRenderer sr = penembakYangKena.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = false;

            UnityEngine.UI.Image img = penembakYangKena.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.enabled = false;

            UpdateUI();

            if (penembakObjects.Count <= 0)
            {
                GameOver();
            }
        }
        else
        {
            Debug.Log("Zombie menabrak lane yang sudah hancur! KALAH!");
            GameOver();
        }
    }


    public void GameOver()
    {
        Debug.Log("GAME OVER!");

        isGameOver = true;

        CancelInvoke("SpawnZombie");

        foreach (WordObject word in new List<WordObject>(activeWords))
        {
            if (word != null)
            {
                Destroy(word.gameObject);
            }
        }
        activeWords.Clear(); // Kosongkan list
        currentWord = null;  // Hapus target

        if (gameOverPanel != null)
        {
            GameObject panel = Instantiate(gameOverPanel, wordCanvas);

            Button tombolRetry = panel.transform.Find("Retry").GetComponent<Button>();

            if (tombolRetry != null)
            {
                tombolRetry.onClick.AddListener(RestartGame);
            }
            else
            {
                Debug.LogError("Tidak bisa menemukan tombol 'Retry' di dalam prefab GameOver!");
            }

            Button tombolHome = panel.transform.Find("Home").GetComponent<Button>();
            if (tombolHome != null)
            {
                tombolHome.onClick.AddListener(GoToMainMenu);
            }
            else
            {
                Debug.LogError("Tidak bisa menemukan tombol 'Home_0' di prefab GameOver!");
            }
        }

    }

    public void RestartGame()
    {
        Debug.Log("TOMBOL RESTART DITEKAN!");
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseBoardPanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            pauseBoardPanel.SetActive(false);
        }
    }

    public void UnpauseGame()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }
    
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }
}