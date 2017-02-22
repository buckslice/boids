using UnityEngine;
using System.Collections.Generic;


public class Boids : MonoBehaviour {
    public GameObject boidPrefab;

    public int initialSpawns = 200;

    // used to find boid reference by collider to avoid many GetComponent calls
    private Dictionary<Collider, Boid> lookup = new Dictionary<Collider, Boid>();

    // Use this for initialization
    void Start() {
        for (int i = 0; i < initialSpawns; ++i) {
            float x = Random.value * 100.0f - 50.0f;
            float z = Random.value * 100.0f - 50.0f;

            SpawnBoid(new Vector3(x * 0.98f, 1.4f, z * 0.98f));
        }
    }

    void SpawnBoid(Vector3 pos) {
        GameObject go = Instantiate(boidPrefab, pos, Quaternion.identity, transform);
        Boid boid = go.GetComponent<Boid>();
        boid.boids = this;
        lookup[boid.col] = boid;
    }

    public Boid GetBoid(Collider c) {
        return lookup[c];
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit info;
            if (Physics.Raycast(ray, out info)) {
                Vector3 p = info.point;
                SpawnBoid(new Vector3(p.x, 1.4f, p.z));
            }
        }
    }
}
