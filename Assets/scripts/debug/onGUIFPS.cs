using UnityEngine;

public class onGUIFPS : MonoBehaviour
{
    public bool showFPS = true;
    public Vector2 position = new Vector2(400, 20);
    public int fontSize = 30;
    public Color textColor = Color.black;
    public float updateInterval = 0.1f;
    
    private float fps;
    private float timer;
    private int frames;
    private GUIStyle style;
    
    void Start()
    {
        style = new GUIStyle();
        style.fontSize = fontSize;
        style.normal.textColor = textColor;
    }
    
    void Update()
    {
        frames++;
        timer += Time.unscaledDeltaTime;
        
        if (timer >= updateInterval)
        {
            fps = frames / timer;
            timer = 0f;
            frames = 0;
            
            if (fps >= 60)
                style.normal.textColor = Color.green;
            else if (fps >= 30)
                style.normal.textColor = Color.yellow;
            else
                style.normal.textColor = Color.red;
        }
    }
    
    void OnGUI()
    {
        if (showFPS)
        {
            // Scale font relative to screen height
            style.fontSize = Mathf.RoundToInt(fontSize * (Screen.height / 1080f));

            string fpsText = $"FPS: {fps:F1}";
            float xPos = position.x * Screen.width;
            float yPos = position.y * Screen.height;

            GUI.Label(new Rect(xPos, yPos, 200, 30), fpsText, style);
        }
    }
}