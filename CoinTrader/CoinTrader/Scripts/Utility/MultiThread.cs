using System;
using System.Collections.Generic;
using System.Threading;

public class MultiThread
{
    public delegate void Method();
    public delegate void Method_Object(object obj);

    public class ThreadInfo
    {
        public static int Count { get; private set; } = 0;

        private Method _method;
        private Method_Object _method_Object;
        public CancellationTokenSource cancellationTokenSource;
        private Action _onFinished;
        public object arg;

        public ThreadInfo(Method method, CancellationTokenSource cancellationTokenSource, Action onFinished)
        {
            _method = method;
            _method_Object = null;
            this.arg = null;
            this.cancellationTokenSource = cancellationTokenSource;
            _onFinished = onFinished;
        }

        public ThreadInfo(Method_Object method_Object, object arg, CancellationTokenSource cancellationTokenSource, Action onFinished)
        {
            _method = null;
            _method_Object = method_Object;
            this.arg = arg;
            this.cancellationTokenSource = cancellationTokenSource;
            _onFinished = onFinished;
        }

        public void Process(object arg)
        {
            ++Count;
            _method?.Invoke();
            _method_Object?.Invoke(arg);
            _onFinished?.Invoke();
            --Count;
        }

        public void Stop()
        {
            var threadInfo = _threadInfos.Find(info => info.Equals(this));
            _threadInfos.Remove(threadInfo);
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }

    private static List<ThreadInfo> _threadInfos = new List<ThreadInfo>();

    /// <summary>
    /// 현재 실행 중인 쓰레드 수
    /// </summary>
    public static int ProcessThreadCount { get { return ThreadInfo.Count; } }

    // 쓰레드가 사용되고 있을 때는 메모리 1MB 가량 차지한다.
    // 동시 실행되고 있는 쓰레드가 1000개는 곧 메모리 1GB를 사용하고 있다는 것을 의미한다.
    public static readonly int THREAD_MIN = 50;
    public static readonly int THREAD_MAX = 2000;   // 동시 사용 메모리 최대 2GB로 하자

    static MultiThread()
    {
        ThreadPool.SetMinThreads(THREAD_MIN, THREAD_MIN);
        ThreadPool.SetMaxThreads(THREAD_MAX, THREAD_MAX);
    }

    /// <summary>
    /// 쓰레드로 함수 실행
    /// </summary>
    /// <param name="method">메소드</param>
    /// <param name="onFinished">종료 이벤트</param>
    public static ThreadInfo Start(Method method, Action onFinished = null)
    {
        if (method != null)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            ThreadInfo methodGroup = new ThreadInfo(method, cts, onFinished);
            ThreadPool.QueueUserWorkItem(methodGroup.Process, cts.Token);
            _threadInfos.Add(methodGroup);
            return methodGroup;
        }
        return default(ThreadInfo);
    }

    /// <summary>
    /// 쓰레드로 함수 실행
    /// </summary>
    /// <param name="method">메소드</param>
    /// <param name="arg">메소드 매개변수</param>
    /// <param name="onFinished">종료 이벤트</param>
    public static ThreadInfo Start(Method_Object method, object arg, Action onFinished = null)
    {
        if (method != null)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            ThreadInfo methodGroup = new ThreadInfo(method, arg, cts, onFinished);
            ThreadPool.QueueUserWorkItem(methodGroup.Process, cts.Token);
            _threadInfos.Add(methodGroup);
            return methodGroup;
        }
        return default(ThreadInfo);
    }

    ///// <summary>
    ///// 쓰레드 정지
    ///// </summary>
    ///// <param name="methodGroup"></param>
    //public static void Stop(ThreadInfo methodGroup)
    //{
    //    if (_threadInfos.Exists(data => data.Equals(methodGroup)))
    //    {
    //        methodGroup.cancellationTokenSource.Cancel();
    //        methodGroup.cancellationTokenSource.Dispose();
    //    }
    //}

    /// <summary>
    /// 모든 멀티 쓰레드 정지
    /// </summary>
    public static void Release()
    {
        // 실행 중인 쓰레드 정지 및 해제
        for (int i = 0; i < _threadInfos.Count; i++)
        {
            _threadInfos[i].cancellationTokenSource.Cancel();
            _threadInfos[i].cancellationTokenSource.Dispose();
        }
    }
}