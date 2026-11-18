using UnityEngine;

public class SampleLogger : MonoBehaviour
{
    private const float LOG_INTERVAL_SECONDS = 0.5f;

    private float nextLogTime;
    private int packet;

    private void Update()
    {
        if (Time.time < nextLogTime)
            return;

        nextLogTime = Time.time + LOG_INTERVAL_SECONDS;
        packet++;

        Debug.Log($"[Networking] Packet {packet} received");

        if (packet % 5 == 0)
            Debug.LogWarning("[Shop] Purchase failed: not enough coins", this);
    }
}
