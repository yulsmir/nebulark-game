# 🌌 NEBULARK

**Nebulark** is a multiplayer, procedurally generated survival sandbox game that blends the systemic depth of *Rust*, the building logic of *Minecraft*, and a fully unique world made from colorful, eye-pleasing spheres.

---

## 🎮 Core Concept

**Nebulark** reimagines voxel survival by replacing cubes with **soft, colored spheres** of varying sizes. Players explore a beautiful, organic world filled with spherical terrain, stylized trees, bubble-like structures, and modular environments. The goal: survive, thrive, and shape the world — together.

---

## 🛠️ Game Features

### 🌍 World Structure
- Everything is built from **spheres** instead of cubes.
- Spheres vary in **size, material, and color**, offering visually rich and readable environments.
- The terrain is **procedurally generated** using noise algorithms to create hills, rivers, caves, and biome transitions.

### 🔧 Survival Mechanics
- Players start with nothing and must **gather, craft, and build**.
- Features **hunger, health, and stamina** systems.
- Day/night cycle and weather-based survival layers (cold biomes, heat zones).
- Build using spheres: bases, farms, machines, defenses.

### 👥 Multiplayer
- Server-based play (Mirror or Fish-Networking).
- Persistent worlds with player-placed structures and sphere modifications.
- Public or private server support.
- Optional PvP and raiding systems.

### 🌱 Farming & Resources
- Grow food spheres on crafted soil patches.
- Harvest sphere-based crops and raise wildlife.
- Resource spheres (wood, stone, ore) scattered across biomes.

### 🏗️ Building & Crafting
- Modular building system using sphere components.
- Snap-to-grid or freeform placement.
- Tools and machines crafted from collected resources.

---

## 🎨 Visual Style

- Low-poly, minimalistic design with smooth edges and bright, harmonious colors.
- All game elements — terrain, trees, structures, items — are stylized as spheres.
- Calm and readable aesthetic that appeals to adults and casual players alike.

---

## 🎯 Target Audience

- Fans of survival, crafting, and creative sandbox games.
- Adults who enjoy **long-term progression**, **community building**, and **strategic resource management**.
- Players seeking a relaxing but deep multiplayer experience.

---

## 🧠 Design Pillars

1. **Simple Shapes, Complex Systems** – Intuitive interactions, deep underlying mechanics.
2. **Beautiful Procedural Worlds** – Every server is a unique, colorful, bubble-like biome.
3. **Social Survival** – Build, farm, explore, and raid with or against other players.
4. **Creative Freedom** – The world is your canvas — shape it one sphere at a time.

---

## 🧰 Tech Stack (Planned)

| Feature            | Tech                     |
|--------------------|--------------------------|
| Engine             | Unity 2022 LTS (URP)     |
| Networking         | Mirror or Fish-Networking |
| Procedural Gen     | FastNoise Lite           |
| Voxel System       | Custom or Voxel Play (adapted) |
| Backend (optional) | JSON or MongoDB (persistence) |
| Art Pipeline       | Blender + Substance (for stylized assets) |

---

## 🚀 Development Roadmap (MVP)

### Phase 1: Core Prototype (2–3 months)
- Procedural terrain made of spheres
- Player controller, camera, sphere mining
- Inventory, crafting, sphere placement
- Host/client multiplayer with basic sync

### Phase 2: Expanded Survival (2 months)
- Biomes, hunger, health
- Sphere-based farming + wildlife
- UI polish and basic audio

### Phase 3: Multiplayer & Progression (3 months)
- Persistent worlds
- Base building, raiding mechanics
- Admin tools, player roles


---
🎨 Nebulark Base Color Palette
🌱 Grass / Leaves
    #A8C686 – muted light green
    #6D925C – deep, natural green
🪵 Dirt / Trunks
    #B58C6B – light brown
    #8A5E3C – rich bark tone
⛰️ Stone
    #9C9C9C – warm medium gray
    #6A6A6A – deep slate
🌊 Water / Sky
    #9DD5E3 – bright sky blue
    #70ADC0 – ocean teal
    #DCF1FF – soft horizon highlight
🌸 Sand / Accent
    #F9C891 – soft orange
    #FFE8B5 – pastel cream
    #F2B7D5 – warm pink (for petals or butterflies)
❄️ Snow / Ice

    #E9F6FA – light icy blue
    #C4DDE6 – cold shadow tone

---

🔧 Next Features

🌍 Terrain Unloading
- [X] Remove or hide spheres far behind the player to save memory.
📦 Object Pooling
- [X] Reuse sphere GameObjects instead of destroying/creating constantly.
🧍 Basic Player Interaction
- [X] Add mining, placing, or harvesting mechanics using raycasts.
🌱 Procedural Flora & Creatures
- [X] Generate stylized trees, flowers, or sphere-creatures in biome regions.
🌤 Biome Logic
- [] Use temperature, humidity, or noise layers to add desert, snow, etc.
🧑‍🤝‍🧑 Multiplayer Setup
- [] Sync world generation with players joining/leaving.
🪓 Tools & Crafting
- []  Combine Rust-style tools with Minecraft-style building.
🌱 Add biome-specific flora (e.g., cactus in sand, pine trees on snow).
⚙️ GPU instancing support for large-scale performance boost.
🗂️ Async chunk generation so Unity doesn't hiccup.
🌍 Terrain blending (e.g., transition zones between biomes).
🧠 Basic AI for creatures to roam or idle.
💾 Saving/loading generated chunks to disk or memory.
