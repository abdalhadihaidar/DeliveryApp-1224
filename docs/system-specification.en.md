# System Specification (English)

## 1. Purpose and Scope
This document specifies DeliveryApp’s functional and non-functional requirements, architecture, data model, integrations, and runtime flows. It targets engineers, product owners, and stakeholders.

## 2. Functional Requirements
- User Management: register/login, roles (admin, manager, restaurant_owner, delivery, customer), profile management
- Restaurant Management: create/update restaurants, categories, menus, offers
- Ordering: browse menu, create order, add items/options, track status
- Payments: Stripe card payments, connected accounts/payouts; Cash on Delivery (COD)
- Advertising: owners submit AdRequests; admin reviews/approves; ads surface in feeds
- Notifications: email (SendPulse), push (Firebase); order and promo events
- Chat/Support: sessions and messages between admin and customers/delivery
- Reporting: delivery performance, financials, and restaurant analytics

## 3. Non-Functional Requirements
- Security: OIDC/OAuth2 via OpenIddict, RBAC, secret management, HTTPS, audit logs
- Performance: scalable DB, background jobs (ABP), caching (optional Redis)
- Reliability: retries for external calls, idempotent webhooks, logs via Serilog
- Observability: structured logs, health endpoints (to add), telemetry (to add)

## 4. High-Level Architecture
```mermaid
flowchart LR
  Client[Web/Blazor/Mobile] -->|HTTPS| WebApp[DeliveryApp.Web]
  Client -->|HTTPS| ApiHost[DeliveryApp.HttpApi.Host]
  subgraph Application Layer
    AppServices[Application Services]
  end
  WebApp <-->|ABP Modules| AppServices
  ApiHost <-->|ABP Modules| AppServices
  AppServices --> Db[(SQL Server)]
  AppServices --> Stripe[(Stripe API)]
  AppServices --> SendPulse[(SendPulse API)]
  AppServices --> Firebase[(Firebase Admin)]
  Redis[(Redis)] --- AppServices
```

## 5. Domain Model (Key Aggregates)
```mermaid
classDiagram
  class Restaurant {+Guid Id
    +string Name
    +decimal? CommissionRate
    +Address Address
    +List~MenuItem~ Menu
    +Guid? CategoryId}
  class MealCategory {+Guid Id +string Name +Guid RestaurantId}
  class Order {+Guid Id +Guid UserId +Guid RestaurantId
    +List~OrderItem~ Items +string Status +string PaymentStatus}
  class PaymentTransaction {+int Id +Guid OrderId
    +string PaymentIntentId +decimal Amount +string Status}
  class Advertisement {+Guid Id +Guid? RestaurantId +Guid? CreatedById}
  class AdRequest {+Guid Id +Guid RestaurantId +Guid? ReviewedById +Guid? AdvertisementId}
  class ChatSession {+Guid Id +string DeliveryId +string CustomerId +bool IsActive}
  class ChatMessage {+Guid Id +Guid SessionId +string SenderId +string Content}

  Restaurant --> MealCategory
  Restaurant --> Order
  Order --> OrderItem
  Order --> PaymentTransaction
  AdRequest --> Advertisement
  ChatSession --> ChatMessage
```

## 6. Key Flows

### 6.1 Registration & Login (OpenIddict)
```mermaid
sequenceDiagram
  participant U as User
  participant W as Web/API
  participant O as OpenIddict
  participant DB as Database
  U->>W: Register/Login request
  W->>O: Token/Authorize
  O->>DB: Validate credentials
  O-->>W: Access/ID token
  W-->>U: Authenticated session
```

### 6.2 Place Order and Pay (Stripe)
```mermaid
sequenceDiagram
  participant C as Customer
  participant API as HTTP API
  participant OS as Order Service
  participant PS as Stripe Payment Service
  participant ST as Stripe
  participant DB as Database
  C->>API: Create order
  API->>OS: Persist order (Pending Payment)
  OS->>DB: Insert Order
  C->>API: Pay (amount, method)
  API->>PS: Create PaymentIntent
  PS->>ST: Create PaymentIntent
  ST-->>PS: ClientSecret/Status
  PS->>DB: Save PaymentTransaction
  API-->>C: ClientSecret
  ST-->>API: Webhook (succeeded/failed)
  API->>PS: HandleWebhook
  PS->>DB: Update PaymentTransaction/Order Status
```

### 6.3 Cash on Delivery (COD)
```mermaid
sequenceDiagram
  participant C as Customer
  participant API as HTTP API
  participant OS as Order Service
  participant DB as Database
  C->>API: Create order (COD)
  API->>OS: Persist order (PaymentStatus=Pending)
  OS->>DB: Insert Order
  Note over API,DB: Settlement happens post-delivery via CODTransaction
```

### 6.4 Ad Request Workflow
```mermaid
sequenceDiagram
  participant RO as Restaurant Owner
  participant API as HTTP API
  participant ADS as AdRequest Service
  participant ADM as Admin
  participant DB as Database
  RO->>API: Submit AdRequest
  API->>ADS: Validate/Save
  ADS->>DB: Insert AdRequest
  ADM->>API: Review/Approve
  API->>ADS: Approve
  ADS->>DB: Link Advertisement
```

### 6.5 Notifications (SendPulse, Firebase)
```mermaid
sequenceDiagram
  participant APP as App Service
  participant SP as SendPulse
  participant FB as Firebase
  participant DB as Database
  APP->>SP: Send email (token->access)
  APP->>FB: Send push message
  APP->>DB: Track notification status
```

## 7. API Conventions
- RESTful endpoints in `DeliveryApp.HttpApi` and `DeliveryApp.Web` controllers
- Versioning (add when needed); standard error shape; RBAC via attributes
- Webhook endpoint for Stripe with signature verification

## 8. Configuration
- appsettings per project and environment
- Secrets via environment variables or secret manager
- Connection strings in `ConnectionStrings:Default`

## 9. Deployment
- Windows/IIS and shared hosting scripts included (SmarterASP)
- Use DbMigrator for migrations and seeding

## 10. Risks and Controls
- Rotate secrets; validate inputs; rate limit login/payment endpoints; enable health checks
```
