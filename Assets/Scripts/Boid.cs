using UnityEngine;
using System.Collections.Generic;

// add option for badboids and goodboids that avoid eachother
// or badboid you can control with wasd

// thanks to jackaperkins for providing a video and source
// https://github.com/jackaperkins/boids
public class Boid : MonoBehaviour {

    public LayerMask boidLayer;
    public LayerMask wallLayer;
    public Transform model;
    public MeshRenderer meshRenderer;
    public Collider col;

    Vector3 vel = Vector3.zero;
    Vector3 cvel;
    Transform tf;
    const float friendRadius = 4.0f;    // radius to search for friends
    const float crowdRadius = friendRadius / 1.5f;
    const float obstacleAvoid = 3.0f;   // how far ahead to look to avoid obstacles

    float maxSpeed;

    const int maxFriends = 16;
    List<Boid> friends = new List<Boid>(maxFriends);

    RaycastHit hit;
    Collider[] friendCols = new Collider[maxFriends];

    public Boids boids { set; get; }

    public void Start() {
        maxSpeed = Random.Range(4.0f, 6.0f);

        // refs
        Vector2 v = Random.insideUnitCircle.normalized * maxSpeed;
        vel = new Vector3(v.x, 0.0f, v.y);
        cvel = vel;
        tf = transform;

        // set color based on speed
        //MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        //float norm = (maxSpeed - speedRange.x) / (speedRange.y - speedRange.x);
        //Color c = Color.HSVToRGB((120.0f * (1.0f - norm) + 60.0f) / 360.0f, 1.0f, 1.0f);
        //mpb.SetColor("_Color", c);
        //meshRenderer.SetPropertyBlock(mpb);
    }

    float lastFind; // dont update friend list every frame
    void Update() {
        lastFind -= Time.deltaTime;
        if (lastFind < 0.0f) {
            FindFriends();
            lastFind = Random.value * 0.2f + 0.1f;
        }
        Flock();
    }

    private void FindFriends() {
        friends.Clear();
        int count = Physics.OverlapSphereNonAlloc(tf.position, friendRadius, friendCols, boidLayer.value);
        for (int i = 0; i < count; ++i) {
            if (friendCols[i] == col) {
                continue;
            }
            Boid f = boids.GetBoid(friendCols[i]);
            friends.Add(f);
        }
    }

    // main velocity calculation function
    private void Flock() {
        vel += GetAvgDir();			// rename to getAlignment()
        vel += GetAvoidDir();		// rename to getSeparation()
        vel += GetAvoidWalls() * 5.0f;
        vel += GetCohesion();

        if (vel.sqrMagnitude > maxSpeed * maxSpeed) {
            vel = vel.normalized * maxSpeed;
        }

        vel.y = 0.0f;
        cvel.y = 0.0f;
        cvel = Vector3.Lerp(cvel, vel, Time.deltaTime * 3.0f);

        // maybe just use kinematic rigidbodies?
        // where you set movement or something but they will still collide with walls
        if (Mathf.Abs(tf.position.x) > 49.0f || Mathf.Abs(tf.position.z) > 49.0f) {
            cvel = -tf.position.normalized * maxSpeed;
        }

        tf.position += cvel * Time.deltaTime;

        tf.rotation = Quaternion.LookRotation(cvel);
        model.Rotate(new Vector3(0.0f, 0.0f, Time.deltaTime * 20.0f * maxSpeed));
    }


    Vector3 GetAvgDir() {
        Vector3 steer = Vector3.zero;
        int count = friends.Count;
        for (int i = 0; i < count; ++i) {
            Boid f = friends[i];
            Vector3 vel = f.vel.normalized;
            vel /= (tf.position - f.tf.position).magnitude;
            steer += vel;
        }

        return steer;
    }

    Vector3 GetAvoidDir() {
        Vector3 steer = new Vector3();
        int count = friends.Count;
        for (int i = 0; i < count; ++i) {
            Boid f = friends[i];
            Vector3 diff = tf.position - f.tf.position;
            float sqrDist = diff.sqrMagnitude;
            if (sqrDist < crowdRadius * crowdRadius) {
                // div by one distance to normalize and another to weight (avoids harder closer things)
                diff /= sqrDist;
                steer += diff;
            }
        }

        return steer;
    }

    bool castDir = true;
    Vector3 GetAvoidWalls() {
        Vector3 steer = new Vector3();

        // alternate angle of cast
        Vector3 forward = (tf.forward + tf.right * (castDir ? 0.3f : -0.3f));
        castDir = !castDir;
        //Debug.DrawRay(tf.position, forward.normalized * obstacleAvoid, Color.magenta);

        if (Physics.Raycast(tf.position, forward, out hit, obstacleAvoid, wallLayer.value)) {
            float dist = (tf.position - hit.point).sqrMagnitude;
            float distClamp = Mathf.Max(0.0001f, dist);
            steer += Vector3.ProjectOnPlane(vel, hit.normal) / distClamp;
            steer += hit.normal / distClamp * 2.0f; // kinda just tweak this to reduce penetration
        }

        return steer;
    }

    // try to stay together with other boids
    Vector3 GetCohesion() {
        Vector3 sum = new Vector3();
        int count = friends.Count;
        for (int i = 0; i < count; ++i) {
            sum += friends[i].tf.position;
        }

        if (count > 0) {
            sum /= count;
            return (sum - tf.position) * 0.05f;
        }
        return Vector3.zero;
    }

}
