package net.sf.saxon.trace;

import java.util.HashMap;
import java.util.Map;

public class ExitCounter
{
	public static Map<String, Long> eventCounts = new HashMap<>();

	public static void count(String name)
	{
		if (eventCounts.containsKey(name))
		{
			eventCounts.put(name, eventCounts.get(name) + 1);
		}
		else
		{
			eventCounts.put(name, 1L);
		}
	}

	static
	{
		Runtime.getRuntime().addShutdownHook(new Thread(new Runnable()
		{
			@Override
			public void run()
			{
				for (Map.Entry<String, Long> entry : eventCounts.entrySet())
				{
					System.out.println(entry.getKey() + " : " + entry.getValue());
				}
			}
		}));
	}
}
