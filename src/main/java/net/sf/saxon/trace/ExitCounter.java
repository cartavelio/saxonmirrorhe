////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
