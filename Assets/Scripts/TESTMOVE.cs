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
        //获取导航代理人
        agent1 = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //如果点击鼠标左键 
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray1 = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray1, out hit))
            {
                //点击位置
                Vector3 point1 = hit.point;
                //
                agent1.SetDestination(point1);
            }
        }
    }
}