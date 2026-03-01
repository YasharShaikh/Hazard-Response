using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Vector2 movement;
    public float speed = 5f;
    [HideInInspector]
    public float baseSpeed; // Original speed for powerup calculations
    public bool canMove = true;
    public CharacterController characterController;
    public Vector3 lockPosition;
    public float smoothTime = 0.25f;

    public Animator animator;

    void Start()
    {
        baseSpeed = speed; // Store original speed
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove)
        {
            // Stop animations when dead
            if (animator != null)
            {
                animator.SetFloat("Horizontal", 0);
                animator.SetFloat("Vertical", 0);
            }
            return;
        }

        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Convert input to relative movement based on player's rotation
        // Flatten vectors to XZ plane to prevent sinking into ground
        Vector3 forward = transform.forward;
        forward.y = 0;
        forward.Normalize();
        
        Vector3 right = transform.right;
        right.y = 0;
        right.Normalize();
        
        // Create movement vector relative to player's facing direction
        Vector3 move = (right * movement.x + forward * movement.y).normalized;
        characterController.Move(move * speed * Time.deltaTime);
        
        if(movement.magnitude > 1)
        {
            movement.Normalize();
        }

        RotateTowardsMovement();

        animator.SetFloat("Horizontal", Mathf.Lerp(animator.GetFloat("Horizontal"), movement.x, smoothTime));
        animator.SetFloat("Vertical", Mathf.Lerp(animator.GetFloat("Vertical"), movement.y, smoothTime));
    }

    private void RotateTowardsMovement()
    {
        // Create a horizontal plane at the player's Y position
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distance;
        
        // Raycast against the virtual ground plane
        if(groundPlane.Raycast(ray, out distance))
        {
            lockPosition = ray.GetPoint(distance);
        }

        Vector3 direction = lockPosition - transform.position;
        direction.y = 0;
        transform.LookAt(transform.position + direction, Vector3.up);
    }
}
 