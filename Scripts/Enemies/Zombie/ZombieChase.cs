using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieChase : ZombieFSMBase {

	private const float CHASE_SPEED = 8f;
	private const float CHASE_THRESHOLD_DISTANCE = 20f;

	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		base.OnStateEnter (animator, stateInfo, layerIndex);
		
		// Use ZombieEnemy chase speed if available, otherwise default
		if (m_zombieScript != null && m_agent != null)
		{
			m_agent.speed = m_zombieScript.chaseSpeed;
		}
		else if (m_agent != null)
		{
			m_agent.speed = CHASE_SPEED;
		}
	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_player == null || m_agent == null) return;
		
		// For new ZombieEnemy system, always chase (never go back to patrol)
		if (m_zombieScript != null)
		{
			if (!m_agent.pathPending && m_agent.isOnNavMesh)
			{
				m_agent.destination = m_player.transform.position;
			}
			return;
		}
		
		// Old Enemy system - can return to patrol if too far
		if (m_enemyScript != null)
		{
			if (Vector3.Distance(m_player.transform.position, NPC.transform.position) > CHASE_THRESHOLD_DISTANCE) {
				animator.SetBool ("Chase", false);
				return;
			}
			if (!m_agent.pathPending && !m_enemyScript.m_isDead) {
				m_agent.destination = m_player.transform.position;
			}
		}
	}
}
