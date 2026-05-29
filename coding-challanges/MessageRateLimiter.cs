using System;
using System.Collections.Generic;

// ============================================================
// PROBLEM: Message Rate Limiter
// PATTERN: Sliding Window + Dictionary + Queue
// DIFFICULTY: Medium
// DATE: 2026-05-29
// ============================================================
//
// PROBLEM STATEMENT:
// A clinician chat system must prevent spam. Given a list of messages
// each with a timestamp (in seconds) and a clinician ID, return only
// the messages that should be allowed through applying this rule:
// No clinician can send more than 3 messages within any 10 second window.
// Messages arrive in chronological order.
//
// EXAMPLE INPUT:
// messages = [
//   (1,  "doc1"),
//   (2,  "doc1"),
//   (3,  "doc1"),
//   (5,  "doc1"),   // blocked — 1,2,3 still in window
//   (15, "doc1"),   // allowed — 1,2,3 have expired
//   (2,  "nurse1"),
//   (3,  "nurse1")
// ]
//
// EXPECTED OUTPUT:
// (1, doc1), (2, doc1), (3, doc1), (15, doc1), (2, nurse1), (3, nurse1)
//
// ============================================================
// KEY INSIGHT — SLIDING WINDOW:
// As time moves forward, old timestamps outside the 10 second window
// are no longer relevant and should be removed. The "window" slides
// forward with each new message timestamp.
//
// For a message at timestamp t:
//   Window = [t-10, t]
//   Any timestamp older than t-10 is EXPIRED and removed from the queue
//
// ============================================================
// DATA STRUCTURES USED:
//
// Dictionary<string, Queue<int>>
//   - Key:   clinician ID (string)
//   - Value: Queue of timestamps for that clinician's recent messages
//   - Dictionary gives O(1) lookup by clinician ID
//   - Queue gives O(1) enqueue (back) and dequeue (front)
//
// WHY QUEUE OVER STACK?
//   Queue is FIFO — First In First Out
//   We always want to check and remove the OLDEST timestamps first
//   which are naturally at the FRONT of the queue.
//   A Stack (LIFO) would give us the newest timestamp first — wrong.
//
// WHY QUEUE OVER JUST A COUNT (int)?
//   A count alone loses the actual timestamp values.
//   Without timestamps we can't tell if old messages have expired.
//   e.g. count=3 at t=15 — were those 3 messages at t=1,2,3 (expired)
//   or t=13,14,15 (still in window)? We can't know without timestamps.
//
// ============================================================
// COMPLEXITY:
//   Time:  O(n) amortised
//          Each timestamp is enqueued once and dequeued at most once
//          across the entire run. The while loop inside foreach does
//          NOT make this O(n²) — total dequeue operations <= n.
//          This is called AMORTISED ANALYSIS.
//
//   Space: O(c × w) where:
//          c = number of unique clinicians
//          w = max messages per clinician in any 10 second window
//          Since w is capped at 3 (the rate limit), this simplifies to O(c)
//
// ============================================================
// AMORTISED ANALYSIS EXPLAINED:
//   Even though the while loop is inside a foreach, each timestamp
//   is only ever enqueued ONCE and dequeued AT MOST ONCE across
//   the entire execution. So total operations = 2n = O(n).
//   Think of it like a bus: expensive to buy (€50,000) but averaged
//   across 10,000 passengers the cost per person is just €5.
//
// ============================================================
// THE BOUNCER ORDER — always follow this sequence:
//   1. Clean expired timestamps from queue front (Peek + Dequeue)
//   2. Check the count
//   3. Decide to allow or block
//   4. If allowed → Enqueue timestamp → Add to result
//   NEVER add to queue before checking — count will be wrong
//
// ============================================================

class Solution
{
    public static List<(int, string)> FilterMessages(List<(int, string)> messages)
    {
        // Key = clinician ID, Value = queue of recent allowed timestamps
        var clinicianMessages = new Dictionary<string, Queue<int>>();

        // Result list of allowed messages
        var answer = new List<(int, string)>();

        foreach (var mess in messages)
        {
            int timestamp = mess.Item1;
            string clinician = mess.Item2;

            // Step 1: Create an empty queue for new clinicians
            // TryAdd is safe to call every time:
            //   - New clinician  → creates empty Queue entry
            //   - Known clinician → does nothing, leaves existing queue
            clinicianMessages.TryAdd(clinician, new Queue<int>());

            // Step 2: Get the queue — store locally to avoid
            // repeated dictionary lookups (cleaner and faster)
            var queue = clinicianMessages[clinician];

            // Step 3: Remove expired timestamps from the front of the queue
            // A timestamp is expired if it falls outside the 10 second window
            // Window for current message = [timestamp-10, timestamp]
            // Peek() checks the oldest timestamp (front of queue) — O(1)
            // Dequeue() removes it — O(1)
            while (queue.Count > 0 && queue.Peek() <= timestamp - 10)
            {
                queue.Dequeue();
            }

            // Step 4: Check if clinician is under the rate limit
            if (queue.Count < 3)
            {
                // Allowed — record this timestamp and add to result
                queue.Enqueue(timestamp);
                answer.Add(mess);
            }
            // else: blocked — do nothing, message is dropped
        }

        return answer;
    }

    static void Main()
    {
        var messages = new List<(int, string)>
        {
            (1,  "doc1"),
            (2,  "doc1"),
            (3,  "doc1"),
            (5,  "doc1"),   // blocked — 1,2,3 still within window (5-10=-5)
            (15, "doc1"),   // allowed — 1,2,3 expired (15-10=5)
            (2,  "nurse1"),
            (3,  "nurse1")
        };

        var result = FilterMessages(messages);

        Console.WriteLine("Output:");
        foreach (var msg in result)
        {
            Console.WriteLine($"{msg.Item1}, {msg.Item2}");
        }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("1, doc1");
        Console.WriteLine("2, doc1");
        Console.WriteLine("3, doc1");
        Console.WriteLine("15, doc1");
        Console.WriteLine("2, nurse1");
        Console.WriteLine("3, nurse1");
    }
}

// ============================================================
// INTERVIEW TALKING POINTS:
//
// Q: Why did you use a Queue over a Stack?
// A: Queue is FIFO — the oldest timestamp is always at the front,
//    making expiry checks O(1) with Peek() and Dequeue(). A Stack
//    would give me the newest timestamp first which is the opposite
//    of what I need for a sliding window.
//
// Q: Why not just store a count per clinician instead of timestamps?
// A: A count alone loses temporal information. Without the actual
//    timestamps I can't determine which messages have expired outside
//    the 10 second window, leading to incorrect blocking or allowing.
//
// Q: Isn't your while loop inside a foreach making this O(n²)?
// A: No — this is O(n) amortised. Each timestamp is enqueued exactly
//    once and dequeued at most once across the entire run. So total
//    dequeue operations across all iterations is at most n, giving
//    us O(n) overall.
//
// Q: What is the space complexity?
// A: O(c) where c is the number of unique clinicians. The queue per
//    clinician is bounded by the rate limit of 3, so it doesn't
//    scale with n — only with the number of distinct clinicians.
//
// Q: How would you handle this at scale — 500k clinicians?
// A: In a distributed system I'd move this logic to a Redis cache
//    using sorted sets per clinician ID, with TTL-based expiry.
//    This avoids in-memory state on a single server and works
//    across multiple instances horizontally.
// ============================================================
