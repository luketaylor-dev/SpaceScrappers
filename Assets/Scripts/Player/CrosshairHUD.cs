using UnityEngine;

namespace SpaceScrappers.Player
{
    public class CrosshairHUD : MonoBehaviour
    {
        [SerializeField] private float size      = 8f;
        [SerializeField] private float thickness = 2f;
        [SerializeField] private Color color     = Color.white;

        private void OnGUI()
        {
            float cx = Screen.width  * 0.5f;
            float cy = Screen.height * 0.5f;

            GUI.color = color;
            GUI.DrawTexture(new Rect(cx - size,            cy - thickness * 0.5f, size * 2,  thickness), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cx - thickness * 0.5f, cy - size,            thickness, size * 2),  Texture2D.whiteTexture);
        }
    }
}
