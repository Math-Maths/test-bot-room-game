using TestBotRoom;
using UnityEngine;

public class OnFinish : StateMachineBehaviour
{
    [SerializeField] private string animationName;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log(stateInfo.length);
        animator.GetComponentInParent<PlayerController>().ShotAnimation(animationName, stateInfo.length);
    }
}
