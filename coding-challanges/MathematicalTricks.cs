// ============================================================
// MATHEMATICAL TRICKS FOR CODILITY INTERVIEWS
// Study Notes
// Date: 2026-05-29
// ============================================================
//
// These mathematical patterns come up regularly in medium/hard
// Codility problems. Knowing them saves significant time and
// unlocks O(n) or O(1) solutions that brute force can't achieve.
//
// ============================================================

using System;

class MathematicalTricks
{
    static void Main()
    {
        Console.WriteLine("=== MATHEMATICAL TRICKS DEMO ===");
        Console.WriteLine();

        // ====================================================
        // 1. SUM OF CONSECUTIVE INTEGERS
        // ====================================================
        // Formula: n * (n + 1) / 2
        // Used for: finding duplicates, finding missing numbers
        //
        // Example: sum of 1 to 10
        // = 10 * 11 / 2 = 55
        //
        // WHY IT WORKS:
        // Pair numbers from both ends: (1+10), (2+9), (3+8)...
        // Each pair sums to n+1, there are n/2 pairs
        // Total = n/2 * (n+1) = n*(n+1)/2
        // ====================================================
        int n = 10;
        int sumConsecutive = n * (n + 1) / 2;
        Console.WriteLine($"Sum 1 to {n} = {sumConsecutive}"); // 55

        // PRACTICAL USE — Find missing number in array:
        // Array should contain 1 to n but one is missing
        int[] arr = { 1, 2, 4, 5, 6 }; // missing 3
        int expectedSum = arr.Length * (arr.Length + 1) / 2;
        int actualSum = 0;
        foreach (int x in arr) actualSum += x;
        Console.WriteLine($"Missing number = {expectedSum - actualSum}"); // 3

        Console.WriteLine();

        // ====================================================
        // 2. SUM OF SQUARES
        // ====================================================
        // Formula: n * (n + 1) * (2n + 1) / 6
        // Used for: finding duplicate when sum alone isn't enough
        //
        // Example: 1² + 2² + 3² + 4² + 5²
        // = 1 + 4 + 9 + 16 + 25 = 55
        // = 5 * 6 * 11 / 6 = 55
        // ====================================================
        int n2 = 5;
        int sumSquares = n2 * (n2 + 1) * (2 * n2 + 1) / 6;
        Console.WriteLine($"Sum of squares 1 to {n2} = {sumSquares}"); // 55

        Console.WriteLine();

        // ====================================================
        // 3. EVEN / ODD NUMBER SUMS
        // ====================================================
        // Sum of first n even numbers = n * (n + 1)
        // Sum of first n odd numbers  = n²
        //
        // Example:
        // 2+4+6+8+10 = 5 * 6 = 30
        // 1+3+5+7+9  = 5²   = 25
        // ====================================================
        int terms = 5;
        int sumEven = terms * (terms + 1);
        int sumOdd = terms * terms;
        Console.WriteLine($"Sum first {terms} even numbers = {sumEven}"); // 30
        Console.WriteLine($"Sum first {terms} odd numbers  = {sumOdd}");  // 25

        Console.WriteLine();

        // ====================================================
        // 4. MODULO (%) — MOST COMMON IN CODILITY
        // ====================================================
        // x % n = remainder after dividing x by n
        //
        // KEY USES:
        //   Even/odd check    → n % 2 == 0 (even), n % 2 != 0 (odd)
        //   Circular array    → (index + 1) % length
        //   Bucket grouping   → id % numberOfBuckets
        //
        // REAL WORLD: Kafka uses modulo to assign messages to partitions
        //   partition = messageId % numberOfPartitions
        // ====================================================

        // Even/odd check
        int num = 47;
        Console.WriteLine($"{num} is {(num % 2 == 0 ? "even" : "odd")}"); // odd

        // Circular array — wrap around
        int[] circular = { 0, 1, 2, 3, 4, 5 };
        int current = 5;
        int next = (current + 1) % circular.Length;
        Console.WriteLine($"After index {current} comes index {next}"); // 0

        // Kafka partition assignment
        int messageId = 7;
        int partitions = 3;
        int partition = messageId % partitions;
        Console.WriteLine($"Message {messageId} → Partition {partition}"); // 1

        Console.WriteLine();

        // ====================================================
        // 5. INTEGER DIVISION — FLOOR DIVISION
        // ====================================================
        // x / n in C# always rounds DOWN (floor division)
        // Used for: finding midpoints, binary search, splitting
        //
        // CRITICAL FOR BINARY SEARCH:
        //   mid = (left + right) / 2
        // ====================================================
        Console.WriteLine($"7 / 2 = {7 / 2}");   // 3 not 3.5
        Console.WriteLine($"10 / 3 = {10 / 3}"); // 3 not 3.33

        // Midpoint — used in binary search
        int left = 0;
        int right = 10;
        int mid = (left + right) / 2;
        Console.WriteLine($"Midpoint of {left} and {right} = {mid}"); // 5

        Console.WriteLine();

        // ====================================================
        // 6. ABSOLUTE VALUE
        // ====================================================
        // Math.Abs(x) removes negative sign
        // Used for: distance problems, difference calculations
        // ====================================================
        Console.WriteLine($"Math.Abs(-5) = {Math.Abs(-5)}"); // 5
        Console.WriteLine($"Math.Abs(5)  = {Math.Abs(5)}");  // 5

        // Distance between two points on a number line
        int a = 3;
        int b = 8;
        int distance = Math.Abs(a - b);
        Console.WriteLine($"Distance between {a} and {b} = {distance}"); // 5

        Console.WriteLine();

        // ====================================================
        // 7. POWER AND SQUARE ROOT
        // ====================================================
        // Math.Pow(base, exponent) → base to the power of exponent
        // Math.Sqrt(n)             → square root of n
        //
        // PRACTICAL USE — Prime number check:
        // Only need to check divisors up to √n
        // If no divisor found up to √n, number is prime
        // ====================================================
        Console.WriteLine($"2^10 = {Math.Pow(2, 10)}");      // 1024
        Console.WriteLine($"√144 = {Math.Sqrt(144)}");        // 12

        // Prime number check using sqrt
        Console.WriteLine($"Is 17 prime? {IsPrime(17)}");     // true
        Console.WriteLine($"Is 15 prime? {IsPrime(15)}");     // false

        Console.WriteLine();

        // ====================================================
        // 8. MIN / MAX
        // ====================================================
        // Math.Min(a, b) → smaller value
        // Math.Max(a, b) → larger value
        // Used for: boundary checks, running min/max tracking
        // ====================================================
        Console.WriteLine($"Min(3,7) = {Math.Min(3, 7)}"); // 3
        Console.WriteLine($"Max(3,7) = {Math.Max(3, 7)}"); // 7

        // Running maximum — find max in array without Sort()
        int[] values = { 3, 7, 1, 9, 4, 6 };
        int runningMax = int.MinValue;
        foreach (int v in values)
            runningMax = Math.Max(runningMax, v);
        Console.WriteLine($"Max value = {runningMax}"); // 9

        Console.WriteLine();

        // ====================================================
        // 9. PREFIX SUM — RANGE QUERIES IN O(1)
        // ====================================================
        // Problem: sum elements between index i and j efficiently
        //
        // NAIVE: O(n) per query — loop through each time
        // PREFIX SUM: O(n) build once, O(1) per query
        //
        // HOW TO BUILD:
        //   prefix[0] = 0
        //   prefix[i] = prefix[i-1] + arr[i-1]
        //
        // HOW TO QUERY sum from index i to j (inclusive):
        //   sum = prefix[j+1] - prefix[i]
        //
        // EXAMPLE:
        //   arr    = [3,  1,  4,  2,  5]
        //   prefix = [0,  3,  4,  8, 10, 15]
        //   Sum index 2 to 4 = prefix[5] - prefix[2] = 15 - 4 = 11
        //   Check: 4 + 2 + 5 = 11 ✅
        // ====================================================
        int[] array = { 3, 1, 4, 2, 5 };
        int[] prefix = BuildPrefixSum(array);

        Console.WriteLine("Prefix sum array:");
        Console.Write("arr    = [");
        Console.WriteLine(string.Join(", ", array) + "]");
        Console.Write("prefix = [");
        Console.WriteLine(string.Join(", ", prefix) + "]");

        // Query: sum of elements from index 2 to 4
        int rangeSum = RangeSum(prefix, 2, 4);
        Console.WriteLine($"Sum index 2 to 4 = {rangeSum}"); // 11

        Console.WriteLine();

        // ====================================================
        // 10. XOR BIT MANIPULATION
        // ====================================================
        // XOR (^) rules:
        //   same values → 0    (5 ^ 5 = 0)
        //   any ^ 0     → itself (5 ^ 0 = 5)
        //   commutative → order doesn't matter
        //
        // USED FOR: finding duplicates, finding unique elements
        //
        // EXAMPLE — find duplicate in [3,1,4,2,5,3]:
        //   XOR all 1..n with all array elements
        //   Everything cancels except the duplicate
        // ====================================================
        int[] dupArray = { 3, 1, 4, 2, 5, 3 };
        int xorResult = 0;
        int xorN = dupArray.Length - 1;
        for (int i = 1; i <= xorN; i++) xorResult ^= i;
        foreach (int x in dupArray) xorResult ^= x;
        Console.WriteLine($"Duplicate via XOR = {xorResult}"); // 3
    }

    // ====================================================
    // HELPER METHODS
    // ====================================================

    // Prime check using sqrt optimisation
    // Only check divisors up to √n — O(√n) time
    static bool IsPrime(int n)
    {
        if (n < 2) return false;
        for (int i = 2; i <= Math.Sqrt(n); i++)
        {
            if (n % i == 0) return false;
        }
        return true;
    }

    // Build prefix sum array — O(n)
    static int[] BuildPrefixSum(int[] arr)
    {
        int[] prefix = new int[arr.Length + 1];
        for (int i = 0; i < arr.Length; i++)
            prefix[i + 1] = prefix[i] + arr[i];
        return prefix;
    }

    // Query range sum in O(1) using prefix array
    static int RangeSum(int[] prefix, int i, int j)
    {
        return prefix[j + 1] - prefix[i];
    }
}

// ============================================================
// QUICK REFERENCE CARD — MEMORISE THESE
// ============================================================
//
// Sum 1 to n          →  n * (n + 1) / 2
// Sum of squares      →  n * (n + 1) * (2n + 1) / 6
// Sum first n evens   →  n * (n + 1)
// Sum first n odds    →  n²
// Even check          →  n % 2 == 0
// Odd check           →  n % 2 != 0
// Circular next index →  (current + 1) % length
// Kafka partition     →  messageId % numPartitions
// Midpoint            →  (left + right) / 2
// Prime check limit   →  Math.Sqrt(n)
// XOR same values     →  x ^ x = 0
// XOR with zero       →  x ^ 0 = x
// Prefix range sum    →  prefix[j+1] - prefix[i]
//
// ============================================================
// INTERVIEW TALKING POINTS:
//
// Q: Why use math sum over HashSet for finding duplicates?
// A: Math sum is O(1) space vs HashSet O(n) space. When the
//    problem guarantees consecutive integers from 1 to n,
//    the math approach is always superior on space complexity.
//
// Q: When would you use prefix sum?
// A: When you need to answer multiple range sum queries on the
//    same array. Building the prefix array costs O(n) once,
//    then each query is O(1) instead of O(n). Essential for
//    problems with many range queries.
//
// Q: What is modulo used for in distributed systems?
// A: Consistent hashing and partition assignment. Kafka uses
//    messageId % numPartitions to assign messages to partitions.
//    Load balancers use it for round robin distribution.
//    It's how you map an arbitrary key to a fixed number of buckets.
//
// Q: Why only check up to √n for prime numbers?
// A: If n has a divisor larger than √n, it must also have one
//    smaller than √n. So checking up to √n is sufficient.
//    This reduces prime checking from O(n) to O(√n).
// ============================================================
