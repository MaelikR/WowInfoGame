# 🧙 WoWGameInfo – Discord Bot pour World of Warcraft

**WoWGameInfo** est un bot Discord immersif et interactif, conçu pour tous les passionnés de World of Warcraft : débutants, vétérans ou curieux. Il fournit des informations en temps réel sur les donjons, raids, builds, classes, races, actualités, quêtes et bien plus encore !

---

## 🎮 Fonctionnalités principales

- 🔍 **Infos sur le jeu** : classes, races, factions, capitales, zones
- 🛠️ **Builds & Talents** : guides PvE Icy Veins, rotations, talents
- 🧠 **Quiz & Points** : test de connaissances WoW + classement des joueurs
- 📖 **Lore & Histoire** : accès rapide aux pages WoWpedia & Wowhead
- 🏰 **Donjons et Raids** : par extension (Lich King, Legion, BfA…)
- 🌐 **Actus en ligne** : news Blizzard & flux RSS Wowhead
- 🎥 **Vidéos WoW** : trailers, builds, raids, addons, actualité
- 🎯 **Défis & Astuces** : défis aléatoires, tips de jeu, anecdotes
- ⏱️ **Heure & reset serveur EU** (CET)
- 📊 **Meta DPS** : lien direct vers le classement actuel

---

## ⚙️ Commandes disponibles (préfixe `!`)

### 📌 Informations générales
- `!info`, `!classes`, `!races`, `!factions`, `!zones`, `!extensions`, `!capitales`, `!serveurs`

### 🧠 Builds & Optimisation
- `!build <classe>`
- `!rotation <classe>`
- `!talents <classe>`
- `!dpsmeta`

### 📚 Lore & Exploration
- `!lore <personnage>`
- `!boss`
- `!quetes`

### ⚔️ Activités
- `!donjons`
- `!raid <extension>`
- `!bg`
- `!farm`, `!routefarm <ressource>`

### 🎮 Quiz & Classement
- `!quiz`, `!reponse <texte>`
- `!points`, `!classement`

### 💡 Divers
- `!defi`, `!citation`, `!anecdote`, `!astuce`, `!faq`
- `!reset`, `!heureeu`

### 📰 News & Vidéos
- `!news`, `!newsrss`
- `!videointro`, `!videoraid`, `!videobuild <classe>`, `!videoaddon`, `!videoactualite`

---

## 🚀 Lancer le bot localement

```bash
git clone https://github.com/votre-utilisateur/WoWGameInfo.git
cd WoWGameInfo
dotnet restore
dotnet run
