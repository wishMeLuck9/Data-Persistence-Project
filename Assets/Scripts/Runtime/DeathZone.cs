using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private GameController gameController;

    private void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.attachedRigidbody != null)
        {
            gameController.GameOver(false);
        }
    }
}
