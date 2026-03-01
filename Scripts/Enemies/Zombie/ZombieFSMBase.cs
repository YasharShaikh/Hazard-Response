using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieFSMBase : StateMachineBehaviour {

	protected GameObject NPC;
	protected NavMeshAgent m_agent;
	protected ZombieEnemy m_zombieScript;
	protected Enemy m_enemyScript; // Keep for backwards compatibility
	protected GameObject m_player;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		NPC = animator.gameObject;
		m_agent = NPC.GetComponent<NavMeshAgent>();
		
		// Try to get ZombieEnemy first (new system)
		m_zombieScript = NPC.GetComponent<ZombieEnemy>();
		
		// Fall back to old Enemy script if ZombieEnemy not found
		m_enemyScript = NPC.GetComponent<Enemy>();
		
		// Get player reference
		if (m_zombieScript != null)
		{
			m_player = m_zombieScript.m_player;
		}
		else if (m_enemyScript != null)
		{
			m_player = m_enemyScript.m_player;
		}
		
		if (m_player == null)
		{
			m_player = GameObject.FindGameObjectWithTag("Player");
		}
	}
	
	// Helper to check if dead (works with both systems)
	protected bool IsDead()
	{
		if (m_zombieScript != null) return m_zombieScript.m_isDead;
		if (m_enemyScript != null) return m_enemyScript.m_isDead;
		return false;
	}
}
