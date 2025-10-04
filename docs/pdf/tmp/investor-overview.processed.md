# Investor Overview

## Executive Summary
DeliveryApp is a vertically integrated food delivery platform targeting restaurants, customers, and delivery personnel. The system is production-ready with a layered architecture, integrated payments, advertising, and customer engagement.

- Product: Admin dashboard + API + web frontends; modular domain for restaurants, menus, orders, chat, ads
- Status: Working codebase with extensive scripts and guides; multi-project .NET solution
- Edge: Built on ABP Framework, accelerating development with proven modules (Identity, OpenIddict, tenancy, permissions)

## Product and Technology
- Platform: .NET 9 + ABP; EF Core with SQL Server
- Authentication: OpenIddict (OIDC/OAuth2), roles for admin/manager/restaurant_owner/delivery/customer
- Payments: Stripe integration for card processing, connected accounts, payouts, refunds; cash-on-delivery supported
- Communications: Email via SendPulse; push notifications via Firebase
- Analytics/Engagement: Notification analytics and campaign entities
- Observability: Serilog logging, environment-based appsettings

## Business Model
- Commission on orders (configurable per restaurant)
- Platform fee on payments and transfers
- Advertising placements via `Advertisement` and `AdRequest` workflows
- Subscription/usage fees for advanced analytics and tools (optional)

## Traction & Readiness Indicators
- Complete domain model for delivery marketplace (restaurants, orders, reviews)
- Payments and payouts implemented end-to-end with fee calculation and webhook handling
- Admin workflows for categories, ads, users, performance reporting
- Extensive deployment and diagnostic scripts (IIS/SmarterASP), suggesting real-world runs

## Go-To-Market
- Start with restaurant onboarding and local partnerships
- Promote owner tools (menu, offers, ad requests) and faster payouts
- Leverage push/email campaigns for customer retention

## KPIs to Track
- GMV, Take Rate, Net Revenue
- Order success rate, Payment authorization success rate
- Delivery time, On-time percentage
- CAC, Retention (D30/D90), MAU/DAU
- Ad spend ROI for advertisers, conversion rate of offers

## Roadmap (High-Level)
- Mobile apps (React Native/Flutter) using existing API
- Advanced search and recommendation (personalization profiles exist)
- Fraud detection for payments and COD
- Multi-tenancy for franchise groups
- Observability upgrades (APM, dashboards, health checks)

## Risks & Mitigations
- Regulatory and payment compliance: use Stripe for KYC, maintain PCI scope via tokens
- Market competition: focus on local partnerships and differentiated owner tools
- Operational reliability: add CI/CD, blue-green deploys, health checks, canaries
- Data privacy: rotate secrets, segregate PII, encryption at rest, Audit logging

## Investment Use
- Expand engineering for mobile and growth features
- Marketing and restaurant acquisition
- Infrastructure for scale, monitoring, and data analytics

## Appendix: Architecture Snapshot
- Monolith with clear layers: Domain, Application, Infra, Web/API, Blazor frontends
- Entities include `Restaurant`, `Order`, `PaymentTransaction`, `Advertisement`, `Chat*` and more
- API and Web controllers secured by roles; OpenIddict clients declared in `appsettings`
- Scripts and guides included: deployment, security, performance, environment
