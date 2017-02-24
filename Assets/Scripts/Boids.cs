using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Boids : MonoBehaviour {
    public GameObject boidPrefab;

    public int initialSpawns = 200;

    public LayerMask boidLayer;
    public LayerMask wallLayer;
    public LayerMask groundLayer;

    // used to find boid reference by collider to avoid many GetComponent calls
    private Dictionary<Collider, Boid> lookup = new Dictionary<Collider, Boid>();

    public bool checkFriends { get; set; }

    public Text boidText;
    public Slider friendRadSlider;
    public Text sliderText;
    int boidCount = 0;

    // Use this for initialization
    void Awake() {
        checkFriends = false;
        for (int i = 0; i < initialSpawns; ++i) {
            float x = Random.value * 100.0f - 50.0f;
            float z = Random.value * 100.0f - 50.0f;

            SpawnBoid(new Vector3(x * 0.98f, 1.4f, z * 0.98f));
        }

        friendRadSlider.onValueChanged.AddListener(delegate { OnSliderChanged(); });
    }

    void OnSliderChanged() {
        Boid.friendRadius = friendRadSlider.value;
        Boid.crowdRadius = Mathf.Min(Boid.friendRadius / 1.5f, 5.0f);
        sliderText.text = Boid.friendRadius.ToString("F1");
    }

    void SpawnBoid(Vector3 pos) {
        GameObject go = Instantiate(boidPrefab, pos, Quaternion.identity, transform);
        Boid boid = go.GetComponent<Boid>();
        boid.boids = this;
        lookup[boid.col] = boid;
        boidText.text = "" + ++boidCount;
    }

    public Boid GetBoid(Collider c) {
        return lookup[c];
    }

    Collider[] cols = new Collider[64];
    RaycastHit hit;
    void LateUpdate() {
        checkFriends = false;

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f, groundLayer.value)) {
                Vector3 p = hit.point;
                for (int i = 0; i < 10; ++i) {
                    float randX = Random.value - 0.5f;  // give them random spawn a bit
                    float randZ = Random.value - 0.5f;  // boid code slightly bugged when boids have exact same positions (todo fix)
                    SpawnBoid(new Vector3(p.x + randX, 1.4f, p.z + randZ));
                }
            }
        }

        if (Input.GetMouseButtonDown(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f, groundLayer.value)) {
                Vector3 p = hit.point;
                int count = Physics.OverlapSphereNonAlloc(hit.point, 8.0f, cols, boidLayer.value);
                if (count > 0) {
                    for (int i = 0; i < count; ++i) {
                        Destroy(cols[i].gameObject);
                    }
                    checkFriends = true;
                }
                boidCount -= count;
                boidText.text = "" + boidCount;
            }

        }


    }
}
