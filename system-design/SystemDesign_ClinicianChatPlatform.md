# System Design: Clinician Real-Time Chat Platform
## Interview Prep Study Notes
## Date: 2026-05-29

---

## THE PROBLEM STATEMENT

Design a real-time two-way chat system for clinicians that supports:
- Clinician to clinician and clinician to patient messaging
- Text, images, and PDF file sharing
- SMS fallback for unread messages
- HIPAA compliance
- Enterprise scale

---

## REQUIREMENTS GATHERED

Always clarify requirements before designing. These are the questions you asked:

| Requirement | Detail |
|-------------|--------|
| Latency | <2 seconds clinician-to-clinician |
| Users | 500k total, 50k concurrent peak |
| Messages | 10M per day |
| Compliance | HIPAA — encrypted in transit AND at rest |
| Access Control | Role based — clinicians only see their conversations |
| File Support | Text, images, PDFs (video/audio out of scope) |
| Retention | 7 years with full audit logs |
| SMS Fallback | Triggers after 5min unread, zero PHI in SMS body |

### KEY INTERVIEW SKILL:
Always ask these clarifying questions before designing:
1. Does it need to be real time?
2. What are the security and compliance requirements?
3. What is the scale — users, messages per day, concurrent users?
4. What data needs to be stored and for how long?
5. What file types need to be supported?
6. How does the system behave when things go wrong? (fallback behaviour)

---

## COMPLETE ARCHITECTURE

```
┌─────────────────────────────────────────────────────────┐
│                    CLIENT LAYER                          │
│            React Native Mobile App (WSS + HTTPS)         │
└─────────────────────────┬───────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────┐
│                   LOAD BALANCER                          │
│         Distributes 50k concurrent connections           │
└─────────────────────────┬───────────────────────────────┘
                          │
                          ↓
┌─────────────────────────────────────────────────────────┐
│              KUBERNETES CLUSTER (AKS)                    │
│                                                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │                  API PODS                         │   │
│  │  1. TLS/HTTPS Encryption                         │   │
│  │  2. Authentication (JWT token verification)       │   │
│  │  3. Authorisation (role based access control)     │   │
│  │  4. Rate Limiting (prevent spam/abuse)            │   │
│  └──────────────────────────────────────────────────┘   │
│                                                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │             WEBSOCKET PODS                        │   │
│  │  - Maintains persistent connections per user      │   │
│  │  - Each pod handles ~17k concurrent connections   │   │
│  │  - Autoscales via Horizontal Pod Autoscaler       │   │
│  └──────────────────────────────────────────────────┘   │
│                                                          │
│  ┌──────────────────────────────────────────────────┐   │
│  │            SMS WORKER MICROSERVICE                │   │
│  │  - Listens to Azure Service Bus delayed jobs      │   │
│  │  - Checks message read status in Cassandra        │   │
│  │  - Triggers Twilio if message unread after 5min   │   │
│  └──────────────────────────────────────────────────┘   │
│                                                          │
└──────────────┬──────────────────┬───────────────────────┘
               │                  │
               ↓                  ↓
┌──────────────────┐   ┌─────────────────────────────────┐
│  REDIS (Azure    │   │         KAFKA TOPICS             │
│  Cache)          │   │                                  │
│                  │   │  - messages topic                │
│  Pub/Sub:        │   │  - notifications topic           │
│  - Pod routing   │   │  - audit topic                   │
│  - "Which pod    │   │                                  │
│    is user on?"  │   │  High throughput, durable,       │
│                  │   │  ordered message delivery        │
│  Session:        │   └──────────────┬──────────────────┘
│  - Active users  │                  │
│  - Read receipts │                  ↓
└──────────────────┘   ┌─────────────────────────────────┐
                       │     CONSUMER MICROSERVICE        │
                       │                                  │
                       │  - Reads from Kafka topics       │
                       │  - Routes to correct pod         │
                       │    via Redis Pub/Sub             │
                       │  - Writes to Cassandra           │
                       │  - Schedules SMS delay jobs      │
                       └──────────────┬──────────────────┘
                                      │
               ┌──────────────────────┼──────────────────┐
               ↓                      ↓                   ↓
┌──────────────────┐  ┌───────────────────┐  ┌───────────────────┐
│    CASSANDRA     │  │  AZURE BLOB       │  │  AZURE SERVICE    │
│    (NoSQL DB)    │  │  STORAGE          │  │  BUS              │
│                  │  │                   │  │                   │
│  - Messages      │  │  - Images         │  │  - Delayed SMS    │
│  - Audit logs    │  │  - PDFs           │  │    jobs           │
│  - Read receipts │  │  - File metadata  │  │  - 5min timer     │
│  - 7yr retention │  │                   │  │    per message    │
│  - AES-256       │  │  - AES-256        │  │                   │
│    encrypted     │  │    encrypted      │  └────────┬──────────┘
└──────────────────┘  └───────────────────┘           │
                                                       ↓
                                            ┌───────────────────┐
                                            │      TWILIO       │
                                            │                   │
                                            │  - SMS delivery   │
                                            │  - Zero PHI       │
                                            │  - Secure link    │
                                            │    back to app    │
                                            └───────────────────┘
```

---

## KEY COMPONENT EXPLANATIONS

### WebSocket
- Protocol that upgrades HTTP into a persistent two-way connection
- Think of it like a phone call vs letters (HTTP)
- Used by WhatsApp, Slack, every real-time chat app
- In code: WSS (WebSocket Secure) = encrypted WebSocket over TLS
- Why not HTTP polling? Too slow, too much overhead at scale

### Kafka
- Distributed message streaming platform
- Messages are published to TOPICS
- Consumer microservices subscribe to topics and process messages
- Why Kafka? Handles 10M+ messages/day, durable, ordered, replayable
- Key concepts: Topics, Partitions, Consumers, Consumer Groups, Offsets

### Redis Pub/Sub + Routing
- Each WebSocket pod subscribes to its OWN Redis channel only
- When a user connects to Pod 2, Redis stores: "userX → pod2"
- When Pod 1 needs to deliver to userX, it looks up Redis → finds pod2
- Pod 1 publishes to "pod2" channel → Pod 2 delivers via WebSocket
- Solves cross-pod delivery without pods knowing about each other

### Cassandra (NoSQL)
- Chosen over SQL because:
  - Horizontal scaling handles 10M messages/day growth
  - High write throughput
  - Simple access pattern — fetch by conversation ID (no JOINs needed)
- Tradeoff: No complex JOIN queries (acceptable for chat)
- AES-256 encryption at rest (HIPAA requirement)

### Azure Blob Storage (equivalent to AWS S3)
- Stores binary files — images, PDFs
- Cassandra stores only the URL reference to the file
- Direct client download from Blob Storage (reduces server load)
- AES-256 encrypted at rest by default in Azure

### Azure Service Bus (equivalent to AWS SQS)
- Supports delayed/scheduled messages
- When message delivered → schedule job for 5 minutes later
- SMS Worker picks up job → checks read status → sends SMS or cancels

---

## MESSAGE FLOW — STEP BY STEP

### Sending a Message:
```
1.  Clinician A types message, hits send
2.  App sends HTTPS POST to API layer
3.  API verifies JWT token (Authentication)
4.  API checks role permissions (Authorisation)
5.  API applies rate limiting
6.  Message encrypted, published to Kafka topic
7.  Consumer microservice picks message off Kafka
8.  Consumer writes message to Cassandra (encrypted)
9.  Consumer looks up Redis: "Which pod is Clinician B on?"
10. Consumer publishes to that pod's Redis channel
11. WebSocket pod pushes message to Clinician B's device
12. Consumer schedules 5min SMS delay job on Azure Service Bus
```

### Read Receipt Flow:
```
1.  Clinician B sees and reads the message
2.  App sends READ RECEIPT back via WebSocket
3.  Cassandra updates: message status = READ
4.  Azure Service Bus job fires after 5 minutes
5.  SMS Worker checks Cassandra: status = READ → cancel, do nothing ✅
```

### SMS Fallback Flow:
```
1.  Azure Service Bus job fires after 5 minutes
2.  SMS Worker checks Cassandra: status = UNREAD
3.  SMS Worker calls Twilio API
4.  Twilio sends SMS: "You have a secure message. Tap to view: [link]"
5.  Zero PHI in SMS body (HIPAA requirement)
6.  Link opens app directly to the message (deep link)
```

### File Upload Flow:
```
1.  Clinician attaches PDF/image
2.  API uploads file to Azure Blob Storage
3.  Blob Storage returns secure URL
4.  URL stored in Cassandra with message record
5.  Recipient receives message with URL reference
6.  Recipient's app fetches file directly from Blob Storage
```

---

## HIPAA COMPLIANCE — FULL ANSWER

HIPAA requires PHI (Protected Health Information) to be protected at every layer:

### In Transit:
- All API calls over HTTPS (TLS 1.2+)
- WebSocket connections use WSS (WebSocket Secure)
- No plaintext data ever travels the network
- File uploads to Blob Storage over HTTPS

### At Rest:
- Cassandra: AES-256 encryption enabled
- Azure Blob Storage: Encrypted by default
- Redis: Azure Cache for Redis with encryption
- Kafka: Encrypted topics
- Audit logs: Encrypted and tamper-proof

### Access Control:
- JWT authentication on every API request
- Role based authorisation — clinicians only access their conversations
- Audit log records every action: who sent, who read, when

### SMS Fallback:
- Zero PHI in SMS body
- Only notification text + secure deep link
- PHI only accessible inside the authenticated app

### Data Retention:
- Messages retained for 7 years per HIPAA regulation
- Secure deletion after retention period
- Full audit trail maintained throughout

---

## AWS → AZURE TRANSLATION

Since you know AWS — here are the equivalents for this Azure role:

| AWS | Azure | Purpose |
|-----|-------|---------|
| S3 | Azure Blob Storage | File storage |
| SQS | Azure Service Bus | Message queuing |
| Lambda | Azure Functions | Serverless compute |
| EKS | AKS (Azure Kubernetes Service) | Kubernetes |
| ElastiCache | Azure Cache for Redis | Redis |
| CloudWatch | Azure Monitor / New Relic | Monitoring |
| RDS | Azure SQL Database | Relational DB |
| DynamoDB | Cosmos DB | NoSQL DB |
| SNS | Azure Notification Hubs | Push notifications |
| Route 53 | Azure DNS | DNS |
| API Gateway | Azure API Management | API Gateway |

---

## SQL vs NoSQL — DECISION GUIDE

| Factor | SQL | NoSQL |
|--------|-----|-------|
| Data relationships | Complex JOINs | Simple access patterns |
| Scale | Vertical (bigger server) | Horizontal (more servers) |
| Write volume | Moderate | Very high |
| Schema | Fixed, structured | Flexible, evolving |
| Consistency | Strong | Eventual |
| Examples | PostgreSQL, SQL Server | Cassandra, MongoDB |
| Use for chat? | No — overkill | Yes — simple + scalable |

### Staff Level Answer Formula:
"I'd choose X because [requirement]. The tradeoff is [weakness],
but that doesn't matter here because [justification]."

---

## KEY TERMINOLOGY TO KNOW

| Term | Definition |
|------|------------|
| WebSocket | Persistent two-way connection between client and server |
| WSS | WebSocket Secure — encrypted WebSocket |
| JWT | JSON Web Token — used for authentication |
| Authentication | Who are you? (verify identity) |
| Authorisation | What can you do? (verify permissions) |
| Pub/Sub | Publish/Subscribe — messaging pattern |
| Kafka Topic | Named channel messages are published to |
| Consumer | Service that reads messages from Kafka |
| Horizontal Scaling | Adding more servers to handle load |
| Vertical Scaling | Making one server more powerful |
| AES-256 | Encryption standard used for data at rest |
| TLS | Transport Layer Security — encrypts data in transit |
| PHI | Protected Health Information — HIPAA regulated data |
| HPA | Horizontal Pod Autoscaler — Kubernetes auto scaling |
| AKS | Azure Kubernetes Service |
| Redis | In-memory data store — fast, temporary, shared state |
| Cassandra | Distributed NoSQL database — high write throughput |
| Twilio | Third party SMS delivery service |
| Deep Link | URL that opens directly to a specific screen in an app |
| Rate Limiting | Preventing users from sending too many requests |
| Audit Log | Record of all actions — who did what and when |

---

## INTERVIEW TIPS

### When Asked a System Design Question:
1. ALWAYS clarify requirements first — never jump to solutions
2. Establish scale early — it drives every decision
3. Start high level — components first, then go deep
4. Think out loud — interviewers want to hear your reasoning
5. Acknowledge tradeoffs — there is no perfect solution
6. Bring it back to requirements — justify every decision

### Likely Follow-Up Questions:
- "How do you ensure HIPAA compliance?" → See HIPAA section above
- "How does your system handle 50k concurrent users?" → Kubernetes + autoscaling
- "What happens if a pod crashes?" → Kubernetes restarts it, Redis routing updates
- "How do you handle message ordering?" → Kafka guarantees ordering within a partition
- "How would you monitor this system?" → New Relic / Azure Monitor, alerts on latency and error rates
- "What would you do differently at 10x scale?" → Partition Cassandra, add more Kafka partitions, regional deployment

---

## HOMEWORK BEFORE TUESDAY

- [ ] Draw this architecture by hand on paper
- [ ] Practice saying the message flow out loud without notes
- [ ] Learn the difference between Authentication vs Authorisation
- [ ] Read about Kafka topics and partitions (15 mins)
- [ ] Review the AWS → Azure translation table
- [ ] Complete 3 more Codility problems (Sunday + Monday)
