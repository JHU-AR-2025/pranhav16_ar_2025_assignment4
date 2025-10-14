using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
 // Rigidbody of the player.
 private Rigidbody rb; 

 // Variable to keep track of collected "PickUp" objects.
 private int count;

 // Movement along X and Y axes.
 private float movementX;
 private float movementY;

 // Speed at which the player moves.
 public float speed = 0;


    public float jumpForce = 7.5f; 
    
    // *** NEW: Check if the player is touching the ground ***
    private bool isGrounded;

    // *** POWER-UP VARIABLES ***
    private bool powerUpCollected;
    public float powerUpSpeedMultiplier = 2f; // How much faster the player moves
    public float powerUpDuration = 5f;        // How long the power-up lasts
    private float originalSpeed;             // To store the initial speed
    public GameObject powerUpPrefab;          // Assign the Power-Up Capsule prefab here in the Inspector
    public Vector3 powerUpSpawnPosition = new Vector3(0, 3.5f, 3.5f); // Where to place the power-up
    private Renderer playerRenderer;          // To change the player's color
    private Color originalColor;              // To store the initial color // UI text component to display count of "PickUp" objects collected.
 public TextMeshProUGUI countText;

 // UI object to display winning text.
 public GameObject winTextObject;

public GameObject powerUpTextObject;

public GameObject startButton;
 public bool isGameActive = false;
 // Start is called before the first frame update.
 void Start()
    {
 // Get and store the Rigidbody component attached to the player.
        rb = GetComponent<Rigidbody>();

         rb.useGravity = false;
 // Initialize count to zero.
        count = 0;

 // Update the count display.
        SetCountText();

 // Initially set the win text to be inactive.
        winTextObject.SetActive(false);
      powerUpTextObject.SetActive(false);
      powerUpPrefab.SetActive(false);
      playerRenderer = GetComponent<Renderer>();
        originalColor = playerRenderer.material.color;
        originalSpeed = speed; // Store the initial speed
    }
 

    public void StartGame()
{
    // 1. Enable movement and physics
    isGameActive = true;
    
    // 2. Enable gravity
    rb.useGravity = true;

    // 3. Hide the start screen UI
    // Assuming 'StartPanel' is the GameObject that holds your StartButton and any intro text.
    // You'll need to link this Panel in the Inspector or pass it as a parameter,
    // but for simplicity, we'll assume you link it or simply hide the button.
    startButton.SetActive(false); 
}



 // This function is called when a move input is detected.
 void OnMove(InputValue movementValue)
    {
      
         if (!isGameActive)
        {
            movementX = 0f;
            movementY = 0f;
            return;
        }
 // Convert the input value into a Vector2 for movement.
        Vector2 movementVector = movementValue.Get<Vector2>();

 // Store the X and Y components of the movement.
        movementX = movementVector.x; 
        movementY = movementVector.y; 
    }

 // FixedUpdate is called once per fixed frame-rate frame.
 private void FixedUpdate() 
    {
         if (!isGameActive)
        {
            // Optional: Freeze the sphere in place if it tries to move from residual force
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }
 // Create a 3D movement vector using the X and Y inputs.
        Vector3 movement = new Vector3 (movementX, 0.0f, movementY);

 // Apply force to the Rigidbody to move the player.
        rb.AddForce(movement * speed); 
    }

 
 void OnTriggerEnter(Collider other) 
    {
 // Check if the object the player collided with has the "PickUp" tag.
 if (other.gameObject.CompareTag("PickUp")) 
        {
 // Deactivate the collided object (making it disappear).
            other.gameObject.SetActive(false);

 // Increment the count of "PickUp" objects collected.
            count = count + 1;

 // Update the count display.
            SetCountText();
        }
 if (other.gameObject.CompareTag("Removable_Wall") && powerUpCollected) 
        {
 // Deactivate the collided object (making it disappear).
            other.gameObject.SetActive(false);


            
        }      

      if (other.gameObject.CompareTag("PowerUp") && !powerUpCollected) 
        {
            powerUpCollected = true;
            other.gameObject.SetActive(false); // Make the power-up disappear

            // 1. Change color and activate speed boost
            StartCoroutine(PowerUpEffect());
        }
      if (other.gameObject.CompareTag("Goal_Zone") && powerUpCollected) 
        {
            powerUpCollected = true;
            winTextObject.SetActive(true);
            powerUpTextObject.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }

 // Function to update the displayed count of "PickUp" objects collected.
 void SetCountText() 
    {
 // Update the count text with the current count.
        countText.text = "Count: " + count.ToString();

 // Check if the count has reached or exceeded the win condition.
 if (count >= 16)
        {
 // Display the win text.
            //winTextObject.SetActive(true);
            if (!powerUpCollected && GameObject.FindGameObjectWithTag("PowerUp") == null)
            {
               powerUpPrefab.SetActive(true);
                SpawnPowerUp();

            }
        }
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefab != null)
        {
            Instantiate(powerUpPrefab, powerUpSpawnPosition, Quaternion.identity);
        }
    }
    IEnumerator PowerUpEffect()
    {
        // Apply the effect
        playerRenderer.material.color = Color.yellow; // Change player color to yellow
        speed *= powerUpSpeedMultiplier;              // Increase speed
        powerUpTextObject.SetActive(true);            // Show power-up UI message

        // Wait for the duration
        yield return new WaitForSeconds(powerUpDuration);

        // Remove the effect
        playerRenderer.material.color = originalColor; // Revert color
        speed = originalSpeed;                         // Revert speed
        powerUpTextObject.SetActive(false);            // Hide power-up UI message
        powerUpCollected = false;                      // Allow another power-up to be collected later (if you reset the scene)
        
        // Final Win Condition (If power-up was the last goal)
        // winTextObject.SetActive(true); 
        // winTextObject.GetComponent<TextMeshProUGUI>().text = "You Win!";
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the sphere has landed on the floor or a platform
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = true;
        }
    }
    void OnCollisionExit(Collision collision)
    {
        // Assume the sphere is no longer grounded when it leaves a surface
        if (collision.gameObject.CompareTag("Floor"))
        {
            isGrounded = false;
        }
    }
   void OnJump()
    {
        // 1. Only allow jumping if the game is active (if you implemented the Start button)
        // 2. Only allow jumping if the sphere is currently touching the ground
        if (isGameActive && isGrounded) // Use 'if (isGrounded)' if you don't have isGameActive
        {
            // Apply an instantaneous upward force using ForceMode.Impulse
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false; // Set to false immediately after jump
        }
    }
}