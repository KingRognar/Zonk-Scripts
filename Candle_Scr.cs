using DG.Tweening;
using UnityEngine;

public class Candle_Scr : MonoBehaviour
{
    [SerializeField] private Light light;
    [SerializeField] private float changeTime = 0.1f, nextTime = 0f;
    [SerializeField] private float offsetMult = 1f;
    [SerializeField] private float baseIntensity = 500f, baseRange = 75f;
    private Vector3 startPos;
    private Vector3 targetPos;

    private void Start()
    {
        startPos = light.transform.localPosition;
        //Sequence sequence = DOTween.Sequence(this);
        //sequence.Append(transform.DOLocalMove(new Vector3(Random.value,0,0), 0.2f))
    }
    private void Update()
    {
        light.transform.localPosition = Vector3.Lerp(light.transform.localPosition, targetPos, Time.deltaTime * 50);
        //light.transform.localPosition += new Vector3(Mathf.Sin(Mathf.Sin(Time.time)), 0, 0);]
        if (nextTime < Time.time)
        {
            Vector3 offset = new Vector3(Random.value , Random.value, Random.value) * offsetMult;
            targetPos = startPos + offset;
            light.intensity = baseIntensity + Random.Range(-40, 40);
            light.range = baseRange + Random.Range(-5, 5);

            nextTime += changeTime;
        }
    }
}
