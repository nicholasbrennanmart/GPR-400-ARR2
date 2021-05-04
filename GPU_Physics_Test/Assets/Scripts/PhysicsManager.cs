using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    struct circleCollider
    {
        public Vector2 position;
        public float radius;
        public Vector2 velocity;
        public float mass;
        public Vector2Int dummy;
    }

    [SerializeField]
    ComputeShader physicsShader;

    [SerializeField]
    int numCircles;
    [SerializeField]
    Vector2 maxSpawnPosition, maxSpawnVelocity;
    [SerializeField]
    float maxMass, maxRadius;

    circleCollider[] circleList;
    ComputeBuffer circleListBuffer;

    GameObject[] circleGameObjectList;
    [SerializeField]
    GameObject circlePrefab;

    //kernel id's
    int handleCollisionsKernel,
        updatePhysicsKernel;        
    
    // Start is called before the first frame update
    void Start()
    {
        InitCircles();
        InitShader();
    }

    // Update is called once per frame
    void Update()
    {
        ShaderTick();
        UpdateGameObjects();
    }

    void InitShader()
    {
        handleCollisionsKernel = physicsShader.FindKernel("handleCollisions");
        updatePhysicsKernel = physicsShader.FindKernel("updatePhysics");

        physicsShader.SetInt("floatToInt", 2 << 17);
        physicsShader.SetFloat("intToFloat", 1f / (2 << 17));

        circleListBuffer = new ComputeBuffer(circleList.Length, sizeof(float) * 6 + sizeof(int) * 2);
        circleListBuffer.SetData(circleList);

        physicsShader.SetBuffer(handleCollisionsKernel, "circleBuffer", circleListBuffer);
        physicsShader.SetBuffer(updatePhysicsKernel, "circleBuffer", circleListBuffer);
    }

    void ShaderTick()
    {
        physicsShader.SetFloat("deltaTime", Time.deltaTime);

        physicsShader.Dispatch(handleCollisionsKernel, circleList.Length / 8, circleList.Length / 8, 1);
        physicsShader.Dispatch(updatePhysicsKernel, circleList.Length / 8, 1, 1);

        circleListBuffer.GetData(circleList);
    }

    void InitCircles()
    {
        circleList = new circleCollider[numCircles];
        circleGameObjectList = new GameObject[numCircles];

        for (int i = 0; i < numCircles; i++)
        {
            circleList[i].position = new Vector2(Random.Range(-maxSpawnPosition.x, maxSpawnPosition.x), Random.Range(-maxSpawnPosition.y, maxSpawnPosition.y));
            circleList[i].velocity = new Vector2(Random.Range(-maxSpawnVelocity.x, maxSpawnVelocity.x), Random.Range(-maxSpawnVelocity.y, maxSpawnVelocity.y));
            circleList[i].radius = Random.Range(0.1f, maxRadius);
            circleList[i].mass = Random.Range(1, maxMass);

            circleGameObjectList[i] = Instantiate(circlePrefab);
            circleGameObjectList[i].transform.position = circleList[i].position;
            circleGameObjectList[i].transform.localScale = new Vector2(circleList[i].radius * 2, circleList[i].radius * 2);
        }


    }

    void UpdateGameObjects()
    {
        for (int i = 0; i < numCircles; i++)
        {
            circleGameObjectList[i].transform.position = circleList[i].position;
        }
    }
}
