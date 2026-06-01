using System;

// ============================================================
// PROBLEM: Find The Duplicate Transaction
// PATTERN: Math Sum Trick + XOR Bit Manipulation
// DIFFICULTY: Medium
// DATE: 2026-05-29
// ============================================================
//
// PROBLEM STATEMENT:
// A hospital billing system processes payment transactions.
// Due to a system bug, some transactions were processed twice.
// Given an array of transaction IDs where every ID appears
// exactly once EXCEPT for one ID which appears exactly twice,
// find and return the duplicate ID.
//
// CONSTRAINTS:
// - Array length between 1 and 100,000
// - Transaction IDs are positive consecutive integers from 1 to n
// - There is always exactly one duplicate
// - Must solve in O(n) time and O(1) space
//
// EXAMPLE:
// Input:  [3, 1, 4, 2, 5, 3]
// Output: 3
//
// ============================================================
// KEY INSIGHT — MATH SUM APPROACH:
//
// If IDs are consecutive from 1 to n, we know exactly what
// the sum SHOULD be using the formula: n * (n + 1) / 2
//
// The difference between actual sum and expected sum = duplicate
//
// Example:
// Array = [3, 1, 4, 2, 5, 3]
// n = 5 (array.length - 1)
// Expected sum = 5 * 6 / 2 = 15
// Actual sum   = 3+1+4+2+5+3 = 18
// Duplicate    = 18 - 15 = 3 ✅
//
// ============================================================
// CRITICAL ASSUMPTION:
// Both math sum and XOR solutions ONLY work when:
// - IDs are consecutive integers starting from 1
// - Array contains every number from 1 to n exactly once
//   plus one duplicate
//
// Invalid input [2, 2] breaks this assumption — 1 is missing
// Valid inputs: [1,1], [1,2,1], [1,2,3,2]
//
// For non-consecutive IDs use HashSet (O(n) space tradeoff)
//
// ============================================================
// APPROACH COMPARISON:
//
// | Approach    | Time       | Space | Consecutive IDs? |
// |-------------|------------|-------|-----------------|
// | Sort + scan | O(n log n) | O(1)  | No              |
// | Math sum    | O(n)       | O(1)  | Yes             |
// | XOR         | O(n)       | O(1)  | Yes             |
// | HashSet     | O(n)       | O(n)  | No              |
//
// ============================================================

class Solution
{
    // --------------------------------------------------------
    // SOLUTION 1 — MATH SUM (Recommended — most readable)
    // --------------------------------------------------------
    // Time:  O(n) — one pass through array
    // Space: O(1) — only two integer variables
    // --------------------------------------------------------
    public static int FindDuplicate(int[] transactions)
    {
        // n = highest expected ID = array length minus the duplicate
        int n = transactions.Length - 1;

        // Expected sum of consecutive integers 1 to n
        // Classic math formula: n * (n + 1) / 2
        int expected = n * (n + 1) / 2;

        // Actual sum of all elements including duplicate
        int actual = 0;
        foreach (int t in transactions)
        {
            actual += t;
        }

        // Duplicate = actual - expected
        // Every unique number cancels out, leaving only the duplicate
        return actual - expected;
    }

    // --------------------------------------------------------
    // SOLUTION 2 — XOR BIT MANIPULATION (Elegant but less readable)
    // --------------------------------------------------------
    // Time:  O(n) — two passes (1 to n, then array)
    // Space: O(1) — single integer variable
    //
    // XOR RULES:
    //   Same values  → 0  (5 ^ 5 = 0)
    //   Any ^ 0      → itself  (5 ^ 0 = 5)
    //   Commutative  → order doesn't matter
    //
    // HOW IT WORKS:
    //   XOR all expected numbers 1..n with all actual numbers
    //   Every number that appears exactly twice cancels to 0
    //   The duplicate appears 3 times → cancels once, leaves itself
    //
    // Example:
    //   Array = [3, 1, 4, 2, 5, 3]
    //   XOR: 1^2^3^4^5 ^ 1^2^3^4^5^3
    //   Everything cancels except 3 → result = 3 ✅
    // --------------------------------------------------------
    public static int FindDuplicateXOR(int[] transactions)
    {
        int xor = 0;
        int n = transactions.Length - 1;

        // XOR all expected numbers 1 to n
        for (int i = 1; i <= n; i++)
        {
            xor ^= i;
        }

        // XOR all actual numbers in array
        // Duplicate appears 3 times total → cancels twice, remains once
        foreach (int t in transactions)
        {
            xor ^= t;
        }

        // Everything cancelled except the duplicate
        return xor;
    }

    // --------------------------------------------------------
    // SOLUTION 3 — HASHSET (Works for non-consecutive IDs)
    // --------------------------------------------------------
    // Time:  O(n)
    // Space: O(n) — violates the O(1) constraint but works universally
    // Use when IDs are not guaranteed to be consecutive from 1
    // --------------------------------------------------------
    public static int FindDuplicateHashSet(int[] transactions)
    {
        var seen = new System.Collections.Generic.HashSet<int>();
        foreach (int t in transactions)
        {
            // Add returns false if already exists → that's our duplicate
            if (!seen.Add(t))
            {
                return t;
            }
        }
        return -1; // Should never reach here given valid input
    }

    static void Main()
    {
        Console.WriteLine("=== MATH SUM SOLUTION ===");
        Console.WriteLine(FindDuplicate(new int[] { 3, 1, 4, 2, 5, 3 })); // 3
        Console.WriteLine(FindDuplicate(new int[] { 1, 3, 4, 2, 2 }));    // 2
        Console.WriteLine(FindDuplicate(new int[] { 1, 1 }));              // 1

        Console.WriteLine();
        Console.WriteLine("=== XOR SOLUTION ===");
        Console.WriteLine(FindDuplicateXOR(new int[] { 3, 1, 4, 2, 5, 3 })); // 3
        Console.WriteLine(FindDuplicateXOR(new int[] { 1, 3, 4, 2, 2 }));    // 2
        Console.WriteLine(FindDuplicateXOR(new int[] { 1, 1 }));              // 1

        Console.WriteLine();
        Console.WriteLine("=== HASHSET SOLUTION (non-consecutive IDs) ===");
        Console.WriteLine(FindDuplicateHashSet(new int[] { 3, 1, 4, 2, 5, 3 })); // 3
        Console.WriteLine(FindDuplicateHashSet(new int[] { 2, 2 }));              // 2 ✅
    }
}

// ============================================================
// COMMON MISTAKES TO AVOID:
//
//   1. Index out of bounds in sort approach
//      WRONG: for(int a = 0; a < arr.Length; a++)
//                 if(arr[a] == arr[a+1])  ← crashes on last element
//      RIGHT: for(int a = 0; a < arr.Length - 1; a++)
//
//   2. Using sort when O(n) time is required
//      Sort is O(n log n) — violates the constraint
//      Math sum and XOR are both O(n)
//
//   3. Assuming math/XOR works for non-consecutive IDs
//      [2, 2] breaks both — missing 1 corrupts the expected sum
//      Always clarify ID format in the interview
//
//   4. Forgetting to state assumptions
//      Always say: "This assumes IDs are consecutive from 1 to n"
//
// ============================================================
// INTERVIEW TALKING POINTS:
//
// Q: What is the time and space complexity?
// A: O(n) time — single pass through the array.
//    O(1) space — only two integer variables regardless of input size.
//
// Q: Why math sum over HashSet?
// A: HashSet is O(n) space. The constraint required O(1) space.
//    The math sum approach achieves the same O(n) time with O(1) space
//    by exploiting the consecutive integer property of the IDs.
//
// Q: What if IDs weren't consecutive?
// A: I'd use a HashSet and accept the O(n) space tradeoff, or sort
//    first if space was critical accepting O(n log n) time instead.
//    There's no O(n) time O(1) space solution without the consecutive
//    integer assumption.
//
// Q: What is XOR and why does it work here?
// A: XOR returns 0 for identical values and the value itself when
//    XORed with 0. By XORing all expected numbers with all actual
//    numbers, every number that appears exactly twice cancels to 0.
//    The duplicate appears three times total — cancels twice —
//    leaving itself as the result.
//
// Q: How would you handle this in a real billing system?
// A: Real billing systems use UUIDs not sequential integers, so
//    neither math nor XOR would apply. I'd use a database unique
//    constraint to prevent duplicates at the source, with an
//    idempotency key on the transaction API to handle retries
//    gracefully without creating duplicates.
// ============================================================
