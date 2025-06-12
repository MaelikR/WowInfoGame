# 🧙 WoWGameInfo – Discord Bot pour World of Warcraft

**WoWGameInfo** est un bot Discord communautaire dédié à l’univers de **World of Warcraft**, toutes extensions confondues (Classic, Retail, MoP, etc.). Il fournit des quiz, des infos, des vidéos, du lore, des builds, des routes de farm, des actualités, et bien plus encore !

---

## ✨ Fonctionnalités principales

### 🎮 Quiz & Classements
- `!quiz` – Lance un quiz aléatoire sur WoW
- `!reponse <ta_réponse>` – Tente ta chance
- `!points` – Affiche ton score
- `!classement` – Top 5 des joueurs

### 📘 Lore & Histoire
- `!lore <perso>` – Infos lore d’un personnage
- `!boss`, `!zones`, `!capitales` – Univers du jeu
- `!extensions`, `!factions`, `!classes`, `!races`
- `!livreswow` – Romans officiels de Blizzard
- `!lieuxrp` – Suggestions RP immersives

### 🛠️ Builds & Métiers
- `!build <classe>` – Guide PvE Icy-Veins
- `!rotation <classe>` – Rotation DPS
- `!talents <classe>` – Arbre interactif Wowhead
- `!métier <nom>` – Guide profession wow-professions

### 🏰 Donjons & Raids
- `!donjons` – Suggestions par niveau
- `!raid <extension>` – Raids majeurs
- `!reset` – Prochain reset hebdomadaire

### 🎥 Vidéos & Actualités
- `!videointro`, `!videoraid`, `!videobuild <classe>`, `!videoaddon`
- `!videoactualite`, `!news`, `!newsrss`
- `!tournoi` – Suivre l’eSport WoW

### 🌿 Farming & Conseils
- `!routefarm <ressource>` – Zones conseillées
- `!farm` – Spot conseillé
- `!astuce`, `!citation`, `!anecdote`, `!defi`

### 🔍 Recherches & Extras
- `!recherche <terme>` – Résultats Wowhead
- `!faq`, `!serveurs`, `!heureeu`

---

## 🚀 Slash Commands & Interaction UI

- `/help` – Menu interactif avec boutons par catégories
- Boutons : 🎮 Quiz, 📘 Lore, 🧠 Builds, 🏰 Raids, 🎥 Vidéos

---

## 🧠 Technologies

- `.NET 7 / 8+` avec C#
- [Discord.Net v3.12+](https://github.com/discord-net/Discord.Net)
- Support des **Commandes classiques** & **Slash**
- Modules intégrés en classes C#

---

## 🛠️ Installation

```bash
git clone https://github.com/ton-utilisateur/WowGameInfo.git
cd WowGameInfo
dotnet restore
dotnet run
