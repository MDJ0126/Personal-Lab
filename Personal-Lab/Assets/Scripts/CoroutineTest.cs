using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CoroutineTest : MonoBehaviour
{
	private void Start()
	{
		Thread thread1 = new Thread(() => DelayedHelloWorld(2f));
		Thread thread2 = new Thread(() => DelayedHelloWorld(1f));
		Thread thread3 = new Thread(() => DelayedHelloWorld(3f));

		thread1.Start();
		thread2.Start();
		thread3.Start();
	}

	private void DelayedHelloWorld(float seconds)
	{
		Thread.Sleep((int)(1000 * seconds));
		Debug.Log("Hello World!");
	}
}
