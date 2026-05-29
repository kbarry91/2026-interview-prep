using System;
using System.Collections.Generic;
using System.Linq;

// ============================================================
// PROBLEM: Message Priority Queue
// PATTERN: LINQ Sorting with multi-condition OrderBy
// DIFFICULTY: Medium
// DATE: 2026-05-29
// ============================================================
//
// PROBLEM STATEMENT:
// A clinician chat system needs to process messages in order of
// urgency, not arrival time. Each message has a priority level
// and content. Return messages sorted highest priority first.
// If two messages have the same priority, preserve original
// arrival order.
//
// Priority levels: 1 = Low, 2 = Medium, 3 = High, 4 = Critical
//
// EXAMPLE INPUT:
// messages = [
//   (2, "Check lab results"),
//   (4, "URGENT: Patient crashing"),
//   (1, "Admin update"),
//   (3, "Review medication"),
//   (4, "URGENT: Room 4 emergency"),
//   (2, "Follow up needed")
// ]
//
// EXPECTED OUTPUT:
// (4, "URGENT: Patient crashing")    ← arrived first of the 4s
// (4, "URGENT: Room 4 emergency")    ← arrived second of the 4s
// (3, "Review medication")
// (2, "Check lab results")           ← arrived first of the 2s
// (2, "Follow up needed")            ← arrived second of the 2s
// (1, "Admin update")
//
// ============================================================
// KEY INSIGHT — PRESERVING ARRIVAL ORDER:
// The original tuple only has (priority, content) — no index.
// Before sorting you must capture the original position using
// Select((msg, index) => ...) otherwise you lose arrival order
// and can't tiebreak correctly.
//
// ============================================================
// DATA STRUCTURES / CONCEPTS USED:
//
// LINQ (Language Integrated Query)
//   - C#'s equivalent of Java Streams
//   - Allows functional-style manipulation of collections
//   - Chains of operations: Select, OrderBy, ThenBy, ToList
//
// Select((item, index) => ...)
//   - Overload of Select that provides the element AND its index
//   - Used here to capture original arrival position before sorting
//
// OrderByDescending(x => x.Property)
//   - Sorts collection highest to lowest by the given property
//   - Primary sort condition
//
// ThenBy(x => x.Property)
//   - Secondary sort condition applied when primary values are equal
//   - Used here to sort by arrival index ascending (earliest first)
//
// ToList()
//   - Materialises the LINQ chain into a concrete List
//   - Always needed at the end of a LINQ chain to get a List back
//
// ============================================================
// COMPLEXITY:
//   Time:  O(n log n)
//          Dominated by OrderByDescending which uses TimSort internally
//          (same algorithm as Java's Collections.sort())
//          The Select operations are O(n) but insignificant next to sort
//
//   Space: O(n)
//          LINQ creates intermediate collections during the chain:
//          - First Select  → new tuple collection O(n)
//          - OrderBy       → sorted collection O(n)
//          - Final Select  → strips index O(n)
//          - ToList        → final result O(n)
//
// ============================================================
// WHAT IS O(n log n)?
//   Think of sorting a deck of 8 cards:
//   - log2(8) = 3  (how many times you can halve 8 before reaching 1)
//   - n = 8 cards
//   - Total operations ≈ 8 × 3 = 24
//
//   For 1000 items:
//   - log2(1000) ≈ 10
//   - Total operations ≈ 1000 × 10 = 10,000
//
//   Compare to O(n²) for 1000 items = 1,000,000 operations
//   O(n log n) is MUCH faster than O(n²) for large inputs
//   This is why good sorting algorithms matter at scale
//
// ============================================================
// COMMON MISTAKES TO AVOID:
//   1. Forgetting to capture index before sorting
//      Once sorted, original order is lost forever
//
//   2. Using List<int, string> — List only takes ONE type argument
//      WRONG: List<int, string>
//      RIGHT: List<(int, string)>
//
//   3. Forgetting ToList() at the end
//      LINQ chains are lazy — nothing executes until materialised
//      ToList() forces execution and returns a concrete List
//
//   4. Forgetting the final Select to strip the index
//      Without it your return type is (int, int, string) not (int, string)
//
// ============================================================

class Solution
{
    public static List<(int, string)> PrioritiseMessages(List<(int, string)> messages)
    {
        return messages
            // Step 1: Capture original arrival index before sorting
            // Transforms (priority, content) → (index, priority, content)
            .Select((msg, index) => (index, msg.Item1, msg.Item2))

            // Step 2: Sort by priority highest first (4 → 3 → 2 → 1)
            .OrderByDescending(x => x.Item2)

            // Step 3: For equal priorities, sort by arrival order earliest first
            .ThenBy(x => x.Item1)

            // Step 4: Strip the index — return to original (priority, content) shape
            .Select(x => (x.Item2, x.Item3))

            // Step 5: Materialise the LINQ chain into a concrete List
            .ToList();
    }

    static void Main()
    {
        var messages = new List<(int, string)>
        {
            (2, "Check lab results"),
            (4, "URGENT: Patient crashing"),
            (1, "Admin update"),
            (3, "Review medication"),
            (4, "URGENT: Room 4 emergency"),
            (2, "Follow up needed")
        };

        var result = PrioritiseMessages(messages);

        Console.WriteLine("Output:");
        foreach (var msg in result)
            Console.WriteLine($"{msg.Item1}, {msg.Item2}");

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("4, URGENT: Patient crashing");
        Console.WriteLine("4, URGENT: Room 4 emergency");
        Console.WriteLine("3, Review medication");
        Console.WriteLine("2, Check lab results");
        Console.WriteLine("2, Follow up needed");
        Console.WriteLine("1, Admin update");
    }
}

// ============================================================
// INTERVIEW TALKING POINTS:
//
// Q: Why did you capture the index before sorting?
// A: The original tuple has no arrival order information. Once you
//    sort, the original positions are lost. By capturing the index
//    first with Select((msg, index) => ...) I preserve arrival order
//    as a tiebreaker for messages with equal priority.
//
// Q: What is the time complexity?
// A: O(n log n) dominated by the sort. The Select operations are
//    O(n) but insignificant. C# uses TimSort internally which is
//    O(n log n) average and worst case.
//
// Q: What is the space complexity?
// A: O(n) — LINQ creates intermediate collections during the chain
//    but none exceed the size of the input.
//
// Q: Why LINQ over a manual sorting approach?
// A: LINQ is expressive, readable, and uses a highly optimised
//    internal sort. At Staff level you should know your language's
//    standard library and avoid reinventing well-solved problems.
//    The intent of the code is immediately clear to any C# developer.
//
// Q: How would you handle dynamic priority levels that change at runtime?
// A: The current solution handles any integer priority automatically —
//    no hardcoded levels. If priorities were weighted differently I'd
//    introduce a priority scoring function and pass it as a parameter
//    making the sorter configurable and testable independently.
// ============================================================
