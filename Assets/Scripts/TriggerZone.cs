using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class TriggerZone : MonoBehaviour
{
   [SerializeField] private PlayableDirector timeline;
   private void OnTriggerEnter(Collider other)
   {
    if (other.CompareTag("Spaceship"))
    {
        timeline.Play();
    }
   }
}