using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestNavMeshAgent : MonoBehaviour
{
    private NavMeshAgent agent1;
    // Start is called before the first frame update
    void Start()
    {
        //��ȡ����������
        agent1 = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //������������ 
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray1 = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray1, out hit))
            {
                //���λ��
                Vector3 point1 = hit.point;
                //
                agent1.SetDestination(point1);
            }
        }
    }
}