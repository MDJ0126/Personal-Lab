public static class NotificationCenter
{
    public delegate void Subscribe(string text);
    private static event Subscribe _subscribes = null;

    /// <summary>
    /// 구독 리스트
    /// </summary>
    public static event Subscribe Subscribes
    {
        add
        {
            _subscribes -= value;
            _subscribes += value;
        }
        remove
        {
            _subscribes -= value;
        }
    }

    /// <summary>
    /// 알림
    /// </summary>
    /// <param name="title"></param>
    /// <param name="text"></param>
    public static void Broadcast(string text)
    {
        _subscribes?.Invoke(text);
    }
}