using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class MyRigidBody : MonoBehaviour {

    // member variables
    public Vector3 velocity; // the current velocity of the rigid body
    public Vector3 netForce; // the current net force acting on the object
    public List<Vector3> forces; // the list of forces acting on the object
    public float mass; // the mass of the rigid body
    public float restitution; // how bouncy the object is
    public float inverseMass; // 1/mass
    public float staticFriction; // friction to be overcome to move an object
    public float dynamicFriction; // the friction while the object is moving
    public abstract Matrix4x4 inertiaTensor; // the inertia tensor of the object
    public Vector3 angularVelocity; // the angular velocity of the object

    private Matrix4x4 localI; // local representation of the inertia tensor
    private Vector3 globalL;
                                                                                                                                                                                                                                                                                        
	// Initialise
	protected void Start () {

        // get the inverse mass
        inverseMass = 1 / mass;
	}

    // Handle update
    protected void FixedUpdate()
    {
        // add all forces
        AddForces();

        // update velocity
        UpdateVelocity();
        
        // update position
        transform.position += velocity * Time.deltaTime; 

        // calculate rotation
        UpdateRotation();
    }

    /// <summary>
    /// Check for a collision between this and another rigid body
    /// This must be implemented on a sub class of specific type
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public abstract MyCollision CheckCollision(MyRigidBody other);

    /// <summary>
    /// Update the rotational position of the object
    /// </summary>
    public void UpdateRotation()
    {

    }

    /// <summary>
    /// Resolves a collision between this and another rigid body
    /// </summary>
    /// <param name="body"></param>
    public void ResolveCollision(MyCollision collision)
    {
        if ((collision.bodyA is MyPlaneRigidBody && (collision.bodyB is MySphereRigidBody || collision.bodyB is MyBoxRigidBody)) || ((collision.bodyA is MySphereRigidBody || collision.bodyA is MyBoxRigidBody) && collision.bodyB is MyPlaneRigidBody))
        {
            // Calculate relative velocity
            Vector3 relativeVelocity = collision.bodyA.velocity - collision.bodyB.velocity;

            // Calculate relative velocity in terms of the normal direction
            float velocityAlongNormal = Vector3.Dot(relativeVelocity, collision.collisionNormal);

            // Do not resolve if velocities are separating
            if (velocityAlongNormal > 0)
                return;

            // Calculate restitution
            float e = Mathf.Min(collision.bodyA.restitution, collision.bodyB.restitution);

            // Calculate impulse scalar
            float j = -(1 + e) * velocityAlongNormal;
            j /= collision.bodyA.inverseMass + collision.bodyB.inverseMass;

            // Apply impulse
            Vector3 impulse = j * collision.collisionNormal;
            collision.bodyA.velocity += collision.bodyA.inverseMass * impulse;
            collision.bodyB.velocity -= collision.bodyB.inverseMass * impulse;

            // recalcuate the relative velocity
            relativeVelocity = collision.bodyA.velocity - collision.bodyB.velocity;

            // get the tangent vector
            Vector3 tangent = Vector3.Dot(relativeVelocity, collision.collisionNormal) * collision.collisionNormal;
            tangent.Normalize();

            // get the magnitude of the force along the friction vactor
            float jt = -Vector3.Dot(relativeVelocity, tangent);
            jt /= collision.bodyA.inverseMass + collision.bodyB.inverseMass;

            // get average coefficioent of friciotn in collision
            float mu = (collision.bodyA.staticFriction + collision.bodyB.staticFriction) / 2;

            // Clamp magnitude of friction and create impulse vector
            Vector3 frictionImpulse;
            if(Mathf.Abs( jt ) < j * mu)
                frictionImpulse = jt * tangent;
            else
            {
                dynamicFriction = (collision.bodyA.dynamicFriction + collision.bodyB.dynamicFriction) / 2;
                frictionImpulse = -j * tangent * dynamicFriction;;
            }
 
            // Apply
            collision.bodyA.velocity += collision.bodyA.inverseMass * frictionImpulse;
            collision.bodyB.velocity -= collision.bodyB.inverseMass * frictionImpulse;

            // correct the position
            PositionalCorrection(collision);
        }

        if (collision.bodyA is MySphereRigidBody && collision.bodyB is MySphereRigidBody)
        {
            // Calculate relative velocity
            Vector3 relativeVelocity = collision.bodyB.velocity - collision.bodyA.velocity;

            // Calculate relative velocity in terms of the normal direction
            float velocityAlongNormal = Vector3.Dot(relativeVelocity, collision.collisionNormal);

            // Do not resolve if velocities are separating
            if (velocityAlongNormal > 0)
                return;

            // Calculate restitution
            float e = Mathf.Min(collision.bodyA.restitution, collision.bodyB.restitution);

            // Calculate impulse scalar
            float j = -(1 + e) * velocityAlongNormal;
            j /= collision.bodyA.inverseMass + collision.bodyB.inverseMass;

            // Apply impulse
            Vector3 impulse = j * collision.collisionNormal;
            collision.bodyA.velocity -= collision.bodyA.inverseMass * impulse;
            collision.bodyB.velocity += collision.bodyB.inverseMass * impulse;

            // recalcuate the relative velocity
            relativeVelocity = collision.bodyA.velocity - collision.bodyB.velocity;

            // get the tangent vector
            Vector3 tangent = Vector3.Dot(relativeVelocity, collision.collisionNormal) * collision.collisionNormal;
            tangent.Normalize();

            // get the magnitude of the force along the friction vactor
            float jt = -Vector3.Dot(relativeVelocity, tangent);
            jt /= collision.bodyA.inverseMass + collision.bodyB.inverseMass;

            // get average coefficioent of friciotn in collision
            float mu = (collision.bodyA.staticFriction + collision.bodyB.staticFriction) / 2;

            // Clamp magnitude of friction and create impulse vector
            Vector3 frictionImpulse;
            if (Mathf.Abs(jt) < j * mu)
                frictionImpulse = jt * tangent;
            else
            {
                dynamicFriction = (collision.bodyA.dynamicFriction + collision.bodyB.dynamicFriction) / 2;
                frictionImpulse = -j * tangent * dynamicFriction; ;
            }

            // Apply
            collision.bodyA.velocity -= collision.bodyA.inverseMass * frictionImpulse;
            collision.bodyB.velocity += collision.bodyB.inverseMass * frictionImpulse;
        }
    }

    /// <summary>
    /// Deal with floating point errors in the positional calculations
    /// </summary>
    /// <param name="collision"></param>
    public void PositionalCorrection(MyCollision collision)
    {
        float percent = 0.2f; // usually 20% to 80%
        float slop = 0.01f; // usually 0.01 to 0.1

        if (collision.bodyA is MyBoxRigidBody || collision.bodyB is MyBoxRigidBody)
            percent = 0.01f;
        Vector3 correction = (Mathf.Max(Mathf.Abs(collision.penetration - slop), 0.0f) / (collision.bodyA.inverseMass + collision.bodyB.inverseMass)) * percent * collision.collisionNormal;
        collision.bodyA.transform.position += collision.bodyA.inverseMass * correction;
        collision.bodyB.transform.position -= collision.bodyB.inverseMass * correction;
    }

    /// <summary>
    /// Add up all of the component forces on the object
    /// </summary>
    public void AddForces()
    {
        // reset the net force
        netForce = Vector3.zero;

        // combine forces
        foreach(var force in forces)
            netForce += force;
    }

    /// <summary>
    /// Update the current velocity based on acceleration
    /// </summary>
    public void UpdateVelocity()
    {
        // calculate the current aceleration
        Vector3 acceleration = netForce / mass;

        // update velocity
        velocity += acceleration * Time.deltaTime;
    }
}
