using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Player;

public class AnimationStateController : MonoBehaviour
{
    const string IDLE = "Idle";
    const string RUNNING = "Running";
    const string RUNNING_BACK = "Running Back";
    const string STRAFE_LEFT = "Strafe Left";
    const string STRAFE_RIGHT = "Strafe Right";
    const string THROW = "Throw";
    const string PICK_UP = "Pick Up";
    const string STUNNED = "Stunned";
    const string TAG = "Tag";
    const string DOOR = "Interact Door";
    const string EMPTY = "Empty";

    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private GameObject lookObject;
    [SerializeField] private TextMeshProUGUI DebugText;
    [SerializeField] private PlayerThrow playerThrow;
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerTag playerTag;
    private string currentState;
    void Start()
    {
        currentState = "Idle";
    }

    private void Update()
    {
        if (playerStatus.Stunned)
        {
            return;
        }

        float movementSpeed = movement.RB.velocity.magnitude;

        if (movementSpeed > 1f)
        {
            // Directional Animation Control
            #region Movement
            Vector3 moveDir = movement.m_MovementDirection;
            if (DebugText != null) DebugText.text = $"{moveDir}";

            Vector3 leftBack = new Vector3(-0.707107f, 0, -0.707107f);
            Vector3 leftForward = new Vector3(-0.707107f, 0, 0.707107f);
            Vector3 rightBack = new Vector3(0.707107f, 0, -0.707107f);
            Vector3 rightForward = new Vector3(0.707107f, 0, 0.707107f);

            Vector3 dir = Vector3.Normalize(lookObject.transform.position - transform.position);
            float dotForward = Vector3.Dot(transform.parent.forward, dir);
            float dotRight = Vector3.Dot(transform.parent.right, dir);

            if (moveDir == Vector3.forward)
            {
                if (dotForward < -0.707f) ChangeAnimationState(RUNNING_BACK);
                if (dotForward > 0.707f) ChangeAnimationState(RUNNING);
                if (dotRight < -0.707f) ChangeAnimationState(STRAFE_RIGHT);
                if (dotRight > 0.707f) ChangeAnimationState(STRAFE_LEFT);
            }

            else if (moveDir == Vector3.back)
            {
                if (dotForward < -0.707f) ChangeAnimationState(RUNNING);
                if (dotForward > 0.707f) ChangeAnimationState(RUNNING_BACK);
                if (dotRight < -0.707f) ChangeAnimationState(STRAFE_LEFT);
                if (dotRight > 0.707f) ChangeAnimationState(STRAFE_RIGHT);
            }

            else if (moveDir == Vector3.left)
            {
                if (dotForward < -0.707f) ChangeAnimationState(STRAFE_RIGHT);
                if (dotForward > 0.707f) ChangeAnimationState(STRAFE_LEFT);
                if (dotRight < -0.707f) ChangeAnimationState(RUNNING);
                if (dotRight > 0.707f) ChangeAnimationState(RUNNING_BACK);
            }

            else if (moveDir == Vector3.right)
            {
                if (dotForward < -0.707f) ChangeAnimationState(STRAFE_LEFT);
                if (dotForward > 0.707f) ChangeAnimationState(STRAFE_RIGHT);
                if (dotRight < -0.707f) ChangeAnimationState(RUNNING_BACK);
                if (dotRight > 0.707f) ChangeAnimationState(RUNNING);
            }

            else if (moveDir == leftBack)
            {
                if (dotForward < -0f && dotForward > -1f  && dotRight < 0f  && dotRight > -1f) ChangeAnimationState(RUNNING);
                if (dotForward > 0f  && dotForward < 1f   && dotRight > 0f  && dotRight < 1f) ChangeAnimationState(RUNNING_BACK);
                if (dotForward < -0f && dotForward > -1f  && dotRight > 0f  && dotRight < 1f) ChangeAnimationState(STRAFE_RIGHT);
                if (dotForward > 0f  && dotForward < 1f   && dotRight < -0f && dotRight > -1f) ChangeAnimationState(STRAFE_LEFT);
            }

            else if (moveDir == rightBack)
            {
                if (dotForward < -0f && dotForward > -1f && dotRight < 0f && dotRight > -1f) ChangeAnimationState(STRAFE_LEFT);
                if (dotForward > 0f && dotForward < 1f && dotRight > 0f && dotRight < 1f) ChangeAnimationState(STRAFE_RIGHT);
                if (dotForward < -0f && dotForward > -1f && dotRight > 0f && dotRight < 1f) ChangeAnimationState(RUNNING);
                if (dotForward > 0f && dotForward < 1f && dotRight < -0f && dotRight > -1f) ChangeAnimationState(RUNNING_BACK);
            }

            else if (moveDir == leftForward)
            {
                if (dotForward < -0f && dotForward > -1f && dotRight < 0f && dotRight > -1f) ChangeAnimationState(STRAFE_RIGHT);
                if (dotForward > 0f && dotForward < 1f && dotRight > 0f && dotRight < 1f) ChangeAnimationState(STRAFE_LEFT);
                if (dotForward < -0f && dotForward > -1f && dotRight > 0f && dotRight < 1f) ChangeAnimationState(RUNNING_BACK);
                if (dotForward > 0f && dotForward < 1f && dotRight < -0f && dotRight > -1f) ChangeAnimationState(RUNNING);
            }

            else if (moveDir == rightForward)
            {
                if (dotForward < -0f && dotForward > -1f && dotRight < 0f && dotRight > -1f) ChangeAnimationState(RUNNING_BACK);
                if (dotForward > 0f && dotForward < 1f && dotRight > 0f && dotRight < 1f) ChangeAnimationState(RUNNING);
                if (dotForward < -0f && dotForward > -1f && dotRight > 0f && dotRight < 1f) ChangeAnimationState(STRAFE_LEFT);
                if (dotForward > 0f && dotForward < 1f && dotRight < -0f && dotRight > -1f) ChangeAnimationState(STRAFE_RIGHT);
            }
            #endregion
        }

        else if (!playerStatus.Stunned)
        {
            ChangeAnimationState(IDLE);
        }
    }

    public void ThrowAnimation()
    {
        ChangeAnimationState(THROW,1);
    }

    public void PickUpAnimation()
    {        
        ChangeAnimationState(PICK_UP,1);
    }

    public void InteractDoorAnimation()
    {
        ChangeAnimationState(DOOR, 1);
    }

    public void StunnedAnimation()
    {
        ChangeAnimationState(EMPTY, 1);
        ChangeAnimationState(STUNNED,0);
        ResetFlags();
    }

    public void TagAnimation()
    {
        ChangeAnimationState(TAG, 1);
    }

    private void ChangeAnimationState(string newState, int layer = 0)
    {
        if (playerStatus.Stunned && currentState == STUNNED && (newState != STUNNED || newState != EMPTY))
        {
            return;
        }

        if (currentState != newState)
        {
            animator.CrossFade(newState, 0.2f,layer);

            currentState = newState;
        }
    }

    private void ResetFlags()
    {
        playerTag.DoingTag = false;
        playerInteract.PlayingInteract = false;
        playerThrow.DoPickup = false;
        playerThrow.AbleToThrow = true;
        playerThrow.IsThrowing = false;
        playerStatus.Busy = false;
    }

    private void DoTag()
    {
        playerTag.DoTag();
    }

    private void EndTag()
    {
        playerTag.DoingTag = false;
        playerStatus.Busy = false;
    }

    private void EndStun()
    {
        playerStatus.Stunned = false;
        playerStatus.Busy = false;
        playerStatus.StartCoroutine(playerStatus.StunDelay());
    }

    private void DoDoor()
    {
        playerInteract.InteractDoor();
    }

    private void EndDoor()
    {
        playerInteract.PlayingInteract = false;
        playerStatus.Busy = false;
    }

    private void DoPickUp()
    {
        playerThrow.DoPickup = true;
    }

    private void DoThrow()
    {
        playerThrow.DoThrow();
    }

    private void EndThrow()
    {
        playerThrow.AbleToThrow = true;
        playerStatus.Busy = false;
    }
}

