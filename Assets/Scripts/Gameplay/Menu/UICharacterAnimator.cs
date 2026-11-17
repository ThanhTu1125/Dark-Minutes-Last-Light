using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UICharacterAnimator : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private float frameRate = 8f; // 8 khung hình/giây

    [Header("Sprite các khung hình bước đi sang phải")]
    public Sprite[] maleWalkFrames;
    public Sprite[] femaleWalkFrames;

    private Sprite[] currentFrames;
    private int currentFrameIndex = 0;
    private float timer = 0f;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    public void SetCharacter(CharacterType characterType)
    {
        currentFrames = (characterType == CharacterType.Male) ? maleWalkFrames : femaleWalkFrames;
        currentFrameIndex = 0;
        timer = 0f;
        UpdateFrame();
    }

    private void Update()
    {
        if (currentFrames == null || currentFrames.Length == 0) return;

        timer += Time.unscaledDeltaTime;
        if (timer >= 1f / frameRate)
        {
            timer -= 1f / frameRate;
            currentFrameIndex = (currentFrameIndex + 1) % currentFrames.Length;
            UpdateFrame();
        }
    }

    private void UpdateFrame()
    {
        if (targetImage != null && currentFrames != null && currentFrames.Length > 0)
        {
            targetImage.sprite = currentFrames[currentFrameIndex];
        }
    }
}
