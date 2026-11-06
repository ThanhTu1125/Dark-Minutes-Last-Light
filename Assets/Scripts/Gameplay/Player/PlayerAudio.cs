using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;
    [Header("Movement Audio")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private float footstepInterval = 0.35f;
    [Header("Action Audio")]
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip healSound;
    [Header("Volume&Variations")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomizePitch = true;
    [Range(0.05f,0.2f)]
    [SerializeField] private float pitchRang = 0.1f;
    private PlayerController playerController;
    private PlayerStatus playerStatus;
    private PlayerDash playerDash;
    private float footstepTimer = 0f;
    private bool wasDashing = false;
    private float previousHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (playerStatus != null)
        {
            previousHealth = playerStatus.currentHealth;
            playerStatus.OnHealthChanged += HandleHealthChanged;
            playerStatus.OnDied += PlayDeathSound;
        }
        footstepTimer = footstepInterval;
    }
    private void Awake()
    {
        audioSource=GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();
        playerStatus = GetComponent<PlayerStatus>();
        playerDash = GetComponent<PlayerDash>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

    }
    // Update is called once per frame
    private void Update()
    {
        if (playerStatus.isDead && playerStatus!=null) return;

        HandleFootsteps();
        HandleDashSound();

    }
    // Hàm phát âm thanh chung
    public void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(clip, volume);
    }
    //Hàm phát âm thanh bước chân
    public void HandleFootsteps()
    {
        if (playerController.Movement != Vector2.zero && !playerDash.IsDashing)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                PlaySound(footstepSound);
                footstepTimer = 0f;
            }

        }
        else
        {
            footstepTimer = 0f;
        }
    }

    // Hàm phát âm thanh sau khi chết
    public void PlayDeathSound()
    {
        if (deathSound == null) return;
        audioSource.Stop();
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(deathSound, volume);
    }
     // Hàm xử lý âm thanh Dash(lướt)
     public void HandleDashSound()
    {
        if (playerDash == null) return;
        if (playerDash.IsDashing && !wasDashing)
        {
            PlaySound(dashSound);
            wasDashing = true;
        }
        else if (!playerDash.IsDashing && wasDashing)
        {
            wasDashing = false;
        }
    }
    // Hàm xử lý âm thanh sát thương và hồi máu
    public void HandleHealthChanged(float current,float max)
    {
        if (current < previousHealth)
        {
            PlaySound(damageSound);
        }
        else if (current > previousHealth)
        {
            PlaySound(healSound);
        }
        previousHealth = current;
    }
    private void OnDestroy()
    {
        if (playerStatus != null) return;
        
         playerStatus.OnHealthChanged -= HandleHealthChanged;
         playerStatus.OnDied -= PlayDeathSound;
        
    }
}
