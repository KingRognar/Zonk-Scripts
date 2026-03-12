
using UnityEngine;

public class Mask_Scr : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;
    [SerializeField] private float freq1, freq2;
    [SerializeField] private float delay1, delay2;
    [SerializeField] private float amp1, amp2;
    [Space(10)]
    [SerializeField] private float xRotMult, yRotMult;
    private void Start()
    {
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }
    void Update()
    {
        float xPosAdd = Mathf.Sin(Time.time * freq1 + delay1) * amp1;
        float yPosAdd = Mathf.Sin(Time.time * freq2 + delay2) * amp2;

        transform.localPosition = startPos + new Vector3(xPosAdd, yPosAdd, 0);

        transform.localRotation = startRot * Quaternion.Euler(xPosAdd * xRotMult, yPosAdd * yRotMult, 0);
    }
}
