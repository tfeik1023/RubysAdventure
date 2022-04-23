using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class RubyController : MonoBehaviour
{
    public float speed;
    public float boostTimer;
    private bool boosting;

    public int maxHealth = 5;
    public float timeInvincible = 2.0f;

    int currentHealth;
    public int health { get { return currentHealth; } }

    bool isInvincible;
    float invincibleTimer;

    Rigidbody2D rigidbody2d;
    public int score;
    public TextMeshProUGUI scoreText;
    public GameObject gameOverText;
    public GameObject youLoseText;
    public GameObject speedText;
    public GameObject invincibiltyText;

    public int cogs;
    public TextMeshProUGUI cogText;

    public bool gameOver = false;

    float horizontal;
    float vertical;

    Animator animator;
    Vector2 lookDirection = new Vector2(1, 0);

    public GameObject projectilePrefab;

    public AudioClip throwSound;
    public AudioClip hitSound;

    public static int level;

    public ParticleSystem HealthRecievePrefab;
    public ParticleSystem HealthDownPrefab;

    AudioSource audioSource;

    public AudioSource musicSource;

    public AudioClip musicClipOne;
    public AudioClip musicClipTwo;
    public AudioClip musicClipThree;

    public AudioClip collectedClip;
    public AudioClip collectedClip2;
    

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();

        score = 0;

        gameOverText.SetActive(false);
        youLoseText.SetActive(false);
        speedText.SetActive(false);
        invincibiltyText.SetActive(false);

        if (gameOver == false)
        {
            musicSource.loop = true;
        }

        cogs = 4;

        SetCogText();

        speed = 3.0f;
        boostTimer = 0;
        boosting = false;

    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        if (boosting)
        {
            boostTimer += Time.deltaTime;
            if(boostTimer >= 4)
            {
                speed = 3.0f;
                boostTimer = 0;
                boosting = false;
                speedText.SetActive(false);
            }
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if (isInvincible == false)
        {
            invincibiltyText.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (cogs >= 1)
            {
                Launch();
                cogs = cogs - 1;

                SetCogText();
            }
        }
            

        if (Input.GetKeyDown(KeyCode.X))
        {   
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                if (score >= 4)
                {
                    SceneManager.LoadScene("Scene2");
                    
                }
                else 
                {
                NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                if (character != null)
                {
                    character.DisplayDialog();
                    PlaySound(collectedClip);
                }
                }
            }
        }
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        if (score >= 4)
        {
            gameOverText.SetActive(true);
            
            
            if (level == 2)
            {
               
            }
            gameOver = true;
                musicSource.clip = musicClipTwo;
                musicSource.PlayOneShot(musicClipTwo);
            
            if (Input.GetKey(KeyCode.R))
            {
                if (gameOver == true)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }

        }

        if (health <= 0)
        {
            youLoseText.SetActive(true);
            gameOver = true;
            speed = 0;

            musicSource.clip = musicClipThree;
            musicSource.PlayOneShot(musicClipThree);

            if (Input.GetKey(KeyCode.R))
            {
                if (gameOver == true)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }
        
         
        


    }

    void SetCogText()
    {
        cogText.text = "Cogs: " + cogs.ToString();
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Cog"))
        {
           other.gameObject.SetActive(false); 
           cogs = cogs + 4;

           SetCogText();
        }
        if(other.tag == "Speed")
        {
            boosting = true;
            speed = 5.0f;
            Destroy(other.gameObject);
            speedText.SetActive(true);
        }
        if(other.tag == "Invincible")
        {
            isInvincible = true;
            invincibiltyText.SetActive(true);
            invincibleTimer = timeInvincible;
    
            Destroy(other.gameObject); 
        }
        
    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");

        PlaySound(throwSound);
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            if (isInvincible)
                return;

            animator.SetTrigger("Hit");
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;
            ParticleSystem HealthDownEffect = Instantiate(HealthDownPrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
            PlaySound(hitSound);

        }
        if (amount > 0)
        {
            ParticleSystem HealthRecieveEffect = Instantiate(HealthRecievePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        }
        

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);
    }

     
    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);

    }

    public void ChangeScore(int scoreAmount)
    {
        score = scoreAmount + score;
        scoreText.text = "Score" + score.ToString();
    }

}

