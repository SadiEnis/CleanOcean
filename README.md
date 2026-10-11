# OceanClean

**OceanClean** is a 2D online co-op multiplayer game developed in Unity.  
Players work together to clean ocean waste, deliver collected trash to a recycling point, earn score, and complete a session-based environmental gameplay loop.

This project was developed as a graduation project with a focus on multiplayer gameplay, lobby flow, player synchronization, WebGL support, gameplay systems, and API-integrated player progression.

---

## Project Overview

OceanClean is designed around a simple and accessible co-op gameplay loop:

1. Players log in to the game.
2. They create or join an online lobby.
3. The host starts the match.
4. Players collect trash from the ocean environment.
5. Collected trash is delivered to a recycling point.
6. Players earn score and in-game currency.
7. Match results are shown at the end of the session.
8. Players can return to the lobby and continue playing.

The project combines Unity gameplay programming, online multiplayer systems, UI flow, WebGL deployment, and persistent player data integration.

---

## Sustainable Development Goals

OceanClean is aligned with the following United Nations Sustainable Development Goals:

### SDG 14: Life Below Water

OceanClean directly supports the theme of **Life Below Water** by focusing on ocean pollution, waste collection, and recycling. The core gameplay loop encourages players to clean ocean waste and deliver collected trash to a recycling point, creating awareness around marine pollution and the importance of protecting aquatic ecosystems.

### SDG 17: Partnerships for the Goals

The project also relates to **Partnerships for the Goals** through its cooperative multiplayer structure. OceanClean is built around teamwork, shared objectives, and collaborative problem-solving, where players work together toward a common environmental goal.

---

## Key Features

- 2D online co-op gameplay
- 1–4 player multiplayer lobby system
- Lobby creation and lobby joining
- Unity Relay-based online connection
- Unity Lobby integration
- Netcode for GameObjects multiplayer architecture
- Lobby-level host migration support
- Player login and register flow
- Player profile and currency display
- Trash collection and recycling gameplay loop
- Score calculation and match result screen
- Leaderboard system
- Shop and inventory UI structure
- WebGL-compatible multiplayer setup
- Local API configuration support

---

## Technologies Used

- Unity
- C#
- Netcode for GameObjects
- Unity Lobby
- Unity Relay
- Unity Transport
- Unity Authentication
- Unity WebGL
- Unity UI
- REST API integration
- Git / GitHub

---

## Gameplay Systems

### Core Gameplay Loop

The main gameplay loop is based on collecting ocean waste and delivering it to a recycling point. This loop is designed to be simple, readable, and suitable for short multiplayer sessions.

```text
Find Trash
→ Collect Trash
→ Move to Recycling Point
→ Deliver Trash
→ Gain Score
→ Continue Cleaning
```

### Player Flow

The player moves through the following main flow:

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

Players move around the map and collect trash objects placed in the environment. The collected trash is delivered to a recycling point to gain score.

### Score System

Players earn score by successfully recycling collected trash. At the end of the match, player scores are displayed on the result screen.

### Leaderboard

The leaderboard displays players based on their total score. This creates long-term motivation and allows players to compare their progress.

### Shop and Inventory UI

The project includes shop and inventory UI structures designed to support player progression and future item-based systems.

---

## Multiplayer Architecture

OceanClean uses Unity multiplayer services and Netcode for GameObjects.

### Lobby System

Players can create or join lobbies before entering the game. The lobby system manages lobby ownership, player list display, lobby code, and the pre-game waiting flow.

### Relay Connection

Unity Relay is used to connect players without requiring direct IP connections. The host creates a Relay allocation, and clients join the session through the Relay join code stored in lobby data.

### Player Synchronization

Netcode for GameObjects is used to synchronize multiplayer gameplay elements such as player presence, scene transitions, and game session flow.

### Host Migration

OceanClean includes lobby-level host migration. If the host leaves the lobby, another player can become the new host. The new host creates a new Relay connection and updates lobby data so the session can continue from the lobby state.

### WebGL Support

The project supports WebGL builds. Since WebGL requires WebSocket-based networking, Relay connection configuration is handled with WebGL compatibility in mind.

---

## API Integration

OceanClean connects to an external API for persistent player data.

The Unity client uses API integration for:

- Login
- Register
- Player profile data
- Currency data
- Leaderboard data
- Inventory data
- Shop data
- Match result updates

For local development, the API base URL can be configured to point to a local backend instance.

Example local API URL:

```text
http://localhost:5211
```

---

## Local Development Setup

### 1. Clone the Repository

```bash
git clone <repository-url>
```

### 2. Open the Project in Unity

Open the project using the Unity version defined in:

```text
ProjectSettings/ProjectVersion.txt
```

### 3. Restore Packages

Unity should automatically restore packages from:

```text
Packages/manifest.json
Packages/packages-lock.json
```

### 4. Configure API Base URL

Set the API base URL to your local or remote backend address.

For local development:

```text
http://localhost:5211
```

Do not include `/api` at the end if the Unity API client already sends endpoints such as:

```text
/api/auth/login
/api/leaderboard
/api/player/profile
```

Correct example:

```text
http://localhost:5211
```

Wrong example if endpoints already include `/api`:

```text
http://localhost:5211/api
```

### 5. Run the Game in Unity Editor

Before entering Play Mode, make sure the backend API is running if you want to test login, profile, leaderboard, shop, inventory, or match result features.

Recommended test order:

```text
Login
→ Main Hub
→ Leaderboard
→ Create Lobby
→ Join Lobby
→ Start Match
→ Finish Match
→ Match Result
```

---

## WebGL Build

To test the WebGL version:

1. Open Unity Build Settings.
2. Select WebGL.
3. Build the project.
4. Run the build with a local server.

Example:

```bash
python -m http.server 8080
```

Open the build in browser:

```text
http://localhost:8080
```

If the Unity client communicates with a local API, make sure the API allows the WebGL origin in CORS settings:

```text
http://localhost:8080
```

---

## Recommended Repository Structure

```text
OceanClean/
├── Assets/
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
├── ProjectSettings/
├── README.md
└── .gitignore
```

---

## Do Not Commit

The following folders and files should not be committed:

```text
Library/
Temp/
Obj/
Build/
Builds/
Logs/
UserSettings/
*.csproj
*.sln
.env
*.sql
*.zip
```

Unity `.meta` files should be committed. They are required for correct asset references.

---

## Screenshots

Screenshots can be added here.

```text
/docs/images/main-menu.png
/docs/images/lobby.png
/docs/images/gameplay.png
/docs/images/result-screen.png
/docs/images/leaderboard.png
```

Example layout:

```markdown
## Screenshots

### Main Menu
![Main Menu](docs/images/main-menu.png)

### Lobby
![Lobby](docs/images/lobby.png)

### Gameplay
![Gameplay](docs/images/gameplay.png)

### Match Result
![Match Result](docs/images/result-screen.png)
```

---

## Project Status

Current project status:

- Unity client is playable.
- Login and register flow is implemented.
- Main Hub UI is implemented.
- Lobby creation and lobby joining are implemented.
- Unity Lobby and Relay integration is implemented.
- Netcode for GameObjects multiplayer flow is implemented.
- Match result screen is implemented.
- Leaderboard system is connected.
- Shop and inventory UI structure is implemented.
- WebGL support is being tested.
- Lobby-level host migration has been implemented.

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
- More polished UI animations
- Better WebGL optimization
- Improved multiplayer recovery during active gameplay

---

## Author

Developed by **Sadi Enis Güçlüer**.

This project was developed as a graduation project focused on Unity game development, online multiplayer systems, WebGL support, and gameplay programming.

---

## License

This project is currently not licensed for public reuse.

If you want to use, modify, or distribute any part of this project, please contact the author.