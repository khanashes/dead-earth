using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour
{

    public AIWaypointNetwork waypointNetwork = null;
    public int currentIndex = 0;
    public bool HasPath = false; 
    public bool pathPending = false;
    public bool PathStale = false;
    public AnimationCurve JumpCurve = new AnimationCurve();
    private NavMeshAgent _navAgent = null;
    public NavMeshPathStatus pathStatus = NavMeshPathStatus.PathInvalid;

    // Start is called before the first frame update
    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();

        if (waypointNetwork == null) return;
        SetNextDestination(false);
    }

    void SetNextDestination(bool increment)
    {
        if (!waypointNetwork) return;
        int incStep = increment ? 1 : 0;
        int nextWaypoint = (currentIndex + incStep >= waypointNetwork.Waypoints.Count) ? 0 : currentIndex + incStep;
        Transform nextWaypointTransform = waypointNetwork.Waypoints[nextWaypoint];
        if(nextWaypointTransform!=null)
        {
            currentIndex = nextWaypoint;
            _navAgent.destination = nextWaypointTransform.position;
            return;
        }
        currentIndex++;
    }
    // Update is called once per frame
    void Update()
    {
        HasPath = _navAgent.hasPath;
        pathPending = _navAgent.pathPending;
        PathStale = _navAgent.isPathStale;
        pathStatus = _navAgent.pathStatus;

        if (_navAgent.isOnOffMeshLink)
        {
            StartCoroutine(Jump(1.0f));
            return;
        }

        if((_navAgent.remainingDistance<= _navAgent.stoppingDistance && !pathPending) || pathStatus==NavMeshPathStatus.PathInvalid)
        {
            SetNextDestination(true);
        }
        else if (_navAgent.isPathStale)
        {
            SetNextDestination(false);
        }
    }

    IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);
        float time = 0.0f;
        while (time <= duration)
        {
            float t = time / duration;
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (JumpCurve.Evaluate(t) * Vector3.up);
            time += Time.deltaTime;
            yield return null;
        }

        _navAgent.CompleteOffMeshLink();
    }
}
