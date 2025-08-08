using UnityEngine;

public class FollowThePath : MonoBehaviour
{
    public Transform[] waypoints;

    [SerializeField]
    private float moveSpeed = 1f;

    [HideInInspector]
    public int waypointIndex = 0;

    public bool moveAllowed = false;

    private void Start()
    {
        // Pastikan posisi awal sesuai waypoint pertama
        transform.position = waypoints[waypointIndex].position;
    }

    private void Update()
    {
        // Gerak hanya jika diizinkan dan belum sampai akhir
        if (moveAllowed && waypointIndex < waypoints.Length)
        {
            Move();
        }
    }

    private void Move()
    {
        // Bergerak menuju waypoint target
        if (transform.position != waypoints[waypointIndex].position)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                waypoints[waypointIndex].position,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            // Setelah sampai, lanjut ke waypoint berikutnya
            waypointIndex++;
        }

        // Jika sudah sampai tujuan akhir, otomatis stop
        if (waypointIndex >= waypoints.Length)
        {
            moveAllowed = false;
        }
    }

    // Fungsi teleport paksa, digunakan saat kalah combat
    public void ForceMoveTo(int newIndex)
    {
        waypointIndex = Mathf.Clamp(newIndex, 0, waypoints.Length - 1);
        transform.position = waypoints[waypointIndex].position;
        moveAllowed = false;
    }
}
