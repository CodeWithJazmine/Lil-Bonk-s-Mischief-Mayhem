public class CountdownTimer
{
    bool timeReached = false;
    float currentTime = 0f;

    public void Tick(float deltaTime)
    {
        currentTime -= deltaTime;

        if (currentTime <= 0)
        {
            timeReached = true;
        }
    }

    public void Reset(float limit)
    {
        currentTime = limit;
        timeReached = false;
    }

    public bool TimeReached()
    {
        return timeReached;
    }
}
