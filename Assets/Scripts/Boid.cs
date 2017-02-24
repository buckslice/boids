using UnityEngine;
using System.Collections.Generic;

// add option for badboids and goodboids that avoid eachother
// or badboid you can control with wasd

// thanks to jackaperkins for providing a video and source
// https://github.com/jackaperkins/boids
public class Boid : MonoBehaviour {

    public Transform model;
    public MeshRenderer meshRenderer;
    public Collider col;

    Vector3 vel = Vector3.zero;
    Vector3 cvel;
    Transform tf;
    public static float friendRadius = 4.0f;    // radius to search for friends
    public static float crowdRadius = friendRadius / 1.5f;
    public static float obstacleAvoid = 4.0f;   // how far ahead to look to avoid obstacles

    float maxSpeed;

    const int maxFriends = 16;
    List<Boid> friends = new List<Boid>(maxFriends);

    RaycastHit hit;
    Collider[] friendCols = new Collider[maxFriends];

    public Boids boids { set; get; }

    public void Awake() {
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
        if (lastFind < 0.0f || boids.checkFriends) {
            FindFriends();
            lastFind = Random.value * 0.2f + 0.1f;
        }
        Flock();
    }

    void FindFriends() {
        friends.Clear();
        if (friendRadius == 0.0f) {
            return;
        }
        int count = Physics.OverlapSphereNonAlloc(tf.position, friendRadius, friendCols, boids.boidLayer.value);
        for (int i = 0; i < count; ++i) {
            if (friendCols[i] == col) {
                continue;
            }
            Boid f = boids.GetBoid(friendCols[i]);
            friends.Add(f);
        }
    }

    // main velocity calculation function
    void Flock() {

        vel += GetAvgDir();
        vel += GetAvoidDir();
        vel += GetCohesion();
        vel += GetAvoidWalls();

        //Vector3 avgDir = GetAvgDir();
        //vel += avgDir;
        //Vector3 avoidDir = GetAvoidDir();
        //vel += avoidDir;
        //Vector3 cohesion = GetCohesion();
        //vel += cohesion;
        //Vector3 avoidWalls = GetAvoidWalls();
        //vel += avoidWalls;
        //float s = 10.0f;
        //Debug.DrawRay(tf.position, avgDir*s, Color.blue);
        //Debug.DrawRay(tf.position, avoidDir*s, Color.magenta);
        //Debug.DrawRay(tf.position, cohesion*s, Color.green);
        //Debug.DrawRay(tf.position, avoidWalls*s, Color.red);

        //Debug.DrawRay(tf.position, vel, Color.magenta);

        vel.y = 0.0f;
        vel = vel.normalized * maxSpeed;
        cvel.y = 0.0f;
        // lerp towards actual vel (smooths it a little bit)
        cvel = Vector3.Lerp(cvel, vel, Time.deltaTime * 4.0f);

        // maybe just use kinematic rigidbodies?
        // where you set movement or something but they will still collide with walls
        if (Mathf.Abs(tf.position.x) > 49.0f || Mathf.Abs(tf.position.z) > 49.0f) {
            cvel = -tf.position.normalized * maxSpeed;
            //vel = -tf.position.normalized * maxSpeed;
        }

        tf.position += cvel * Time.deltaTime;
        //tf.position += vel * Time.deltaTime;

        tf.rotation = Quaternion.LookRotation(cvel);
        //tf.rotation = Quaternion.LookRotation(vel);
        //model.Rotate(new Vector3(0.0f, 0.0f, Time.deltaTime * 20.0f * maxSpeed));

        Vector3 q = tf.rotation.eulerAngles;
        model.localRotation = Quaternion.Euler(0.0f, 0.0f, q.y + Time.time * 2.0f);
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

        if (Physics.Raycast(tf.position, forward, out hit, obstacleAvoid, boids.wallLayer.value)) {
            float dist = (tf.position - hit.point).sqrMagnitude;
            float distClamp = Mathf.Max(0.0001f, dist);
            steer += Vector3.ProjectOnPlane(vel, hit.normal) / distClamp;
            steer += hit.normal / distClamp * 2.0f; // kinda just tweak this to reduce penetration
        }

        return steer * 5.0f;
    }

    // try to stay together with other boids
    Vector3 GetCohesion() {
        int count = friends.Count;
        if (count == 0) {
            return Vector3.zero;
        }

        Vector3 sum = new Vector3();
        for (int i = 0; i < count; ++i) {
            sum += friends[i].tf.position;
        }
        sum /= count;
        return (sum - tf.position) * 0.05f; // cohesion force?
    }

}
