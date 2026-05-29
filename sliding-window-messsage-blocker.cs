using System;
using System.Collections.Generic;

/*
PROBLEM:
Given a list of messages (timestamp, clinician), allow at most
3 messages per clinician within a 10-second sliding window.

APPROACH:
- Use a Dictionary<clinician, Queue<timestamps>>
- Each queue represents a sliding time window
- Remove expired timestamps (older than 10 seconds)
- Only allow message if queue size < 3

TYPE:
✅ Sliding Window + Hash Map (Dictionary)
✅ Queue-based rate limiting
*/

public class HelloWorld
{
    public static void Main(string[] args)
    {
        var messages = new List<(int, string)>
        {
            (1, "doc1"),
            (2, "doc1"),
            (3, "doc1"),
            (5, "doc1"),
            (15, "doc1"),
            (2, "nurse1"),
            (3, "nurse1"),
        };

        var allowed = Solution(messages);

        Console.WriteLine("\n--- Allowed Messages ---");
        foreach (var item in allowed)
        {
            Console.WriteLine($"{item.Item1}, {item.Item2}");
        }
    }

    public static List<(int, string)> Solution(List<(int, string)> messages)
    {
        // clinician -> timestamps in last 10s window
        var clinicianMessages = new Dictionary<string, Queue<int>>();

        var answer = new List<(int, string)>();

        foreach (var mess in messages)
        {
            int timestamp = mess.Item1;
            string clinician = mess.Item2;

            // Ensure queue exists
            clinicianMessages.TryAdd(clinician, new Queue<int>());

            var queue = clinicianMessages[clinician];

            // ✅ Remove expired timestamps (older than 10 seconds)
            while (queue.Count > 0 && queue.Peek() <= timestamp - 10)
            {
                queue.Dequeue();
            }

            /*
            ✅ Check constraint:
            Allow max 3 messages per clinician in sliding window
            */
            if (queue.Count < 3)
            {
                queue.Enqueue(timestamp);
                answer.Add(mess);
            }
        }

        return answer;
    }
}

/*
TIME COMPLEXITY:
O(n)
- Each message is processed once
- Each timestamp is enqueued and dequeued at most once

SPACE COMPLEXITY:
O(n)
- In worst case all timestamps stored

INTERVIEW FOLLOW-UPS:
1. What if timestamps are not sorted?
   → Sort first: O(n log n)

2. What if window is dynamic?
   → Replace 10 with a parameter

3. What if messages arrive in real-time?
   → This becomes a rate limiter system

4. What if distributed system?
   → Use Redis + sliding window counters

KEY PATTERN:
✅ Sliding Window using Queue
✅ HashMap + FIFO structure
*/
``
