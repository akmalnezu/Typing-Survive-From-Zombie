using UnityEngine;

public class BossMovement : MonoBehaviour
{
    private float moveSpeed = 0f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    // WordManager akan memanggil fungsi ini
    public void StartMoving(Vector3 targetPos, float speed)
    {
        targetPosition = targetPos;
        moveSpeed = speed;
        isMoving = true;
    }

    void Update()
    {
        // 1. Jika tidak disuruh bergerak, jangan lakukan apa-apa
        if (!isMoving) return; 

        // 2. Bergerak menuju targetPosition dengan kecepatan moveSpeed
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // 3. Jika sudah sampai di target, berhenti bergerak
        if (transform.localPosition == targetPosition)
        {
            isMoving = false;
        }
    }
}