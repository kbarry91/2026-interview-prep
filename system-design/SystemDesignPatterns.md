# System Design — Reusable Patterns & Vocabulary

**Source:** Derived from Monday's full mock (appointment reminder system).
**Purpose:** These patterns are NOT specific to that prompt — they transfer to almost any
system design question. When Thursday's prompt is something different (ride-sharing,
URL shortener, chat system, whatever) — look for where these same shapes apply.

---

## 1. The Framework (Skeleton For Any Prompt)

1. **Requirements** — functional + non-functional, ask before designing
2. **Scale estimation** — rough numbers, sanity-checked
3. **Data model** — derive from relationships, not guesswork
4. **High-level design** — boxes + arrows + communication methods
5. **Deep dives** — wherever the interviewer probes
6. **Wrap-up** — security, performance, uptime, scalability (CVS rubric explicitly wants this)

Pace target for Thursday: requirements ~8 min, estimation ~3 min, schema ~5 min,
high-level ~15 min, deep dives ~20 min, wrap-up ~5 min. Today we went much slower
because we were learning — Thursday needs to move faster.

---

## 2. Reconciliation (NOT "fallback")

**Definition:** Periodically comparing two sources of truth and fixing discrepancies.

**When it applies:** Any time you have a "fast path" (webhook, event, real-time push)
that COULD silently fail or be missed. Reconciliation is the "always-on safety net"
running in parallel — not a backup that activates only on failure.

**Today's example:** Webhook (fast path) + periodic API poll (reconciliation) for
appointment data. Same diff-detection pattern as PDM.

**Where else this applies:** Cache + source-of-truth sync, search index + database sync,
any event-driven system where "what if the event was dropped?" is a real question.

**The phrase to use:** *"I'd add a reconciliation job — periodically comparing our
local copy against the source of truth and fixing any drift. This is the same
diff-detection pattern I built in PDM."*

---

## 3. Tiered Reconciliation / Tiered Polling

**Definition:** Different urgency = different frequency, same underlying mechanism.

**Today's example:** Daily broad sweep (90 days out, catches new bookings) +
hourly narrow sweep (24-48hrs out, catches recent changes).

**The general pattern:** When data has varying "freshness requirements" depending on
how soon it'll be acted on, don't use one frequency for everything — tier it.

**The phrase to use:** *"I'd tier the reconciliation — a broad, infrequent sweep for
things that aren't urgent yet, and a narrow, frequent sweep for things close to
their action point."*

---

## 4. The Sync vs Async Decision Rule

**The single question:** *"Does the caller need to wait for the result before
it can proceed?"*

- **Yes → synchronous** (REST/RPC). Caller blocks, gets immediate answer, tightly coupled.
- **No → asynchronous** (queue/topic). Caller fires and moves on, decoupled.

**Today's example:** Scheduler doesn't need to know "SMS sent successfully" before
dispatching the next reminder → async.

**Don't forget the honest counterpoint:** async costs you eventual consistency and
extra infrastructure. At low volume, sync-with-retry is ALSO defensible. State both
sides, then pick one with reasoning — don't pretend the choice is free.

---

## 5. Fault Isolation Through Service Boundaries

**Definition:** Splitting by "what can fail independently" so one failure doesn't
cascade into unrelated areas.

**Today's example:** Separate SMS/Email/Push services — an SMS provider outage
shouldn't block email or push.

**The test to apply:** *"If component A goes down, what ELSE breaks that shouldn't?"*
If the answer is "things that have nothing to do with A" — that's a fault isolation
boundary you should draw.

---

## 6. Status-Field + Reconciliation (For Async Work Tracking)

**The problem:** Async messaging means the producer doesn't get an immediate
success/fail response. So how do you know if something worked?

**The pattern:**
1. Producer sets status = `"dispatched"` when publishing
2. Consumer updates status = `"sent"` or `"failed"` after processing
3. A periodic check looks for anything stuck in `"dispatched"` too long → alert/retry

**This is the same idempotency/tracking toolkit from Kafka prep, applied to a new domain.**

**The phrase to use:** *"The producer marks it as dispatched, the consumer updates
the status on completion, and a periodic check catches anything stuck — same pattern
as detecting a stalled Kafka consumer via lag monitoring."*

---

## 7. SLA Framing For "Is This Performance Acceptable?"

**The insight:** Not every system needs low latency. The question is: **what does
the BUSINESS lose if this is slow?**

**Today's example:** A reminder being 2 minutes late is invisible to the user.
A toll booth payment being 2 seconds late backs up traffic on a motorway.

**The technique:** When asked about performance, don't default to "it needs to be fast."
Ask: *"What's the consequence of latency here, concretely?"* Sometimes the honest
answer is "this isn't a performance-sensitive system, and that's fine — here's why."

**The phrase to use:** *"This isn't a performance-critical path — the inherent slack
in [X] means [Y] seconds of latency is invisible to the end user. I'd flag where it
WOULD matter: [specific edge case], and that should be an explicit SLA decision."*

---

## 8. "Either Works, Here's Why I'd Pick One Anyway"

**The insight:** Not every design decision has a dramatically right/wrong answer.
Sometimes BOTH options are fine at the given scale, and pretending there's a
deep trade-off wastes interview time.

**Today's example:** Scheduler polling every 1 min vs 5 min — negligible difference
at this volume. Pick one, briefly say why, move on.

**The phrase to use:** *"Honestly, either X or Y works fine here — the difference
is negligible at this scale. I'd pick X because [minor reason], but this isn't
a decision worth dwelling on."*

**Why this matters:** recognizing LOW-STAKES decisions is as much a signal of
seniority as reasoning through HIGH-STAKES ones. Don't over-analyze everything.

---

## 9. Self-Correction As A Skill (Not A Weakness)

**Today's examples:**
- "Wait, 48 hours doesn't work for monthly appointments" — caught your own
  inconsistency mid-design
- The unit-conversion error (2000/hour → wrongly became 1000/sec) — caught
  and corrected by working through it step by step out loud

**The technique:** When you notice something doesn't add up — SAY SO. Don't
quietly hope no one noticed, and don't panic. *"Wait — I think that conflicts
with what I said earlier about X. Let me adjust."*

**Why this works:** it demonstrates you're actually reasoning, not reciting.
Staff engineers have seen hundreds of rehearsed answers — genuine reasoning
with visible self-correction stands out.

---

## 10. Scope Management — "Parking" Ideas

**Today's example:** The AI-calling-bot fallback idea — creative, but you
correctly flagged it as "V2, out of scope for initial design."

**The phrase to use:** *"That's an interesting extension, but I'd consider it
out of scope for the initial design — flagging it as a V2 consideration so we
can focus on the core architecture."*

**Why this matters:** shows time-management and prioritization under a 60-minute
constraint — exactly what a Staff engineer does daily.

---

## 11. Schema Design Via Relationship Reasoning

**The technique that worked today:** Don't start with tables. Start with:
*"What is one-to-many here? What is many-to-one?"*

- One patient → many appointments → Appointment table has `patient_id` FK
- One doctor → many appointments → Appointment table has `doctor_id` FK
- One appointment → many reminders → Reminder table has `appointment_id` FK

**The general rule:** the foreign key lives on the "many" side, pointing to the "one" side.

**Apply this to ANY new domain Thursday** — identify the entities, then ask
"for each pair, which is the many side?"

---

## 12. Estimation Technique — Step By Step, Sanity-Checked

**The technique:**
1. Start from a number you're confident about (e.g. patients/day per provider)
2. Multiply outward methodically (× providers, × reminders per appointment)
3. Convert units ONE STEP AT A TIME (day → hour → minute → second)
4. SANITY CHECK against something you know: *"Is this big or small compared to
   [system I've worked with]?"*

**Today's example:** ~24K reminders/day → ~0.5-1/sec average → "this is TINY
compared to what SQS or Kafka handle, so infrastructure choice isn't driven by volume."

**If you get confused mid-calculation (likely under pressure):** say so, and work
through it out loud step by step. This is MORE impressive than getting it right
instantly — see Pattern 9.

---

## Quick-Fire Reminders For Thursday

- Ask clarifying questions BEFORE designing — don't jump to boxes
- Propose your own numbers for estimation rather than only asking the interviewer
- Label communication methods on every arrow (CVS rubric explicitly wants this)
- For every component, ask: "what happens if THIS goes down?"
- Name patterns explicitly (reconciliation, fault isolation, idempotency) — vocabulary
  signals depth
- When stuck, narrate — "let me think through this..." — don't go silent
- Connect to real production experience wherever genuinely relevant (FindCare, PDM,
  Spark/EMR, Kafka)
- Watch the clock — wrap-up (security/performance/uptime/scalability) must happen
  even if rushed
