using System;
using System.Collections.Generic;

// ============================================================
// PROBLEM: Group Clinicians By Department
// PATTERN: Dictionary Grouping + Sorting
// DIFFICULTY: Medium
// DATE: 2026-05-29
// ============================================================
//
// PROBLEM STATEMENT:
// Given a list of clinicians each with a name and department,
// group clinicians by their department and return a dictionary
// where the key is the department name and the value is a list
// of clinician names sorted alphabetically.
//
// EXAMPLE INPUT:
// clinicians = [
//   ("Alice", "Cardiology"),
//   ("Bob", "Neurology"),
//   ("Charlie", "Cardiology"),
//   ("Diana", "Neurology"),
//   ("Eve", "Cardiology"),
//   ("Frank", "Oncology")
// ]
//
// EXPECTED OUTPUT:
// Cardiology: Alice, Charlie, Eve
// Neurology: Bob, Diana
// Oncology: Frank
//
// ============================================================
// KEY INSIGHT:
// This is a classic "group by" pattern — extremely common in
// real world systems. The key is choosing the right data structure
// to accumulate values under a shared key efficiently.
//
// ============================================================
// DATA STRUCTURES USED:
//
// Dictionary<string, List<string>>
//   - Key:   department name
//   - Value: list of clinician names in that department
//   - Dictionary gives O(1) lookup by department
//   - List grows dynamically as clinicians are added
//
// TryAdd(key, value)
//   - Safe way to initialise a new key if it doesn't exist
//   - Does nothing if key already exists
//   - Replaces verbose if/else ContainsKey checks
//   - Always follow with .Add() to append the actual value
//
// List.Sort()
//   - Sorts list alphabetically in place
//   - No new list created — modifies existing list
//   - Sort AFTER the loop — never inside it
//
// ============================================================
// COMPLEXITY:
//   Time:  O(n log n)
//          - Looping through clinicians: O(n)
//          - Sorting each department list: O(n log n) total
//            Even though we sort each list separately, total
//            elements across all lists still adds up to n
//          - Dominant cost: O(n log n)
//
//   Space: O(n)
//          - Every clinician name stored once in a list
//          - Worst case: every clinician is in a different department
//          - Dictionary + all lists combined = O(n)
//
// ============================================================
// COMMON MISTAKES TO AVOID:
//   1. Sorting inside the loop instead of after
//      Sorting on every insert is wasteful — sort once at the end
//
//   2. Using TryAdd without the subsequent .Add()
//      TryAdd only creates the empty list — you still need to add the name
//
//   3. Using name as a unique identifier
//      Names are display data only — two clinicians can share a name
//      Always use a UUID as the unique identifier in real systems
//
//   4. Wrong tuple field for dictionary key
//      Item1 = name, Item2 = department
//      Key must be Item2 (department) not the whole tuple
//
// ============================================================

class Solution
{
    // --------------------------------------------------------
    // BASIC SOLUTION
    // --------------------------------------------------------
    public static Dictionary<string, List<string>> GroupByDepartment(
        List<(string, string)> clinicians)
    {
        // Key = department name, Value = list of clinician names
        var departments = new Dictionary<string, List<string>>();

        foreach (var clinician in clinicians)
        {
            string name = clinician.Item1;
            string department = clinician.Item2;

            // Create empty list for new departments
            // Does nothing if department already exists
            departments.TryAdd(department, new List<string>());

            // Add clinician name to their department list
            departments[department].Add(name);
        }

        // Sort each department list alphabetically AFTER the loop
        // Sorting inside the loop would be wasteful
        foreach (var dept in departments)
        {
            dept.Value.Sort();
        }

        return departments;
    }

    // --------------------------------------------------------
    // ENHANCED SOLUTION — WITH UUID PER CLINICIAN
    // --------------------------------------------------------
    // Real world problem: two clinicians can have the same name
    // Solution: assign a unique UUID to each clinician at registration
    // The UUID is the true identifier — name is just display data
    //
    // This also allows tracking individual clinicians across departments
    // if they work in multiple departments simultaneously
    // --------------------------------------------------------

    // Enhanced clinician model with UUID
    public class Clinician
    {
        // Globally unique identifier — never changes, never duplicates
        public string Id { get; set; }
        public string Name { get; set; }
        public string Department { get; set; }

        public Clinician(string name, string department)
        {
            // Guid.NewGuid() generates a universally unique identifier
            // e.g. "a3b4c5d6-e7f8-9012-abcd-ef1234567890"
            // Probability of collision is astronomically small
            Id = Guid.NewGuid().ToString();
            Name = name;
            Department = department;
        }
    }

    // Enhanced grouping — groups by department, sorted by name
    // Now handles duplicate names correctly using UUID as true identity
    public static Dictionary<string, List<Clinician>> GroupByDepartmentEnhanced(
        List<Clinician> clinicians)
    {
        var departments = new Dictionary<string, List<Clinician>>();

        foreach (var clinician in clinicians)
        {
            departments.TryAdd(clinician.Department, new List<Clinician>());
            departments[clinician.Department].Add(clinician);
        }

        // Sort by name alphabetically — if names match, sort by UUID
        // This ensures consistent, deterministic ordering always
        foreach (var dept in departments)
        {
            dept.Value.Sort((a, b) =>
            {
                int nameComparison = string.Compare(a.Name, b.Name,
                    StringComparison.OrdinalIgnoreCase);

                // If names are equal, use UUID as tiebreaker
                // Ensures deterministic ordering even for duplicate names
                return nameComparison != 0
                    ? nameComparison
                    : string.Compare(a.Id, b.Id, StringComparison.Ordinal);
            });
        }

        return departments;
    }

    static void Main()
    {
        Console.WriteLine("=== BASIC SOLUTION ===");
        Console.WriteLine();

        var clinicians = new List<(string, string)>
        {
            ("Alice", "Cardiology"),
            ("Bob", "Neurology"),
            ("Charlie", "Cardiology"),
            ("Diana", "Neurology"),
            ("Eve", "Cardiology"),
            ("Frank", "Oncology")
        };

        var result = GroupByDepartment(clinicians);

        Console.WriteLine("Output:");
        foreach (var dept in result)
        {
            Console.WriteLine($"{dept.Key}: {string.Join(", ", dept.Value)}");
        }

        Console.WriteLine();
        Console.WriteLine("Expected:");
        Console.WriteLine("Cardiology: Alice, Charlie, Eve");
        Console.WriteLine("Neurology: Bob, Diana");
        Console.WriteLine("Oncology: Frank");

        Console.WriteLine();
        Console.WriteLine("=== ENHANCED SOLUTION WITH UUID ===");
        Console.WriteLine();

        // Two clinicians named Alice — same name, same department
        // Basic solution would show "Alice" twice with no way to distinguish
        // Enhanced solution tracks them separately via UUID
        var enhancedClinicians = new List<Clinician>
        {
            new Clinician("Alice", "Cardiology"),
            new Clinician("Bob", "Neurology"),
            new Clinician("Alice", "Cardiology"),  // duplicate name!
            new Clinician("Diana", "Neurology"),
            new Clinician("Eve", "Cardiology"),
            new Clinician("Frank", "Oncology")
        };

        var enhancedResult = GroupByDepartmentEnhanced(enhancedClinicians);

        Console.WriteLine("Output (with UUIDs):");
        foreach (var dept in enhancedResult)
        {
            Console.WriteLine($"{dept.Key}:");
            foreach (var clinician in dept.Value)
            {
                Console.WriteLine($"  {clinician.Name} (ID: {clinician.Id})");
            }
        }
    }
}

// ============================================================
// INTERVIEW TALKING POINTS:
//
// Q: Why Dictionary<string, List<string>> over other structures?
// A: Dictionary gives O(1) lookup by department name. List grows
//    dynamically and can be sorted. Together they solve grouping
//    and ordering efficiently.
//
// Q: Why sort after the loop instead of during?
// A: Sorting inside the loop would sort on every insert — O(n log n)
//    per insert making it O(n² log n) overall. Sorting once after
//    the loop costs O(n log n) total. Always defer sorting until
//    the collection is complete.
//
// Q: What is the time complexity?
// A: O(n log n) — the loop is O(n) but sorting dominates.
//    Total elements across all department lists still sum to n,
//    so overall sorting cost is O(n log n).
//
// Q: What if two clinicians have the same name?
// A: Same name in different departments is fine — they live in
//    separate lists. Same name in the same department would create
//    a duplicate entry in the list. In a real system I'd never use
//    name as an identifier — every clinician gets a UUID at
//    registration. Names are display data only.
//
// Q: What is Guid.NewGuid()?
// A: A Globally Unique Identifier — a 128 bit number generated
//    to be unique across time and space. The probability of two
//    GUIDs colliding is so small it's effectively impossible.
//    Standard practice for unique IDs in enterprise systems.
//
// Q: How would you handle a clinician in multiple departments?
// A: With UUID as the identifier, the same clinician could appear
//    in multiple department lists. Their UUID stays the same —
//    only their department assignment changes. This is a many-to-many
//    relationship better modelled with a join table in SQL or a
//    list of department IDs on the clinician document in NoSQL.
// ============================================================
