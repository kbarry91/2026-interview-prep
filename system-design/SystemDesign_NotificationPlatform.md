# System Design: Healthcare Notification Platform
## Interview Prep Study Notes
## Date: 2026-05-29

---

## THE PROBLEM STATEMENT

Design a notification system for a healthcare platform where:
- Clinicians receive notifications via push, SMS, and email
- Clinicians configure their own channel preferences
- System must be reliable — no notification ever lost
- Must handle fan out to thousands of clinicians simultaneously

---

## REQUIREMENTS GATHERED

### Questions You Should Always Ask:

| Question | Why It Matters |
|----------|---------------|
| Does it need to be real time? | Drives architecture complexity |
| How many concurrent users? | Drives scaling decisions |
| How many messages per day? | Drives infrastructure sizing |
| What are the fan out requirements? | Single event → many recipients |
| Can users set a priority channel? | Drives fallback logic |
| Is there a role based priority? | Doctors before nurses etc |
| What happens when providers fail? | Drives retry/reliability design |
| HIPAA retention requirements? | Drives storage and audit design |

### Answers For This System:

| Requirement | Detail |
|-------------|--------|
| Latency | <5 seconds delivery |
| Users | 300k total, 30k concurrent |
| Volume | 5M notifications per day |
| Fan out | Up to 10k simultaneous recipients |
| Channels | Push notifications, SMS, Email |
| Preferences | Clinician configurable, priority order |
| Fallback | Push fails → SMS within 3 seconds |
| Email | Always sent regardless as permanent record |
| Priority tiers | Critical = all at once, General = role based, Info = batch |
| Provider SLA | No guarantees — must handle failures gracefully |
| Retention | 7 years, fully auditable |
| HIPAA | Compliant across all channels |

### STAFF LEVEL INSIGHT — SLA Question:
Always ask about provider SLAs in any system using third party services.
No provider guarantees 100% delivery. This directly drives your retry
strategy, dead letter queue design, and fallback logic.

> "We don't own Twilio, we don't own the mobile devices, we don't own
> the email servers. What SLA do we have from these providers? This
> determines how aggressively we need to retry and monitor."

---

## COMPLETE ARCHITECTURE

```
┌─────────────────────────────────────────────────────────────┐
│                      CLIENT LAYER                            │
│                                                              │
│  ┌─────────────────┐        ┌──────────────────────────┐   │
│  │  Clinician App   │        │      Admin Portal         │   │
│  │                  │        │                           │   │
│  │ - Set preferences│        │ - Send hospital alerts    │   │
│  │ - View history   │        │ - Manage notification     │   │
│  │ - Channel config │        │   templates               │   │
│  └────────┬─────────┘        └───────────┬───────────────┘  │
└───────────┼──────────────────────────────┼──────────────────┘
            │ HTTPS / WSS                  │ HTTPS
            ↓                              ↓
┌─────────────────────────────────────────────────────────────┐
│           AZURE APPLICATION GATEWAY (Load Balancer)          │
│                                                              │
│  - Distributes traffic across API pods                       │
│  - SSL termination (decrypts HTTPS, forwards HTTP internally)│
│  - Health checks every 30 seconds                            │
│  - Removes unhealthy pods automatically                      │
│  - Sticky sessions for WebSocket connections                 │
│  - Algorithms: Round Robin or Least Connections              │
└─────────────────────────┬───────────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────────┐
│                  KUBERNETES CLUSTER (AKS)                    │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │                   API PODS                            │   │
│  │                                                       │   │
│  │  1. Authentication  → Verify JWT token                │   │
│  │  2. Authorisation   → Check role permissions          │   │
│  │  3. Rate Limiting   → Prevent abuse                   │   │
│  │  4. TLS Encryption  → All data encrypted in transit   │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              FAN OUT SERVICE PODS                     │   │
│  │                                                       │   │
│  │  - Reads from Kafka notifications topic               │   │
│  │  - Looks up User Preference Service                   │   │
│  │  - Determines channels + priority order per clinician │   │
│  │  - Publishes individual messages per clinician        │   │
│  │  - Handles role based priority ordering               │   │
│  │  - Scales horizontally via HPA                        │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌────────────┐  ┌────────────┐  ┌────────────────────┐    │
│  │ Push Pods  │  │  SMS Pods  │  │    Email Pods       │    │
│  │(Firebase)  │  │ (Twilio)   │  │   (SendGrid)        │    │
│  └────────────┘  └────────────┘  └────────────────────┘    │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │              RETRY SERVICE PODS                       │   │
│  │                                                       │   │
│  │  - Listens to retry Kafka topic                       │   │
│  │  - Retries failed deliveries up to 3 times            │   │
│  │  - Failed after 3 retries → Dead Letter Queue         │   │
│  │  - Dead letter → alert on-call engineer               │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
└──────────┬──────────────────────────────────────────────────┘
           │
           ↓
┌─────────────────────────────────────────────────────────────┐
│                      KAFKA TOPICS                            │
│                                                              │
│  notifications topic  → fan out service reads this          │
│  push topic           → push pods read this                 │
│  sms topic            → SMS pods read this                  │
│  email topic          → email pods read this                │
│  retry topic          → retry service reads this            │
│  audit topic          → audit service reads this            │
│                                                              │
│  WHY KAFKA SOLVES FAN OUT:                                   │
│  1 event → publish 10k messages to Kafka                     │
│  Multiple partitions → multiple consumer pods                │
│  All 10k processed in parallel → under 5 seconds            │
└──────────┬──────────────────────────────────────────────────┘
           │
           ↓
┌──────────────────┐  ┌──────────────────┐  ┌───────────────┐
│    CASSANDRA     │  │  USER PREFERENCE  │  │ AZURE SERVICE │
│    (NoSQL DB)    │  │     SERVICE       │  │     BUS       │
│                  │  │                   │  │               │
│ - Notification   │  │ - Channel prefs   │  │ - Scheduled   │
│   history        │  │ - Priority order  │  │   retry jobs  │
│ - Delivery status│  │ - Role mappings   │  │ - Delay queues│
│ - Audit logs     │  │ - Fallback rules  │  │ - Dead letter │
│ - 7yr retention  │  │                   │  │   queue       │
│ - AES-256        │  │ - Redis cached    │  │               │
│   encrypted      │  │   for speed       │  │               │
└──────────────────┘  └──────────────────┘  └───────────────┘
```

---

## KEY COMPONENT DEEP DIVES

### Load Balancer — Azure Application Gateway
The load balancer sits in front of EVERY service with multiple pods.

**Rule: Anywhere you have multiple pods doing the same job → load balancer**

```
Multiple API pods?          → Load balancer ✅
Multiple Fan Out pods?      → Load balancer ✅
Multiple SMS pods?          → Load balancer ✅
Kafka partitions?           → Kafka handles this itself ✅
Single database?            → No load balancer needed
```

**Load balancing algorithms:**
- Round Robin → requests distributed evenly in order
- Least Connections → new request goes to least busy pod

**What else load balancers do:**
- SSL Termination → decrypts HTTPS so pods don't have to
- Health Checks → removes crashed pods automatically
- Sticky Sessions → keeps WebSocket users on same pod

---

### Kafka — Self Load Balancing
Kafka distributes work WITHOUT a load balancer:

**Producer side — partition assignment:**
```
With message key    → hash(key) % partitions → same partition always
Without message key → round robin across partitions
```

**Consumer side — automatic rebalancing:**
```
3 partitions, 3 consumer pods:
  Partition 0 → Pod 1
  Partition 1 → Pod 2
  Partition 2 → Pod 3

Pod 2 crashes:
  Kafka detects via heartbeat timeout
  Reassigns Partition 1 → Pod 1 automatically
  Pod 2 recovers → resumes from last offset
  Zero messages lost ✅
```

**Why Kafka for fan out:**
```
Hospital wide alert → 10,000 clinicians
        ↓
Fan out service publishes 10,000 messages to Kafka
        ↓
Kafka distributes across partitions
        ↓
Consumer pods process in parallel
        ↓
All 10,000 notified in under 5 seconds ✅
```

---

### Fan Out — Role Based Priority

```
CRITICAL alert:
  → All clinicians simultaneously regardless of role

GENERAL alert:
  → Doctors first (highest priority partition)
  → Nurses second
  → Trainees last

INFORMATIONAL:
  → Batch send acceptable
  → Up to 60 second delay
```

Implementation: use separate Kafka partitions per priority tier,
or publish with priority metadata that fan out service respects.

---

### Retry Strategy + Dead Letter Queue

```
Notification attempt 1 → fails
        ↓ (wait 1 second)
Retry attempt 2 → fails
        ↓ (wait 2 seconds)
Retry attempt 3 → fails
        ↓
Dead Letter Queue
        ↓
Alert on-call engineer
        ↓
Manual investigation + resolution
```

**Dead Letter Queue is critical for HIPAA:**
No notification should silently disappear. Every failure must be
logged, retained, and investigated. Audit trail must be complete.

---

### User Preference Service

Dedicated microservice — not just a database table.

**Why dedicated service:**
- Consulted by fan out service on EVERY notification
- Must be extremely fast → Redis cache in front of it
- Separation of concerns — preferences logic isolated
- Can be updated independently without touching notification logic

**What it stores:**
```
clinicianId: "doc-123"
preferences: {
  channels: ["push", "sms", "email"],
  priority: ["push", "sms"],     // try push first, fallback to SMS
  fallbackDelay: 3,              // seconds before fallback
  quietHours: "22:00-07:00",     // no SMS during sleep hours
  role: "doctor"                 // determines alert priority tier
}
```

---

## HIPAA COMPLIANCE — FULL ANSWER

### In Transit:
- All API calls over HTTPS (TLS 1.2+)
- WebSocket connections use WSS
- No plaintext data travels the network

### At Rest:
- Cassandra: AES-256 encryption enabled
- Azure Blob Storage: Encrypted by default
- Redis: Azure Cache for Redis with encryption
- Kafka: Encrypted topics

### Access Control:
- JWT authentication on every API request
- Role based authorisation
- Clinicians only see their own notification history

### Notification Content:
- Push notifications: may contain summary only, no detailed PHI
- SMS: zero PHI — notification text + secure deep link only
- Email: sent to verified clinical email — can contain more detail
- All content decisions documented and auditable

### Audit Logging:
- Every notification sent, delivered, failed logged
- Who sent, who received, which channel, timestamp
- Retained 7 years per HIPAA regulation
- Tamper proof — append only audit log

---

## AWS → AZURE TRANSLATION

| AWS | Azure | Used In This System |
|-----|-------|-------------------|
| S3 | Azure Blob Storage | File attachments |
| SQS | Azure Service Bus | Retry queues, dead letter |
| SNS | Azure Notification Hubs | Push notifications |
| Lambda | Azure Functions | Serverless triggers |
| EKS | AKS | Kubernetes cluster |
| ElastiCache | Azure Cache for Redis | Preference caching |
| CloudWatch | Azure Monitor / New Relic | Monitoring + alerts |
| Route 53 | Azure DNS | DNS routing |

---

## MONITORING — NEW RELIC vs SPLUNK

### Your Current Experience (Splunk):
- Log aggregation across Kubernetes microservices
- Manual instrumentation needed for performance metrics
- Query language for complex log analysis
- Dashboard building

### New Relic Adds (APM):
- Automatic performance instrumentation — no manual logging
- Response time per endpoint automatically tracked
- Distributed tracing — follow one notification through every service
- Error rate tracking
- Throughput metrics
- Alerting on thresholds

### Key Metrics To Monitor For This System:
```
1. Notification delivery latency    → alert if > 5 seconds
2. Delivery success rate            → alert if < 99%
3. Dead letter queue depth          → alert if > 0
4. Kafka consumer lag               → alert if falling behind
5. Fan out service throughput       → messages per second
6. Provider error rates             → Twilio, SendGrid, Firebase
```

### Interview Answer:
> "I've used Splunk in my current role for log aggregation across
> Kubernetes. I had to manually instrument performance metrics —
> logging query times explicitly then querying Splunk to surface them.
> New Relic's APM solves this — it automatically instruments the
> application and captures response times without manual logging.
> Distributed tracing is particularly valuable here — I can follow
> a single notification through the API, Kafka, fan out service,
> and delivery provider in one trace. That visibility is critical
> for debugging latency issues in a distributed system."

---

## NOTIFICATION FLOW — STEP BY STEP

### Hospital Wide Critical Alert:
```
1.  Admin sends critical alert via portal
2.  API authenticates + authorises admin user
3.  API publishes to Kafka notifications topic
4.  Fan out service reads from Kafka
5.  Looks up all 300k clinicians (or relevant subset)
6.  Publishes 10,000 individual messages to Kafka
7.  Multiple consumer pods process in parallel
8.  Each pod consults User Preference Service
9.  Determines channels per clinician (push/SMS/email)
10. Push notification sent via Firebase
11. Email sent via SendGrid simultaneously
12. If push undelivered after 3 seconds → SMS via Twilio
13. All delivery attempts logged to Cassandra
14. Audit log written for every action
15. All 10,000 notified in under 5 seconds ✅
```

### Failed Delivery Flow:
```
1.  Push notification → Firebase returns error
2.  Retry service picks up failure
3.  Retries up to 3 times with exponential backoff
4.  Still failing → moves to Dead Letter Queue
5.  Azure Service Bus alerts on-call engineer
6.  Engineer investigates + resolves manually
7.  All failures logged for HIPAA audit trail
```

---

## INTERVIEW TIPS FOR THIS DESIGN

### Likely Follow Up Questions:

**"How do you handle 10,000 simultaneous notifications?"**
> "Kafka partitioning and parallel consumer pods. The fan out service
> publishes 10,000 individual messages to Kafka. Multiple partitions
> distribute work across consumer pods automatically. All processed
> in parallel — no single bottleneck."

**"What if Twilio goes down?"**
> "The retry service attempts delivery 3 times with exponential backoff.
> If all retries fail the message goes to a dead letter queue and alerts
> our on-call engineer. We'd also consider a secondary SMS provider as
> a fallback for critical notifications given no provider SLA guarantees
> 100% delivery."

**"How do you ensure no notification is lost?"**
> "Kafka's offset tracking guarantees no message is lost — consumers
> track their position and restart from there if they crash. The retry
> service handles provider failures. Dead letter queues capture anything
> that can't be delivered after retries. Every step is logged to
> Cassandra for a complete audit trail."

**"Why NoSQL over SQL for notification storage?"**
> "High write volume — 5M notifications per day — benefits from
> Cassandra's horizontal scaling. Access pattern is simple — fetch
> by clinician ID — so we don't need complex JOINs. The tradeoff is
> losing relational querying but that's acceptable for this use case."

**"How does the load balancer know which pod to send traffic to?"**
> "Azure Application Gateway uses least connections algorithm —
> routing new requests to the pod with fewest active connections.
> It performs health checks every 30 seconds and automatically removes
> unhealthy pods from the pool. For WebSocket connections it uses
> sticky sessions to keep a clinician on the same pod throughout
> their session."

---

## HOMEWORK — DO THIS BEFORE TOMORROW

- [ ] Draw this architecture by hand on paper
- [ ] Say the critical alert flow out loud without notes
- [ ] Remember: User Preference Service is a dedicated component
- [ ] Remember: Always mention encryption at rest AND in transit unprompted
- [ ] Remember: Dead letter queue shows reliability thinking
- [ ] Review AWS → Azure translation table
