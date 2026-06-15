# Interview Cheat Sheet — Friday 10:30am

**Role:** Senior Software Engineer, CVS Health / Oak Street Health (Galway)
**Format:** 60 mins total — ~20 mins technical discussion + ~40 mins Codility coding challenge
**Interviewers:** Emer F. (Principal, Signify Health bg, distributed systems/messaging) + Alex Simatov (Senior, Full-stack, Avaya bg)

---

## 1. The Three Meta-Goals (Read First Every Morning)

Everything you say should serve one of these:

1. **Product ownership** — you don't just code, you own outcomes
2. **Technical trust** — specific numbers, real systems, honest gaps
3. **Honesty** — bridge gaps to transferable depth, never bluff

**The recruiter said:** *"they're not looking for general or vague answers, they're looking for examples and demonstration of technical depth."*

---

## 2. The FindCare Opener (Most Important 10 Seconds)

> *"The most challenging piece I've built was a batch pipeline for provider data ingestion at scale. The core challenge was failure isolation — a single bad entity couldn't block valid ones — across hundreds of files and over a million records daily."*

Two phrases to lock in:
- *"batch pipeline for provider data ingestion at scale"*
- *"failure isolation across hundreds of files and over a million records daily"*

---

## 3. STAR Stories — Story → Question Mapping

### Story 1 — FindCare (use for: "most challenging", "system you built")
- **Open:** see opener above
- **Then:** S3 → Lambda → Step Function → Spark on EMR splits file → SQS + DynamoDB tracking → diff vs previous → RDS + Kafka → EKS autoscaling
- **Hard parts:** parallel Spark coordination, partial success reporting, self-healing
- **Result:** hundreds of files, 1M+ records daily, replaced manual entry

### Story 2 — PDM (use for: "architectural decision", "cross-team collaboration")
- **Open:** *"Building PDM, we needed to consolidate change events from multiple upstream sources — legacy systems, UI, external vendors. The architectural challenge was handling duplicate messages safely while preserving a full audit trail."*
- **Then:** Kafka-driven microservice, record-level unique key with diff detection, history table + error table, republish to Kafka
- **Hard parts:** idempotency from day one, audit completeness vs performance
- **Result:** single source of truth across the org with full traceability

### Story 3 — Backup/Restore (use for: "production incident", "ownership")
- **Open:** *"On the same FindCare pipeline, we had a class of failure where invalid input values would corrupt our current-and-previous file state, blocking subsequent runs entirely."*
- **Then:** Quick recovery script pulling last good state from RDS → permanent fix backup/restore in Step Function + SNS alerts
- **Hard parts:** corruption identification, safe recovery, automatic prevention
- **Result:** zero recurrence, pipeline self-healing with full observability

### Story 4 — Think First, Code Later (use for: "mentoring", "team influence", "AI use")
- **Open:** *"As tech lead I noticed junior engineers reaching for AI to generate code without really understanding what they were building. So I established a 'think first, code later' practice."*
- **Then:** plain-English solution before code, AI as validation/learning tool not generation
- **Result:** stronger fundamentals in juniors, better PR reviews, mature AI culture

---

## 4. Why Are You Leaving Current Role

**Never say money.** Money is a recruiter conversation.

**Use this structure:**

1. *"Over three years at my current role — great experience, shipped multiple production systems end-to-end, mentored juniors, built deep expertise in event-driven architectures."*

2. *"What I'm looking for now is a step up in scope and challenge — harder problems at larger scale, more ownership of architectural decisions, and a team where I can contribute senior work and continue learning."*

3. *"Oak Street specifically — healthcare mission matters to me, company is in growth mode, modern stack and practices, Senior role on this team is exactly the next step I'm looking for."*

**If pressed on money:** *"Compensation isn't my primary driver but I'd expect any move to reflect the level and value I'd bring. Happy to discuss that with the recruiter at the right stage."*

---

## 5. Issue Only You Know About — Sequence

1. **Confirm** it's real — false red flags damage trust
2. **Scope** — blast radius, getting worse?
3. **Document** as you go
4. **Quick fix + prevention** thought through
5. **Escalate with options** not just problem
6. **Own through resolution + post-mortem** (learning not blame)

**Severity changes urgency, not approach.** For PHI/high-severity: escalate in parallel with analysis, not after.

**Close with FindCare example** — *"I've lived this — invalid values corrupting pipeline state. I confirmed, contained via manual restore from RDS, then built permanent backup/restore in Step Function with SNS alerts."*

---

## 6. Tech Stack Choice Answers ("Why X over Y")

### Kafka vs REST
**Headline:** *"It's about coupling and synchronicity, not scale."*
- **REST:** synchronous, caller waits, coupled, fails if receiver down → use when caller needs immediate response
- **Kafka:** async, decoupled, fire-and-forget, durable, replayable → use when events have multiple/future consumers, producer shouldn't block
- **Heuristic:** *"Does the caller need to wait for the result? Yes → REST. No → Kafka."*
- **Your example:** PDM consumes from multiple upstream producers — decoupling and audit trail "for free"

### Document DB vs Relational
**Headline:** *"Relational when relationships and integrity matter, document when schema is fluid or hierarchical."*
- **Postgres/MySQL:** well-defined schema, ACID across multiple tables, complex joins/queries — healthcare clinical and billing data
- **MongoDB:** flexible/evolving schema, hierarchical data, single-document reads — ingestion landing zones, external vendor payloads
- **Mention:** ACID matters in healthcare for patient/billing integrity

### Microservices vs Monolith
**Headline:** *"It's organisational scale, not just technical scale."*
- **Monolith:** small team, early product, simpler ops — keep until coordination friction
- **Microservices:** multiple teams deploying independently, different scaling/reliability profiles, failure isolation (healthcare)
- **Avoid:** distributed monolith — services split in name but coupled in deployment. All cost, no benefit.
- **Mention Conway's Law:** service boundary should match team boundary
- **Your example:** FindCare + PDM split because different concerns, different volumes, different failure modes

### Constructor vs Field Injection
**Four reasons:**
1. **Testability** — instantiate with mocks, no Spring context needed
2. **Immutability** — final fields, thread-safe by design
3. **Fail-fast** — cannot construct without dependencies, startup-time errors
4. **Explicit dependencies** — too many parameters = design smell field injection hides

---

## 7. Gap One-Liners (Memorise These)

### Kotlin
*"I haven't used Kotlin professionally but I understand it runs on the JVM and interops fully with Java. My Java depth gives me a strong foundation and ramping up is something I'm actively looking forward to."*

### MongoDB
*"Tutorial level. I understand document modelling and when you'd choose it over relational. The concepts translate well from my Postgres/MySQL experience."*

### Postgres
*"My relational DB experience is primarily MySQL. Fundamentals are the same — schema design, indexing, query optimisation. I'm aware of Postgres specifics like JSONB and more advanced indexing types."*

### GCP / AKS / Azure
*"Cloud experience is primarily AWS but architectural patterns translate directly. I've worked with managed Kubernetes on EKS extensively — Kubernetes itself is cloud-agnostic. Short ramp for GCP/Azure-specific tooling."*

### BDD (Cucumber/Gherkin)
*"I practice TDD consistently. BDD I understand conceptually — bridging business requirements into testable specs — but haven't used it in production yet."*

---

## 8. Questions To Ask Them (If Flipped Or At End)

### If they flip the script — "what would you ask?"

**Q1 — Leadership under pressure:**
*"You've inherited a project two weeks from production. Code works but minimal tests, no logging, hard to extend. Stakeholders are committed to the deadline. Do you ship as-is, push to delay, or something else?"*

**Q2 — AI engineering culture:**
*"AI can now write code, generate tests, produce test data. If all three are AI-generated, how do you know the code does what it should — tests might be written to pass, data might not reflect reality. What guard rails would you put in place?"*

**Q3 — Architecture trade-off:**
*"You've built a clean template pattern for 40 transformation services — input to output, fully decoupled. A new requirement needs one service to coordinate with four others in sequence. Do you extend the framework or build a separate orchestrator?"*

### If you ask them at the end (your turn)

- *"How does the team approach quality vs delivery when stakeholders have hard deadlines?"*
- *"What's the team's philosophy on AI tool use?"*
- *"How mature is the CI/CD pipeline — what does a typical deployment look like?"*
- *"What does the Foundry → Databricks migration timeline look like?"*
- *"How is Kotlin used today — full migration from Java or mixed?"*

---

## 9. Coding — Quick Reference

### The 3 Patterns You Know
1. **HashMap O(1) lookup** — Two Sum, group anagrams, duplicates
2. **Two pointers** — palindromes, sorted array, removing duplicates
3. **Sliding window** — longest/shortest substring with property

### Pattern Recognition Order (Mental Checklist)
1. What's the input? (array, string, tree, graph)
2. Is it sorted? → two pointers or binary search
3. Need fast lookup? → HashMap/HashSet
4. Contiguous range? → sliding window
5. Brackets/LIFO? → stack
6. Best K? → heap
7. All combinations? → recursion
8. Overlapping subproblems? → DP

### Recovery Playbook (If Stuck)
1. **Don't go silent.** Narrate.
2. **Start with brute force** — *"O(n²) approach first, then I'll optimise"*
3. **Walk through small example** — make it concrete
4. **Name what's confusing** — invites a hint without asking
5. **Ask specifically** — *"should I store value or index?"* not *"any hint?"*
6. **Even without working code, talk architecture** — pattern, complexity, trade-offs

### Codility Workflow
1. **Read problem twice** — don't rush
2. **Ask clarifying questions** — null inputs, edge cases, constraints
3. **State approach in plain English first**
4. **State time/space complexity BEFORE coding**
5. **Code it**
6. **Test edge cases out loud** — empty, single element, duplicates, negatives

### Complexity — Name Your Variables
- ❌ "O(n log n)"
- ✅ "O(n · k log k) where n is the number of strings and k is the max string length"

### Java Quick Syntax
```java
// Always use diamond operator
Map<K, V> m = new HashMap<>();
Set<T> s = new HashSet<>();

// Stack/Queue
Deque<T> stack = new ArrayDeque<>();   // push/pop
Deque<T> queue = new ArrayDeque<>();   // offer/poll

// Heap
PriorityQueue<Integer> minHeap = new PriorityQueue<>();
PriorityQueue<Integer> maxHeap = new PriorityQueue<>(Comparator.reverseOrder());

// Idiomatic
map.computeIfAbsent(key, k -> new ArrayList<>()).add(value);
map.getOrDefault(key, 0);

// String sorting
char[] chars = str.toCharArray();
Arrays.sort(chars);
String sorted = new String(chars);
```

### Naming Conventions
- Variables/methods: `camelCase`
- Classes: `PascalCase`
- Constants: `UPPER_SNAKE_CASE`

---

## 10. Kafka — Senior Talking Points

### Core Concepts
- **Topic** split into **partitions**; each partition is ordered log
- **Offsets are per-partition** (not per-topic)
- **Consumer group**: each partition assigned to one consumer; consumers > partitions = idle
- **Producer partition strategies:** explicit, key-based hash (preserves order per key), round-robin

### Delivery Guarantees
- **At most once** — may lose
- **At least once** — may duplicate (most common, requires idempotent consumer)
- **Exactly once** — idempotent producer + transactions, expensive

### Idempotency Patterns (Senior Buzzword)
1. Upsert / conditional update
2. Track processed message IDs
3. Natural unique key as primary key
4. Diff detection (your PDM pattern — duplicates become no-ops)

### DLQ vs Retry Decision
- **Retry** transient failures (DB down, network blip) with exponential backoff + jitter
- **DLQ** permanent failures (schema mismatch, business rule violation, max retries)
- **Heuristic:** *"would retry in 5 mins likely succeed?"* Yes → retry. No → DLQ.

### Ordering
- Guaranteed within partition only, not across topic
- Choose partition key to preserve order per logical entity (e.g. customerId)

---

## 11. Spring Boot — Senior Talking Points

### What Spring Boot Is
- Spring + opinionated defaults + auto-configuration
- Removes XML boilerplate, embedded server, starter dependencies

### Dependency Injection
- **IoC** — you declare needs, Spring provides via constructor
- **At startup:** component scan → dependency graph → bottom-up bean creation → stored in application context
- **Default scope:** singleton (must be thread-safe)

### Stereotype Annotations
- `@Component` generic
- `@Service` business logic
- `@Repository` data access (exception translation)
- `@RestController` HTTP layer + auto-JSON serialisation
- `@Configuration` bean definitions

### REST Best Practices
- Proper verbs: GET, POST, PUT, PATCH, DELETE
- Proper status codes: 200, 201, 204, 400, 401, 403, 404, 409, 500
- Nouns in URLs (`/users/123` not `/getUser/123`)
- Version your APIs (`/api/v1/`)
- Pagination on collections
- Idempotency keys for critical POSTs

### PUT vs POST Idempotency
- **PUT** idempotent — same effect on state regardless of how many calls
- **POST** not idempotent — each call creates new state
- Use idempotency keys on critical POSTs to handle client retries

### Exception Handling
- `@RestControllerAdvice` for global handler
- Specific exception → specific status (e.g. `NotFoundException` → 404)
- Generic fallback logs full trace server-side, returns sanitised message
- Consistent error envelope format

### Spring Data JPA Magic
- Empty interface extending `JpaRepository`
- Spring generates implementation at runtime via **dynamic proxy**
- Method names parsed (e.g. `findByEmail`) to generate queries

---

## 12. Security — Senior Talking Points

### Auth vs Authz
- **Authentication** = who are you (login, token)
- **Authorisation** = what can you do (permissions, roles)

### JWT — Why It Matters For Microservices
- **Stateless** — server doesn't store token, only validates signature
- Any pod in the cluster can validate — no sticky sessions, no Redis dependency
- Trade-off: revocation is hard (no central record to delete)

### JWT Security
- Payload is **encoded not encrypted** — never put sensitive data inside
- Signature prevents tampering — attacker can't modify payload
- **Stolen JWT danger:** direct impersonation until expiry (no crypto needed to break)
- Mitigations: short expiry (15 min) + refresh tokens, blacklist for emergency revocation, TLS everywhere

### Spring Security Quick
- Filter chain processes every request before controller
- `@RestControllerAdvice` for global error handling
- `@PreAuthorize("hasRole('ADMIN')")` for method-level
- For stateless API: disable CSRF, `SessionCreationPolicy.STATELESS`

### Healthcare Context (Mention If Relevant)
- PHI = Protected Health Information
- HIPAA requires audit logging of every access
- Principle of least privilege
- TLS for data in transit

---

## 13. Final Reminders

### What To Lead With On Every Answer
1. **The headline** in the first sentence
2. **Scale or impact numbers** when you have them
3. **Then the detail**
4. **End with the result**

### What Earns Trust With Principal
- **Specific numbers** (1M records, hundreds of files)
- **Trade-off thinking** (not "this is best" but "this fits when...")
- **Honest gaps** with bridge to transferable depth
- **Production scars** (incidents you've worked through)
- **Naming patterns** (idempotency, Conway's Law, sagas, DLQ)

### What Loses Trust
- Vague answers without examples
- Bluffing on Kotlin / MongoDB / Postgres
- Criticising current employer
- Going silent under pressure
- Skipping complexity analysis
- Saying "money" as reason for leaving

### Codility — Time Management
- 40 minutes for the coding challenge
- Don't rush into code — 5 mins for understanding and approach is well spent
- Working O(n²) > broken O(n)
- Test with edge cases before declaring done

### Energy Notes For Friday
- Set up Codility environment 10 mins early
- Coffee but don't over-caffeinate
- Water nearby
- Phone on silent, notifications off
- Backup internet plan if home connection drops

---

## 14. Three Closing Sentences To Have Ready

**If asked "do you have any questions":**
- Use one of the questions from Section 8 above
- Avoid logistics (start date, salary) — that's for recruiter

**If asked "anything else to add":**
*"Just that I'm genuinely excited about this role — the healthcare mission resonates with me, the stack is one I'm strong in, and the team's modern engineering practices are exactly what I'm looking for. Thank you for the time today."*

**If asked "what's your timeline":**
*"I'm interviewing actively but selectively. Happy to align on whatever pace works for your team."*

---

## 15. 15-Minute Morning Review Plan

If you only have 15 mins Friday morning, in this order:

1. **2 mins** — Read Section 1 (the three meta-goals)
2. **3 mins** — Read Section 2 (the FindCare opener) — say it out loud once
3. **3 mins** — Skim Section 3 (story → question mapping)
4. **2 mins** — Read Section 4 (why leaving) — say it out loud once
5. **2 mins** — Skim Section 7 (gap one-liners)
6. **2 mins** — Read Section 13 (final reminders)
7. **1 min** — Breathe. Drink water. You've prepared more than enough.

---

**You're ready. Sleep well.**
