# InvestNaija

A secure backend API for user authentication, Azure video storage, and stock portfolio management built with ASP.NET Core Web API.

---

 Controllers

1. InvestNaijaAuthController

Handles user authentication, registration, session tracking, and secure token management.

Endpoints

- **POST** `/api/InvestNaijaAuth/Signup`  
  Create a new user account (Admin or User roles only)

- **POST** `/api/InvestNaijaAuth/Login`  
  Authenticate a user and return access/refresh tokens

- **POST** `/api/InvestNaijaAuth/Logout`  
  Revoke a refresh token and end a user session

- **PATCH** `/api/InvestNaijaAuth/UpdateUser/{id}`  
  Partially update user profile fields (Admin/User)

- **DELETE** `/api/InvestNaijaAuth/DeleteUser/{id}`  
  Hard delete a user from the database (Admin only)

---

 2. AzureBlobController

Handles video upload and retrieval using Azure Blob Storage.

**Endpoints**

- **POST** `/api/AzureBlob/UploadVideo`  
  Upload a video file (Admin only)

- **DELETE** `/api/AzureBlob/DeleteVideo/{fileName}`  
  Delete a video by filename (Admin only)

- **GET** `/api/AzureBlob/GetAllVideos`  
  Retrieve all video URLs (Admin/User)

- **GET** `/api/AzureBlob/GetVideoByFileName/{fileName}`  
  Retrieve a single video by its filename (Admin/User)

---

3. PortfolioController

Handles buying, selling, and retrieving a user's stock portfolio.

**Endpoints**

- **POST** `/api/Portfolio/BuyStocks`  
  Buy stocks for the authenticated user

- **POST** `/api/Portfolio/SellStocks`  
  Sell owned stocks from the authenticated user's portfolio

- **GET** `/api/Portfolio/GetUserPortfolio`  
  Retrieve the current authenticated user's portfolio

---

 4. StockController

Handles operations related to stock data, scraping, and manual updates.

**Endpoints**

- **POST** `/api/Stock/GetLiveStocks`  
  Scrape and persist live stock data from the Nigerian Stock Exchange

- **GET** `/api/Stock/GetAllStocks`  
  Retrieve all stocks from the database

- **GET** `/api/Stock/GetStockBySymbol/{symbol}`  
  Retrieve a single stock by its symbol

- **PUT** `/api/Stock/UpdateStock/{symbol}`  
  Update the stock record by symbol

- **DELETE** `/api/Stock/DeleteStock/{symbol}`  
  Hard delete a stock by symbol

- **GET** `/api/Stock/SearchforStock?query=`  
  Search stocks by name or sector

---

 5. WalletController

Manages wallet creation, transactions (credit/debit), and transaction history.

**Endpoints**

- **POST** `/api/Wallet/CreateWallet`  
  Create a wallet for a new user

- **POST** `/api/Wallet/CreditWallet`  
  Fund (credit) the user's wallet

- **POST** `/api/Wallet/Debitwallet`  
  Deduct (debit) funds from the user's wallet

- **GET** `/api/Wallet/Getwallet/{walletId}`  
  Retrieve a wallet by ID (Admin only)

- **GET** `/api/Wallet/{walletId}/GetWalletTransactions`  
  Retrieve all wallet transactions (Admin only)

- **GET** `/api/Wallet/{walletId}/UserExists`  
  Check if a wallet exists for the given ID (Admin only)

---

 Authentication & Authorization

- All endpoints are protected using **JWT bearer authentication**.
- Role-based access is enforced using `[Authorize(Roles = "...")]`.
- Roles used in the system: `Admin`, `User`.

---

 Technologies Used

- ASP.NET Core Web API  
- Entity Framework Core  
- AutoMapper  
- Serilog (with `HttpContext.TraceIdentifier` logging)  
- Azure Blob Storage SDK  
- SQL Server  

---

Running the API

1. **Set up the database**  
   Configure your connection string in `appsettings.json`.

2. **Apply migrations**  
   ```bash
   dotnet ef database update
