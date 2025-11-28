using UnityEngine;

public class AnimatedKey : MonoBehaviour
{
    // 1. Atur ini di Inspector
    //    Ini adalah tombol keyboard fisik yang akan "didengarkan"
    public KeyCode keyToListenFor; 
    
    private Animator myAnimator;

    void Start()
    {
        // 2. Otomatis ambil komponen Animator dari objek ini
        myAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        // 3. Cek jika tombol yang di-assign ditekan
        if (Input.GetKeyDown(keyToListenFor))
        {
            // 4. Mainkan Trigger "Pressed"
            //    Pastikan Anda punya trigger "Pressed" di Animator Anda
            myAnimator.SetTrigger("Pressed");
        }
    }
}