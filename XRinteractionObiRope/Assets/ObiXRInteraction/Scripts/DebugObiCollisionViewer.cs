using Obi;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugObiCollisionViewer : MonoBehaviour
{
    [SerializeField] ObiContactEventDispatcher contactEventDispatcher;

    private ObiCollider selfObiCollider;

    private HashSet<Vector3> collisionAPoints = new HashSet<Vector3>();

    private HashSet<Vector3> collisionBPoints = new HashSet<Vector3>();

    private Oni.Contact[] contacts;

    private void Awake()
    {
        this.selfObiCollider = GetComponent<ObiCollider>();
    }

    private void OnEnable()
    {
        contactEventDispatcher.onContactEnter.AddListener(SolverContact_OnCollisionEnter);
        contactEventDispatcher.onContactStay.AddListener(SolverContact_OnCollisionStay);
        contactEventDispatcher.onContactExit.AddListener(SolverContact_OnExit);
        contactEventDispatcher.onContacts.AddListener(SolverContacts);
    }

    private void OnDisable()
    {
        contactEventDispatcher.onContactEnter.RemoveListener(SolverContact_OnCollisionEnter);
        contactEventDispatcher.onContactStay.RemoveListener(SolverContact_OnCollisionStay);
        contactEventDispatcher.onContactExit.RemoveListener(SolverContact_OnExit);
        contactEventDispatcher.onContacts.RemoveListener(SolverContacts);
    }

    public void SolverContact_OnCollisionEnter(ObiSolver sender, Oni.Contact contact)
    {
        this.collisionAPoints.Add(contact.pointA);
        this.collisionBPoints.Add(contact.pointB);
    }

    public void SolverContact_OnCollisionStay(ObiSolver sender, Oni.Contact contact)
    {
        this.collisionAPoints.Add(contact.pointA);
        this.collisionBPoints.Add(contact.pointB);
    }

    public void SolverContact_OnExit(ObiSolver sender, Oni.Contact contact)
    {
       
    }

    public void SolverContacts(ObiSolver sender, Oni.Contact[] contacts)
    {
        this.contacts = contacts;
    }

    private void OnDrawGizmos()
    {
       // DrawPoints(this.collisionAPoints, Color.red);
        this.collisionAPoints.Clear();
        //DrawPoints(this.collisionBPoints, Color.blue);
        this.collisionBPoints.Clear();

        if (this.contacts != null)
        {
            DrawContactPoints(this.contacts, Color.green);
            this.contacts = null;
        }
        
    }

    private void DrawPoints(HashSet<Vector3> points, Color gizmoColor)
    {
        var cacheColor = Gizmos.color;
        Gizmos.color = gizmoColor;
        foreach (var one in points)
        {
            Gizmos.DrawSphere(one, 0.1f);
        }
        Gizmos.color = cacheColor;
    }

    private void DrawContactPoints(Oni.Contact[] contacts, Color gizmoColor)
    {
        var cacheColor = Gizmos.color;
        Gizmos.color = gizmoColor;
        foreach (var contact in contacts)
        {
            if (contact.distance < 0.05f)
            {
                Gizmos.DrawSphere(contact.pointB, 0.1f);
            }
        }
        Gizmos.color = cacheColor;
    }
}
