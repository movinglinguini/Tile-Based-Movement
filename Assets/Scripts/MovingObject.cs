using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour {

    public float moveTime; //The time it takes an object to move in seconds
    public LayerMask blockingLayer; //The layer in which we will check for collision;

    private BoxCollider2D boxCollider; //Reference to the unit's box collider
    private Rigidbody2D rb2d; //Reference to the unit's rigid body
    private float inverseMoveTime; //Will allow us to do calculations using multiplication instead of division

    //Initialize private variables
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;

    }

    protected bool Move (int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position; //Start where we're standing
        Vector2 end = start + new Vector2(xDir, yDir); //End at some other position on the board

        boxCollider.enabled = false; //Disable our hitbox so we don't self-collide
        hit = Physics2D.Linecast(start, end, blockingLayer); //Find out if there's something in our way
        boxCollider.enabled = true; //Reenable our hitbox

        //If there's nothing in the way that can block our movement, then move.
        if(hit.transform == null)
        {
            StartCoroutine(SmoothMovement(end));
            return true;
        }

        return false;
    }

    //Do smooth movement from one position to the next.
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        //If we are not very, very close to our end position (i.e. we have a distance greater than some epsilon),
        //then keep moving.
        while(sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2d.position, end, inverseMoveTime * Time.deltaTime);
            rb2d.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }

    //Try to move. If the move was unsuccessful
    protected virtual void AttemptMove<T>(int xDir, int yDir) where T : Component
    {
        RaycastHit2D hit;
        bool canMove = Move(xDir, yDir, out hit);
        
        //Aren't checking `hit.transform` and `canMove` essentially the same thing?
        if(hit.transform == null)
        {
            return;
        }

        //If we hit something, get the Component type of the thing we hit
        T hitComponent = hit.transform.GetComponent<T>();

        //If we can't move, then interact with it.
        if(!canMove)
        {
            OnCantMove(hitComponent);
        }
    }

    //Function that will define how the moveable object interacts with an obstacle.
    //For example, the player can attack walls and enemies. Enemies can attack the player.
    protected abstract void OnCantMove<T>(T component) where T : Component;
}
