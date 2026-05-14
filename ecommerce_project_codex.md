# E-Commerce Platform - Technical Codex

## 1. Architecture & Tech Stack Overview
* **Backend Framework:** C# / .NET Core Web API
* **ORM:** Entity Framework Core
* **Database:** SQL Server
* **Authentication:** ASP.NET Core Identity with JWT (JSON Web Tokens)

---

## 2. Module Specifications

### Module 1: User & Identity Management
* **Core Entities:** `ApplicationUser` (inherits `IdentityUser`), `Address`, `WishlistItem`, `UserReview`
* **Roles:** `Admin`, `Seller`, `Customer`
* **Features:**
  * **Auth:** Registration & Login (Email/Phone) integrated with Email Confirmation flows.
  * **Profile:** Management of personal details, multiple shipping/billing addresses, and saved payment details.
  * **Engagement:** Wishlist & Favorites tracking linked to the user account.
  * **History:** Dedicated endpoints for querying user-specific Order history.
  * **Feedback:** Reviews & Ratings system (1-5 scale) tied to purchased products.

### Module 2: Product & Catalog Management
* **Core Entities:** `Product`, `Category`, `ProductImage`
* **Features:**
  * **Categorization:** Hierarchical or flat category structures.
  * **Listings:** Product entities containing multiple images, rich descriptions, and pricing tiers.
  * **Inventory:** Real-time stock availability tracking.
  * **Discovery:** Search by name and dynamic LINQ-based filtration (by price range, category, rating, etc.).

### Module 3: Shopping Cart & Checkout
* **Core Entities:** `Cart`, `CartItem` (Consider storing active carts in Redis or an optimized SQL table for performance)
* **Features:**
  * **Cart Operations:** Add/remove items, quantity adjustments with immediate stock validation.
  * **Summaries:** Real-time order summary calculation including subtotal, taxes, and shipping price breakdowns.
  * **Guest Access:** Session-based guest checkout options.
  * **Payment Methods:** Support for Credit Card, PayPal, Cash on Delivery (COD), and internal Wallet balances.

### Module 4: Order Management
* **Core Entities:** `Order`, `OrderItem`, `OrderStatusHistory`
* **Features:**
  * **Transactions:** Secure order placement and confirmation.
  * **State Machine:** Order tracking with distinct status updates (e.g., *Pending, Processing, Shipped, Delivered, Cancelled*).
  * **Notifications:** Asynchronous email notifications triggered on status changes.

### Module 5: Payment Integration
* **Features:**
  * **Gateways:** Integration with Stripe, PayPal, and Razorpay APIs.
  * **Security:** Webhook implementations to securely verify payment success before order fulfillment.

### Module 6: Admin Panel Operations
* **Features:**
  * **User Moderation:** Approve, restrict, or suspend users (Implemented via a `Soft Delete` boolean flag and EF Core Global Query Filters).
  * **Catalog Oversight:** Global Product & Category management.
  * **Fulfillment:** Master view for Order & Shipping management.
  * **CMS:** Content management controls for dynamic homepage banners and promotional content.

### Module 7: Seller (Vendor) Management
* **Features:**
  * **Onboarding:** Seller registration and store profile setup.
  * **Vendor Scoping:** Product listing & inventory management scoped securely to the authenticated Seller's ID.

---

## 3. Implementation Notes & Best Practices
* **Soft Deletion:** Implement an `IsDeleted` property on core entities (Users, Products, Orders) rather than hard-deleting records to maintain historical data integrity.
* **Asynchronous Programming:** Utilize `async/await` patterns across all controller endpoints and database calls to maximize thread pool availability.
* **DTOs & AutoMapper:** Strictly use Data Transfer Objects (DTOs) for API requests/responses to prevent over-posting and decouple the database schema from the presentation layer.
