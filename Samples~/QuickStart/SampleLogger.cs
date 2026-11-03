using UnityEngine;

public class SampleLogger : MonoBehaviour
{
    private const float Interval = 0.5f;

    private float nextLogTime;
    private int packet;

    private void Update()
    {
        if (Time.time < nextLogTime)
            return;

        nextLogTime = Time.time + Interval;
        packet++;

        Debug.Log($"[Networking] Packet {packet} received");

        if (packet % 5 == 0)
            Debug.LogWarning("[Shop] Purchase failed: not enough coins", this);
    }
}
