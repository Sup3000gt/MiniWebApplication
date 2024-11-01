MiniWebApplication
MiniWebApplication is a web application built with ASP.NET Core MVC that provides core functionalities for product browsing, shopping cart management, payment processing, user authentication, and reviews. It uses Entity Framework Core for data management and integrates with Azure SQL and Azure Cosmos DB for database operations.

Features
Product Browsing: Users can view and search for available products.
Shopping Cart Management: Users can add, update, and remove products from their shopping cart.
Payment Integration: Users can manage payment cards (My Wallet) and select a card for checkout.
User Authentication: Secure login and registration system with email verification.
Email Verification: New users receive a verification email to activate their account after registration.
Order Management: Users can view their order history and detailed order information.
Reviews Feature: Users can leave and view reviews for products, powered by Azure Cosmos DB (NoSQL).
Computer-Optimized Interface: The application is optimized for computer screens only.
Project Structure
Controllers
HomeController: Manages navigation to main pages like product browsing and search.
ShoppingCartController: Manages shopping cart operations, including adding, updating, and removing items.
PaymentController: Manages payment information for users, allowing them to add, delete, and select payment cards.
AccountController: Manages user authentication, including login, logout, registration, and email verification.
OrdersController: Allows users to view their order history and order details.
ReviewsController: Handles adding, displaying, and managing product reviews using Azure Cosmos DB.
Data
ApplicationDbContext: Sets up the database context for Entity Framework with Azure SQL Database as the primary database for relational data. This includes managing tables for users, products, orders, and shopping cart items.
CosmosDbService: Manages the connection and operations with Azure Cosmos DB for the review feature, allowing flexible and efficient storage for product reviews.
Models
User: Represents users with properties for authentication and email verification.
Product: Represents a product with details like name, description, price, and image URL.
ShoppingCartItem: Represents items in the shopping cart, linked to products and users.
PaymentCard: Stores user payment card information, including card number, expiration date, and balance.
Order: Represents user orders, including ordered product details.
Review: Represents product reviews stored in Azure Cosmos DB, with fields like rating, comments, and user information.
ViewModels
Custom ViewModels: Provides structured data for views, maintaining a clear separation between UI and backend logic.
Services
CosmosDbService: Handles CRUD operations for reviews in Azure Cosmos DB.
EmailService: Sends email verification links to users after registration.
Optimization Service: Improves backend performance, especially for complex queries in the product and order modules.
Views
Home: Main page and product browsing interface.
ShoppingCart: Displays the user's shopping cart items and allows checkout.
Payment: Allows users to manage payment cards (My Wallet).
Orders: Displays past orders and details.
Reviews: Shows and adds product reviews.
Account: Login, registration, and email verification pages.
wwwroot
CSS: Custom styles for a unique design.
JS: JavaScript for interactivity and form validation.
Images: Contains logos and product images.
