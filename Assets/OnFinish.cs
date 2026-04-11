using TestBotRoom;
using TestBotRoom.Gameplay;
using UnityEngine;

public class OnFinish : StateMachineBehaviour
{
    [SerializeField] private string animationName;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.LogWarning($"The animation '{animationName}' has finished with a duration of {stateInfo.length} seconds.");
        animator.GetComponentInParent<PlayerController>().ShotAnimation(animationName, stateInfo.length);
    }
}
