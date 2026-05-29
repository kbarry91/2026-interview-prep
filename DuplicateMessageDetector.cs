using System;
using System.Collections.Generic;

// ============================================================
// PROBLEM: Duplicate Message Detector
// PATTERN: HashSet for O(1) membership tracking
// DIFFICULTY: Easy/Medium
// DATE: 2026-05-29
// ============================================================
//
// PROBLEM STATEMENT:
// A clinician chat system receives messages that may be delivered
// multiple times due to network issues. Given a list of messages
// with an ID and content, return only the first occurrence of
// each message, preserving original order.
//
// KEY INSIGHT:
// Only the message ID determines uniqueness — content is irrelevant.
// Identifying what data actually matters is a critical interview skill.
//
// ============================================================
// DATA STRUCTURES USED:
//
// HashSet<int>
//   - Stores only the message IDs we have seen
//   - O(1) average time for Add and Contains
//   - Use HashSet (not List) when you only need membership checking
//   - Use HashSet (not Dictionary) when you don't need a value, just a key
//
// List<(int, string)>
//   - Stores the result messages in original order
//   - Preserves order which HashSet alone cannot do
//
// ============================================================
// COMPLEXITY:
//   Time:  O(n) — single pass through all n messages
//   Space: O(n) — worst case all messages are unique,
//                 both HashSet and result List grow to size n
//
// ============================================================
// KEY C# TRICK:
//   HashSet.Add() returns a bool:
//     true  = item was NEW, successfully added
//     false = item ALREADY EXISTED, nothing changed
//
//   This means you can replace:
//     if (!seen.Contains(id)) { seen.Add(id); result.Add(msg); }
//   With the cleaner single operation:
//     if (seen.Add(id)) { result.Add(msg); }
//
// ============================================================
// COMMON MISTAKES TO AVOID:
//   1. Storing the whole tuple in HashSet instead of just the ID
//      WRONG: new HashSet<(int, string)>()
//      RIGHT: new HashSet<int>()
//
//   2. Inverting the Add logic with !
//      WRONG: if (!seen.Add(id)) → adds duplicates not uniques
//      RIGHT: if (seen.Add(id))  → adds only new messages
//
//   3. Using a List instead of HashSet for membership checking
//      List.Contains() is O(n) — gets slow with large inputs
//      HashSet.Contains() is O(1) — always fast
//
// ============================================================

class Solution
{
    public static List<(int, string)> RemoveDuplicates(List<(int, string)> messages)
    {
        // Track which message IDs we have already seen
        // HashSet gives us O(1) lookup — faster than List O(n)
        var seen = new HashSet<int>();

        // Result list preserves original order of first occurrences
        var result = new List<(int, string)>();

        foreach (var msg in messages)
        {
            // seen.Add() returns true if ID is new, false if already exists
            // This single operation both checks AND adds in one step
            if (seen.Add(msg.Item1))
            {
                // ID was new — add the full message to our result
                result.Add(msg);
            }
            // else: ID already seen — skip this duplicate
        }

        return result;
    }

    static void Main()
    {
        var messages = new List<(int, string)>
        {
            (1, "Patient needs review"),
            (2, "Check lab results"),
            (1, "Patient needs review"),  // duplicate — should be skipped
            (3, "Urgent: Room 4"),
            (2, "Check lab results"),     // duplicate — should be skipped
            (3, "Urgent: Room 4")         // duplicate — should be skipped
        };

        var result = RemoveDuplicates(messages);

        Console.WriteLine("Output:");
        foreach (var msg in result)
        {
            Console.WriteLine($"{msg.Item1}, {msg.Item2}");
        }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("1, Patient needs review");
        Console.WriteLine("2, Check lab results");
        Console.WriteLine("3, Urgent: Room 4");
    }
}

// ============================================================
// INTERVIEW TALKING POINTS:
//
// Q: Why HashSet over List for tracking seen IDs?
// A: HashSet gives O(1) lookup vs List's O(n). With millions of
//    messages the difference is enormous.
//
// Q: Why not store the full tuple in the HashSet?
// A: Only the ID determines uniqueness. Storing content too would
//    allow (1, "different content") to pass through as a new message
//    which violates the requirements.
//
// Q: What is the space complexity?
// A: O(n) — in the worst case where all messages are unique, both
//    the HashSet and result List grow to the size of the input.
//
// Q: Could you do this in O(1) space?
// A: Not while preserving order. If order didn't matter you could
//    sort and deduplicate in place, but that would be O(n log n) time.
// ============================================================
