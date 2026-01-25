using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Interaction
{
    public class GoalTrigger : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Vector3 ballResetPosition;

        private int score = 0;

        private void Start()
        {
            UpdateScoreUI(score);
        }

        private void UpdateScoreUI(int newScore)
        {
            if (scoreText != null)
            {
                scoreText.text = newScore.ToString();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("OnTriggerEnter");
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer) return;
            Debug.Log("IsServer");
            if (!other.CompareTag("Ball"))
                return;

            Debug.Log("Ball entered goal!");

            score++;
            UpdateScoreUI(score);

            ResetBall(other.gameObject);
        }

        private void ResetBall(GameObject ball)
        {
            ball.transform.position = ballResetPosition;
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                ballRb.linearVelocity = Vector3.zero;
                ballRb.angularVelocity = Vector3.zero;
            }
        }
    }
}
