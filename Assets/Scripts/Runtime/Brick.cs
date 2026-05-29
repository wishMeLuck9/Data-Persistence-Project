using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Brick : MonoBehaviour
{
    public int PointValue { get; set; } = 5;

    private bool hit;
    private GameController gameController;

    private void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hit || collision.rigidbody == null)
        {
            return;
        }

        hit = true;
        gameController.AddScore(PointValue);
        gameController.BrickDestroyed();
        Destroy(gameObject);
    }
}
