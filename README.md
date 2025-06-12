# 🧙‍♂️ WoWGameInfo – Discord Bot pour World of Warcraft

WoWGameInfo est un bot Discord complet et interactif dédié à l’univers de **World of Warcraft**, conçu en **C# avec Discord.Net**. Il fournit des quiz, des guides, des vidéos, des liens utiles, des infos en temps réel et bien plus encore pour toutes les versions du jeu (Retail, Classic, MoP Classic, etc.).

## ✨ Fonctionnalités principales

- 🎮 **Quiz & Score** avec classement
- 🧠 **Builds PvE**, rotations et talents
- 📘 **Lore, races, classes, boss**
- 🏰 **Raids, donjons, capitales**
- 📦 **Addons, professions, métiers**
- 📺 **Vidéos utiles** (guides, intro, actu)
- 📰 **Actus WoW (flux RSS & news)**
- ⏰ **Infos serveur en temps réel** (heure, reset)
- 📈 **Meta DPS & PvP**
- 🌿 **Farm routes, réputations, objectifs**

## 📸 Aperçu

![screenshot](./assets/preview-wowgameinfo.png)

## 🚀 Commandes disponibles

> Préfixe classique `!` + support slash `/commands`

### 🎮 Quiz
- `!quiz` – Lancer un quiz aléatoire
- `!reponse <ta_réponse>`
- `!points` – Ton score actuel
- `!classement` – Top 5 joueurs

### 📘 Lore & Univers
- `!lore <nom>` – L'histoire d’un perso
- `!boss`, `!races`, `!classes`, `!zones`
- `!factions`, `!metiers`, `!extensions`

### 🧠 Builds & Talents
- `!build <classe>` – Guide PvE Icy Veins
- `!rotation <classe>`
- `!talents <classe>`

### 🏰 Raids et Donjons
- `!raid <extension>` – Raids de WotLK, Legion, BFA
- `!donjons` – Donjons populaires

### 📺 Vidéos
- `!videointro`, `!videoraid`, `!videobuild <classe>`
- `!videoaddon`, `!videoactualite`

### 📰 Infos & News
- `!news` – Actu Blizzard
- `!newsrss` – Flux RSS de Wowhead
- `!heureeu`, `!reset` – Heure & reset hebdo

### 🛠️ Extras utiles
- `!routefarm <ressource>` – Zones de farm
- `!addons`, `!reputations`, `!objectifs`
- `!pvpmeta`, `!dpsmeta`
- `!faq`, `!capitales`, `!serveurs`

## 🧪 Slash Commandes & Boutons
- `/help` – Affiche un menu interactif
- Intégration de composants interactifs (`buttons`, `select menus`, etc.)

## 🔧 Installation

1. Clone ce repo :
   ```bash
   git clone https://github.com/ton-profil/WoWGameInfoBot.git
   cd WoWGameInfoBot
