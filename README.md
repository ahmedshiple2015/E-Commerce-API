# E-Commerce API

ASP.NET Core Web API for an e-commerce platform with authentication, catalog management, carts, checkout, orders, payments, seller tools, admin operations, and wishlist support.

## Tech Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- JWT authentication
- Swagger / OpenAPI
- Stripe-ready payment integration
- Mailtrap email delivery for confirmation and notification emails

## Project Structure

```text
ECommerce.API             HTTP controllers, DTOs, app configuration
ECommerce.Application     Application interfaces and payment contracts
ECommerce.Domain          Domain entities and enums
ECommerce.Infrastructure  EF Core DbContext, migrations, services
```

## Main Features

- User registration, login, email confirmation, JWT issuance
- Customer, Seller, and Admin roles
- User profiles, addresses, wishlist
- Categories and products with images, reviews, filtering, and pagination
- Guest and authenticated carts
- Checkout, orders, order status history
- Payment endpoints and webhook handling
- Seller onboarding and seller-scoped product/order access
- Admin user moderation, seller approval, order overview, and banners
- Static product image uploads under `wwwroot/uploads/products`

## Local Setup

### Prerequisites

- .NET 8 SDK
- SQL Server / SQL Server Express
- EF Core CLI tools

### 1. Configure the API

Edit `ECommerce.API/appsettings.json`.

Important settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=ECommerceDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False"
  },
  "App": {
    "ApiBaseUrl": "http://localhost:61125"
  },
  "Jwt": {
    "Key": "replace-with-a-long-secret",
    "Issuer": "ECommerceApi",
    "Audience": "ECommerceUsers",
    "DurationInMinutes": 60
  },
  "Mailtrap": {
    "ApiToken": "set-with-user-secrets-or-environment-variable",
    "ApiUrl": "https://send.api.mailtrap.io/api/send",
    "FromEmail": "your-sender@example.com",
    "FromName": "Your Store",
    "OverrideRecipientEmail": ""
  }
}
```

For real projects, keep JWT keys, mail tokens, and payment secrets in user secrets or environment variables rather than committed JSON.

### 2. Restore and build

```bash
dotnet restore ECommerceSolution.sln
dotnet build ECommerceSolution.sln
```

### 3. Apply migrations

```bash
dotnet ef database update --project ECommerce.Infrastructure --startup-project ECommerce.API
```

Current migrations include:

- Initial schema
- Guest cart/order access tokens
- Mock product seed data
- Mock product photos
- Mock users

### 4. Run the API

```bash
dotnet run --project ECommerce.API
```

Default local URLs from the launch profile:

- HTTP: `http://localhost:61125`
- HTTPS: `https://localhost:61124`
- Swagger: `http://localhost:61125/swagger`

The API CORS policy allows the Angular frontend at:

```text
http://localhost:4200
```

## Seeded Development Data

### Mock accounts

All seeded accounts use:

```text
Password123!
```

| Role | Email |
| --- | --- |
| Customer | `mock.customer@ecommerce.local` |
| Seller | `mock.seller@ecommerce.local` |
| Admin | `mock.admin@ecommerce.local` |

Seed data also includes:

- 30 mock products
- 5 categories
- Product image URLs
- 1 approved mock seller store

## Authentication Flow

1. Register with `POST /api/auth/register`
2. Confirm the account with the emailed confirmation link
3. Login with `POST /api/auth/login`
4. Send the returned JWT in protected requests:

```http
Authorization: Bearer <token>
```

Email confirmation links are built from:

```text
App:ApiBaseUrl
```

Keep that value aligned with the port where the API is actually running.

## Core Endpoints

### Auth

| Method | Endpoint | Purpose |
| --- | --- | --- |
| POST | `/api/auth/register` | Register user |
| POST | `/api/auth/login` | Login and receive JWT |
| GET | `/api/auth/confirm-email` | Confirm email |
| POST | `/api/auth/resend-confirmation` | Resend confirmation email |

### Products and Categories

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/products` | Product list with filters and pagination |
| GET | `/api/products/{id}` | Product details |
| GET | `/api/products/{id}/reviews` | Product reviews |
| POST | `/api/products` | Create product, Seller/Admin |
| PUT | `/api/products/{id}` | Update product, Seller/Admin |
| DELETE | `/api/products/{id}` | Soft-delete product, Seller/Admin |
| POST | `/api/products/images` | Upload product image, Seller/Admin |
| POST | `/api/products/{id}/reviews` | Add review, Customer/Admin |
| GET | `/api/categories` | List categories |
| POST | `/api/categories` | Create category, Admin |

Supported product query parameters include:

```text
search
categoryId
minPrice
maxPrice
minRating
page
pageSize
```

### Users

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/users/{id}/profile` | Read profile |
| PUT | `/api/users/{id}/profile` | Update profile |
| POST | `/api/users/{id}/addresses` | Add address |
| PATCH | `/api/users/{id}/addresses/{addressId}/default-shipping` | Set default shipping address |
| DELETE | `/api/users/{id}/addresses/{addressId}` | Delete address |
| GET | `/api/users/{id}/wishlist` | Read wishlist |
| POST | `/api/users/{id}/wishlist/{productId}` | Add product to wishlist |
| DELETE | `/api/users/{id}/wishlist/{productId}` | Remove product from wishlist |

### Cart

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/carts` | Read cart |
| POST | `/api/carts/items` | Add item |
| PUT | `/api/carts/items/{productId}` | Update quantity |
| DELETE | `/api/carts/items/{productId}` | Remove item |
| POST | `/api/carts/merge` | Merge guest cart into user cart |

Cart access supports authenticated users and guest sessions.

### Sellers

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/sellers/me` | Current seller profile |
| POST | `/api/sellers` | Register seller profile |
| GET | `/api/sellers/{id}/products` | Seller products |
| GET | `/api/sellers/{id}/orders` | Seller orders |

### Orders and Payments

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/orders` | Read user orders |
| GET | `/api/orders/{id}` | Read order |
| GET | `/api/orders/guest/{id}` | Read guest order |
| POST | `/api/orders/checkout` | Checkout |
| PATCH | `/api/orders/{id}/status` | Update status, Admin/Seller |
| POST | `/api/payments/create-intent` | Create Stripe payment intent |
| GET | `/api/payments/{id}` | Read payment |
| GET | `/api/payments/guest/{id}` | Read guest payment |
| POST | `/api/payments/webhook` | Generic payment webhook |
| POST | `/api/payments/stripe/webhook` | Stripe webhook |

### Admin and Banners

| Method | Endpoint | Purpose |
| --- | --- | --- |
| GET | `/api/admin/users` | List users |
| PATCH | `/api/admin/users/{id}/suspend` | Suspend/unsuspend user |
| PATCH | `/api/admin/users/{id}/activate` | Activate user |
| DELETE | `/api/admin/users/{id}` | Soft-delete user |
| GET | `/api/admin/sellers` | List sellers |
| PATCH | `/api/admin/sellers/{id}/approve` | Approve seller |
| GET | `/api/admin/orders` | List all orders |
| POST | `/api/admin/banners` | Create banner |
| GET | `/api/admin/banners` | List banners |
| GET | `/api/banners` | Public active banners |

## Example Requests

### Login

```bash
curl -X POST http://localhost:61125/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "mock.customer@ecommerce.local",
    "password": "Password123!"
  }'
```

### Get products

```bash
curl "http://localhost:61125/api/products?page=1&pageSize=12"
```

### Add to wishlist

```bash
curl -X POST http://localhost:61125/api/users/900101/wishlist/900001 \
  -H "Authorization: Bearer <token>"
```

## Notes

- Users, products, and orders use soft deletion where appropriate.
- JWT-protected endpoints enforce ownership unless the caller is an Admin.
- Seller endpoints are scoped to the authenticated seller unless the caller is an Admin.
- Reviews can only be added by customers who previously purchased the product.
- If builds fail because DLLs are locked, stop the running `ECommerce.API` process and rebuild.

