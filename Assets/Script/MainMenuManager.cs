using UnityEngine;
using UnityEngine.SceneManagement; // <-- PENTING

public class MainMenuManager : MonoBehaviour
{
    // 1. Variabel untuk menampung Animator
    public Animator menuContainerAnimator;

    // 2. FUNGSI INI UNTUK TOMBOL PLAY
    public void PlayGame_StartZoom()
    {
        // Beri tahu Animator untuk memutar trigger "StartZoom"
        menuContainerAnimator.SetTrigger("StartZoom");
    }

    // 3. FUNGSI INI UNTUK ANIMASI (saat selesai)
    public void LoadGameScene()
    {
        // Ganti "SampleScene" dengan nama scene game Anda
        SceneManager.LoadScene("SampleScene"); 
    }

    // 4. FUNGSI UNTUK TOMBOL TUTORIAL
    public void GoToTutorial()
    {
        // Ganti "TutorialScene" dengan nama scene tutorial Anda
        SceneManager.LoadScene("TutorialScene");
    }

    // 5. FUNGSI UNTUK TOMBOL QUIT
    public void QuitGame()
    {
        Debug.Log("PERINTAH QUIT DITEKAN!");
        Application.Quit();
    }

}