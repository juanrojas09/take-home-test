# Loan Management System

A full-stack loan management application built with .NET Core 8 and Angular 18.

---

## 🚀 Quick Start

### Prerequisites

- Docker Desktop installed or docker on linux machines
- Ports 4200, 5001, and 1433 available

### Running the Application

1. **Clone and navigate to project:**
   ```bash
   git clone <repository-url>
   cd take-home-test
   ```

2. **Start all services:**
   ```bash
   docker-compose up -d
   ```

3. **Access the application:**
   - Frontend: http://localhost:4200
   - Backend API: http://localhost:5001

4. **Login credentials:**
   
   **Admin:**
   ```
   Email: admin@fundo.com
   Password: Admin123!
   ```
   
   **User:**
   ```
   Email: user1@fundo.com
   Password: User123!
   ```

5. **Stop services:**
   ```bash
   docker-compose down
   ```

---

## 🛠️ Local Development (Without Docker)

### Backend

```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```
API runs on http://localhost:5000

### Frontend

```bash
cd frontend
npm install
ng serve
```
Frontend App runs on http://localhost:4200

---

## 📋 API Endpoints

### Authentication
- `POST /api/auth/login` - User login


### Loans
- `GET /api/loans` - List all loans (Admin only)
- `GET /api/loans/my` - List user's loans
- `GET /api/loans/{id}` - Get loan details
- `POST /api/loans` - Create new loan
- `POST /api/loans/{id}/payment` - Make payment

---

## 🧪 Running Tests

```bash
cd backend
dotnet test
```

---

## 🏗️ Tech Stack

**Backend:**
- .NET Core 8
- Entity Framework Core
- SQL Server
- JWT Authentication

**Frontend:**
- Angular 18
- Standalone Components
- Signals for state management

**DevOps:**
- Docker & Docker Compose
- Multi-stage builds

---

## 🔐 Features

- JWT authentication with HttpOnly cookies
- Role-based access control (Admin/User)
- Paginated loan listing
- Responsive UI design
- Automatic database seeding
- Comprehensive error handling

---

## 📝 Project Structure

```
take-home-test/
├── backend/              # .NET Core API
├── frontend/             # Angular app
├── docker-compose.yml    # Services orchestration
└── README.md
```

---

## 🐛 Troubleshooting

**Containers won't start:**
```bash
docker-compose down -v
docker-compose up --build
```

**View logs:**
```bash
docker-compose logs -f
```

**Check running containers:**
```bash
docker-compose ps
```

---

Built with .NET Core 8 & Angular 18

---

## 📌 Implementation Notes

### Future Enhancements

Due to time constraints, the following features were not implemented but the infrastructure is already in place:

**Event Sourcing with MongoDB:**
- The backend is fully configured to work with domain events
- MongoDB service is included in the Docker orchestration
- Intended implementation: Log payment events using Event Sourcing pattern in MongoDB
- Domain events like `LoanPaymentEvent` would be captured and stored in a decoupled manner
- All necessary backend infrastructure for MongoDB integration is ready

**Environment Variables & Secrets Management:**
- Production environment should use secure secret management
- Recommended approach: Azure Key Vault or similar services for sensitive configuration
- Current implementation uses environment variables for development purposes

**Next Steps:**
- Implement domain event handlers for loan payment tracking
- Integrate MongoDB for event sourcing persistence
- Configure Azure Key Vault for production secrets
- Add comprehensive logging and monitoring

### Implemented Features

**Authentication & Authorization:**
- JWT-based authentication system with HttpOnly cookies for enhanced security
- Role-based access control implementation (Admin/User roles)
- Dynamic endpoint routing based on user roles:
  - **Admin users**: Access to all loans via `/api/loans` endpoint
  - **Regular users**: Access only to their own loans via `/api/loans/my` endpoint
- Token validation and refresh mechanism
- Frontend interceptors for automatic token handling
- Secure session management with dual storage approach (cookies + sessionStorage)