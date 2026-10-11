# OceanClean

**OceanClean** is a 2D online co-op multiplayer game developed in Unity.  
Players work together to clean ocean waste, deliver collected trash to a recycling point, earn score, and progress through a session-based gameplay loop.

The project was developed as a graduation project and focuses on online multiplayer gameplay, lobby flow, player synchronization, WebGL support, persistent player data, and an admin dashboard for monitoring game activity.

---

## Project Overview

OceanClean is designed around a simple but meaningful gameplay loop:

1. Players log in to the game.
2. They create or join an online lobby.
3. The host starts the match.
4. Players collect trash from the ocean environment.
5. Collected trash is delivered to a recycling point.
6. Players earn score and in-game currency.
7. Match results are saved and shown at the end of the session.

The project combines gameplay programming, multiplayer networking, backend integration, persistent player progression, and web-based administration.

---

## Key Features

- 2D online co-op gameplay
- 1–4 player multiplayer lobby system
- Lobby creation and lobby joining
- Unity Relay-based online connection
- Unity Lobby integration
- Netcode for GameObjects multiplayer architecture
- Host migration support at lobby level
- Player login and register flow
- Player profile and currency system
- Trash collection and recycling gameplay loop
- Score calculation and match result screen
- Leaderboard system
- Shop and inventory UI structure
- WebGL-compatible multiplayer setup
- Admin dashboard for player, match, economy, and activity data
- Local and server-based API support

---

## Technologies Used

### Game Client

- Unity
- C#
- Netcode for GameObjects
- Unity Lobby
- Unity Relay
- Unity Transport
- Unity WebGL
- Unity UI

### Backend

- ASP.NET Core Web API
- .NET 8
- MySQL
- MySqlConnector
- BCrypt password hashing
- JWT authentication
- Swagger / OpenAPI

### Admin Panel

- React
- Vite
- TypeScript
- Tailwind CSS
- shadcn/ui
- REST API integration

### Development Tools

- Rider
- Git / GitHub
- MySQL Workbench
- Postman / Swagger
- Linux server deployment
- Nginx
- systemd

---

## Gameplay Systems

### Player Flow

The player starts from the main menu and proceeds through authentication, main hub, lobby selection, gameplay, and result screens.

Main flow:

```text
Main Menu
→ Login / Register
→ Main Hub
→ Lobby List / Create Lobby
→ Lobby Info
→ Game Scene
→ Match Result
→ Return to Lobby
```

### Trash Collection

Players move around the map and collect trash objects. Trash is delivered to a recycling point to gain score. This system forms the core gameplay loop of the project.

### Score System

Each successful recycling action contributes to the player score. At the end of the match, scores are displayed on the result screen and can be saved to the backend.

### Leaderboard

The leaderboard lists active players based on their total score. This creates long-term motivation and allows players to compare their progress.

### Shop and Inventory

The project includes shop and inventory UI structures connected to backend data. These systems are designed to support long-term progression and future cosmetic or item-based features.

---

## Multiplayer Architecture

OceanClean uses Unity’s multiplayer services and Netcode for GameObjects.

### Lobby System

Players can create or join lobbies before entering the game. Lobby data includes player information, lobby ownership, and Relay join code.

### Relay Connection

Unity Relay is used to connect players without requiring direct IP connections. The host creates a Relay allocation, and clients join through the Relay join code stored in lobby data.

### Host Migration

The project includes lobby-level host migration. If the host leaves the lobby, another player can become the new host. The new host creates a new Relay connection and updates the lobby data so other players can reconnect.

### WebGL Support

WebGL builds require WebSocket-based Relay configuration. The project handles WebGL-specific Relay connection setup to support browser-based multiplayer gameplay.

---

## Backend Overview

The backend API manages persistent player and game data.

Main backend responsibilities:

- User registration
- User login
- Password hashing
- Player profile management
- Currency tracking
- Match result saving
- Leaderboard data
- Inventory data
- Shop item data
- Admin dashboard data
- Player login activity logs
- Admin audit logs

The Unity client communicates with the API through REST endpoints.

---

## Database Overview

The project uses a MySQL database. Main tables include:

- `users`
- `player_profiles`
- `player_login_logs`
- `matches`
- `match_players`
- `player_action_logs`
- `shop_items`
- `player_inventory`
- `currency_transactions`
- `admin_users`
- `admin_audit_logs`

The database stores both gameplay-related data and admin panel data.

---

## Admin Panel

The admin panel is used to monitor and manage game data.

Main admin features:

- Dashboard overview
- Player list
- Player detail page
- Player status management
- Match list
- Match detail page
- Economy tracking
- Activity charts
- Event logs
- Admin profile
- Admin audit logs

This makes the project more than a standalone Unity prototype by adding live data monitoring and management capabilities.

---

## Screenshots

> Screenshots can be added here.

```text
/docs/images/main-menu.png
/docs/images/lobby.png
/docs/images/gameplay.png
/docs/images/result-screen.png
/docs/images/admin-dashboard.png
```

---

## Local Development Setup

### 1. Backend API

Clone the backend project and restore dependencies:

```bash
dotnet restore
dotnet build
```

Run the API:

```bash
dotnet run
```

Swagger will be available at:

```text
http://localhost:5211/swagger
```

Example local connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=clean_oceans_db;User=root;Password=your_password;"
  }
}
```

### 2. Admin Panel

Install dependencies:

```bash
npm install
```

Create a `.env` file:

```env
VITE_API_BASE_URL=http://localhost:5211/api
```

Run the admin panel:

```bash
npm run dev
```

Admin panel will be available at:

```text
http://localhost:5173
```

### 3. Unity Client

Open the Unity project and set the API base URL to your local API address:

```text
http://localhost:5211
```

Then enter Play Mode and test:

- Login
- Register
- Main Hub
- Leaderboard
- Lobby creation
- Lobby joining
- Match start
- Match result update

### 4. WebGL Build

Build the Unity project for WebGL.

Run the build with a local server:

```bash
python -m http.server 8080
```

Open:

```text
http://localhost:8080
```

Make sure the API allows the WebGL origin in CORS settings:

```text
http://localhost:8080
```

---

## Important Notes

Do not commit sensitive local configuration files such as:

```text
appsettings.Development.json
.env
bin/
obj/
Library/
Temp/
Logs/
UserSettings/
```

Recommended files to keep as examples:

```text
appsettings.Development.example.json
.env.example
```

---

## Project Status

Current project status:

- Unity client is playable.
- Local backend API is functional.
- MySQL database integration is working.
- Admin panel is functional.
- Leaderboard system is connected.
- Lobby and Relay systems are implemented.
- WebGL support is being tested.
- Host migration has been implemented at lobby level.

---

## Future Improvements

Planned or possible future improvements:

- More trash types with different score values
- Heavy trash objects requiring two players
- Character customization
- Cosmetic shop items
- Daily and weekly leaderboard
- More maps
- Mission and achievement systems
- Improved in-game economy
- Better WebGL UI optimization
- More detailed analytics dashboard
- Full in-game host migration or controlled match recovery

---

## Author

Developed by **Sadi Enis Güçlüer**.

This project was developed as a graduation project focused on game development, multiplayer systems, backend integration, and full-stack game infrastructure.

---

## License

This project is currently not licensed for public reuse.

If you want to use, modify, or distribute any part of this project, please contact the author.