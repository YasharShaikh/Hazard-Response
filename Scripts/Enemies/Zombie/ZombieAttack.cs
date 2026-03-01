using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAttack : ZombieFSMBase {

	private const float ATTACK_DISTANCE = 2f;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter (animator, stateInfo, layerIndex);
		
		// Stop movement while attacking
		if (m_agent != null)
		{
			m_agent.isStopped = true;
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!IsDead() && m_player != null) {
			// Check if player moved out of attack range
			float distance = Vector3.Distance(m_player.transform.position, NPC.transform.position);
			if (distance > ATTACK_DISTANCE * 1.5f) { // Little buffer to prevent flickering
				animator.SetBool ("Attack", false);
				
				// Resume movement
				if (m_agent != null)
				{
					m_agent.isStopped = false;
				}
			}

			// Face the player while attacking
			Vector3 relativePos = m_player.transform.position - NPC.transform.position;
			relativePos.y = 0; // Keep rotation horizontal
			if (relativePos != Vector3.zero)
			{
				Quaternion rotation = Quaternion.LookRotation(relativePos);
				NPC.transform.rotation = rotation;
			}
		}
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		// Resume movement when leaving attack state
		if (m_agent != null && !IsDead())
		{
			m_agent.isStopped = false;
		}
	}
}
