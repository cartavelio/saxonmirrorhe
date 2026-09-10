package net.sf.saxon.trace;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

public class Locksmith
{
	public static Map<String, Long> lockCounts = new ConcurrentHashMap<>();

	public static void countLock(String lockName)
	{
		if (lockCounts.containsKey(lockName))
		{
			lockCounts.put(lockName, lockCounts.get(lockName) + 1);
		}
		else
		{
			lockCounts.put(lockName, 1L);
		}
	}

	static
	{
		Runtime.getRuntime().addShutdownHook(new Thread(new Runnable()
		{
			@Override
			public void run()
			{
				for (Map.Entry<String, Long> entry : lockCounts.entrySet())
				{
					System.out.println("LOCK COUNT: " + entry.getKey() + " : " + entry.getValue());
				}
			}
		}));
	}
}
