using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombiePatrol : ZombieFSMBase {

	private const float WALK_SPEED = 1.7f;
	private const float CHASE_INITIATE_DISTANCE = 15f;

	//OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter (animator, stateInfo, layerIndex);
		
		if (m_agent != null)
		{
			m_agent.speed = WALK_SPEED;
		}
		
		// If using new ZombieEnemy system, skip patrol and go straight to chase
		if (m_zombieScript != null)
		{
			animator.SetBool("Chase", true);
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (!IsDead() && m_player != null) {
			// Check if player is close enough to chase
			if (Vector3.Distance(m_player.transform.position, NPC.transform.position) < CHASE_INITIATE_DISTANCE) {
				animator.SetBool ("Chase", true);
				return;
			}
			
			// Wander behavior (only for old Enemy system)
			if (m_enemyScript != null && m_agent != null)
			{
				if (!m_agent.pathPending) {
					if (m_agent.remainingDistance <= m_agent.stoppingDistance) {
						m_agent.destination = m_enemyScript.GetRandomNavMeshPositin();
					}
				}
			}
		}
	}
}
