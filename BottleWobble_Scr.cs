using System;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class BottleWobble_Scr : MonoBehaviour
{
    private Renderer rend;

    public float depletingWobbleX = 0, depletingWobbleZ = 0, depletingFoam = 0;
    public float maxWobble = 1f;
    public float dissipation = 1f;
    public float freq = 1f;
    public float foamBuildTime = 0.2f, foamDeplTime = 0;

    float foamSave = 0;
    float fillCur = 0.8f;

    Vector3 prevPos = Vector3.zero, prevRot = Vector3.zero;


    //TODO: прибраться

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {

        depletingWobbleX = Mathf.Lerp(depletingWobbleX, 0, Time.deltaTime * dissipation);
        depletingWobbleZ = Mathf.Lerp(depletingWobbleZ, 0, Time.deltaTime * dissipation);


        float wobbleX = depletingWobbleX * Mathf.Sin(freq * Time.time);
        float wobbleZ = depletingWobbleZ * Mathf.Sin(freq * Time.time);

        rend.material.SetFloat("_WobbleX", wobbleZ);
        rend.material.SetFloat("_WobbleZ", wobbleX);
        rend.material.SetFloat("_FoamThick", depletingFoam);
        rend.material.SetFloat("_Fill", Mathf.Clamp(fillCur + depletingFoam / 2, 0, 1));

        Vector3 velocity = (prevPos - transform.position) / Time.deltaTime;
        Vector3 angVelocity = transform.rotation.eulerAngles - prevRot;

        float xMovement = velocity.x + angVelocity.z / 5;
        float zMovement = velocity.z + angVelocity.x / 5;

        float wobbleChngX = Mathf.Clamp(xMovement * maxWobble, -maxWobble, maxWobble);
        float wobbleChngZ = Mathf.Clamp(zMovement * maxWobble, -maxWobble, maxWobble);
        float foamChng = Mathf.Clamp(Mathf.Max(Mathf.Abs(wobbleChngX)/5, Mathf.Abs(wobbleChngZ))/5, 0, 0.2f);

        if (foamChng != 0)
        {
            foamDeplTime = Time.time + foamBuildTime;
            foamSave = Mathf.Max(foamSave, foamChng);
        }
        if (foamDeplTime > Time.time)
        {
            depletingFoam = Mathf.Lerp(depletingFoam, foamSave, 1 - ((foamDeplTime - Time.time) / foamBuildTime));
        }
        else
        {
            depletingFoam = Mathf.Lerp(depletingFoam, 0, Time.deltaTime * dissipation / 3);
            if (depletingFoam < 0.005f) depletingFoam = 0;
        }

        //depletingFoam += foamChng;
        //depletingFoam = Mathf.Clamp(depletingFoam, 0, 0.4f);
        depletingWobbleX += wobbleChngX;
        depletingWobbleZ += wobbleChngZ;

        prevPos = transform.position;
        prevRot = transform.rotation.eulerAngles;

    }
}
