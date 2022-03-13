using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace Player
{
    public class Vision : MonoBehaviour
    {
        public float viewRange;
        public float closeViewRange;
        [Range(0, 360)]
        public float viewAngle;
        private PhotonView PV;

        public LayerMask targetMask;
        public LayerMask obstacleMask;

        public List<Transform> visibleTargets = new List<Transform>();

        private void Start()
        {
            PV = GetComponent<PhotonView>();
            StartCoroutine("FindTargetsWithDelay", 0.1f);
        }

        IEnumerator FindTargetsWithDelay(float delay)
        {
            while (true)
            {
                yield return new WaitForSeconds(delay);
                FindVisibleTargets();
            }
        }

        private void FindVisibleTargets()
        {
            if (PV.IsMine && PhotonNetwork.IsConnected)
            {
                visibleTargets.Clear();
                Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRange, targetMask);
                
                for (int i = 0; i < targetsInViewRadius.Length; i++)
                {
                    Transform target = targetsInViewRadius[i].transform;
                    if (target != null)
                    {
                        Vector3 dirToTarget = (target.position - transform.position).normalized;

                        float distanceToTarget = Vector3.Distance(transform.position, target.position);
                        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                        {
                            if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
                            {
                                visibleTargets.Add(target);
                            }
                        }
                    }
                }

                foreach (Game.Player player in Game.GameManager.Instance.Players)
                {
                    if (player.PlayerObject != null)
                    {
                        if (Vector3.Distance(player.PlayerObject.transform.position, transform.position) > viewRange)
                        {
                            player.PlayerObject.GetComponent<Vision>().RevealMesh(false);
                        }
                    }
                }

                for (int i = 0; i < targetsInViewRadius.Length; i++)
                {
                    Transform target = targetsInViewRadius[i].transform;
                    if (target != transform && target != null)
                    {
                        Vision targetVisionComponent = target.GetComponent<Vision>();
                        if (visibleTargets.Contains(target))
                        {
                            targetVisionComponent.RevealMesh(true);
                        }
                        else
                        {
                            targetVisionComponent.RevealMesh(false);
                        }
                    }
                }
            }
        }

        public void RevealMesh(bool reveal)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                MeshRenderer mr = transform.GetChild(i).GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.enabled = reveal;
                }

                Canvas playername = transform.GetChild(i).GetComponent<Canvas>();
                if (playername != null)
                {
                    playername.enabled = reveal;
                }
            }
        }
    }
}