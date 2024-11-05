# MiniWebApplication

MiniWebApplication is a web application built with ASP.NET Core MVC, providing functionalities for product browsing, shopping cart management, payment processing, user authentication, and reviews. It uses **Azure SQL** and **Azure Cosmos DB** for database management, and Entity Framework Core for ORM.

## Features

- **Product Browsing**: Search and view available products.
- **Popular Items**: Display most frequently purchased dishes and trending items.
- **Personalized Recommendations**: Rule-based recommendation system based on user preferences and tags.
- **Shopping Cart Management**: Add, update, and remove products from the shopping cart.
- **Payment Integration**: Manage payment cards in "My Wallet" and select cards for checkout.
- **User Authentication**: Secure login and registration system.
- **Email Verification**: Email verification upon registration.
- **Order Management**: View order history and order details.
- **Reviews**: Leave and view product reviews (powered by Azure Cosmos DB).
- **Desktop Interface**: Optimized for computer screens only.

## Project Structure

### Controllers

- **HomeController**: Main pages, product browsing, and search.
- **ShoppingCartController**: Shopping cart operations (add, update, remove).
- **PaymentController**: Manage payment cards.
- **AccountController**: User login, logout, registration, and email verification.
- **OrdersController**: View order history and details.
- **ReviewsController**: Handle product reviews with Azure Cosmos DB.
- **RecommendationController**: Handle popular items and personalized recommendations.

### Data

- **ApplicationDbContext**: Database context with **Azure SQL Database** for relational data (users, products, orders, etc.).
- **CosmosDbService**: Connects and manages **Azure Cosmos DB** for reviews.

### Models

- **User**: Represents registered users with email verification and preference tags.
- **Product**: Product details, including name, description, price, image URL, and category tags.
- **ShoppingCartItem**: Items in the shopping cart.
- **PaymentCard**: Payment card details, including balance and card information.
- **Order**: User orders and product details.
- **Review**: Product reviews stored in **Azure Cosmos DB**.
- **PopularItem**: Tracks product popularity metrics and trending status.
- **UserPreference**: Stores user preference tags for recommendation system.

### ViewModels

- **Custom ViewModels**: Data structures for clean separation between UI and backend logic.

### Services

- **CosmosDbService**: CRUD operations for reviews in **Azure Cosmos DB**.
- **EmailService**: Sends email verifications.
- **OptimizationService**: Improves performance, especially for complex queries.
- **RecommendationService**: Handles popular items calculation and rule-based recommendations.

### Views

- **Home**: Product browsing and main page with popular items section.
- **ShoppingCart**: Shopping cart overview and checkout.
- **Payment**: Manage payment cards ("My Wallet").
- **Orders**: Past orders and details.
- **Reviews**: View and add product reviews.
- **Account**: Login, registration, and verification.
- **Recommendations**: Personalized product recommendations based on user tags.

### wwwroot

- **CSS**: Custom styles.
- **JS**: JavaScript for interactivity.
- **Images**: Logos and product images.

## Database Setup

### Azure SQL Database

1. **Setup**: Create an Azure SQL Database and add the connection string in `appsettings.json`.
2. **Apply Migrations**:
   ```bash
   dotnet ef database update
   ```

## Azure Cosmos DB

1. **Setup**: Create an Azure Cosmos DB account specifically for storing product reviews.

2. **Configure Connection**: In `appsettings.json`, add your Cosmos DB settings as shown below:

   ```json
   "CosmosDbSettings": {
       "AccountEndpoint": "<your_cosmosdb_account_endpoint>",
       "AccountKey": "<your_cosmosdb_account_key>",
       "DatabaseName": "<your_database_name>",
       "ContainerName": "<your_container_name>"
   }
   ```

3. **Initialize the Container (if needed)**: Ensure the specified container exists in the Cosmos DB database for the reviews feature.

## Email Setup

1. **Configure Email Settings**: Add the following section to your `appsettings.json`:

   ```json
   "EmailSettings": {
       "SmtpServer": "<smtp_server>",
       "SmtpPort": "<smtp_port>",
       "SenderEmail": "<sender_email>",
       "SenderPassword": "<sender_password>"
   }
   ```

2. **Features**
  - **Email Verification**: New users receive an email verification link upon registration.

## Usage Guide

### Key Pages
- **Products**: Browse available products on the main landing page.
- **Popular Items**: View trending and most purchased dishes.
- **Recommended for You**: Browse personalized product recommendations based on your preferences.
- **My Wallet**: Manage payment cards to be used during checkout.
- **Shopping Cart**: Add, view, and manage items before placing an order.
- **My Orders**: Access past order details.
- **Reviews**: View and submit reviews for each product.

### User Authentication
- **Registration**: Users register and verify their email before accessing full site features.
- **Login/Logout**: Standard login/logout functionality to protect user data.
- **Preferences**: Users can set their preference tags for personalized recommendations.

## Deployment

### Publish the Application
- Use Visual Studio or CLI to publish the application to Azure.

### Configure Azure App Settings
- **Set up environment variables** in Azure App Service to match `appsettings.json`.
- **Database Connections**: Ensure Azure SQL and Cosmos DB connections are securely configured.

## Recent Updates

- **Popular Items Feature**: Added tracking and display of most popular dishes.
- **Recommendation System**: Implemented rule-based recommendations using user preference tags.
- **Shopping Cart Bug Fixes**: Resolved cart display issues.
- **Enhanced Payment Integration**: Improved wallet management features.
- **Review System**: Added Cosmos DB integration for product reviews.
- **UI Enhancements**: Adjustments to button arrangement and page layout for improved user experience.
