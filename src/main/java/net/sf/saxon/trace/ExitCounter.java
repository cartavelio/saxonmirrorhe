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

	public static void countCaller()
	{
		StackTraceElement[] traceElements = Thread.currentThread().getStackTrace();

		StackTraceElement threeBack = traceElements[3];
		count(threeBack.getClassName() + "." + threeBack.getMethodName() + ":" + threeBack.getLineNumber());
	}

	public static void countTrace(int count)
	{
		StackTraceElement[] traceElements = Thread.currentThread().getStackTrace();

		StringBuilder builder = new StringBuilder();

		for (int pos = 3; pos < 3+count; pos++)
		{
			StackTraceElement element = traceElements[pos];

			builder.append(
					element.getClassName()).append(".").append(element.getMethodName()).append(":").append(element.getLineNumber()).append(" ====> ");
		}

		count(builder.toString());
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
