# 🌆 Hayy - The Ultimate Local Business Discovery & Marketing Platform

**Hayy** is a smart digital ecosystem designed to solve the marketing dilemma for small local businesses (such as restaurants, cafes, gyms, and burger shops) by replacing expensive blogger promotions with authentic community discovery. The platform allows merchants to showcase their services and products as a powerful marketing tool, while enabling everyday users to discover hidden, high-quality gems right in their neighborhood based on real people's reviews and interactions.

---

## 🏗️ System Architecture & Polyglot Persistence
The platform is engineered using **Clean Architecture** to ensure independent domain deployment. To achieve maximum throughput and prevent server degradation under heavy traffic, the system applies a highly optimized **Polyglot Persistence & Asynchronous Offloading** strategy:


[ B2C Mobile App / Web ]
│
▼ (User Actions / Clicks / Searches)
[ SQL Server ] ──► (Core Transactional Data Only)
│
▼ (Asynchronous Interception)
[ Hangfire Background Jobs ]
│
▼ (High-Volume Offloading)
[ MongoDB Atlas ] ──► [ UserLogs & AI Recommendations Only ] ──► [ AI Engine Process ]

* **SQL Server (Primary Database):** Stores 100% of the core transactional and relational business data (Users, Payments, Business Subscription Plans, Verifications, Places, etc.) to guarantee strict consistency.
* **MongoDB Atlas (High-Volume Log Store):** Dedicated **strictly** to storing `UserLogs` and `AI Recommendations`. Because user actions generate massive telemetry data that can overload relational servers, these two high-traffic collections were offloaded to MongoDB.
* **Background Sync Pipeline:** **Hangfire** intercepts user actions in the background, pushing them asynchronously to MongoDB Atlas so the AI Engine can process recommendations without affecting primary database performance.

---
## 🧠 Intelligent AI Recommendation Loop
To deliver a highly personalized experience on the mobile app, the platform features a dedicated **AI & Recommendations Domain**[cite: 1]:
* **Smart Filtering:** When a user selects a specific category, the AI engine processes the request against the database to instantly surface the best-fitting local businesses.
* **Dynamic Feedback Loop:** The AI continuously monitors user behavior (likes, comments, bookmarked places, and reviews)[cite: 1]. It constantly adapts and mutates the user's home feed in real time to match their evolving interests.

---

## 🔒 Advanced Authentication Suite
A secure and unified identity system was implemented across both Web and Mobile platforms, supporting:
* **Custom JWT (JSON Web Tokens):** For secure, stateless role-based authorization (Admin, Merchant, and Consumer).
* **Social Auth & Verification:** Integrated **Sign in with Google** alongside a custom **OTP (One-Time Password)** engine for secure registration and identity verification.

---

## ⭐ My Technical Ownership & Contributions
As the Core Backend, DevOps, and Infrastructure Engineer, I personally architected, coded, and deployed the following core components:

### 1. Cloud Infrastructure, Security & DevOps (Azure)
**Team IAM & Access Control: Governed Azure cloud environments for the development team and implemented strict data protection by whitelisting developer IPs via Azure SQL Firewalls**.
**System Integration & Hosting:** Managed the deployment of the Frontend Web Portal, configuring reverse proxies, CORS headers, and securing communication pipelines with the hosted .NET 10 Web API.
* **CI/CD Pipelines:** Built automated workflows using GitHub Actions for seamless, zero-downtime deployment directly to Azure App Services upon code pushes.

### 2. Distributed Automation (Hangfire) & Concurrency
* **Hangfire Automation Engine:** Implemented and configured the Hangfire Dashboard to handle heavy background tasks, including checking subscription expirations, managing dynamic waitlist countdowns, and executing automated database maintenance.
* **Concurrency-Safe Ticketing:** Engineered an **Optimistic Concurrency Control** mechanism using `RowVersion` to protect event ticket quantities from over-selling during concurrent peak reservation traffic.
* **Intelligent Waitlist Queue:** Developed background tasks to automatically manage over-capacity events, moving users to a ranked waitlist and auto-promoting the next available user if a pending ticket expires.

### 3. FinTech, Real-Time Hubs & Optimization
* **Dual-Payment Gateways:** Built a decoupled payment structure separating B2B SaaS subscription billing (Merchant Packages)[cite: 1] from B2C Event Ticketing checkouts.
* **Dynamic QR Ticket Generator:** Designed an automated system to generate unique, secure QR codes upon successful payment, allowing instant digital check-ins at event gates.
* **Real-Time Core (SignalR):** Designed full-duplex WebSocket communication via SignalR for live push notifications, instant community feed updates, and real-time user-to-merchant interactions.
* **Custom Pagination & Filters:** Developed high-performance pagination logic at the data repository layer to ensure blazing-fast API responses and minimal mobile data consumption.

---

## 🤝 Team Collaboration & Agile Methodology
We managed this cross-functional ecosystem using the **Agile/Scrum Framework**, executing tasks in iterative sprints, participating in daily stand-ups, and practicing continuous integration. While I governed the foundational infrastructure, FinTech engines, and background pipelines, my team collaboratively developed standard CRUD operations across the following functional domain.
* **Place & Category Domain:** Managing merchant store profiles, dynamic business tags, and operating hours.
* **Events & Offers Domain:** Standard pipelines for managing local business discounts, deal posts, and event listings.
* **Content Domain:** Facilitating standard user-generated content, including post comments, review stars, and report flags.

---

## 🛠️ The Tech Stack Arsenal
* **Backend Framework:** .NET 10 Web API (C#) & **LINQ (Language Integrated Query)**
* **Data Access:** Entity Framework Core & MongoDB Driver
* **Real-Time Framework:** Microsoft SignalR
* **Task Scheduler:** Hangfire Dashboard
* **Cloud Infrastructure:** Azure App Service, Azure SQL, GitHub Actions
* **Logging:** Serilog Structured Logging.
* ![System Architecture]()
